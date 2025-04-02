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
        [FromQuery] string[]? departmentNames,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        var wardsQuery = context.Wards
            .Include(ward => ward.Department)
            .AsNoTracking();

        if (names.IsSignificant())
            wardsQuery = wardsQuery.Where(ward => names.Contains(ward.Name));
        if (placesFrom != null)
            wardsQuery = wardsQuery.Where(ward => ward.Places >= placesFrom);
        if (placesTo != null)
            wardsQuery = wardsQuery.Where(ward => ward.Places <= placesTo);
        if (departmentNames.IsSignificant())
            wardsQuery = wardsQuery.Where(ward => departmentNames.Contains(ward.Department.Name));

        wardsQuery = wardsQuery.OrderBy(ward => ward.Id);

        var wards = await wardsQuery
            .Select(ward => ward.ToDto())
            .ToArrayAsync(cancellationToken);

        return wards;
    }

    [HttpGet("{id:int}")]
    public async Task<WardDto> Get(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        var ward = await context.Wards
            .Include(ward => ward.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(ward => ward.Id == id, cancellationToken);

        return ward == null
            ? throw new NotFoundException($"Не найдена палата с id = {id}")
            : ward.ToDto();
    }

    [HttpPost]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<WardDto> Create(
        [FromBody] WardDto wardDto,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        var department = await ValidateWardModelAsync(context, wardDto, null, cancellationToken);
        if (!ModelState.IsValid)
            throw new BadRequestException(ModelState.JoinErrors());

        var ward = new Ward
        {
            Name = wardDto.Name,
            Places = wardDto.Places,
            DepartmentId = department.Id
        };

        context.Wards.Add(ward);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation($"Ward with id = {ward.Id} created");

        return ward.ToDto();
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<WardDto> Update(
        [FromRoute] int id,
        [FromBody] WardDto wardDto,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        var ward = await context.Wards
            .FirstOrDefaultAsync(ward => ward.Id == id, cancellationToken);

        if (ward == null)
            throw new NotFoundException($"Не найдена палата с id = {id}");

        var department = await ValidateWardModelAsync(context, wardDto, ward.Id, cancellationToken);
        if (!ModelState.IsValid)
            throw new BadRequestException(ModelState.JoinErrors());

        ward.Name = wardDto.Name;
        ward.Places = wardDto.Places;
        ward.DepartmentId = department.Id;

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation($"Ward with id = {id} updated");

        return ward.ToDto();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task Delete(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        var ward = await context.Wards
            .FirstOrDefaultAsync(ward => ward.Id == id, cancellationToken);

        if (ward == null)
            throw new NotFoundException($"Не найдена палата с id = {id}");

        context.Wards.Remove(ward);

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation($"Ward with id = {id} deleted");
    }

    private async Task<Department> ValidateWardModelAsync(
        ApplicationContext context,
        WardDto? wardDto,
        int? currentId,
        CancellationToken cancellationToken)
    {
        if (wardDto == null)
            return null!;

        if (wardDto.Name.Length > 20)
            ModelState.AddModelError(nameof(wardDto.Name), "Название должно быть строкой с максимальной длиной 20.");

        if (wardDto.Places <= 0)
            ModelState.AddModelError(nameof(wardDto.Places), "Количество мест должно быть больше 0.");

        if (wardDto.Name.IsNullOrEmpty())
        {
            ModelState.AddModelError(nameof(wardDto.Name), "Название обязательно для заполнения.");
        }
        else
        {
            var hasConflictedName = await context.Wards.AnyAsync(ward =>
                    (!currentId.HasValue || ward.Id != currentId.Value) &&
                    EF.Functions.Like(wardDto.Name, ward.Name),
                cancellationToken);

            if (hasConflictedName)
                ModelState.AddModelError(nameof(wardDto.Name), "Название должно быть уникальным.");
        }

        var currentDepartment = (Department?) null;

        if (wardDto.DepartmentName.IsNullOrEmpty())
        {
            ModelState.AddModelError(nameof(wardDto.DepartmentName), "Название департамента обязательно для заполнения.");
        }
        else
        {
            currentDepartment = await context.Departments.FirstOrDefaultAsync(department =>
                    EF.Functions.Like(wardDto.DepartmentName, department.Name),
                cancellationToken);

            if (currentDepartment == null)
                ModelState.AddModelError(nameof(wardDto.Name), "Департамент не найден.");
        }

        return currentDepartment!;
    }
}