using System.ComponentModel;

#pragma warning disable CS8618

namespace AspNetExample.Models.Wards;

public class WardsIndexModel
{
    [DisplayName("Названия")]
    public string? Names { get; set; }

    [DisplayName("Количество мест от")]
    public int? PlacesFrom { get; set; }

    [DisplayName("Количество мест до")]
    public int? PlacesTo { get; set; }

    [DisplayName("Названия департаментов")]
    public string? DepartmentNames { get; set; }

    public WardModel[]? Wards { get; set; }

    public int? Page { get; set; }
    public int? TotalCount { get; set; }

    public int? TotalPages => TotalCount.HasValue
        ? (int) Math.Ceiling(TotalCount.Value / (double) Constants.PageSize)
        : null;

    public bool HasPrevPage => Page is > Constants.FirstPage;
    public bool HasNextPage => Page.HasValue && Page < TotalPages;
}