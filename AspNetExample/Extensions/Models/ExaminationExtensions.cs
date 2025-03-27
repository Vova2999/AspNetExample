using AspNetExample.Domain.Entities;
using AspNetExample.Models.Examinations;

namespace AspNetExample.Extensions.Models;

public static class ExaminationExtensions
{
    public static ExaminationModel ToModel(this Examination examination)
    {
        return new ExaminationModel
        {
            Id = examination.Id,
            Name = examination.Name
        };
    }

    public static ExaminationDetailsModel ToDetailsModel(this Examination examination)
    {
        return new ExaminationDetailsModel
        {
            Examination = examination.ToModel(),
            DoctorExaminations = examination.DoctorsExaminations
                .OrderBy(doctorExamination => doctorExamination.Id)
                .Select(doctorExamination => doctorExamination.ToModel())
                .ToArray()
        };
    }
}