using AspNetExample.Common.Extensions;
using AspNetExample.Database.Context;
using AspNetExample.Database.Context.Factory;
using AspNetExample.Domain;
using AspNetExample.Domain.Entities;
using AspNetExample.Extensions.Models;
using AspNetExample.Models.DoctorExaminations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AspNetExample.Controllers;

[Authorize]
public class DoctorExaminationsController : Controller
{
    private readonly IApplicationContextFactory _applicationContextFactory;
    private readonly ILogger<DoctorExaminationsController> _logger;

    public DoctorExaminationsController(
        IApplicationContextFactory applicationContextFactory,
        ILogger<DoctorExaminationsController> logger)
    {
        _applicationContextFactory = applicationContextFactory;
        _logger = logger;
    }

    public async Task<IActionResult> Index(
        [FromQuery] DoctorExaminationsIndexModel? model,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        var doctorExaminationsQuery = context.DoctorsExaminations
            .Include(doctorExamination => doctorExamination.Disease)
            .Include(doctorExamination => doctorExamination.Doctor)
            .Include(doctorExamination => doctorExamination.Examination)
            .Include(doctorExamination => doctorExamination.Ward)
            .AsNoTracking();

        var dateFrom = model?.DateFrom?.ToDateOnly();
        var dateTo = model?.DateTo?.ToDateOnly();
        var diseaseNames = model?.DiseaseNames?.Split(";");
        var doctorNames = model?.DoctorNames?.Split(";");
        var examinationNames = model?.ExaminationNames?.Split(";");
        var wardNames = model?.WardNames?.Split(";");

        if (dateFrom != null)
            doctorExaminationsQuery = doctorExaminationsQuery.Where(doctorExamination => doctorExamination.Date >= dateFrom);
        if (dateTo != null)
            doctorExaminationsQuery = doctorExaminationsQuery.Where(doctorExamination => doctorExamination.Date <= dateTo);
        if (diseaseNames.IsSignificant())
            doctorExaminationsQuery = doctorExaminationsQuery.Where(doctorExamination => diseaseNames.Contains(doctorExamination.Disease.Name));
        if (doctorNames.IsSignificant())
            doctorExaminationsQuery = doctorExaminationsQuery.Where(doctorExamination => doctorNames.Contains(doctorExamination.Doctor.Name));
        if (examinationNames.IsSignificant())
            doctorExaminationsQuery = doctorExaminationsQuery.Where(doctorExamination => examinationNames.Contains(doctorExamination.Examination.Name));
        if (wardNames.IsSignificant())
            doctorExaminationsQuery = doctorExaminationsQuery.Where(doctorExamination => wardNames.Contains(doctorExamination.Ward.Name));

        doctorExaminationsQuery = model?.SortBy switch
        {
            nameof(DoctorExaminationModel.Id) => doctorExaminationsQuery.OrderBy(doctorExamination => doctorExamination.Id),
            nameof(DoctorExaminationModel.Id) + Constants.DescSuffix => doctorExaminationsQuery.OrderByDescending(doctorExamination => doctorExamination.Id),
            nameof(DoctorExaminationModel.Date) => doctorExaminationsQuery.OrderBy(doctorExamination => doctorExamination.Date),
            nameof(DoctorExaminationModel.Date) + Constants.DescSuffix => doctorExaminationsQuery.OrderByDescending(doctorExamination => doctorExamination.Date),
            nameof(DoctorExaminationModel.DiseaseName) => doctorExaminationsQuery.OrderBy(doctorExamination => doctorExamination.Disease.Name),
            nameof(DoctorExaminationModel.DiseaseName) + Constants.DescSuffix => doctorExaminationsQuery.OrderByDescending(doctorExamination => doctorExamination.Disease.Name),
            nameof(DoctorExaminationModel.DoctorName) => doctorExaminationsQuery.OrderBy(doctorExamination => doctorExamination.Doctor.Name),
            nameof(DoctorExaminationModel.DoctorName) + Constants.DescSuffix => doctorExaminationsQuery.OrderByDescending(doctorExamination => doctorExamination.Doctor.Name),
            nameof(DoctorExaminationModel.ExaminationName) => doctorExaminationsQuery.OrderBy(doctorExamination => doctorExamination.Examination.Name),
            nameof(DoctorExaminationModel.ExaminationName) + Constants.DescSuffix => doctorExaminationsQuery.OrderByDescending(doctorExamination => doctorExamination.Examination.Name),
            nameof(DoctorExaminationModel.WardName) => doctorExaminationsQuery.OrderBy(doctorExamination => doctorExamination.Ward.Name),
            nameof(DoctorExaminationModel.WardName) + Constants.DescSuffix => doctorExaminationsQuery.OrderByDescending(doctorExamination => doctorExamination.Ward.Name),
            _ => doctorExaminationsQuery.OrderBy(ward => ward.Id)
        };

        var page = Math.Max(Constants.FirstPage, model?.Page ?? Constants.FirstPage);
        var totalCount = await doctorExaminationsQuery.CountAsync(cancellationToken);
        var doctorExaminations = await doctorExaminationsQuery
            .Skip((page - Constants.FirstPage) * Constants.PageSize)
            .Take(Constants.PageSize)
            .Select(doctorExamination => doctorExamination.ToModel())
            .ToArrayAsync(cancellationToken);

        return View(new DoctorExaminationsIndexModel
        {
            DoctorExaminations = doctorExaminations,
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

        var doctorExamination = await context.DoctorsExaminations
            .Include(doctorExamination => doctorExamination.Disease)
            .Include(doctorExamination => doctorExamination.Doctor)
            .Include(doctorExamination => doctorExamination.Examination)
            .Include(doctorExamination => doctorExamination.Ward)
            .ThenInclude(ward => ward.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(doctorExamination => doctorExamination.Id == id, cancellationToken);

        if (doctorExamination == null)
            return NotFound();

        return View(doctorExamination.ToDetailsModel());
    }

    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        ViewBag.Diseases = await GetDiseasesAsync(context, null, cancellationToken);
        ViewBag.Doctors = await GetDoctorsAsync(context, null, cancellationToken);
        ViewBag.Examinations = await GetExaminationsAsync(context, null, cancellationToken);
        ViewBag.Wards = await GetWardsAsync(context, null, cancellationToken);
        return View(new DoctorExaminationModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Create(
        [FromForm] DoctorExaminationModel model,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateDoctorExaminationModelAsync(context, model, cancellationToken);

        if (!ModelState.IsValid)
        {
            ViewBag.Diseases = await GetDiseasesAsync(context, null, cancellationToken);
            ViewBag.Doctors = await GetDoctorsAsync(context, null, cancellationToken);
            ViewBag.Examinations = await GetExaminationsAsync(context, null, cancellationToken);
            ViewBag.Wards = await GetWardsAsync(context, null, cancellationToken);
            return View(model);
        }

        var doctorExamination = new DoctorExamination
        {
            Date = model.Date.ToDateOnly(),
            DiseaseId = model.DiseaseId,
            DoctorId = model.DoctorId,
            ExaminationId = model.ExaminationId,
            WardId = model.WardId
        };

        context.DoctorsExaminations.Add(doctorExamination);

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

        var doctorExamination = await context.DoctorsExaminations
            .Include(doctorExamination => doctorExamination.Disease)
            .Include(doctorExamination => doctorExamination.Doctor)
            .Include(doctorExamination => doctorExamination.Examination)
            .Include(doctorExamination => doctorExamination.Ward)
            .AsNoTracking()
            .FirstOrDefaultAsync(doctorExamination => doctorExamination.Id == id, cancellationToken);

        if (doctorExamination == null)
            return NotFound();

        ViewBag.Diseases = await GetDiseasesAsync(context, doctorExamination.DiseaseId, cancellationToken);
        ViewBag.Doctors = await GetDoctorsAsync(context, doctorExamination.DoctorId, cancellationToken);
        ViewBag.Examinations = await GetExaminationsAsync(context, doctorExamination.ExaminationId, cancellationToken);
        ViewBag.Wards = await GetWardsAsync(context, doctorExamination.WardId, cancellationToken);
        return View(doctorExamination.ToModel());
    }

    [HttpPost("[controller]/[action]/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Edit(
        [FromRoute] int id,
        [FromForm] DoctorExaminationModel model,
        CancellationToken cancellationToken)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateDoctorExaminationModelAsync(context, model, cancellationToken);

        if (!ModelState.IsValid)
        {
            ViewBag.Diseases = await GetDiseasesAsync(context, model.DiseaseId, cancellationToken);
            ViewBag.Doctors = await GetDoctorsAsync(context, model.DoctorId, cancellationToken);
            ViewBag.Examinations = await GetExaminationsAsync(context, model.ExaminationId, cancellationToken);
            ViewBag.Wards = await GetWardsAsync(context, model.WardId, cancellationToken);
            return View(model);
        }

        var doctorExamination = await context.DoctorsExaminations
            .FirstOrDefaultAsync(doctorExamination => doctorExamination.Id == id, cancellationToken);

        if (doctorExamination == null)
            return NotFound();

        doctorExamination.Date = model.Date.ToDateOnly();
        doctorExamination.DiseaseId = model.DiseaseId;
        doctorExamination.DoctorId = model.DoctorId;
        doctorExamination.ExaminationId = model.ExaminationId;
        doctorExamination.WardId = model.WardId;

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
        var doctorExamination = await context.DoctorsExaminations
            .FirstOrDefaultAsync(doctorExamination => doctorExamination.Id == id, cancellationToken);

        if (doctorExamination == null)
            return NotFound();

        context.DoctorsExaminations.Remove(doctorExamination);

        await context.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    private static async Task<SelectListItem[]> GetDiseasesAsync(
        ApplicationContext context,
        int? selectedDiseaseId,
        CancellationToken cancellationToken)
    {
        return await context.Diseases
            .OrderBy(e => e.Id)
            .Select(disease => new SelectListItem
            {
                Value = disease.Id.ToString(),
                Text = $"{disease.Name} ({disease.Id})",
                Selected = disease.Id == selectedDiseaseId
            })
            .ToArrayAsync(cancellationToken);
    }

    private static async Task<SelectListItem[]> GetDoctorsAsync(
        ApplicationContext context,
        int? selectedDoctorId,
        CancellationToken cancellationToken)
    {
        return await context.Doctors
            .OrderBy(e => e.Id)
            .Select(doctor => new SelectListItem
            {
                Value = doctor.Id.ToString(),
                Text = $"{doctor.Name} ({doctor.Id})",
                Selected = doctor.Id == selectedDoctorId
            })
            .ToArrayAsync(cancellationToken);
    }

    private static async Task<SelectListItem[]> GetExaminationsAsync(
        ApplicationContext context,
        int? selectedExaminationId,
        CancellationToken cancellationToken)
    {
        return await context.Examinations
            .OrderBy(e => e.Id)
            .Select(examination => new SelectListItem
            {
                Value = examination.Id.ToString(),
                Text = $"{examination.Name} ({examination.Id})",
                Selected = examination.Id == selectedExaminationId
            })
            .ToArrayAsync(cancellationToken);
    }

    private static async Task<SelectListItem[]> GetWardsAsync(
        ApplicationContext context,
        int? selectedWardId,
        CancellationToken cancellationToken)
    {
        return await context.Wards
            .OrderBy(e => e.Id)
            .Select(ward => new SelectListItem
            {
                Value = ward.Id.ToString(),
                Text = $"{ward.Name} ({ward.Id})",
                Selected = ward.Id == selectedWardId
            })
            .ToArrayAsync(cancellationToken);
    }

    private async Task ValidateDoctorExaminationModelAsync(
        ApplicationContext context,
        DoctorExaminationModel model,
        CancellationToken cancellationToken)
    {
        ModelState.Remove(nameof(model.DiseaseName));
        ModelState.Remove(nameof(model.DoctorName));
        ModelState.Remove(nameof(model.ExaminationName));
        ModelState.Remove(nameof(model.WardName));

        if (model.Date > DateTime.Now)
            ModelState.AddModelError(nameof(model.Date), "Дата должна быть не больше текущей.");

        var isDiseaseExists = await context.Diseases.AnyAsync(disease =>
                disease.Id == model.DiseaseId,
            cancellationToken);

        if (!isDiseaseExists)
            ModelState.AddModelError(nameof(model.DiseaseId), "Болезнь не найдена.");

        var isDoctorExists = await context.Doctors.AnyAsync(doctor =>
                doctor.Id == model.DoctorId,
            cancellationToken);

        if (!isDoctorExists)
            ModelState.AddModelError(nameof(model.DoctorId), "Доктор не найден.");

        var isExaminationExists = await context.Examinations.AnyAsync(examination =>
                examination.Id == model.ExaminationId,
            cancellationToken);

        if (!isExaminationExists)
            ModelState.AddModelError(nameof(model.ExaminationId), "Обследование не найдено.");

        var isWardExists = await context.Wards.AnyAsync(ward =>
                ward.Id == model.WardId,
            cancellationToken);

        if (!isWardExists)
            ModelState.AddModelError(nameof(model.WardId), "Палата не найдена.");
    }
}