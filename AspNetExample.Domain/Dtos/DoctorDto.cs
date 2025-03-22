#pragma warning disable CS8618

namespace AspNetExample.Domain.Dtos;

public class DoctorDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Salary { get; set; }
    public string Surname { get; set; }
}