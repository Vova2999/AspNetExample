﻿#pragma warning disable CS8618
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace AspNetExample.Domain.Entities;

public class Disease
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<DoctorExamination> DoctorsExaminations { get; set; }
}