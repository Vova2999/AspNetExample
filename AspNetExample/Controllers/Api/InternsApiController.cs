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
[Route("api/interns")]
[Produces(MediaTypeNames.Application.Json)]
public class InternsApiController : ControllerBase
{
    private readonly IApplicationContextFactory _applicationContextFactory;
    private readonly ILogger<InternsApiController> _logger;

    public InternsApiController(
        IApplicationContextFactory applicationContextFactory,
        ILogger<InternsApiController> logger)
    {
        _applicationContextFactory = applicationContextFactory;
        _logger = logger;
    }

    [HttpGet]
    public async Task<InternDto[]> GetAll(
        [FromQuery] string[]? doctorNames,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        var internsQuery = context.Interns
            .Include(intern => intern.Doctor)
            .AsNoTracking();

        if (doctorNames.IsSignificant())
            internsQuery = internsQuery.Where(intern => doctorNames.Contains(intern.Doctor.Name));

        internsQuery = internsQuery.OrderBy(intern => intern.Id);

        var interns = await internsQuery
            .Select(intern => intern.ToDto())
            .ToArrayAsync(cancellationToken);

        return interns;
    }

    [HttpGet("{id:int}")]
    public async Task<InternDto> Get(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        var intern = await context.Interns
            .Include(intern => intern.Doctor)
            .AsNoTracking()
            .FirstOrDefaultAsync(intern => intern.Id == id, cancellationToken);

        return intern == null
            ? throw new NotFoundException($"Не найден стажер с id = {id}")
            : intern.ToDto();
    }

    [HttpPost]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<InternDto> Create(
        [FromBody] InternDto internDto,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        var doctor = await ValidateInternModelAsync(context, internDto, null, cancellationToken);
        if (!ModelState.IsValid)
            throw new BadRequestException(ModelState.JoinErrors());

        var intern = new Intern
        {
            DoctorId = doctor.Id
        };

        context.Interns.Add(intern);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation($"Intern with id = {intern.Id} created");

        return intern.ToDto();
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<InternDto> Update(
        [FromRoute] int id,
        [FromBody] InternDto internDto,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        var intern = await context.Interns
            .FirstOrDefaultAsync(intern => intern.Id == id, cancellationToken);

        if (intern == null)
            throw new NotFoundException($"Не найден стажер с id = {id}");

        var doctor = await ValidateInternModelAsync(context, internDto, intern.Id, cancellationToken);
        if (!ModelState.IsValid)
            throw new BadRequestException(ModelState.JoinErrors());

        intern.DoctorId = doctor.Id;

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation($"Intern with id = {id} updated");

        return intern.ToDto();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task Delete(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        var intern = await context.Interns
            .FirstOrDefaultAsync(intern => intern.Id == id, cancellationToken);

        if (intern == null)
            throw new NotFoundException($"Не найден стажер с id = {id}");

        context.Interns.Remove(intern);

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation($"Intern with id = {id} deleted");
    }

    private async Task<Doctor> ValidateInternModelAsync(
        ApplicationContext context,
        InternDto? internDto,
        int? currentId,
        CancellationToken cancellationToken)
    {
        if (internDto == null)
            return null!;

        var currentDoctor = (Doctor?) null;

        if (internDto.DoctorName.IsNullOrEmpty())
        {
            ModelState.AddModelError(nameof(internDto.DoctorName), "Имя доктора обязательно для заполнения.");
        }
        else
        {
            currentDoctor = await context.Doctors.FirstOrDefaultAsync(doctor =>
                    EF.Functions.Like(internDto.DoctorName, doctor.Name),
                cancellationToken);

            if (currentDoctor == null)
            {
                ModelState.AddModelError(nameof(internDto.DoctorName), "Доктор не найден.");
            }
            else
            {
                var isAlreadyIntern = await context.Interns.AnyAsync(intern =>
                        (!currentId.HasValue || intern.Id != currentId.Value) &&
                        intern.DoctorId == currentDoctor.Id,
                    cancellationToken);

                if (isAlreadyIntern)
                    ModelState.AddModelError(nameof(internDto.DoctorName), "Доктор уже является стажером.");
            }

            return currentDoctor!;
        }

        return currentDoctor!;
    }
}