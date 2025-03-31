#pragma warning disable CS8618
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace AspNetExample.Domain.Dtos;

public class WardDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Places { get; set; }
    public string DepartmentName { get; set; }
}