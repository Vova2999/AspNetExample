using AspNetExample.Domain.Dtos;
using AspNetExample.Domain.Entities;

namespace AspNetExample.Domain.Extensions;

public static class DiseaseExtensions
{
	public static DiseaseDto ToDto(this Disease disease)
	{
		return new DiseaseDto
		{
			Id = disease.Id,
			Name = disease.Name
		};
	}
}