#pragma warning disable CS8618
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel;
using AspNetExample.Models.Diseases;
using AspNetExample.Models.Doctors;
using AspNetExample.Models.Examinations;
using AspNetExample.Models.Wards;

namespace AspNetExample.Models.DoctorExaminations;

public class DoctorExaminationDetailsModel
{
    [DisplayName("Осмотр")]
    public DoctorExaminationModel DoctorExamination { get; set; }

    [DisplayName("Болезнь")]
    public DiseaseModel Disease { get; set; }

    [DisplayName("Доктор")]
    public DoctorModel Doctor { get; set; }

    [DisplayName("Обследование")]
    public ExaminationModel Examination { get; set; }

    [DisplayName("Палата")]
    public WardModel Ward { get; set; }
}