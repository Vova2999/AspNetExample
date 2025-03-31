#pragma warning disable CS8618
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel;

namespace AspNetExample.Models.Diseases;

public class DiseaseModel
{
    [DisplayName("Id")]
    public int Id { get; set; }

    [DisplayName("Название")]
    public string Name { get; set; }
}