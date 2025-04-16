namespace Northwind.Services.EntityFramework.Entities;

public class Supplier
{
    public int SupplierId { get; set; }

    public string? CompanyName { get; set; } = default!;

    public string? ContactName { get; set; } = default!;

    public string? ContactTitle { get; set; } = default!;

    public string? Address { get; set; } = default!;

    public string? City { get; set; } = default!;

    public string? Region { get; set; } = default!;

    public string? PostalCode { get; set; } = default!;

    public string? Country { get; set; } = default!;

    public string? Phone { get; set; } = default!;

    public string? Fax { get; set; } = default!;

    public string? HomePage { get; set; } = default!;

    public virtual IList<Product> Products { get; set; } = [];
}
