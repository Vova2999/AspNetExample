using System.ComponentModel;

#pragma warning disable CS8618

namespace AspNetExample.Models.DoctorExaminations;

public class DoctorExaminationsIndexModel : PaginationModelBase
{
    [DisplayName("Дата от")]
    public DateOnly? DateFrom { get; set; }

    [DisplayName("Дата до")]
    public DateOnly? DateTo { get; set; }

    [DisplayName("Названия болезней")]
    public string? DiseaseNames { get; set; }

    [DisplayName("Имена докторов")]
    public string? DoctorNames { get; set; }

    [DisplayName("Названия обследований")]
    public string? ExaminationNames { get; set; }

    [DisplayName("Названия палат")]
    public string? WardNames { get; set; }

    public DoctorExaminationModel[]? DoctorExaminations { get; set; }
}