using System.ComponentModel;

#pragma warning disable CS8618

namespace AspNetExample.Models.Examinations;

public class ExaminationModel
{
    [DisplayName("Id")]
    public int Id { get; set; }

    [DisplayName("Название")]
    public string Name { get; set; }
}