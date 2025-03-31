#pragma warning disable CS8618
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel;
using AspNetExample.Models.Doctors;

namespace AspNetExample.Models.Interns;

public class InternDetailsModel
{
    [DisplayName("Стажер")]
    public InternModel Intern { get; set; }

    [DisplayName("Доктор")]
    public DoctorModel Doctor { get; set; }
}