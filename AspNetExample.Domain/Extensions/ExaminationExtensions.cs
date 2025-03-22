using AspNetExample.Domain.Dtos;
using AspNetExample.Domain.Entities;

namespace AspNetExample.Domain.Extensions;

public static class ExaminationExtensions
{
    public static ExaminationDto ToDto(this Examination examination)
    {
        return new ExaminationDto
        {
            Id = examination.Id,
            Name = examination.Name
        };
    }
}