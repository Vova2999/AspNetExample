using System.ComponentModel;

#pragma warning disable CS8618

namespace AspNetExample.Models.Diseases;

public class DiseasesIndexModel : PaginationModelBase
{
    [DisplayName("Названия")]
    public string? Names { get; set; }

    public DiseaseModel[]? Diseases { get; set; }
}