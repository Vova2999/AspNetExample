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
}