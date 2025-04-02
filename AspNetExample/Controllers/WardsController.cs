using AspNetExample.Common.Extensions;
using AspNetExample.Database.Context;
using AspNetExample.Database.Context.Factory;
using AspNetExample.Domain;
using AspNetExample.Domain.Entities;
using AspNetExample.Extensions.Models;
using AspNetExample.Models.Wards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AspNetExample.Controllers;

[Authorize]
public class WardsController : Controller
{
    private readonly IApplicationContextFactory _applicationContextFactory;
    private readonly ILogger<WardsController> _logger;

    public WardsController(
        IApplicationContextFactory applicationContextFactory,
        ILogger<WardsController> logger)
    {
        _applicationContextFactory = applicationContextFactory;
        _logger = logger;
    }

    public async Task<IActionResult> Index(
        [FromQuery] WardsIndexModel? model,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        var wardsQuery = context.Wards
            .Include(ward => ward.Department)
            .AsNoTracking();

        var names = model?.Names?.Split(';');
        var placesFrom = model?.PlacesFrom;
        var placesTo = model?.PlacesTo;
        var departmentNames = model?.DepartmentNames?.Split(";");

        if (names.IsSignificant())
            wardsQuery = wardsQuery.Where(ward => names.Contains(ward.Name));
        if (placesFrom != null)
            wardsQuery = wardsQuery.Where(ward => ward.Places >= placesFrom);
        if (placesTo != null)
            wardsQuery = wardsQuery.Where(ward => ward.Places <= placesTo);
        if (departmentNames != null)
            wardsQuery = wardsQuery.Where(ward => departmentNames.Contains(ward.Department.Name));

        wardsQuery = model?.SortBy switch
        {
            nameof(WardModel.Id) => wardsQuery.OrderBy(ward => ward.Id),
            nameof(WardModel.Id) + Constants.DescSuffix => wardsQuery.OrderByDescending(ward => ward.Id),
            nameof(WardModel.Name) => wardsQuery.OrderBy(ward => ward.Name),
            nameof(WardModel.Name) + Constants.DescSuffix => wardsQuery.OrderByDescending(ward => ward.Name),
            nameof(WardModel.Places) => wardsQuery.OrderBy(ward => ward.Places),
            nameof(WardModel.Places) + Constants.DescSuffix => wardsQuery.OrderByDescending(ward => ward.Places),
            nameof(WardModel.DepartmentName) => wardsQuery.OrderBy(ward => ward.Department.Name),
            nameof(WardModel.DepartmentName) + Constants.DescSuffix => wardsQuery.OrderByDescending(ward => ward.Department.Name),
            _ => wardsQuery.OrderBy(ward => ward.Id)
        };

        var page = Math.Max(Constants.FirstPage, model?.Page ?? Constants.FirstPage);
        var totalCount = await wardsQuery.CountAsync(cancellationToken);
        var wards = await wardsQuery
            .Skip((page - Constants.FirstPage) * Constants.PageSize)
            .Take(Constants.PageSize)
            .Select(ward => ward.ToModel())
            .ToArrayAsync(cancellationToken);

        return View(new WardsIndexModel
        {
            Wards = wards,
            SortBy = model?.SortBy,
            Page = page,
            TotalCount = totalCount
        });
    }

    [HttpGet("[controller]/[action]/{id:int}")]
    public async Task<IActionResult> Details(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        var ward = await context.Wards
            .Include(ward => ward.Department)
            .Include(ward => ward.DoctorsExaminations)
            .Include(examination => examination.DoctorsExaminations)
            .ThenInclude(doctorExamination => doctorExamination.Disease)
            .Include(examination => examination.DoctorsExaminations)
            .ThenInclude(doctorExamination => doctorExamination.Doctor)
            .Include(disease => disease.DoctorsExaminations)
            .ThenInclude(doctorExamination => doctorExamination.Examination)
            .AsNoTracking()
            .FirstOrDefaultAsync(ward => ward.Id == id, cancellationToken);

        if (ward == null)
            return NotFound();

        return View(ward.ToDetailsModel());
    }

    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        ViewBag.Departments = await GetDepartmentsAsync(context, null, cancellationToken);
        return View(new WardModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Create(
        [FromForm] WardModel model,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateWardModelAsync(context, model, null, cancellationToken);

        if (!ModelState.IsValid)
        {
            ViewBag.Departments = await GetDepartmentsAsync(context, null, cancellationToken);
            return View(model);
        }

        var ward = new Ward
        {
            Name = model.Name,
            Places = model.Places,
            DepartmentId = model.DepartmentId
        };

        context.Wards.Add(ward);

        await context.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("[controller]/[action]/{id:int}")]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Edit(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        var ward = await context.Wards
            .Include(ward => ward.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(ward => ward.Id == id, cancellationToken);

        if (ward == null)
            return NotFound();

        ViewBag.Departments = await GetDepartmentsAsync(context, ward.DepartmentId, cancellationToken);
        return View(ward.ToModel());
    }

    [HttpPost("[controller]/[action]/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Edit(
        [FromRoute] int id,
        [FromForm] WardModel model,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateWardModelAsync(context, model, id, cancellationToken);

        if (!ModelState.IsValid)
        {
            ViewBag.Departments = await GetDepartmentsAsync(context, model.DepartmentId, cancellationToken);
            return View(model);
        }

        var ward = await context.Wards
            .FirstOrDefaultAsync(ward => ward.Id == id, cancellationToken);

        if (ward == null)
            return NotFound();

        ward.Name = model.Name;
        ward.Places = model.Places;
        ward.DepartmentId = model.DepartmentId;

        await context.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("[controller]/[action]/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Delete(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();
        var ward = await context.Wards
            .FirstOrDefaultAsync(ward => ward.Id == id, cancellationToken);

        if (ward == null)
            return NotFound();

        context.Wards.Remove(ward);

        await context.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    private static async Task<SelectListItem[]> GetDepartmentsAsync(
        ApplicationContext context,
        int? selectedDepartmentId,
        CancellationToken cancellationToken)
    {
        return await context.Departments
            .OrderBy(e => e.Id)
            .Select(department => new SelectListItem
            {
                Value = department.Id.ToString(),
                Text = $"{department.Name} ({department.Id})",
                Selected = department.Id == selectedDepartmentId
            })
            .ToArrayAsync(cancellationToken);
    }

    private async Task ValidateWardModelAsync(
        ApplicationContext context,
        WardModel model,
        int? currentId,
        CancellationToken cancellationToken)
    {
        ModelState.Remove(nameof(model.DepartmentName));

        if (model.Places <= 0)
            ModelState.AddModelError(nameof(model.Places), "Количество мест должно быть больше 0.");

        if (model.Name.IsSignificant())
        {
            if (model.Name.Length > 20)
                ModelState.AddModelError(nameof(model.Name), "Название должно быть строкой с максимальной длиной 20.");

            var hasConflictedName = await context.Wards.AnyAsync(ward =>
                    (!currentId.HasValue || ward.Id != currentId.Value) &&
                    EF.Functions.Like(model.Name, ward.Name),
                cancellationToken);

            if (hasConflictedName)
                ModelState.AddModelError(nameof(model.Name), "Название должно быть уникальным.");
        }

        var isDepartmentExists = await context.Departments.AnyAsync(department =>
                department.Id == model.DepartmentId,
            cancellationToken);

        if (!isDepartmentExists)
            ModelState.AddModelError(nameof(model.DepartmentId), "Департамент не найден.");
    }
}