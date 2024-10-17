using Microsoft.EntityFrameworkCore;
using NoCaching.DTOs;
using NoCaching.Models;

public class QueryService(AdventureWorksProductContext _context) : IQueryService
{
    public async Task<ProductDTO> GetProductAsync(int id)
    {
        var product = await _context.Products
            .Where(p => p.ProductID == id)
            .Select(x => new ProductDTO { Name = x.Name, ProductID = x.ProductID })
            .FirstOrDefaultAsync();

        return product;
    }

    public async Task<ProductCategoryDTO> GetProductCategoryAsync(int subcategoryId)
    {
        var subcategories = await _context.ProductCategories
                             .Include(x => x.Products)
                             .Where(x => x.ProductCategoryID == subcategoryId)
                             .Select(x => new ProductCategoryDTO
                             {
                                 ProductCategoryID = x.ProductCategoryID,
                                 ParentProductCategoryID = x.ParentProductCategoryID,
                                 Name = x.Name,
                                 Rowguid = x.Rowguid,
                                 ModifiedDate = x.ModifiedDate,
                                 Products = x.Products.Select(p => new ProductDTO
                                 {
                                     ProductID = p.ProductID,
                                     Name = p.Name
                                 }).ToList()
                             })
                             .FirstOrDefaultAsync();
        return subcategories;
    }
}