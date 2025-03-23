using System.ComponentModel;

#pragma warning disable CS8618

namespace AspNetExample.Models.Doctors;

public class DoctorsIndexModel
{
    [DisplayName("Имена")]
    public string? Names { get; set; }

    [DisplayName("Зарплата от")]
    public decimal? SalaryFrom { get; set; }

    [DisplayName("Зарплата до")]
    public decimal? SalaryTo { get; set; }

    [DisplayName("Фамилии")]
    public string? Surnames { get; set; }

    public DoctorModel[]? Doctors { get; set; }

    public int? Page { get; set; }
    public int? TotalCount { get; set; }

    public int? TotalPages => TotalCount.HasValue
        ? (int) Math.Ceiling(TotalCount.Value / (double) Constants.PageSize)
        : null;

    public bool HasPrevPage => Page is > Constants.FirstPage;
    public bool HasNextPage => Page.HasValue && Page < TotalPages;
}