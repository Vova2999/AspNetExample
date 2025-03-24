using AspNetExample.Common.Extensions;
using AspNetExample.Domain.Entities;
using AspNetExample.Models.DoctorExaminations;

namespace AspNetExample.Extensions.Models;

public static class DoctorExaminationExtensions
{
    public static DoctorExaminationModel ToModel(this DoctorExamination doctorExamination)
    {
        return new DoctorExaminationModel
        {
            Id = doctorExamination.Id,
            Date = doctorExamination.Date.ToDateTime(),
            DiseaseId = doctorExamination.DiseaseId,
            DiseaseName = doctorExamination.Disease.Name,
            DoctorId = doctorExamination.DoctorId,
            DoctorName = doctorExamination.Doctor.Name,
            ExaminationId = doctorExamination.ExaminationId,
            ExaminationName = doctorExamination.Examination.Name,
            WardId = doctorExamination.WardId,
            WardName = doctorExamination.Ward.Name
        };
    }
}