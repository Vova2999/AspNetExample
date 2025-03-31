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
[Route("api/examinations")]
[Produces(MediaTypeNames.Application.Json)]
public class ExaminationsApiController : ControllerBase
{
    private readonly IApplicationContextFactory _applicationContextFactory;
    private readonly ILogger<ExaminationsApiController> _logger;

    public ExaminationsApiController(
        IApplicationContextFactory applicationContextFactory,
        ILogger<ExaminationsApiController> logger)
    {
        _applicationContextFactory = applicationContextFactory;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ExaminationDto[]> GetAll(
        [FromQuery] string[]? names)
    {
        await using var context = _applicationContextFactory.Create();

        var examinationsQuery = context.Examinations
            .AsNoTracking();

        if (names.IsSignificant())
            examinationsQuery = examinationsQuery.Where(examination => names.Contains(examination.Name));

        examinationsQuery = examinationsQuery.OrderBy(examination => examination.Id);

        var examinations = await examinationsQuery
            .Select(examination => examination.ToDto())
            .ToArrayAsync();

        return examinations;
    }

    [HttpGet("{id:int}")]
    public async Task<ExaminationDto> Get(
        [FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();

        var examination = await context.Examinations
            .AsNoTracking()
            .FirstOrDefaultAsync(examination => examination.Id == id);

        return examination == null
            ? throw new NotFoundException($"Не найдено обследование с id = {id}")
            : examination.ToDto();
    }

    [HttpPost]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<ExaminationDto> Create(
        [FromBody] ExaminationDto examinationDto)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateExaminationModelAsync(context, examinationDto);
        if (!ModelState.IsValid)
            throw new BadRequestException(ModelState.JoinErrors());

        var examination = new Examination
        {
            Name = examinationDto.Name
        };

        context.Examinations.Add(examination);
        await context.SaveChangesAsync();

        _logger.LogInformation($"Examination with id = {examination.Id} created");

        return examination.ToDto();
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<ExaminationDto> Update(
        [FromRoute] int id,
        [FromBody] ExaminationDto examinationDto)
    {
        await using var context = _applicationContextFactory.Create();

        var examination = await context.Examinations
            .FirstOrDefaultAsync(examination => examination.Id == id);

        if (examination == null)
            throw new NotFoundException($"Не найдено обследование с id = {id}");

        await ValidateExaminationModelAsync(context, examinationDto, examination.Id);
        if (!ModelState.IsValid)
            throw new BadRequestException(ModelState.JoinErrors());

        examination.Name = examinationDto.Name;

        await context.SaveChangesAsync();

        _logger.LogInformation($"Examination with id = {id} updated");

        return examination.ToDto();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task Delete(
        [FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();

        var examination = await context.Examinations
            .FirstOrDefaultAsync(examination => examination.Id == id);

        if (examination == null)
            throw new NotFoundException($"Не найдено обследование с id = {id}");

        context.Examinations.Remove(examination);

        await context.SaveChangesAsync();

        _logger.LogInformation($"Examination with id = {id} deleted");
    }

    private async Task ValidateExaminationModelAsync(
        ApplicationContext context,
        ExaminationDto? examinationDto,
        int? currentId = null)
    {
        if (examinationDto == null)
            return;

        if (examinationDto.Name.IsNullOrEmpty())
        {
            ModelState.AddModelError(nameof(examinationDto.Name), "Название обязательно для заполнения.");
        }
        else
        {
            var hasConflictedName = await context.Examinations.AnyAsync(examination =>
                (!currentId.HasValue || examination.Id != currentId.Value) &&
                EF.Functions.Like(examinationDto.Name, examination.Name));

            if (hasConflictedName)
                ModelState.AddModelError(nameof(examinationDto.Name), "Название должно быть уникальным.");
        }
    }
}