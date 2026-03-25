using AutoMapper;
using INVENTO_FLOW.DTOs.Order;
using INVENTO_FLOW.Models;
using INVENTO_FLOW.Repositories.Interfaces;
using INVENTO_FLOW.Services.Interfaces;

namespace INVENTO_FLOW.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IProductRepository _productRepo;
        private readonly IMapper _mapper;

        public OrderService(IOrderRepository orderRepo, IProductRepository productRepo, IMapper mapper)
        {
            _orderRepo = orderRepo;
            _productRepo = productRepo;
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
                var product = await _productRepo.GetByIdAsync(itemDto.ProductId);

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
                _productRepo.Update(product); // Cập nhật trạng thái sản phẩm

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
            await _orderRepo.AddAsync(order);
            await _orderRepo.SaveChangesAsync();

            // 5. Trả về kết quả dưới dạng DTO
            return _mapper.Map<OrderResponseDto>(order);
        }

        public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
        }

        public async Task<OrderResponseDto?> GetOrderByIdAsync(int id)
        {
            // Sử dụng hàm có Include chi tiết để hiển thị đầy đủ thông tin
            var order = await _orderRepo.GetOrderWithDetailsAsync(id);
            return _mapper.Map<OrderResponseDto>(order);
        }

        public async Task<IEnumerable<OrderResponseDto>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = await _orderRepo.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
        }
    }
}