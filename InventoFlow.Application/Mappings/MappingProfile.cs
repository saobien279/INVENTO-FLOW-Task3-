using AutoMapper;
using InventoFlow.Application.DTOs.Order;
using InventoFlow.Application.DTOs.Product; // Đảm bảo đúng namespace của DTOs bạn đã tạo
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
            CreateMap<Product, ProductResponseDto>();

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