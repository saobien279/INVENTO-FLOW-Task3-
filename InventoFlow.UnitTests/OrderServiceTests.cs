using AutoMapper; // QUAN TRỌNG NHẤT - Sửa lỗi CS0246
using Moq;
using Xunit;
using InventoFlow.Application.Services;
using InventoFlow.Application.Interfaces.Repositories;
using InventoFlow.Application.DTOs.Order;
using InventoFlow.Domain.Entities;

namespace InventoFlow.UnitTests
{
    public class OrderServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _orderService = new OrderService(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task CreateOrderAsync_WhenStockIsNotEnough_ThrowsException()
        {
            // --- 1. ARRANGE ---
            var dto = new OrderCreateDto { 
                UserId = 1, 
            Items = new List<OrderItemCreateDto> { new OrderItemCreateDto { ProductId = 1, Quantity = 100 } } 
            };

            var productInDb = new Product { Id = 1, Name = "Whey", StockQuantity = 10 }; // Chỉ còn 10 hộp

            // Giả lập: Tìm thấy sản phẩm trong kho
            _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(1)).ReturnsAsync(productInDb);

            // --- 2. ACT & ASSERT ---
            // Kiểm tra xem hàm có ném ra Exception khi đặt 100 hộp (trong khi kho chỉ có 10) không
            var exception = await Assert.ThrowsAsync<Exception>(() => _orderService.CreateOrderAsync(dto));

            // Kiểm tra nội dung câu chửi (Error Message) có đúng không
            Assert.Contains("không đủ hàng", exception.Message);

            // QUAN TRỌNG: Đảm bảo UoW.CompleteAsync CHƯA BAO GIỜ được gọi (vì lỗi nửa chừng)
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_WhenEverythingIsValid_ReturnsOrderResponseDto()
        {
            // --- 1. ARRANGE ---
            var productId = 1;
            var initialStock = 10;
            var price = 500m;
            var buyQuantity = 2;

            var dto = new OrderCreateDto
            {
                UserId = 1,
                Items = new List<OrderItemCreateDto> 
                { 
                    new OrderItemCreateDto { ProductId = productId, Quantity = buyQuantity } 
                }
            };

            var productInDb = new Product 
            { 
                Id = productId, 
                Name = "Whey Protein", 
                StockQuantity = initialStock, 
                Price = price 
            };

            // Thiết lập Mock cho Repository
            _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(productId))
                           .ReturnsAsync(productInDb);

            // Mock cho hàm Update sản phẩm
            _mockUnitOfWork.Setup(u => u.Products.Update(It.IsAny<Product>()));

            // Mock cho hàm Add đơn hàng
            _mockUnitOfWork.Setup(u => u.Orders.AddAsync(It.IsAny<Order>()))
                           .Returns(Task.CompletedTask);

            // Mock cho hàm Lưu Thay Đổi
            _mockUnitOfWork.Setup(u => u.CompleteAsync())
                           .ReturnsAsync(1);

            // Mock cho Mapper
            _mockMapper.Setup(m => m.Map<OrderResponseDto>(It.IsAny<Order>()))
                       .Returns(new OrderResponseDto { TotalAmount = buyQuantity * price });

            // --- 2. ACT ---
            var result = await _orderService.CreateOrderAsync(dto);

            // --- 3. ASSERT ---
            // Kiểm tra tổng tiền (buyQuantity * price = 1000)
            Assert.Equal(1000, result.TotalAmount);

            // Kiểm tra số lượng tồn kho (10 - 2 = 8)
            Assert.Equal(8, productInDb.StockQuantity);

            // Kiểm tra xem CompleteAsync có được gọi đúng 1 lần không
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
            
            // Kiểm tra xem có lệnh Update sản phẩm không
            _mockUnitOfWork.Verify(u => u.Products.Update(productInDb), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_WhenProductNotFound_ThrowsException()
        {
            // --- 1. ARRANGE ---
            var productId = 999; // ID không tồn tại
            var dto = new OrderCreateDto
            {
                UserId = 1,
                Items = new List<OrderItemCreateDto> 
                { 
                    new OrderItemCreateDto { ProductId = productId, Quantity = 1 } 
                }
            };

            // Giả lập: Repository trả về null (không tìm thấy máy)
            _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(productId))
                        .ReturnsAsync((Product)null!);

            // --- 2. ACT & ASSERT ---
            // Kiểm tra xem Service có ném ra lỗi "không tìm thấy" như mong đợi không
            var exception = await Assert.ThrowsAsync<Exception>(() => _orderService.CreateOrderAsync(dto));

            // Kiểm tra nội dung tin nhắn lỗi
            Assert.Contains("không tồn tại", exception.Message.ToLower());

            // Đảm bảo KHÔNG CÓ lệnh lưu dữ liệu nào được thực hiện
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Never);
        }
    }
}