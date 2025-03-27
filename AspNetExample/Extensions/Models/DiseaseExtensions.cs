using AspNetExample.Domain.Entities;
using AspNetExample.Models.Diseases;

namespace AspNetExample.Extensions.Models;

public static class DiseaseExtensions
{
    public static DiseaseModel ToModel(this Disease disease)
    {
        return new DiseaseModel
        {
            Id = disease.Id,
            Name = disease.Name
        };
    }

    public static DiseaseDetailsModel ToDetailsModel(this Disease disease)
    {
        return new DiseaseDetailsModel
        {
            Disease = disease.ToModel(),
            DoctorExaminations = disease.DoctorsExaminations
                .OrderBy(doctorExamination => doctorExamination.Id)
                .Select(doctorExamination => doctorExamination.ToModel())
                .ToArray()
        };
    }
}