using AspNetExample.Common.Extensions;
using AspNetExample.Database.Context;
using AspNetExample.Database.Context.Factory;
using AspNetExample.Domain;
using AspNetExample.Domain.Entities;
using AspNetExample.Extensions.Models;
using AspNetExample.Models.Departments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetExample.Controllers;

[Authorize]
public class DepartmentsController : Controller
{
    private readonly IApplicationContextFactory _applicationContextFactory;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(
        IApplicationContextFactory applicationContextFactory,
        ILogger<DepartmentsController> logger)
    {
        _applicationContextFactory = applicationContextFactory;
        _logger = logger;
    }

    public async Task<IActionResult> Index([FromQuery] DepartmentsIndexModel? model)
    {
        await using var context = _applicationContextFactory.Create();

        var departmentsQuery = context.Departments
            .AsNoTracking();

        var buildings = model?.Buildings?.SplitAndParse<int>(';', int.TryParse);
        var financingFrom = model?.FinancingFrom;
        var financingTo = model?.FinancingTo;
        var names = model?.Names?.Split(';');

        if (buildings.IsSignificant())
            departmentsQuery = departmentsQuery.Where(department => buildings.Contains(department.Building));
        if (financingFrom.HasValue)
            departmentsQuery = departmentsQuery.Where(department => department.Financing >= financingFrom);
        if (financingTo.HasValue)
            departmentsQuery = departmentsQuery.Where(department => department.Financing <= financingTo);
        if (names.IsSignificant())
            departmentsQuery = departmentsQuery.Where(department => names.Contains(department.Name));

        departmentsQuery = model?.SortBy switch
        {
            nameof(DepartmentModel.Id) => departmentsQuery.OrderBy(department => department.Id),
            nameof(DepartmentModel.Id) + Constants.DescSuffix => departmentsQuery.OrderByDescending(department => department.Id),
            nameof(DepartmentModel.Building) => departmentsQuery.OrderBy(department => department.Building),
            nameof(DepartmentModel.Building) + Constants.DescSuffix => departmentsQuery.OrderByDescending(department => department.Building),
            nameof(DepartmentModel.Financing) => departmentsQuery.OrderBy(department => department.Financing),
            nameof(DepartmentModel.Financing) + Constants.DescSuffix => departmentsQuery.OrderByDescending(department => department.Financing),
            nameof(DepartmentModel.Name) => departmentsQuery.OrderBy(department => department.Name),
            nameof(DepartmentModel.Name) + Constants.DescSuffix => departmentsQuery.OrderByDescending(department => department.Name),
            _ => departmentsQuery.OrderBy(department => department.Id)
        };

        var page = Math.Max(Constants.FirstPage, model?.Page ?? Constants.FirstPage);
        var totalCount = departmentsQuery.Count();
        var departments = await departmentsQuery
            .Skip((page - Constants.FirstPage) * Constants.PageSize)
            .Take(Constants.PageSize)
            .Select(department => department.ToModel())
            .ToArrayAsync();

        return View(new DepartmentsIndexModel
        {
            Departments = departments,
            SortBy = model?.SortBy,
            Page = page,
            TotalCount = totalCount
        });
    }

    [HttpGet("[controller]/[action]/{id:int}")]
    public async Task<IActionResult> Details([FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();

        var department = await context.Departments
            .Include(department => department.Wards)
            .AsNoTracking()
            .FirstOrDefaultAsync(department => department.Id == id);

        if (department == null)
            return NotFound();

        return View(department.ToDetailsModel());
    }

    [Authorize(Roles = RoleTokens.AdminRole)]
    public IActionResult Create()
    {
        return View(new DepartmentModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Create([FromForm] DepartmentModel model)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateDepartmentModelAsync(context, model);

        if (!ModelState.IsValid)
            return View(model);

        var department = new Department
        {
            Building = model.Building,
            Financing = model.Financing,
            Name = model.Name
        };

        context.Departments.Add(department);

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("[controller]/[action]/{id:int}")]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Edit([FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();

        var department = await context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(department => department.Id == id);

        if (department == null)
            return NotFound();

        return View(department.ToModel());
    }

    [HttpPost("[controller]/[action]/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Edit([FromRoute] int id, [FromForm] DepartmentModel model)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateDepartmentModelAsync(context, model, id);

        if (!ModelState.IsValid)
            return View(model);

        var department = await context.Departments
            .FirstOrDefaultAsync(department => department.Id == id);

        if (department == null)
            return NotFound();

        department.Building = model.Building;
        department.Financing = model.Financing;
        department.Name = model.Name;

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("[controller]/[action]/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();
        var department = await context.Departments
            .FirstOrDefaultAsync(department => department.Id == id);

        if (department == null)
            return NotFound();

        context.Departments.Remove(department);

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateDepartmentModelAsync(
        ApplicationContext context,
        DepartmentModel model,
        int? currentId = null)
    {
        if (model.Building is < 1 or > 5)
            ModelState.AddModelError(nameof(model.Building), "Здание должно быть между 1 и 5.");

        if (model.Financing < 0)
            ModelState.AddModelError(nameof(model.Financing), "Финансирование должно быть положительным.");

        if (model.Name.IsSignificant())
        {
            var hasConflictedName = await context.Departments.AnyAsync(department =>
                (!currentId.HasValue || department.Id != currentId.Value) &&
                EF.Functions.Like(model.Name, department.Name));

            if (hasConflictedName)
                ModelState.AddModelError(nameof(model.Name), "Название должно быть уникальным.");
        }
    }
}