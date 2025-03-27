using AspNetExample.Domain.Entities;
using AspNetExample.Models.Wards;

namespace AspNetExample.Extensions.Models;

public static class WardExtensions
{
    public static WardModel ToModel(this Ward ward)
    {
        return new WardModel
        {
            Id = ward.Id,
            Name = ward.Name,
            Places = ward.Places,
            DepartmentId = ward.DepartmentId,
            DepartmentName = ward.Department.Name
        };
    }

    public static WardDetailsModel ToDetailsModel(this Ward ward)
    {
        return new WardDetailsModel
        {
            Ward = ward.ToModel(),
            Department = ward.Department.ToModel(),
            DoctorExaminations = ward.DoctorsExaminations
                .OrderBy(doctorExamination => doctorExamination.Id)
                .Select(doctorExamination => doctorExamination.ToModel())
                .ToArray()
        };
    }
}