﻿using AspNetExample.Common.Extensions;
using AspNetExample.Database.Context;
using AspNetExample.Database.Context.Factory;
using AspNetExample.Domain;
using AspNetExample.Domain.Entities;
using AspNetExample.Extensions.Models;
using AspNetExample.Models.Interns;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AspNetExample.Controllers;

[Authorize]
public class InternsController : Controller
{
    private readonly IApplicationContextFactory _applicationContextFactory;
    private readonly ILogger<InternsController> _logger;

    public InternsController(
        IApplicationContextFactory applicationContextFactory,
        ILogger<InternsController> logger)
    {
        _applicationContextFactory = applicationContextFactory;
        _logger = logger;
    }

    public async Task<IActionResult> Index(
        [FromQuery] InternsIndexModel? model,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        var internsQuery = context.Interns
            .Include(intern => intern.Doctor)
            .AsNoTracking();

        var doctorNames = model?.DoctorNames?.Split(';');

        if (doctorNames.IsSignificant())
            internsQuery = internsQuery.Where(intern => doctorNames.Contains(intern.Doctor.Name));

        internsQuery = model?.SortBy switch
        {
            nameof(InternModel.Id) => internsQuery.OrderBy(intern => intern.Id),
            nameof(InternModel.Id) + Constants.DescSuffix => internsQuery.OrderByDescending(intern => intern.Id),
            nameof(InternModel.DoctorName) => internsQuery.OrderBy(intern => intern.Doctor.Name),
            nameof(InternModel.DoctorName) + Constants.DescSuffix => internsQuery.OrderByDescending(intern => intern.Doctor.Name),
            _ => internsQuery.OrderBy(intern => intern.Id)
        };

        var page = Math.Max(Constants.FirstPage, model?.Page ?? Constants.FirstPage);
        var totalCount = await internsQuery.CountAsync(cancellationToken);
        var interns = await internsQuery
            .Skip((page - Constants.FirstPage) * Constants.PageSize)
            .Take(Constants.PageSize)
            .Select(intern => intern.ToModel())
            .ToArrayAsync(cancellationToken);

        return View(new InternsIndexModel
        {
            Interns = interns,
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

        var intern = await context.Interns
            .Include(intern => intern.Doctor)
            .AsNoTracking()
            .FirstOrDefaultAsync(intern => intern.Id == id, cancellationToken);

        if (intern == null)
            return NotFound();

        return View(intern.ToDetailsModel());
    }

    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        ViewBag.Doctors = await GetDoctorsAsync(context, null, cancellationToken);
        return View(new InternModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Create(
        [FromForm] InternModel model,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateInternModelAsync(context, model, null, cancellationToken);

        if (!ModelState.IsValid)
        {
            ViewBag.Doctors = await GetDoctorsAsync(context, null, cancellationToken);
            return View(model);
        }

        var intern = new Intern
        {
            DoctorId = model.DoctorId
        };

        context.Interns.Add(intern);

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

        var intern = await context.Interns
            .Include(intern => intern.Doctor)
            .AsNoTracking()
            .FirstOrDefaultAsync(intern => intern.Id == id, cancellationToken);

        if (intern == null)
            return NotFound();

        ViewBag.Doctors = await GetDoctorsAsync(context, intern.DoctorId, cancellationToken);
        return View(intern.ToModel());
    }

    [HttpPost("[controller]/[action]/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Edit(
        [FromRoute] int id,
        [FromForm] InternModel model,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateInternModelAsync(context, model, id, cancellationToken);

        if (!ModelState.IsValid)
        {
            ViewBag.Doctors = await GetDoctorsAsync(context, model.DoctorId, cancellationToken);
            return View(model);
        }

        var intern = await context.Interns
            .FirstOrDefaultAsync(intern => intern.Id == id, cancellationToken);

        if (intern == null)
            return NotFound();

        intern.DoctorId = model.DoctorId;

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
        var intern = await context.Interns
            .FirstOrDefaultAsync(intern => intern.Id == id, cancellationToken);

        if (intern == null)
            return NotFound();

        context.Interns.Remove(intern);

        await context.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    private static async Task<SelectListItem[]> GetDoctorsAsync(
        ApplicationContext context,
        int? selectedDepartmentId,
        CancellationToken cancellationToken)
    {
        return await context.Doctors
            .OrderBy(e => e.Id)
            .Select(doctor => new SelectListItem
            {
                Value = doctor.Id.ToString(),
                Text = $"{doctor.Name} {doctor.Surname} ({doctor.Id})",
                Selected = doctor.Id == selectedDepartmentId
            })
            .ToArrayAsync(cancellationToken);
    }

    private async Task ValidateInternModelAsync(
        ApplicationContext context,
        InternModel model,
        int? currentId,
        CancellationToken cancellationToken)
    {
        ModelState.Remove(nameof(model.DoctorName));

        var doctor = await context.Doctors.FirstOrDefaultAsync(doctor =>
                doctor.Id == model.DoctorId,
            cancellationToken);

        if (doctor == null)
        {
            ModelState.AddModelError(nameof(model.DoctorId), "Доктор не найден.");
        }
        else
        {
            var isAlreadyIntern = await context.Interns.AnyAsync(intern =>
                    (!currentId.HasValue || intern.Id != currentId.Value) &&
                    intern.DoctorId == doctor.Id,
                cancellationToken);

            if (isAlreadyIntern)
                ModelState.AddModelError(nameof(model.DoctorId), "Доктор уже является стажером.");
        }
    }
}