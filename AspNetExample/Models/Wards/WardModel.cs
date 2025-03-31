#pragma warning disable CS8618
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel;

namespace AspNetExample.Models.Wards;

public class WardModel
{
    [DisplayName("Id")]
    public int Id { get; set; }

    [DisplayName("Название")]
    public string Name { get; set; }

    [DisplayName("Количество мест")]
    public int Places { get; set; }

    [DisplayName("Департамент")]
    public int DepartmentId { get; set; }

    [DisplayName("Департамент")]
    public string DepartmentName { get; set; }
}