﻿using AspNetExample.Domain.Entities;
using AspNetExample.Models.Professors;

namespace AspNetExample.Extensions.Models;

public static class ProfessorExtensions
{
    public static ProfessorModel ToModel(this Professor professor)
    {
        return new ProfessorModel
        {
            Id = professor.Id,
            DoctorId = professor.DoctorId,
            DoctorName = professor.Doctor.Name,
        };
    }
}