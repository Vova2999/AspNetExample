﻿using AspNetExample.Common.Extensions;
using AspNetExample.Database.Context;
using AspNetExample.Database.Context.Factory;
using AspNetExample.Domain;
using AspNetExample.Domain.Entities;
using AspNetExample.Extensions.Models;
using AspNetExample.Models.Examinations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetExample.Controllers;

[Authorize]
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

    public async Task<IActionResult> Index(
        [FromQuery] ExaminationsIndexModel? model,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        var examinationsQuery = context.Examinations
            .AsNoTracking();

        var names = model?.Names?.Split(';');

        if (names.IsSignificant())
            examinationsQuery = examinationsQuery.Where(examination => names.Contains(examination.Name));

        examinationsQuery = model?.SortBy switch
        {
            nameof(ExaminationModel.Id) => examinationsQuery.OrderBy(examination => examination.Id),
            nameof(ExaminationModel.Id) + Constants.DescSuffix => examinationsQuery.OrderByDescending(examination => examination.Id),
            nameof(ExaminationModel.Name) => examinationsQuery.OrderBy(examination => examination.Name),
            nameof(ExaminationModel.Name) + Constants.DescSuffix => examinationsQuery.OrderByDescending(examination => examination.Name),
            _ => examinationsQuery.OrderBy(examination => examination.Id)
        };

        var page = Math.Max(Constants.FirstPage, model?.Page ?? Constants.FirstPage);
        var totalCount = await examinationsQuery.CountAsync(cancellationToken);
        var examinations = await examinationsQuery
            .Skip((page - Constants.FirstPage) * Constants.PageSize)
            .Take(Constants.PageSize)
            .Select(examination => examination.ToModel())
            .ToArrayAsync(cancellationToken);

        return View(new ExaminationsIndexModel
        {
            Examinations = examinations,
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

        var examination = await context.Examinations
            .Include(examination => examination.DoctorsExaminations)
            .ThenInclude(doctorExamination => doctorExamination.Disease)
            .Include(examination => examination.DoctorsExaminations)
            .ThenInclude(doctorExamination => doctorExamination.Doctor)
            .Include(disease => disease.DoctorsExaminations)
            .ThenInclude(doctorExamination => doctorExamination.Ward)
            .AsNoTracking()
            .FirstOrDefaultAsync(examination => examination.Id == id, cancellationToken);

        if (examination == null)
            return NotFound();

        return View(examination.ToDetailsModel());
    }

    [Authorize(Roles = RoleTokens.AdminRole)]
    public IActionResult Create()
    {
        return View(new ExaminationModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Create(
        [FromForm] ExaminationModel model,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateExaminationModelAsync(context, model, null, cancellationToken);

        if (!ModelState.IsValid)
            return View(model);

        var examination = new Examination
        {
            Name = model.Name
        };

        context.Examinations.Add(examination);

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

        var examination = await context.Examinations
            .AsNoTracking()
            .FirstOrDefaultAsync(examination => examination.Id == id, cancellationToken);

        if (examination == null)
            return NotFound();

        return View(examination.ToModel());
    }

    [HttpPost("[controller]/[action]/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Edit(
        [FromRoute] int id,
        [FromForm] ExaminationModel model,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateExaminationModelAsync(context, model, id, cancellationToken);

        if (!ModelState.IsValid)
            return View(model);

        var examination = await context.Examinations
            .FirstOrDefaultAsync(examination => examination.Id == id, cancellationToken);

        if (examination == null)
            return NotFound();

        examination.Name = model.Name;

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
        var examination = await context.Examinations
            .FirstOrDefaultAsync(examination => examination.Id == id, cancellationToken);

        if (examination == null)
            return NotFound();

        context.Examinations.Remove(examination);

        await context.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateExaminationModelAsync(
        ApplicationContext context,
        ExaminationModel model,
        int? currentId,
        CancellationToken cancellationToken)
    {
        if (model.Name.IsSignificant())
        {
            var hasConflictedName = await context.Examinations.AnyAsync(examination =>
                    (!currentId.HasValue || examination.Id != currentId.Value) &&
                    EF.Functions.Like(model.Name, examination.Name),
                cancellationToken);

            if (hasConflictedName)
                ModelState.AddModelError(nameof(model.Name), "Название должно быть уникальным.");
        }
    }
}