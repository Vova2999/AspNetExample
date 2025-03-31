#pragma warning disable CS8618
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel;
using AspNetExample.Models.DoctorExaminations;

namespace AspNetExample.Models.Examinations;

public class ExaminationDetailsModel
{
    [DisplayName("Обследование")]
    public ExaminationModel Examination { get; set; }

    [DisplayName("Осмотры")]
    public IReadOnlyCollection<DoctorExaminationModel> DoctorExaminations { get; set; }
}