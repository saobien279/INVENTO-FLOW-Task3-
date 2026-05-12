using AutoMapper;
using InventoFlow.Application.DTOs.Order;
using InventoFlow.Application.DTOs.Product; // Đảm bảo đúng namespace của DTOs bạn đã tạo
using InventoFlow.Application.DTOs.Category;
using InventoFlow.Domain.Entities;

namespace InventoFlow.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // 1. Ánh xạ từ DTO gửi lên sang Model (Dùng cho Create/Update)
            CreateMap<ProductCreateDto, Product>();
            CreateMap<ProductUpdateDto, Product>();

            // 2. Ánh xạ từ Model sang DTO để trả về kết quả cho Client
            CreateMap<Product, ProductResponseDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : null));

            // 3. Mapping cho Category
            CreateMap<CategoryCreateDto, Category>();
            CreateMap<CategoryUpdateDto, Category>();
            CreateMap<Category, CategoryResponseDto>();

            // 4. Mapping cho Supplier
            CreateMap<InventoFlow.Application.DTOs.Supplier.SupplierCreateDto, Supplier>();
            CreateMap<InventoFlow.Application.DTOs.Supplier.SupplierUpdateDto, Supplier>();
            CreateMap<Supplier, InventoFlow.Application.DTOs.Supplier.SupplierResponseDto>();

            // Mapping từ Model Order sang DTO để trả về kết quả
            CreateMap<Order, OrderResponseDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems));

            // Mapping từ Model OrderItem sang DTO
            // .ForMember giúp lấy Name từ object Product lồng bên trong
            CreateMap<OrderItem, OrderItemResponseDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product!.Name));
        }
    }
}