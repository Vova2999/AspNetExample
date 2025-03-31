#pragma warning disable CS8618
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel;
using AspNetExample.Models.DoctorExaminations;

namespace AspNetExample.Models.Diseases;

public class DiseaseDetailsModel
{
    [DisplayName("Болезнь")]
    public DiseaseModel Disease { get; set; }

    [DisplayName("Осмотры")]
    public IReadOnlyCollection<DoctorExaminationModel> DoctorExaminations { get; set; }
}