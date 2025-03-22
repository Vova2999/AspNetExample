#pragma warning disable CS8618

namespace AspNetExample.Domain.Dtos;

public class DepartmentDto
{
    public int Id { get; set; }
    public int Building { get; set; }
    public decimal Financing { get; set; }
    public string Name { get; set; }
}