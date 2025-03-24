using System.Net.Mime;
using AspNetExample.Common.Extensions;
using AspNetExample.Database.Context;
using AspNetExample.Database.Context.Factory;
using AspNetExample.Domain.Dtos;
using AspNetExample.Domain.Entities;
using AspNetExample.Domain.Extensions;
using AspNetExample.Exceptions.Api;
using AspNetExample.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetExample.Controllers.Api;

[ApiController]
[Route("api/doctorExaminations")]
[Produces(MediaTypeNames.Application.Json)]
public class DoctorExaminationsApiController : ControllerBase
{
    private readonly IApplicationContextFactory _applicationContextFactory;
    private readonly ILogger<DoctorExaminationsApiController> _logger;

    public DoctorExaminationsApiController(
        IApplicationContextFactory applicationContextFactory,
        ILogger<DoctorExaminationsApiController> logger)
    {
        _applicationContextFactory = applicationContextFactory;
        _logger = logger;
    }

    [HttpGet]
    public async Task<DoctorExaminationDto[]> GetAll(
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] string[]? diseaseNames,
        [FromQuery] string[]? doctorNames,
        [FromQuery] string[]? examinationNames,
        [FromQuery] string[]? wardNames)
    {
        await using var context = _applicationContextFactory.Create();

        var doctorExaminationsQuery = context.DoctorsExaminations
            .Include(doctorExamination => doctorExamination.Disease)
            .Include(doctorExamination => doctorExamination.Doctor)
            .Include(doctorExamination => doctorExamination.Examination)
            .Include(doctorExamination => doctorExamination.Ward)
            .AsNoTracking();

        if (dateFrom != null)
            doctorExaminationsQuery = doctorExaminationsQuery.Where(doctorExamination => doctorExamination.Date >= dateFrom);
        if (dateTo != null)
            doctorExaminationsQuery = doctorExaminationsQuery.Where(doctorExamination => doctorExamination.Date <= dateTo);
        if (diseaseNames?.Any() == true)
            doctorExaminationsQuery = doctorExaminationsQuery.Where(doctorExamination => diseaseNames.Contains(doctorExamination.Disease.Name));
        if (doctorNames?.Any() == true)
            doctorExaminationsQuery = doctorExaminationsQuery.Where(doctorExamination => doctorNames.Contains(doctorExamination.Doctor.Name));
        if (examinationNames?.Any() == true)
            doctorExaminationsQuery = doctorExaminationsQuery.Where(doctorExamination => examinationNames.Contains(doctorExamination.Examination.Name));
        if (wardNames?.Any() == true)
            doctorExaminationsQuery = doctorExaminationsQuery.Where(doctorExamination => wardNames.Contains(doctorExamination.Ward.Name));

        doctorExaminationsQuery = doctorExaminationsQuery.OrderBy(doctorExamination => doctorExamination.Id);

        var doctorExaminations = await doctorExaminationsQuery
            .Select(doctorExamination => doctorExamination.ToDto())
            .ToArrayAsync();

        return doctorExaminations;
    }

    [HttpGet("{id:int}")]
    public async Task<DoctorExaminationDto> Get(
        [FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();

        var doctorExaminations = await context.DoctorsExaminations
            .Include(doctorExamination => doctorExamination.Disease)
            .Include(doctorExamination => doctorExamination.Doctor)
            .Include(doctorExamination => doctorExamination.Examination)
            .Include(doctorExamination => doctorExamination.Ward)
            .AsNoTracking()
            .FirstOrDefaultAsync(doctorExamination => doctorExamination.Id == id);

        return doctorExaminations == null
            ? throw new NotFoundException($"Не найден осмотр с id = {id}")
            : doctorExaminations.ToDto();
    }

    [HttpPost]
    public async Task<DoctorExaminationDto> Create(
        [FromBody] DoctorExaminationDto doctorExaminationDto)
    {
        await using var context = _applicationContextFactory.Create();

        var (disease, doctor, examination, ward) = await ValidateDoctorExaminationModelAsync(context, doctorExaminationDto);
        if (!ModelState.IsValid)
            throw new BadRequestException(ModelState.JoinErrors());

        var doctorExamination = new DoctorExamination
        {
            Date = doctorExaminationDto.Date,
            DiseaseId = disease.Id,
            DoctorId = doctor.Id,
            ExaminationId = examination.Id,
            WardId = ward.Id
        };

        context.DoctorsExaminations.Add(doctorExamination);
        await context.SaveChangesAsync();

        _logger.LogInformation($"DoctorExamination with id = {doctorExamination.Id} created");

        return doctorExamination.ToDto();
    }

    [HttpPut("{id:int}")]
    public async Task<DoctorExaminationDto> Update(
        [FromRoute] int id,
        [FromBody] DoctorExaminationDto doctorExaminationDto)
    {
        await using var context = _applicationContextFactory.Create();

        var doctorExamination = await context.DoctorsExaminations
            .FirstOrDefaultAsync(doctorExamination => doctorExamination.Id == id);

        if (doctorExamination == null)
            throw new NotFoundException($"Не найден осмотр с id = {id}");

        var (disease, doctor, examination, ward) = await ValidateDoctorExaminationModelAsync(context, doctorExaminationDto);
        if (!ModelState.IsValid)
            throw new BadRequestException(ModelState.JoinErrors());

        doctorExamination.Date = doctorExaminationDto.Date;
        doctorExamination.DiseaseId = disease.Id;
        doctorExamination.DoctorId = doctor.Id;
        doctorExamination.ExaminationId = examination.Id;
        doctorExamination.WardId = ward.Id;

        await context.SaveChangesAsync();

        _logger.LogInformation($"DoctorExamination with id = {id} updated");

        return doctorExamination.ToDto();
    }

    [HttpDelete("{id:int}")]
    public async Task Delete(
        [FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();

        var doctorExamination = await context.DoctorsExaminations
            .FirstOrDefaultAsync(doctorExamination => doctorExamination.Id == id);

        if (doctorExamination == null)
            throw new NotFoundException($"Не найден осмотр с id = {id}");

        context.DoctorsExaminations.Remove(doctorExamination);

        await context.SaveChangesAsync();

        _logger.LogInformation($"DoctorExamination with id = {id} deleted");
    }

    private async Task<(Disease, Doctor, Examination, Ward)> ValidateDoctorExaminationModelAsync(
        ApplicationContext context,
        DoctorExaminationDto? doctorExaminationDto)
    {
        if (doctorExaminationDto == null)
            return (null, null, null, null)!;

        if (doctorExaminationDto.Date > DateTime.Now.ToDateOnly())
            ModelState.AddModelError(nameof(doctorExaminationDto.Date), "Дата должна быть не больше текущей.");

        var currentDisease = (Disease?) null;
        var currentDoctor = (Doctor?) null;
        var currentExamination = (Examination?) null;
        var currentWard = (Ward?) null;

        if (doctorExaminationDto.DiseaseName.IsSignificant())
        {
            currentDisease = await context.Diseases.FirstOrDefaultAsync(disease =>
                EF.Functions.Like(doctorExaminationDto.DiseaseName, disease.Name));

            if (currentDisease == null)
                ModelState.AddModelError(nameof(doctorExaminationDto.DiseaseName), "Болезнь не найдена.");
        }

        if (doctorExaminationDto.DoctorName.IsSignificant())
        {
            currentDoctor = await context.Doctors.FirstOrDefaultAsync(doctor =>
                EF.Functions.Like(doctorExaminationDto.DoctorName, doctor.Name));

            if (currentDoctor == null)
                ModelState.AddModelError(nameof(doctorExaminationDto.DoctorName), "Доктор не найден.");
        }

        if (doctorExaminationDto.ExaminationName.IsSignificant())
        {
            currentExamination = await context.Examinations.FirstOrDefaultAsync(examination =>
                EF.Functions.Like(doctorExaminationDto.ExaminationName, examination.Name));

            if (currentExamination == null)
                ModelState.AddModelError(nameof(doctorExaminationDto.ExaminationName), "Обследование не найдено.");
        }

        if (doctorExaminationDto.WardName.IsSignificant())
        {
            currentWard = await context.Wards.FirstOrDefaultAsync(ward =>
                EF.Functions.Like(doctorExaminationDto.WardName, ward.Name));

            if (currentWard == null)
                ModelState.AddModelError(nameof(doctorExaminationDto.WardName), "Палата не найдена.");
        }

        return (currentDisease, currentDoctor, currentExamination, currentWard)!;
    }
}