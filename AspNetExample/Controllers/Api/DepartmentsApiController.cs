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
[Route("api/departments")]
[Produces(MediaTypeNames.Application.Json)]
public class DepartmentsApiController : ControllerBase
{
    private readonly IApplicationContextFactory _applicationContextFactory;
    private readonly ILogger<DepartmentsApiController> _logger;

    public DepartmentsApiController(
        IApplicationContextFactory applicationContextFactory,
        ILogger<DepartmentsApiController> logger)
    {
        _applicationContextFactory = applicationContextFactory;
        _logger = logger;
    }

    [HttpGet]
    public async Task<DepartmentDto[]> GetAll(
        [FromQuery] int[]? buildings,
        [FromQuery] decimal? financingFrom,
        [FromQuery] decimal? financingTo,
        [FromQuery] string[]? names)
    {
        await using var context = _applicationContextFactory.Create();

        var departmentsQuery = context.Departments
            .AsNoTracking();

        if (buildings?.Any() == true)
            departmentsQuery = departmentsQuery.Where(department => buildings.Contains(department.Building));
        if (financingFrom.HasValue)
            departmentsQuery = departmentsQuery.Where(department => department.Financing >= financingFrom);
        if (financingTo.HasValue)
            departmentsQuery = departmentsQuery.Where(department => department.Financing <= financingTo);
        if (names?.Any() == true)
            departmentsQuery = departmentsQuery.Where(department => names.Contains(department.Name));

        departmentsQuery = departmentsQuery.OrderBy(department => department.Id);

        var departments = await departmentsQuery
            .Select(department => department.ToDto())
            .ToArrayAsync();

        return departments;
    }

    [HttpGet("{id:int}")]
    public async Task<DepartmentDto> Get(
        [FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();

        var department = await context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(department => department.Id == id);

        return department == null
            ? throw new NotFoundException($"Не найден департамент с id = {id}")
            : department.ToDto();
    }

    [HttpPost]
    public async Task<DepartmentDto> Create(
        [FromBody] DepartmentDto departmentDto)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateDepartmentModelAsync(context, departmentDto);
        if (!ModelState.IsValid)
            throw new BadRequestException(ModelState.JoinErrors());

        var department = new Department
        {
            Building = departmentDto.Building,
            Financing = departmentDto.Financing,
            Name = departmentDto.Name
        };

        context.Departments.Add(department);
        await context.SaveChangesAsync();

        _logger.LogInformation($"Department with id = {department.Id} created");

        return department.ToDto();
    }

    [HttpPut("{id:int}")]
    public async Task<DepartmentDto> Update(
        [FromRoute] int id,
        [FromBody] DepartmentDto departmentDto)
    {
        await using var context = _applicationContextFactory.Create();

        var department = await context.Departments
            .FirstOrDefaultAsync(department => department.Id == id);

        if (department == null)
            throw new NotFoundException($"Не найден департамент с id = {id}");

        await ValidateDepartmentModelAsync(context, departmentDto, department.Id);
        if (!ModelState.IsValid)
            throw new BadRequestException(ModelState.JoinErrors());

        department.Building = departmentDto.Building;
        department.Financing = departmentDto.Financing;
        department.Name = departmentDto.Name;

        await context.SaveChangesAsync();

        _logger.LogInformation($"Department with id = {id} updated");

        return department.ToDto();
    }

    [HttpDelete("{id:int}")]
    public async Task Delete(
        [FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();

        var department = await context.Departments
            .FirstOrDefaultAsync(department => department.Id == id);

        if (department == null)
            throw new NotFoundException($"Не найден департамент с id = {id}");

        context.Departments.Remove(department);

        await context.SaveChangesAsync();

        _logger.LogInformation($"Department with id = {id} deleted");
    }

    private async Task ValidateDepartmentModelAsync(
        ApplicationContext context,
        DepartmentDto? departmentDto,
        int? currentId = null)
    {
        if (departmentDto == null)
            return;

        if (departmentDto.Building is < 1 or > 5)
            ModelState.AddModelError(nameof(departmentDto.Building), "Здание должно быть между 1 и 5.");

        if (departmentDto.Financing < 0)
            ModelState.AddModelError(nameof(departmentDto.Financing), "Финансирование должно быть положительным.");

        if (departmentDto.Name.IsSignificant())
        {
            var hasConflictedName = await context.Departments.AnyAsync(department =>
                (!currentId.HasValue || department.Id != currentId.Value) &&
                EF.Functions.Like(departmentDto.Name, department.Name));

            if (hasConflictedName)
                ModelState.AddModelError(nameof(departmentDto.Name), "Название должно быть уникальным.");
        }
    }
}