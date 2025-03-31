#pragma warning disable CS8618
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel;

namespace AspNetExample.Models.Diseases;

public class DiseasesIndexModel : SortingPaginationModelBase
{
    [DisplayName("Названия")]
    public string? Names { get; set; }

    public DiseaseModel[] Diseases { get; set; }
}