using System.ComponentModel;

#pragma warning disable CS8618

namespace AspNetExample.Models.Interns;

public class InternsIndexModel : PaginationModelBase
{
    [DisplayName("Имена докторов")]
    public string? DoctorNames { get; set; }

    public InternModel[]? Interns { get; set; }
}