#pragma warning disable CS8618
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel;
using AspNetExample.Models.Departments;
using AspNetExample.Models.DoctorExaminations;

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