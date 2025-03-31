using System.Net.Mime;
using AspNetExample.Common.Extensions;
using AspNetExample.Database.Context;
using AspNetExample.Database.Context.Factory;
using AspNetExample.Domain;
using AspNetExample.Domain.Dtos;
using AspNetExample.Domain.Entities;
using AspNetExample.Domain.Extensions;
using AspNetExample.Exceptions.Api;
using AspNetExample.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetExample.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/professors")]
[Produces(MediaTypeNames.Application.Json)]
public class ProfessorsApiController : ControllerBase
{
    private readonly IApplicationContextFactory _applicationContextFactory;
    private readonly ILogger<ProfessorsApiController> _logger;

    public ProfessorsApiController(
        IApplicationContextFactory applicationContextFactory,
        ILogger<ProfessorsApiController> logger)
    {
        _applicationContextFactory = applicationContextFactory;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ProfessorDto[]> GetAll(
        [FromQuery] string[]? doctorNames)
    {
        await using var context = _applicationContextFactory.Create();

        var professorsQuery = context.Professors
            .Include(professor => professor.Doctor)
            .AsNoTracking();

        if (doctorNames.IsSignificant())
            professorsQuery = professorsQuery.Where(professor => doctorNames.Contains(professor.Doctor.Name));

        professorsQuery = professorsQuery.OrderBy(professor => professor.Id);

        var professors = await professorsQuery
            .Select(professor => professor.ToDto())
            .ToArrayAsync();

        return professors;
    }

    [HttpGet("{id:int}")]
    public async Task<ProfessorDto> Get(
        [FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();

        var professor = await context.Professors
            .Include(professor => professor.Doctor)
            .AsNoTracking()
            .FirstOrDefaultAsync(professor => professor.Id == id);

        return professor == null
            ? throw new NotFoundException($"Не найден профессор с id = {id}")
            : professor.ToDto();
    }

    [HttpPost]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<ProfessorDto> Create(
        [FromBody] ProfessorDto professorDto)
    {
        await using var context = _applicationContextFactory.Create();

        var doctor = await ValidateProfessorModelAsync(context, professorDto);
        if (!ModelState.IsValid)
            throw new BadRequestException(ModelState.JoinErrors());

        var professor = new Professor
        {
            DoctorId = doctor.Id
        };

        context.Professors.Add(professor);
        await context.SaveChangesAsync();

        _logger.LogInformation($"Professor with id = {professor.Id} created");

        return professor.ToDto();
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<ProfessorDto> Update(
        [FromRoute] int id,
        [FromBody] ProfessorDto professorDto)
    {
        await using var context = _applicationContextFactory.Create();

        var professor = await context.Professors
            .FirstOrDefaultAsync(professor => professor.Id == id);

        if (professor == null)
            throw new NotFoundException($"Не найден профессор с id = {id}");

        var doctor = await ValidateProfessorModelAsync(context, professorDto, professor.Id);
        if (!ModelState.IsValid)
            throw new BadRequestException(ModelState.JoinErrors());

        professor.DoctorId = doctor.Id;

        await context.SaveChangesAsync();

        _logger.LogInformation($"Professor with id = {id} updated");

        return professor.ToDto();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task Delete(
        [FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();

        var professor = await context.Professors
            .FirstOrDefaultAsync(professor => professor.Id == id);

        if (professor == null)
            throw new NotFoundException($"Не найден профессор с id = {id}");

        context.Professors.Remove(professor);

        await context.SaveChangesAsync();

        _logger.LogInformation($"Professor with id = {id} deleted");
    }

    private async Task<Doctor> ValidateProfessorModelAsync(
        ApplicationContext context,
        ProfessorDto? professorDto,
        int? currentId = null)
    {
        if (professorDto == null)
            return null!;

        var currentDoctor = (Doctor?) null;

        if (professorDto.DoctorName.IsNullOrEmpty())
        {
            ModelState.AddModelError(nameof(professorDto.DoctorName), "Имя доктора обязательно для заполнения.");
        }
        else
        {
            currentDoctor = await context.Doctors.FirstOrDefaultAsync(doctor =>
                EF.Functions.Like(professorDto.DoctorName, doctor.Name));

            if (currentDoctor == null)
            {
                ModelState.AddModelError(nameof(professorDto.DoctorName), "Доктор не найден.");
            }
            else
            {
                var isAlreadyProfessor = await context.Professors.AnyAsync(professor =>
                    (!currentId.HasValue || professor.Id != currentId.Value) &&
                    professor.DoctorId == currentDoctor.Id);

                if (isAlreadyProfessor)
                    ModelState.AddModelError(nameof(professorDto.DoctorName), "Доктор уже является профессором.");
            }
        }

        return currentDoctor!;
    }
}