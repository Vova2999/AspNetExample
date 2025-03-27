using System.ComponentModel;
using AspNetExample.Models.Doctors;

#pragma warning disable CS8618

namespace AspNetExample.Models.Interns;

public class InternDetailsModel
{
    [DisplayName("Стажер")]
    public InternModel Intern { get; set; }

    [DisplayName("Доктор")]
    public DoctorModel Doctor { get; set; }
}