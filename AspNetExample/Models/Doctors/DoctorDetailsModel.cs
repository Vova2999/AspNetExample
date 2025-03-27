using System.ComponentModel;
using AspNetExample.Models.DoctorExaminations;
using AspNetExample.Models.Interns;
using AspNetExample.Models.Professors;

#pragma warning disable CS8618

namespace AspNetExample.Models.Doctors;

public class DoctorDetailsModel
{
    [DisplayName("Доктор")]
    public DoctorModel Doctor { get; set; }

    [DisplayName("Стажеры")]
    public IReadOnlyCollection<InternModel> Interns { get; set; }

    [DisplayName("Профессоры")]
    public IReadOnlyCollection<ProfessorModel> Professors { get; set; }

    [DisplayName("Осмотры")]
    public IReadOnlyCollection<DoctorExaminationModel> DoctorExaminations { get; set; }
}