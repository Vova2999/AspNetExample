using AspNetExample.Domain.Dtos;
using AspNetExample.Domain.Entities;

namespace AspNetExample.Domain.Extensions;

public static class ProfessorExtensions
{
    public static ProfessorDto ToDto(this Professor professor)
    {
        return new ProfessorDto
        {
            Id = professor.Id,
            DoctorName = professor.Doctor.Name
        };
    }
}