﻿#pragma warning disable CS8618
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel;

namespace AspNetExample.Models.Doctors;

public class DoctorsIndexModel : SortingPaginationModelBase
{
    [DisplayName("Имена")]
    public string? Names { get; set; }

    [DisplayName("Зарплата от")]
    public decimal? SalaryFrom { get; set; }

    [DisplayName("Зарплата до")]
    public decimal? SalaryTo { get; set; }

    [DisplayName("Фамилии")]
    public string? Surnames { get; set; }

    public DoctorModel[] Doctors { get; set; }
}