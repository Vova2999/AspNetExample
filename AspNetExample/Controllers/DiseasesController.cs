using AspNetExample.Common.Extensions;
using AspNetExample.Database.Context;
using AspNetExample.Database.Context.Factory;
using AspNetExample.Domain.Entities;
using AspNetExample.Extensions.Models;
using AspNetExample.Models.Diseases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetExample.Controllers;

public class DiseasesController : Controller
{
    private readonly IApplicationContextFactory _applicationContextFactory;
    private readonly ILogger<DiseasesController> _logger;

    public DiseasesController(
        IApplicationContextFactory applicationContextFactory,
        ILogger<DiseasesController> logger)
    {
        _applicationContextFactory = applicationContextFactory;
        _logger = logger;
    }

    public async Task<IActionResult> Index([FromQuery] DiseasesIndexModel? model)
    {
        await using var context = _applicationContextFactory.Create();

        var diseasesQuery = context.Diseases
            .AsNoTracking();

        var names = model?.Names?.Split(';');

        if (names?.Any() == true)
            diseasesQuery = diseasesQuery.Where(d => names.Contains(d.Name));

        diseasesQuery = diseasesQuery.OrderBy(disease => disease.Id);

        var page = Math.Max(Constants.FirstPage, model?.Page ?? Constants.FirstPage);
        var totalCount = diseasesQuery.Count();
        var diseases = await diseasesQuery
            .Skip((page - Constants.FirstPage) * Constants.PageSize)
            .Take(Constants.PageSize)
            .Select(disease => disease.ToModel())
            .ToArrayAsync();

        return View(new DiseasesIndexModel
        {
            Diseases = diseases,
            Page = page,
            TotalCount = totalCount
        });
    }

    public IActionResult Create()
    {
        return View(new DiseaseModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] DiseaseModel model)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateDiseaseModelAsync(context, model);

        if (!ModelState.IsValid)
            return View(model);

        var disease = new Disease
        {
            Name = model.Name
        };

        context.Diseases.Add(disease);

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("[controller]/[action]/{id:int}")]
    public async Task<IActionResult> Edit([FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();

        var disease = await context.Diseases
            .AsNoTracking()
            .FirstOrDefaultAsync(disease => disease.Id == id);

        if (disease == null)
            return NotFound();

        return View(disease.ToModel());
    }

    [HttpPost("[controller]/[action]/{id:int}")]
    public async Task<IActionResult> Edit([FromRoute] int id, [FromForm] DiseaseModel model)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateDiseaseModelAsync(context, model, id);

        if (!ModelState.IsValid)
            return View(model);

        var disease = await context.Diseases
            .FirstOrDefaultAsync(disease => disease.Id == id);

        if (disease == null)
            return NotFound();

        disease.Name = model.Name;

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("[controller]/[action]/{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();
        var disease = await context.Diseases
            .FirstOrDefaultAsync(disease => disease.Id == id);

        if (disease == null)
            return NotFound();

        context.Diseases.Remove(disease);

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateDiseaseModelAsync(
        ApplicationContext context,
        DiseaseModel model,
        int? currentId = null)
    {
        if (model.Name.IsSignificant())
        {
            var hasConflictedName = await context.Diseases.AnyAsync(disease =>
                (!currentId.HasValue || disease.Id != currentId.Value) &&
                EF.Functions.Like(model.Name, disease.Name));

            if (hasConflictedName)
                ModelState.AddModelError(nameof(model.Name), "Название должно быть уникальным.");
        }
    }
}