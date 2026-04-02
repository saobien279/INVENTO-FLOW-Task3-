using MediatR;
using AutoMapper;
using InventoFlow.Application.Interfaces.Repositories;
using InventoFlow.Application.DTOs.Order;
using InventoFlow.Domain.Entities;

namespace InventoFlow.Application.Features.Orders.Commands.CreateOrder
{
    public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateOrderHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<OrderResponseDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            // 1. Khởi tạo đối tượng Order mới
            var order = new Order
            {
                UserId = request.UserId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 0,
                OrderItems = new List<OrderItem>()
            };

            // 2. Duyệt qua từng mặt hàng khách đặt
            foreach (var itemDto in request.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);

                if (product == null)
                    throw new Exception($"Sản phẩm có ID {itemDto.ProductId} không tồn tại.");

                if (product.StockQuantity < itemDto.Quantity)
                    throw new Exception($"Sản phẩm '{product.Name}' không đủ hàng trong kho (Còn lại: {product.StockQuantity}).");

                // 3. Trừ tồn kho
                product.StockQuantity -= itemDto.Quantity;
                _unitOfWork.Products.Update(product);

                // 4. Tạo chi tiết đơn hàng và chốt giá tại thời điểm mua
                var orderItem = new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    PriceAtPurchase = product.Price
                };

                order.TotalAmount += orderItem.Quantity * orderItem.PriceAtPurchase;
                order.OrderItems.Add(orderItem);
            }

            // 5. Lưu đơn hàng
            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.CompleteAsync();

            // 6. Map và trả về kết quả đầy đủ
            return _mapper.Map<OrderResponseDto>(order);
        }
    }
}
