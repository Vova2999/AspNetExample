using AspNetExample.Common.Extensions;
using AspNetExample.Database.Context;
using AspNetExample.Database.Context.Factory;
using AspNetExample.Domain;
using AspNetExample.Domain.Entities;
using AspNetExample.Extensions.Models;
using AspNetExample.Models.Professors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AspNetExample.Controllers;

[Authorize]
public class ProfessorsController : Controller
{
    private readonly IApplicationContextFactory _applicationContextFactory;
    private readonly ILogger<ProfessorsController> _logger;

    public ProfessorsController(
        IApplicationContextFactory applicationContextFactory,
        ILogger<ProfessorsController> logger)
    {
        _applicationContextFactory = applicationContextFactory;
        _logger = logger;
    }

    public async Task<IActionResult> Index([FromQuery] ProfessorsIndexModel? model)
    {
        await using var context = _applicationContextFactory.Create();

        var professorsQuery = context.Professors
            .Include(professor => professor.Doctor)
            .AsNoTracking();

        var doctorNames = model?.DoctorNames?.Split(';');

        if (doctorNames.IsSignificant())
            professorsQuery = professorsQuery.Where(professor => doctorNames.Contains(professor.Doctor.Name));

        professorsQuery = model?.SortBy switch
        {
            nameof(ProfessorModel.Id) => professorsQuery.OrderBy(professor => professor.Id),
            nameof(ProfessorModel.Id) + Constants.DescSuffix => professorsQuery.OrderByDescending(professor => professor.Id),
            nameof(ProfessorModel.DoctorName) => professorsQuery.OrderBy(professor => professor.Doctor.Name),
            nameof(ProfessorModel.DoctorName) + Constants.DescSuffix => professorsQuery.OrderByDescending(professor => professor.Doctor.Name),
            _ => professorsQuery.OrderBy(professor => professor.Id)
        };

        var page = Math.Max(Constants.FirstPage, model?.Page ?? Constants.FirstPage);
        var totalCount = professorsQuery.Count();
        var professors = await professorsQuery
            .Skip((page - Constants.FirstPage) * Constants.PageSize)
            .Take(Constants.PageSize)
            .Select(professor => professor.ToModel())
            .ToArrayAsync();

        return View(new ProfessorsIndexModel
        {
            Professors = professors,
            SortBy = model?.SortBy,
            Page = page,
            TotalCount = totalCount
        });
    }

    [HttpGet("[controller]/[action]/{id:int}")]
    public async Task<IActionResult> Details([FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();

        var professor = await context.Professors
            .Include(professor => professor.Doctor)
            .AsNoTracking()
            .FirstOrDefaultAsync(professor => professor.Id == id);

        if (professor == null)
            return NotFound();

        return View(professor.ToDetailsModel());
    }

    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Create()
    {
        await using var context = _applicationContextFactory.Create();

        ViewBag.Doctors = await GetDoctorsAsync(context);
        return View(new ProfessorModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Create([FromForm] ProfessorModel model)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateProfessorModelAsync(context, model);

        if (!ModelState.IsValid)
        {
            ViewBag.Doctors = await GetDoctorsAsync(context);
            return View(model);
        }

        var professor = new Professor
        {
            DoctorId = model.DoctorId
        };

        context.Professors.Add(professor);

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("[controller]/[action]/{id:int}")]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Edit([FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();

        var professor = await context.Professors
            .Include(professor => professor.Doctor)
            .AsNoTracking()
            .FirstOrDefaultAsync(professor => professor.Id == id);

        if (professor == null)
            return NotFound();

        ViewBag.Doctors = await GetDoctorsAsync(context, professor.DoctorId);
        return View(professor.ToModel());
    }

    [HttpPost("[controller]/[action]/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Edit([FromRoute] int id, [FromForm] ProfessorModel model)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateProfessorModelAsync(context, model, id);

        if (!ModelState.IsValid)
        {
            ViewBag.Doctors = await GetDoctorsAsync(context, model.DoctorId);
            return View(model);
        }

        var professor = await context.Professors
            .FirstOrDefaultAsync(professor => professor.Id == id);

        if (professor == null)
            return NotFound();

        professor.DoctorId = model.DoctorId;

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("[controller]/[action]/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();
        var professor = await context.Professors
            .FirstOrDefaultAsync(professor => professor.Id == id);

        if (professor == null)
            return NotFound();

        context.Professors.Remove(professor);

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private static async Task<SelectListItem[]> GetDoctorsAsync(
        ApplicationContext context,
        int? selectedDepartmentId = null)
    {
        return await context.Doctors
            .OrderBy(e => e.Id)
            .Select(doctor => new SelectListItem
            {
                Value = doctor.Id.ToString(),
                Text = $"{doctor.Name} {doctor.Surname} ({doctor.Id})",
                Selected = doctor.Id == selectedDepartmentId
            })
            .ToArrayAsync();
    }

    private async Task ValidateProfessorModelAsync(
        ApplicationContext context,
        ProfessorModel model,
        int? currentId = null)
    {
        ModelState.Remove(nameof(model.DoctorName));

        var doctor = await context.Doctors.FirstOrDefaultAsync(doctor =>
            doctor.Id == model.DoctorId);

        if (doctor == null)
        {
            ModelState.AddModelError(nameof(model.DoctorId), "Доктор не найден.");
        }
        else
        {
            var isAlreadyProfessor = await context.Professors.AnyAsync(professor =>
                (!currentId.HasValue || professor.Id != currentId.Value) &&
                professor.DoctorId == doctor.Id);

            if (isAlreadyProfessor)
                ModelState.AddModelError(nameof(model.DoctorId), "Доктор уже является профессором.");
        }
    }
}