#pragma warning disable CS8618
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel;

namespace AspNetExample.Models.Doctors;

public class DoctorModel
{
    [DisplayName("Id")]
    public int Id { get; set; }

    [DisplayName("Имя")]
    public string Name { get; set; }

    [DisplayName("Зарплата")]
    public decimal Salary { get; set; }

    [DisplayName("Фамилия")]
    public string Surname { get; set; }
}