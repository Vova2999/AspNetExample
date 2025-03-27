using System.ComponentModel;
using AspNetExample.Models.Doctors;

#pragma warning disable CS8618

namespace AspNetExample.Models.Professors;

public class ProfessorDetailsModel
{
    [DisplayName("Профессор")]
    public ProfessorModel Professor { get; set; }

    [DisplayName("Доктор")]
    public DoctorModel Doctor { get; set; }
}