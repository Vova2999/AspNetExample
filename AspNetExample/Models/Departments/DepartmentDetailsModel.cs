#pragma warning disable CS8618
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel;
using AspNetExample.Models.Wards;

namespace AspNetExample.Models.Departments;

public class DepartmentDetailsModel
{
    [DisplayName("Департамент")]
    public DepartmentModel Department { get; set; }

    [DisplayName("Палаты")]
    public IReadOnlyCollection<WardModel> Wards { get; set; }
}