using AspNetExample.Common.Extensions;
using AspNetExample.Database.Context;
using AspNetExample.Database.Context.Factory;
using AspNetExample.Domain;
using AspNetExample.Domain.Entities;
using AspNetExample.Extensions.Models;
using AspNetExample.Models.Diseases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetExample.Controllers;

[Authorize]
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

    public async Task<IActionResult> Index(
        [FromQuery] DiseasesIndexModel? model,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        var diseasesQuery = context.Diseases
            .AsNoTracking();

        var names = model?.Names?.Split(';');

        if (names.IsSignificant())
            diseasesQuery = diseasesQuery.Where(d => names.Contains(d.Name));

        diseasesQuery = model?.SortBy switch
        {
            nameof(DiseaseModel.Id) => diseasesQuery.OrderBy(disease => disease.Id),
            nameof(DiseaseModel.Id) + Constants.DescSuffix => diseasesQuery.OrderByDescending(disease => disease.Id),
            nameof(DiseaseModel.Name) => diseasesQuery.OrderBy(disease => disease.Id),
            nameof(DiseaseModel.Name) + Constants.DescSuffix => diseasesQuery.OrderByDescending(disease => disease.Name),
            _ => diseasesQuery.OrderBy(department => department.Id)
        };

        var page = Math.Max(Constants.FirstPage, model?.Page ?? Constants.FirstPage);
        var totalCount = await diseasesQuery.CountAsync(cancellationToken);
        var diseases = await diseasesQuery
            .Skip((page - Constants.FirstPage) * Constants.PageSize)
            .Take(Constants.PageSize)
            .Select(disease => disease.ToModel())
            .ToArrayAsync(cancellationToken);

        return View(new DiseasesIndexModel
        {
            Diseases = diseases,
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

        var disease = await context.Diseases
            .Include(disease => disease.DoctorsExaminations)
            .ThenInclude(doctorExamination => doctorExamination.Doctor)
            .Include(disease => disease.DoctorsExaminations)
            .ThenInclude(doctorExamination => doctorExamination.Examination)
            .Include(disease => disease.DoctorsExaminations)
            .ThenInclude(doctorExamination => doctorExamination.Ward)
            .AsNoTracking()
            .FirstOrDefaultAsync(disease => disease.Id == id, cancellationToken);

        if (disease == null)
            return NotFound();

        return View(disease.ToDetailsModel());
    }

    [Authorize(Roles = RoleTokens.AdminRole)]
    public IActionResult Create()
    {
        return View(new DiseaseModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Create(
        [FromForm] DiseaseModel model,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateDiseaseModelAsync(context, model, null, cancellationToken);

        if (!ModelState.IsValid)
            return View(model);

        var disease = new Disease
        {
            Name = model.Name
        };

        context.Diseases.Add(disease);

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

        var disease = await context.Diseases
            .AsNoTracking()
            .FirstOrDefaultAsync(disease => disease.Id == id, cancellationToken);

        if (disease == null)
            return NotFound();

        return View(disease.ToModel());
    }

    [HttpPost("[controller]/[action]/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Edit(
        [FromRoute] int id,
        [FromForm] DiseaseModel model,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateDiseaseModelAsync(context, model, id, cancellationToken);

        if (!ModelState.IsValid)
            return View(model);

        var disease = await context.Diseases
            .FirstOrDefaultAsync(disease => disease.Id == id, cancellationToken);

        if (disease == null)
            return NotFound();

        disease.Name = model.Name;

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
        var disease = await context.Diseases
            .FirstOrDefaultAsync(disease => disease.Id == id, cancellationToken);

        if (disease == null)
            return NotFound();

        context.Diseases.Remove(disease);

        await context.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateDiseaseModelAsync(
        ApplicationContext context,
        DiseaseModel model,
        int? currentId,
        CancellationToken cancellationToken)
    {
        if (model.Name.IsSignificant())
        {
            var hasConflictedName = await context.Diseases.AnyAsync(disease =>
                    (!currentId.HasValue || disease.Id != currentId.Value) &&
                    EF.Functions.Like(model.Name, disease.Name),
                cancellationToken);

            if (hasConflictedName)
                ModelState.AddModelError(nameof(model.Name), "Название должно быть уникальным.");
        }
    }
}