#pragma warning disable CS8618
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel;

namespace AspNetExample.Models.Departments;

public class DepartmentModel
{
    [DisplayName("Id")]
    public int Id { get; set; }

    [DisplayName("Здание")]
    public int Building { get; set; }

    [DisplayName("Финансирование")]
    public decimal Financing { get; set; }

    [DisplayName("Название")]
    public string Name { get; set; }
}