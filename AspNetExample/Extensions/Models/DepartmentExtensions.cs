using AspNetExample.Domain.Entities;
using AspNetExample.Models.Departments;

namespace AspNetExample.Extensions.Models;

public static class DepartmentExtensions
{
	public static DepartmentModel ToModel(this Department department)
	{
		return new DepartmentModel
		{
			Id = department.Id,
			Building = department.Building,
			Financing = department.Financing,
			Name = department.Name
		};
	}
}