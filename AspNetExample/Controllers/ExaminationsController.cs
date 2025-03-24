using AspNetExample.Common.Extensions;
using AspNetExample.Database.Context;
using AspNetExample.Database.Context.Factory;
using AspNetExample.Domain.Entities;
using AspNetExample.Extensions.Models;
using AspNetExample.Models.Examinations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetExample.Controllers;

public class ExaminationsController : Controller
{
    private readonly IApplicationContextFactory _applicationContextFactory;
    private readonly ILogger<ExaminationsController> _logger;

    public ExaminationsController(
        IApplicationContextFactory applicationContextFactory,
        ILogger<ExaminationsController> logger)
    {
        _applicationContextFactory = applicationContextFactory;
        _logger = logger;
    }

    public async Task<IActionResult> Index([FromQuery] ExaminationsIndexModel? model)
    {
        await using var context = _applicationContextFactory.Create();

        var examinationsQuery = context.Examinations
            .AsNoTracking();

        var names = model?.Names?.Split(';');

        if (names?.Any() == true)
            examinationsQuery = examinationsQuery.Where(examination => names.Contains(examination.Name));

        examinationsQuery = examinationsQuery.OrderBy(examination => examination.Id);

        var page = Math.Max(Constants.FirstPage, model?.Page ?? Constants.FirstPage);
        var totalCount = examinationsQuery.Count();
        var examinations = await examinationsQuery
            .Skip((page - Constants.FirstPage) * Constants.PageSize)
            .Take(Constants.PageSize)
            .Select(examination => examination.ToModel())
            .ToArrayAsync();

        return View(new ExaminationsIndexModel
        {
            Examinations = examinations,
            Page = page,
            TotalCount = totalCount
        });
    }

    public IActionResult Create()
    {
        return View(new ExaminationModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] ExaminationModel model)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateExaminationModelAsync(context, model);

        if (!ModelState.IsValid)
            return View(model);

        var examination = new Examination
        {
            Name = model.Name
        };

        context.Examinations.Add(examination);

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("[controller]/[action]/{id:int}")]
    public async Task<IActionResult> Edit([FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();

        var examination = await context.Examinations
            .AsNoTracking()
            .FirstOrDefaultAsync(examination => examination.Id == id);

        if (examination == null)
            return NotFound();

        return View(examination.ToModel());
    }

    [HttpPost("[controller]/[action]/{id:int}")]
    public async Task<IActionResult> Edit([FromRoute] int id, [FromForm] ExaminationModel model)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateExaminationModelAsync(context, model, id);

        if (!ModelState.IsValid)
            return View(model);

        var examination = await context.Examinations
            .FirstOrDefaultAsync(examination => examination.Id == id);

        if (examination == null)
            return NotFound();

        examination.Name = model.Name;

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("[controller]/[action]/{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();
        var examination = await context.Examinations
            .FirstOrDefaultAsync(examination => examination.Id == id);

        if (examination == null)
            return NotFound();

        context.Examinations.Remove(examination);

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateExaminationModelAsync(
        ApplicationContext context,
        ExaminationModel model,
        int? currentId = null)
    {
        if (model.Name.IsSignificant())
        {
            var hasConflictedName = await context.Examinations.AnyAsync(examination =>
                (!currentId.HasValue || examination.Id != currentId.Value) &&
                EF.Functions.Like(model.Name, examination.Name));

            if (hasConflictedName)
                ModelState.AddModelError(nameof(model.Name), "Название должно быть уникальным.");
        }
    }
}