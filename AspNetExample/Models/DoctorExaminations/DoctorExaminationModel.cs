using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#pragma warning disable CS8618

namespace AspNetExample.Models.DoctorExaminations;

public class DoctorExaminationModel
{
    [DisplayName("Id")]
    public int Id { get; set; }

    [DisplayName("Дата")]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    [DisplayName("Болезнь")]
    public int DiseaseId { get; set; }

    [DisplayName("Название болезни")]
    public string DiseaseName { get; set; }

    [DisplayName("Доктор")]
    public int DoctorId { get; set; }

    [DisplayName("Имя доктора")]
    public string DoctorName { get; set; }

    [DisplayName("Обследование")]
    public int ExaminationId { get; set; }

    [DisplayName("Название обследования")]
    public string ExaminationName { get; set; }

    [DisplayName("Палата")]
    public int WardId { get; set; }

    [DisplayName("Название палаты")]
    public string WardName { get; set; }
}