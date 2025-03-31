#pragma warning disable CS8618
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel;

namespace AspNetExample.Models.Professors;

public class ProfessorModel
{
    [DisplayName("Id")]
    public int Id { get; set; }

    [DisplayName("Доктор")]
    public int DoctorId { get; set; }

    [DisplayName("Доктор")]
    public string DoctorName { get; set; }
}