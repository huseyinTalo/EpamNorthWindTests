namespace Northwind.Services.EntityFramework.Entities;

public class Employee
{
    public int EmployeeId { get; set; }

    public string? LastName { get; set; } = default!;

    public string? FirstName { get; set; } = default!;

    public string? Title { get; set; } = default!;

    public string? TitleOfCourtesy { get; set; } = default!;

    public DateTime? BirthDate { get; set; }

    public DateTime? HireDate { get; set; }

    public string? Address { get; set; } = default!;

    public string? City { get; set; } = default!;

    public string? Region { get; set; } = default!;

    public string? PostalCode { get; set; } = default!;

    public string? Country { get; set; } = default!;

    public string? HomePhone { get; set; } = default!;

    public string? Extension { get; set; } = default!;

    public string? Notes { get; set; } = default!;

    public int? ReportsTo { get; set; } = default!;

    public string? PhotoPath { get; set; } = default!;

    public virtual IList<Order> Orders { get; set; } = [];
}
