using System.ComponentModel;

#pragma warning disable CS8618

namespace AspNetExample.Models.Diseases;

public class DiseasesIndexModel
{
    [DisplayName("Названия")]
    public string? Names { get; set; }

    public DiseaseModel[]? Diseases { get; set; }

    public int? Page { get; set; }
    public int? TotalCount { get; set; }

    public int? TotalPages => TotalCount.HasValue
        ? (int) Math.Ceiling(TotalCount.Value / (double) Constants.PageSize)
        : null;

    public bool HasPrevPage => Page is > Constants.FirstPage;
    public bool HasNextPage => Page.HasValue && Page < TotalPages;
}