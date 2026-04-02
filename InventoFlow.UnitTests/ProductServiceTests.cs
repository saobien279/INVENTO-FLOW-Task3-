using Moq;
using Xunit;
using AutoMapper;
using InventoFlow.Application.Services;
using InventoFlow.Application.Interfaces.Repositories;
using InventoFlow.Domain.Entities;
using InventoFlow.Application.DTOs.Product;

namespace InventoFlow.UnitTests
{
    public class ProductServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            // 1. ARRANGE: Chuẩn bị "đồ giả" (Mock)
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            
            // Khởi tạo Service với các Mock đã tạo
            _productService = new ProductService(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Fact] // Đánh dấu đây là một bài Test
        public async Task GetProductByIdAsync_WhenProductExists_ReturnsProductDto()
        {
            // --- ARRANGE ---
            var productId = 2;
            var product = new Product { Id = productId, Name = "Whey Protein", Price = 500 };
            var productDto = new ProductResponseDto { Id = productId, Name = "Whey Protein" };

            // Thiết lập: Khi gọi UoW.Products.GetById, hãy trả về object 'product' ở trên
            _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(productId))
                           .ReturnsAsync(product);

            // Thiết lập: Khi gọi Mapper, hãy trả về 'productDto'
            _mockMapper.Setup(m => m.Map<ProductResponseDto>(product))
                       .Returns(productDto);

            // --- ACT --- (Hành động thực tế)
            var result = await _productService.GetProductByIdAsync(productId);

            // --- ASSERT --- (Kiểm tra kết quả)
            Assert.NotNull(result);
            Assert.Equal(productId, result.Id);
            Assert.Equal("Whey Protein", result.Name);
            
            // Kiểm tra xem Service có thực sự gọi vào Repository không (Verify)
            _mockUnitOfWork.Verify(u => u.Products.GetByIdAsync(productId), Times.Once);
        }
    }
}