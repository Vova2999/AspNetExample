using System.ComponentModel;
using AspNetExample.Models.DoctorExaminations;

#pragma warning disable CS8618

namespace AspNetExample.Models.Diseases;

public class DiseaseDetailsModel
{
    [DisplayName("Болезнь")]
    public DiseaseModel Disease { get; set; }

    [DisplayName("Осмотры")]
    public IReadOnlyCollection<DoctorExaminationModel> DoctorExaminations { get; set; }
}