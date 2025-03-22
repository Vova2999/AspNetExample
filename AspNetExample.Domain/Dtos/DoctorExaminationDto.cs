#pragma warning disable CS8618

namespace AspNetExample.Domain.Dtos;

public class DoctorExaminationDto
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string DiseaseName { get; set; }
    public string DoctorName { get; set; }
    public string ExaminationName { get; set; }
    public string WardName { get; set; }
}