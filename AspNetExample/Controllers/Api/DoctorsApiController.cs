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
[Route("api/doctors")]
[Produces(MediaTypeNames.Application.Json)]
public class DoctorsApiController : ControllerBase
{
    private readonly IApplicationContextFactory _applicationContextFactory;
    private readonly ILogger<DoctorsApiController> _logger;

    public DoctorsApiController(
        IApplicationContextFactory applicationContextFactory,
        ILogger<DoctorsApiController> logger)
    {
        _applicationContextFactory = applicationContextFactory;
        _logger = logger;
    }

    [HttpGet]
    public async Task<DoctorDto[]> GetAll(
        [FromQuery] string[]? names,
        [FromQuery] decimal? salaryFrom,
        [FromQuery] decimal? salaryTo,
        [FromQuery] string[]? surnames)
    {
        await using var context = _applicationContextFactory.Create();

        var doctorsQuery = context.Doctors
            .AsNoTracking();

        if (names?.Any() == true)
            doctorsQuery = doctorsQuery.Where(doctor => names.Contains(doctor.Name));
        if (salaryFrom != null)
            doctorsQuery = doctorsQuery.Where(doctor => doctor.Salary >= salaryFrom);
        if (salaryTo != null)
            doctorsQuery = doctorsQuery.Where(doctor => doctor.Salary <= salaryTo);
        if (surnames?.Any() == true)
            doctorsQuery = doctorsQuery.Where(doctor => surnames.Contains(doctor.Surname));

        doctorsQuery = doctorsQuery.OrderBy(doctor => doctor.Id);

        var doctors = await doctorsQuery
            .Select(doctor => doctor.ToDto())
            .ToArrayAsync();

        return doctors;
    }

    [HttpGet("{id:int}")]
    public async Task<DoctorDto> Get(
        [FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();

        var doctor = await context.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(doctor => doctor.Id == id);

        return doctor == null
            ? throw new NotFoundException($"Не найден доктор с id = {id}")
            : doctor.ToDto();
    }

    [HttpPost]
    public async Task<DoctorDto> Create(
        [FromBody] DoctorDto doctorDto)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateDoctorModelAsync(context, doctorDto);
        if (!ModelState.IsValid)
            throw new BadRequestException(ModelState.JoinErrors());

        var doctor = new Doctor
        {
            Name = doctorDto.Name,
            Salary = doctorDto.Salary,
            Surname = doctorDto.Surname
        };

        context.Doctors.Add(doctor);
        await context.SaveChangesAsync();

        _logger.LogInformation($"Doctor with id = {doctor.Id} created");

        return doctor.ToDto();
    }

    [HttpPut("{id:int}")]
    public async Task<DoctorDto> Update(
        [FromRoute] int id,
        [FromBody] DoctorDto doctorDto)
    {
        await using var context = _applicationContextFactory.Create();

        var doctor = await context.Doctors
            .FirstOrDefaultAsync(doctor => doctor.Id == id);

        if (doctor == null)
            throw new NotFoundException($"Не найден доктор с id = {id}");

        await ValidateDoctorModelAsync(context, doctorDto, doctor.Id);
        if (!ModelState.IsValid)
            throw new BadRequestException(ModelState.JoinErrors());

        doctor.Name = doctorDto.Name;
        doctor.Salary = doctorDto.Salary;
        doctor.Surname = doctorDto.Surname;

        await context.SaveChangesAsync();

        _logger.LogInformation($"Doctor with id = {id} updated");

        return doctor.ToDto();
    }

    [HttpDelete("{id:int}")]
    public async Task Delete(
        [FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();

        var doctor = await context.Doctors
            .FirstOrDefaultAsync(doctor => doctor.Id == id);

        if (doctor == null)
            throw new NotFoundException($"Не найден доктор с id = {id}");

        context.Doctors.Remove(doctor);

        await context.SaveChangesAsync();

        _logger.LogInformation($"Doctor with id = {id} deleted");
    }

    private async Task ValidateDoctorModelAsync(
        ApplicationContext context,
        DoctorDto? doctorDto,
        int? currentId = null)
    {
        if (doctorDto == null)
            return;

        if (doctorDto.Salary <= 0)
            ModelState.AddModelError(nameof(doctorDto.Salary), "Зарплата должна быть больше 0.");

        if (doctorDto.Name.IsSignificant() && doctorDto.Surname.IsSignificant())
        {
            var hasConflictedName = await context.Doctors.AnyAsync(doctor =>
                (!currentId.HasValue || doctor.Id != currentId.Value) &&
                EF.Functions.Like(doctorDto.Name, doctor.Name) &&
                EF.Functions.Like(doctorDto.Surname, doctor.Surname));

            if (hasConflictedName)
            {
                ModelState.AddModelError(nameof(doctorDto.Name), "Имя и фамилия должны быть уникальными.");
                ModelState.AddModelError(nameof(doctorDto.Surname), "Имя и фамилия должны быть уникальными.");
            }
        }
    }
}