using System.ComponentModel;

#pragma warning disable CS8618

namespace AspNetExample.Models.Interns;

public class InternModel
{
    [DisplayName("Id")]
    public int Id { get; set; }

    [DisplayName("Доктор")]
    public int DoctorId { get; set; }

    [DisplayName("Имя доктора")]
    public string DoctorName { get; set; }
}