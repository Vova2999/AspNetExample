using System.ComponentModel;
using AspNetExample.Models.Wards;

#pragma warning disable CS8618

namespace AspNetExample.Models.Departments;

public class DepartmentDetailsModel
{
    [DisplayName("Департамент")]
    public DepartmentModel Department { get; set; }

    [DisplayName("Палаты")]
    public IReadOnlyCollection<WardModel> Wards { get; set; }
}