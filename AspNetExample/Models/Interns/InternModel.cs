#pragma warning disable CS8618
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel;

namespace AspNetExample.Models.Interns;

public class InternModel
{
    [DisplayName("Id")]
    public int Id { get; set; }

    [DisplayName("Доктор")]
    public int DoctorId { get; set; }

    [DisplayName("Доктор")]
    public string DoctorName { get; set; }
}