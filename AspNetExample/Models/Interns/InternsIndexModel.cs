#pragma warning disable CS8618
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel;

namespace AspNetExample.Models.Interns;

public class InternsIndexModel : SortingPaginationModelBase
{
    [DisplayName("Имена докторов")]
    public string? DoctorNames { get; set; }

    public InternModel[] Interns { get; set; }
}