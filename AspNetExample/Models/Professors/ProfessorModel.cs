using System.ComponentModel;

#pragma warning disable CS8618

namespace AspNetExample.Models.Professors;

public class ProfessorModel
{
    [DisplayName("Id")]
    public int Id { get; set; }

    [DisplayName("Доктор")]
    public int DoctorId { get; set; }

    [DisplayName("Имя доктора")]
    public string DoctorName { get; set; }
}