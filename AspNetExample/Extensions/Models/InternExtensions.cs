﻿using AspNetExample.Domain.Entities;
using AspNetExample.Models.Interns;

namespace AspNetExample.Extensions.Models;

public static class InternExtensions
{
    public static InternModel ToModel(this Intern intern)
    {
        return new InternModel
        {
            Id = intern.Id,
            DoctorId = intern.DoctorId,
            DoctorName = intern.Doctor.Name
        };
    }

    public static InternDetailsModel ToDetailsModel(this Intern intern)
    {
        return new InternDetailsModel
        {
            Intern = intern.ToModel(),
            Doctor = intern.Doctor.ToModel()
        };
    }
}