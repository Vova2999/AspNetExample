using AspNetExample.Domain.Dtos;
using AspNetExample.Domain.Entities;

namespace AspNetExample.Domain.Extensions;

public static class DoctorExtensions
{
	public static DoctorDto ToDto(this Doctor doctor)
	{
		return new DoctorDto
		{
			Id = doctor.Id,
			Name = doctor.Name,
			Salary = doctor.Salary,
			Surname = doctor.Surname
		};
	}
}