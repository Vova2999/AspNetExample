using AspNetExample.Domain.Dtos;
using AspNetExample.Domain.Entities;

namespace AspNetExample.Domain.Extensions;

public static class InternExtensions
{
	public static InternDto ToDto(this Intern intern)
	{
		return new InternDto
		{
			Id = intern.Id,
			DoctorName = intern.Doctor.Name
		};
	}
}