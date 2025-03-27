using System.ComponentModel;
using AspNetExample.Models.DoctorExaminations;

#pragma warning disable CS8618

namespace AspNetExample.Models.Examinations;

public class ExaminationDetailsModel
{
    [DisplayName("Обследование")]
    public ExaminationModel Examination { get; set; }

    [DisplayName("Осмотры")]
    public IReadOnlyCollection<DoctorExaminationModel> DoctorExaminations { get; set; }
}