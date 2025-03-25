using System.ComponentModel;

#pragma warning disable CS8618

namespace AspNetExample.Models.Interns;

public class InternsIndexModel : SortingPaginationModelBase
{
    [DisplayName("Имена докторов")]
    public string? DoctorNames { get; set; }

    public InternModel[]? Interns { get; set; }
}