#pragma warning disable CS8618
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel;

namespace AspNetExample.Models.Examinations;

public class ExaminationModel
{
    [DisplayName("Id")]
    public int Id { get; set; }

    [DisplayName("Название")]
    public string Name { get; set; }
}