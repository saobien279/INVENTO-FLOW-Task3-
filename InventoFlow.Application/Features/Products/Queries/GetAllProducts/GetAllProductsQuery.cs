using MediatR;
using InventoFlow.Application.DTOs.Product;
using InventoFlow.Application.PageQuery;

namespace InventoFlow.Application.Features.Products.Queries.GetAllProducts
{
    // Query: "Lấy danh sách sản phẩm có phân trang + tìm kiếm + sort"
    public record GetAllProductsQuery(ProductQueryParams Params) : IRequest<PagedResult<ProductResponseDto>>;
}
