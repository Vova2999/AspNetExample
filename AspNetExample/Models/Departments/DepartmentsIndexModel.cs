using System.ComponentModel;

#pragma warning disable CS8618

namespace AspNetExample.Models.Departments;

public class DepartmentsIndexModel
{
    [DisplayName("Здания")]
    public string? Buildings { get; set; }

    [DisplayName("Финансирование\u00A0от")]
    public decimal? FinancingFrom { get; set; }

    [DisplayName("Финансирование\u00A0до")]
    public decimal? FinancingTo { get; set; }

    [DisplayName("Названия")]
    public string? Names { get; set; }

    public DepartmentModel[]? Departments { get; set; }

    public int? Page { get; set; }
    public int? TotalCount { get; set; }

    public int? TotalPages => TotalCount.HasValue
        ? (int) Math.Ceiling(TotalCount.Value / (double) Constants.PageSize)
        : null;

    public bool HasPrevPage => Page is > Constants.FirstPage;
    public bool HasNextPage => Page.HasValue && Page < TotalPages;
}