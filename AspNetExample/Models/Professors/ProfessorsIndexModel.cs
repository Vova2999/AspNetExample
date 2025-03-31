#pragma warning disable CS8618
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel;

namespace AspNetExample.Models.Professors;

public class ProfessorsIndexModel : SortingPaginationModelBase
{
    [DisplayName("Имена докторов")]
    public string? DoctorNames { get; set; }

    public ProfessorModel[] Professors { get; set; }
}