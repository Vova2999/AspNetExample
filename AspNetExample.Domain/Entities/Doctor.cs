#pragma warning disable CS8618
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace AspNetExample.Domain.Entities;

public class Doctor
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Salary { get; set; }
    public string Surname { get; set; }
    public ICollection<Intern> Interns { get; set; } = new List<Intern>();
    public ICollection<Professor> Professors { get; set; } = new List<Professor>();
    public ICollection<DoctorExamination> DoctorsExaminations { get; set; } = new List<DoctorExamination>();
}