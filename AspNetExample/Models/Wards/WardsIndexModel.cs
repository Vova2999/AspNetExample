using System.ComponentModel;

#pragma warning disable CS8618

namespace AspNetExample.Models.Wards;

public class WardsIndexModel : SortingPaginationModelBase
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
}