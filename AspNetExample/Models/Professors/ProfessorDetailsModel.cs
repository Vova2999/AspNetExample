﻿#pragma warning disable CS8618
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel;
using AspNetExample.Models.Doctors;

namespace AspNetExample.Models.Professors;

public class ProfessorDetailsModel
{
    [DisplayName("Профессор")]
    public ProfessorModel Professor { get; set; }

    [DisplayName("Доктор")]
    public DoctorModel Doctor { get; set; }
}