#pragma warning disable CS8618
// ReSharper disable CollectionNeverUpdated.Global

namespace AspNetExample.Domain.Entities;

public class Intern
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; }
}