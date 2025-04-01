#pragma warning disable CS8618
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace AspNetExample.Domain.Entities;

public class Department
{
    public int Id { get; set; }
    public int Building { get; set; }
    public decimal Financing { get; set; }
    public string Name { get; set; }
    public ICollection<Ward> Wards { get; set; }
}