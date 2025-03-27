using System.ComponentModel;
using AspNetExample.Models.Departments;
using AspNetExample.Models.DoctorExaminations;

#pragma warning disable CS8618

namespace AspNetExample.Models.Wards;

public class WardDetailsModel
{
    [DisplayName("Палата")]
    public WardModel Ward { get; set; }

    [DisplayName("Департамент")]
    public DepartmentModel Department { get; set; }

    [DisplayName("Осмотры")]
    public IReadOnlyCollection<DoctorExaminationModel> DoctorExaminations { get; set; }
}