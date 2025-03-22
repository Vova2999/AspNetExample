using AspNetExample.Domain.Dtos;
using AspNetExample.Domain.Entities;

namespace AspNetExample.Domain.Extensions;

public static class DoctorExaminationExtensions
{
	public static DoctorExaminationDto ToDto(this DoctorExamination doctorExamination)
	{
		return new DoctorExaminationDto
		{
			Id = doctorExamination.Id,
			Date = doctorExamination.Date,
			DiseaseName = doctorExamination.Disease.Name,
			DoctorName = doctorExamination.Doctor.Name,
			ExaminationName = doctorExamination.Examination.Name,
			WardName = doctorExamination.Ward.Name
		};
	}
}