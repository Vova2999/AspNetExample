using System.Net.Mime;
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
[Route("api/wards")]
[Produces(MediaTypeNames.Application.Json)]
public class WardsApiController : ControllerBase
{
    private readonly IApplicationContextFactory _applicationContextFactory;
    private readonly ILogger<WardsApiController> _logger;

    public WardsApiController(
        IApplicationContextFactory applicationContextFactory,
        ILogger<WardsApiController> logger)
    {
        _applicationContextFactory = applicationContextFactory;
        _logger = logger;
    }

    [HttpGet]
    public async Task<WardDto[]> GetAll(
        [FromQuery] string[]? names,
        [FromQuery] int? placesFrom,
        [FromQuery] int? placesTo,
        [FromQuery] string[]? departmentNames)
    {
        await using var context = _applicationContextFactory.Create();

        var wardsQuery = context.Wards
            .Include(ward => ward.Department)
            .AsNoTracking();

        if (names?.Any() == true)
            wardsQuery = wardsQuery.Where(d => names.Contains(d.Name));
        if (placesFrom != null)
            wardsQuery = wardsQuery.Where(d => d.Places >= placesFrom);
        if (placesTo != null)
            wardsQuery = wardsQuery.Where(d => d.Places <= placesTo);
        if (departmentNames?.Any() == true)
            wardsQuery = wardsQuery.Where(d => departmentNames.Contains(d.Department.Name));

        var wards = await wardsQuery
            .Select(ward => ward.ToDto())
            .ToArrayAsync();

        return wards;
    }

    [HttpGet("{id:int}")]
    public async Task<WardDto> Get(
        [FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();

        var ward = await context.Wards
            .Include(ward => ward.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(ward => ward.Id == id);

        return ward == null
            ? throw new NotFoundException($"Не найдена палата с id = {id}")
            : ward.ToDto();
    }

    [HttpPost]
    public async Task<WardDto> Create(
        [FromBody] WardDto wardDto)
    {
        await using var context = _applicationContextFactory.Create();

        var department = await ValidateWardModelAsync(context, wardDto);
        if (!ModelState.IsValid)
            throw new BadRequestException(ModelState.JoinErrors());

        var ward = new Ward
        {
            Name = wardDto.Name,
            Places = wardDto.Places,
            DepartmentId = department.Id
        };

        context.Wards.Add(ward);
        await context.SaveChangesAsync();

        _logger.LogInformation($"Ward with id = {ward.Id} created");

        return ward.ToDto();
    }

    [HttpPut("{id:int}")]
    public async Task<WardDto> Update(
        [FromRoute] int id,
        [FromBody] WardDto wardDto)
    {
        await using var context = _applicationContextFactory.Create();

        var ward = await context.Wards
            .FirstOrDefaultAsync(ward => ward.Id == id);

        if (ward == null)
            throw new NotFoundException($"Не найдена палата с id = {id}");

        var department = await ValidateWardModelAsync(context, wardDto, ward.Id);
        if (!ModelState.IsValid)
            throw new BadRequestException(ModelState.JoinErrors());

        ward.Name = wardDto.Name;
        ward.Places = wardDto.Places;
        ward.DepartmentId = department.Id;

        await context.SaveChangesAsync();

        _logger.LogInformation($"Ward with id = {id} updated");

        return ward.ToDto();
    }

    [HttpDelete("{id:int}")]
    public async Task Delete(
        [FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();

        var ward = await context.Wards
            .FirstOrDefaultAsync(ward => ward.Id == id);

        if (ward == null)
            throw new NotFoundException($"Не найдена палата с id = {id}");

        context.Wards.Remove(ward);

        await context.SaveChangesAsync();

        _logger.LogInformation($"Ward with id = {id} deleted");
    }

    private async Task<Department> ValidateWardModelAsync(
        ApplicationContext context,
        WardDto? wardDto,
        int? currentId = null)
    {
        if (wardDto == null)
            return null!;

        if (wardDto.Name.Length > 20)
            ModelState.AddModelError(nameof(wardDto.Name), "Название должно быть строкой с максимальной длиной 20.");

        if (wardDto.Places <= 0)
            ModelState.AddModelError(nameof(wardDto.Places), "Количество мест должно быть больше 0.");

        var hasConflictedName = await context.Wards.AnyAsync(ward =>
            (!currentId.HasValue || ward.Id != currentId.Value) &&
            EF.Functions.Like(wardDto.Name, ward.Name));

        if (hasConflictedName)
            ModelState.AddModelError(nameof(wardDto.Name), "Название должно быть уникальным.");

        var department = await context.Departments.FirstOrDefaultAsync(department =>
            EF.Functions.Like(wardDto.DepartmentName, department.Name));

        if (department == null)
            ModelState.AddModelError(nameof(wardDto.Name), "Департамент не найден.");

        return department!;
    }
}