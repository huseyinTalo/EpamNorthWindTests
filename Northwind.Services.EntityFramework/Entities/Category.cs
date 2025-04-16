using Northwind.Services.Repositories;

namespace Northwind.Services.EntityFramework.Entities;

public class Category
{
    public int CategoryId { get; set; }

    public string? CategoryName { get; set; } = default!;

    public string? Description { get; set; } = default!;

    public virtual IList<Product> Products { get; set; } = [];
}
