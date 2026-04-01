using AutoMapper;
using InventoFlow.Application.DTOs.Order;
using InventoFlow.Application.PageQuery;
using InventoFlow.Domain.Entities;
using InventoFlow.Application.Interfaces.Repositories;
using InventoFlow.Application.Interfaces.Services;

namespace InventoFlow.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<OrderResponseDto> CreateOrderAsync(OrderCreateDto dto)
        {
            // 1. Khởi tạo đối tượng Order mới
            var order = new Order
            {
                UserId = dto.UserId,
                OrderDate = DateTime.Now,
                TotalAmount = 0,
                OrderItems = new List<OrderItem>()
            };

            // 2. Duyệt qua từng món hàng khách đặt trong DTO
            foreach (var itemDto in dto.Items)
            {
                // Tìm sản phẩm trong Database
                var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);

                if (product == null)
                {
                    throw new Exception($"Sản phẩm có ID {itemDto.ProductId} không tồn tại.");
                }

                // Kiểm tra tồn kho
                if (product.StockQuantity < itemDto.Quantity)
                {
                    throw new Exception($"Sản phẩm '{product.Name}' không đủ hàng trong kho (Còn lại: {product.StockQuantity}).");
                }

                // 3. Xử lý logic nghiệp vụ
                // Trừ số lượng tồn kho
                product.StockQuantity -= itemDto.Quantity;
                _unitOfWork.Products.Update(product);

                // Tạo OrderItem và chốt giá mua
                var orderItem = new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    PriceAtPurchase = product.Price // Lấy giá hiện tại của sản phẩm
                };

                // Cộng dồn vào tổng tiền đơn hàng
                order.TotalAmount += (orderItem.Quantity * orderItem.PriceAtPurchase);

                // Thêm vào danh sách chi tiết đơn hàng
                order.OrderItems.Add(orderItem);
            }

            // 4. Lưu đơn hàng và các thay đổi sản phẩm vào Database
            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.CompleteAsync();

            // 5. Trả về kết quả dưới dạng DTO
            return _mapper.Map<OrderResponseDto>(order);
        }

        public async Task<PagedResult<OrderResponseDto>> GetAllOrdersAsync(OrderQueryParams query)
        {
            var (orders, totalCount) = await _unitOfWork.Orders.GetAllAsync(query);

            return new PagedResult<OrderResponseDto>
            {
                Items = _mapper.Map<List<OrderResponseDto>>(orders),
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<OrderResponseDto?> GetOrderByIdAsync(int id)
        {
            // Sử dụng hàm có Include chi tiết để hiển thị đầy đủ thông tin
            var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(id);
            return _mapper.Map<OrderResponseDto>(order);
        }

        public async Task<IEnumerable<OrderResponseDto>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = await _unitOfWork.Orders.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
        }
    }
}