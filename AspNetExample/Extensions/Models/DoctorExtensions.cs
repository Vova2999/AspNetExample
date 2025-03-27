using AspNetExample.Domain.Entities;
using AspNetExample.Models.Doctors;

namespace AspNetExample.Extensions.Models;

public static class DoctorExtensions
{
    public static DoctorModel ToModel(this Doctor doctor)
    {
        return new DoctorModel
        {
            Id = doctor.Id,
            Name = doctor.Name,
            Salary = doctor.Salary,
            Surname = doctor.Surname
        };
    }

    public static DoctorDetailsModel ToDetailsModel(this Doctor doctor)
    {
        return new DoctorDetailsModel
        {
            Doctor = doctor.ToModel(),
            Interns = doctor.Interns
                .OrderBy(intern => intern.Id)
                .Select(intern => intern.ToModel())
                .ToArray(),
            Professors = doctor.Professors
                .OrderBy(professor => professor.Id)
                .Select(professor => professor.ToModel())
                .ToArray(),
            DoctorExaminations = doctor.DoctorsExaminations
                .OrderBy(doctorExamination => doctorExamination.Id)
                .Select(doctorExamination => doctorExamination.ToModel())
                .ToArray()
        };
    }
}