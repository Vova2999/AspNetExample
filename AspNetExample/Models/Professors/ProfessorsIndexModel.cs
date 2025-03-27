using System.ComponentModel;

#pragma warning disable CS8618

namespace AspNetExample.Models.Professors;

public class ProfessorsIndexModel : SortingPaginationModelBase
{
    [DisplayName("Имена докторов")]
    public string? DoctorNames { get; set; }

    public ProfessorModel[] Professors { get; set; }
}