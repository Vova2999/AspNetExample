using AspNetExample.Common.Extensions;
using AspNetExample.Database.Context;
using AspNetExample.Database.Context.Factory;
using AspNetExample.Domain.Entities;
using AspNetExample.Extensions.Models;
using AspNetExample.Models.Wards;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AspNetExample.Controllers;

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

    public async Task<IActionResult> Index([FromQuery] WardsIndexModel? model)
    {
        await using var context = _applicationContextFactory.Create();

        var wardsQuery = context.Wards
            .Include(ward => ward.Department)
            .AsNoTracking();

        var names = model?.Names?.Split(';');
        var placesFrom = model?.PlacesFrom;
        var placesTo = model?.PlacesTo;
        var departmentNames = model?.DepartmentNames?.Split(";");

        if (names?.Any() == true)
            wardsQuery = wardsQuery.Where(ward => names.Contains(ward.Name));
        if (placesFrom != null)
            wardsQuery = wardsQuery.Where(ward => ward.Places >= placesFrom);
        if (placesTo != null)
            wardsQuery = wardsQuery.Where(ward => ward.Places <= placesTo);
        if (departmentNames != null)
            wardsQuery = wardsQuery.Where(ward => departmentNames.Contains(ward.Department.Name));

        wardsQuery = wardsQuery.OrderBy(ward => ward.Id);

        var page = Math.Max(Constants.FirstPage, model?.Page ?? Constants.FirstPage);
        var totalCount = wardsQuery.Count();
        var wards = await wardsQuery
            .Skip((page - Constants.FirstPage) * Constants.PageSize)
            .Take(Constants.PageSize)
            .Select(ward => ward.ToModel())
            .ToArrayAsync();

        return View(new WardsIndexModel
        {
            Wards = wards,
            Page = page,
            TotalCount = totalCount
        });
    }

    public async Task<IActionResult> Create()
    {
        await using var context = _applicationContextFactory.Create();

        ViewBag.Departments = await GetDepartmentsAsync(context);
        return View(new WardModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] WardModel model)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateWardModelAsync(context, model);

        if (!ModelState.IsValid)
        {
            ViewBag.Departments = await GetDepartmentsAsync(context);
            return View(model);
        }

        var ward = new Ward
        {
            Name = model.Name,
            Places = model.Places,
            DepartmentId = model.DepartmentId
        };

        context.Wards.Add(ward);

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("[controller]/[action]/{id}")]
    public async Task<IActionResult> Edit([FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();

        var ward = await context.Wards
            .Include(ward => ward.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(ward => ward.Id == id);

        if (ward == null)
            return NotFound();

        ViewBag.Departments = await GetDepartmentsAsync(context, ward.DepartmentId);
        return View(ward.ToModel());
    }

    [HttpPost("[controller]/[action]/{id}")]
    public async Task<IActionResult> Edit([FromRoute] int id, [FromForm] WardModel model)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateWardModelAsync(context, model, id);

        if (!ModelState.IsValid)
        {
            ViewBag.Departments = await GetDepartmentsAsync(context, model.DepartmentId);
            return View(model);
        }

        var ward = await context.Wards
            .FirstOrDefaultAsync(ward => ward.Id == id);

        if (ward == null)
            return NotFound();

        ward.Name = model.Name;
        ward.Places = model.Places;
        ward.DepartmentId = model.DepartmentId;

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("[controller]/[action]/{id}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();
        var ward = await context.Wards
            .FirstOrDefaultAsync(ward => ward.Id == id);

        if (ward == null)
            return NotFound();

        context.Wards.Remove(ward);

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private static async Task<SelectListItem[]> GetDepartmentsAsync(
        ApplicationContext context,
        int? selectedDepartmentId = null)
    {
        return await context.Departments
            .OrderBy(e => e.Id)
            .Select(department => new SelectListItem
            {
                Value = department.Id.ToString(),
                Text = $"{department.Name} ({department.Id})",
                Selected = department.Id == selectedDepartmentId
            })
            .ToArrayAsync();
    }

    private async Task ValidateWardModelAsync(
        ApplicationContext context,
        WardModel model,
        int? currentId = null)
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
                EF.Functions.Like(model.Name, ward.Name));

            if (hasConflictedName)
                ModelState.AddModelError(nameof(model.Name), "Название должно быть уникальным.");
        }

        var isDepartmentExists = await context.Departments.AnyAsync(department =>
            department.Id == model.DepartmentId);

        if (!isDepartmentExists)
            ModelState.AddModelError(nameof(model.DepartmentId), "Департамент не найден.");
    }
}