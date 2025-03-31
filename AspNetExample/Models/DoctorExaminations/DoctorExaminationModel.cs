#pragma warning disable CS8618
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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

    [DisplayName("Болезнь")]
    public string DiseaseName { get; set; }

    [DisplayName("Доктор")]
    public int DoctorId { get; set; }

    [DisplayName("Доктор")]
    public string DoctorName { get; set; }

    [DisplayName("Обследование")]
    public int ExaminationId { get; set; }

    [DisplayName("Обследование")]
    public string ExaminationName { get; set; }

    [DisplayName("Палата")]
    public int WardId { get; set; }

    [DisplayName("Палата")]
    public string WardName { get; set; }
}