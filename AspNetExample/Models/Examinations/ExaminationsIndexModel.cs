using System.ComponentModel;

#pragma warning disable CS8618

namespace AspNetExample.Models.Examinations;

public class ExaminationsIndexModel : SortingPaginationModelBase
{
    [DisplayName("Названия")]
    public string? Names { get; set; }

    public ExaminationModel[] Examinations { get; set; }
}