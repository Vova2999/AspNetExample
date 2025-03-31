#pragma warning disable CS8618
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel;

namespace AspNetExample.Models.Departments;

public class DepartmentsIndexModel : SortingPaginationModelBase
{
    [DisplayName("Здания")]
    public string? Buildings { get; set; }

    [DisplayName("Финансирование от")]
    public decimal? FinancingFrom { get; set; }

    [DisplayName("Финансирование до")]
    public decimal? FinancingTo { get; set; }

    [DisplayName("Названия")]
    public string? Names { get; set; }

    public DepartmentModel[] Departments { get; set; }
}