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
[Route("api/diseases")]
[Produces(MediaTypeNames.Application.Json)]
public class DiseasesApiController : ControllerBase
{
    private readonly IApplicationContextFactory _applicationContextFactory;
    private readonly ILogger<DiseasesApiController> _logger;

    public DiseasesApiController(
        IApplicationContextFactory applicationContextFactory,
        ILogger<DiseasesApiController> logger)
    {
        _applicationContextFactory = applicationContextFactory;
        _logger = logger;
    }

    [HttpGet]
    public async Task<DiseaseDto[]> GetAll(
        [FromQuery] string[]? names)
    {
        await using var context = _applicationContextFactory.Create();

        var diseasesQuery = context.Diseases
            .AsNoTracking();

        if (names?.Any() == true)
            diseasesQuery = diseasesQuery.Where(disease => names.Contains(disease.Name));

        var diseases = await diseasesQuery
            .Select(disease => disease.ToDto())
            .ToArrayAsync();

        return diseases;
    }

    [HttpGet("{id:int}")]
    public async Task<DiseaseDto> Get(
        [FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();

        var disease = await context.Diseases
            .AsNoTracking()
            .FirstOrDefaultAsync(disease => disease.Id == id);

        return disease == null
            ? throw new NotFoundException($"Не найдена болезнь с id = {id}")
            : disease.ToDto();
    }

    [HttpPost]
    public async Task<DiseaseDto> Create(
        [FromBody] DiseaseDto diseaseDto)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateDiseaseModelAsync(context, diseaseDto);
        if (!ModelState.IsValid)
            throw new BadRequestException(ModelState.JoinErrors());

        var disease = new Disease
        {
            Name = diseaseDto.Name
        };

        context.Diseases.Add(disease);
        await context.SaveChangesAsync();

        _logger.LogInformation($"Disease with id = {disease.Id} created");

        return disease.ToDto();
    }

    [HttpPut("{id:int}")]
    public async Task<DiseaseDto> Update(
        [FromRoute] int id,
        [FromBody] DiseaseDto diseaseDto)
    {
        await using var context = _applicationContextFactory.Create();

        var disease = await context.Diseases
            .FirstOrDefaultAsync(disease => disease.Id == id);

        if (disease == null)
            throw new NotFoundException($"Не найдена болезнь с id = {id}");

        await ValidateDiseaseModelAsync(context, diseaseDto, disease.Id);
        if (!ModelState.IsValid)
            throw new BadRequestException(ModelState.JoinErrors());

        disease.Name = diseaseDto.Name;

        await context.SaveChangesAsync();

        _logger.LogInformation($"Disease with id = {id} updated");

        return disease.ToDto();
    }

    [HttpDelete("{id:int}")]
    public async Task Delete(
        [FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();

        var disease = await context.Diseases
            .FirstOrDefaultAsync(disease => disease.Id == id);

        if (disease == null)
            throw new NotFoundException($"Не найдена болезнь с id = {id}");

        context.Diseases.Remove(disease);

        await context.SaveChangesAsync();

        _logger.LogInformation($"Disease with id = {id} deleted");
    }

    private async Task ValidateDiseaseModelAsync(
        ApplicationContext context,
        DiseaseDto? diseaseDto,
        int? currentId = null)
    {
        if (diseaseDto == null)
            return;

        if (diseaseDto.Name.IsSignificant())
        {
            var hasConflictedName = await context.Diseases.AnyAsync(disease =>
                (!currentId.HasValue || disease.Id != currentId.Value) &&
                EF.Functions.Like(diseaseDto.Name, disease.Name));

            if (hasConflictedName)
                ModelState.AddModelError(nameof(diseaseDto.Name), "Название должно быть уникальным.");
        }
    }
}