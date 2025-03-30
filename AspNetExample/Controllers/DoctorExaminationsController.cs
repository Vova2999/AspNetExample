using AspNetExample.Common.Extensions;
using AspNetExample.Database;
using AspNetExample.Database.Context;
using AspNetExample.Database.Context.Factory;
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

    public async Task<IActionResult> Index([FromQuery] DoctorExaminationsIndexModel? model)
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
        if (diseaseNames?.Any() == true)
            doctorExaminationsQuery = doctorExaminationsQuery.Where(doctorExamination => diseaseNames.Contains(doctorExamination.Disease.Name));
        if (doctorNames?.Any() == true)
            doctorExaminationsQuery = doctorExaminationsQuery.Where(doctorExamination => doctorNames.Contains(doctorExamination.Doctor.Name));
        if (examinationNames?.Any() == true)
            doctorExaminationsQuery = doctorExaminationsQuery.Where(doctorExamination => examinationNames.Contains(doctorExamination.Examination.Name));
        if (wardNames?.Any() == true)
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
        var totalCount = doctorExaminationsQuery.Count();
        var doctorExaminations = await doctorExaminationsQuery
            .Skip((page - Constants.FirstPage) * Constants.PageSize)
            .Take(Constants.PageSize)
            .Select(doctorExamination => doctorExamination.ToModel())
            .ToArrayAsync();

        return View(new DoctorExaminationsIndexModel
        {
            DoctorExaminations = doctorExaminations,
            SortBy = model?.SortBy,
            Page = page,
            TotalCount = totalCount
        });
    }

    [HttpGet("[controller]/[action]/{id:int}")]
    public async Task<IActionResult> Details([FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();

        var doctorExamination = await context.DoctorsExaminations
            .Include(doctorExamination => doctorExamination.Disease)
            .Include(doctorExamination => doctorExamination.Doctor)
            .Include(doctorExamination => doctorExamination.Examination)
            .Include(doctorExamination => doctorExamination.Ward)
            .ThenInclude(ward => ward.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(doctorExamination => doctorExamination.Id == id);

        if (doctorExamination == null)
            return NotFound();

        return View(doctorExamination.ToDetailsModel());
    }

    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Create()
    {
        await using var context = _applicationContextFactory.Create();

        ViewBag.Diseases = await GetDiseasesAsync(context);
        ViewBag.Doctors = await GetDoctorsAsync(context);
        ViewBag.Examinations = await GetExaminationsAsync(context);
        ViewBag.Wards = await GetWardsAsync(context);
        return View(new DoctorExaminationModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Create([FromForm] DoctorExaminationModel model)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateDoctorExaminationModelAsync(context, model);

        if (!ModelState.IsValid)
        {
            ViewBag.Diseases = await GetDiseasesAsync(context);
            ViewBag.Doctors = await GetDoctorsAsync(context);
            ViewBag.Examinations = await GetExaminationsAsync(context);
            ViewBag.Wards = await GetWardsAsync(context);
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

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("[controller]/[action]/{id:int}")]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Edit([FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();

        var doctorExamination = await context.DoctorsExaminations
            .Include(doctorExamination => doctorExamination.Disease)
            .Include(doctorExamination => doctorExamination.Doctor)
            .Include(doctorExamination => doctorExamination.Examination)
            .Include(doctorExamination => doctorExamination.Ward)
            .AsNoTracking()
            .FirstOrDefaultAsync(doctorExamination => doctorExamination.Id == id);

        if (doctorExamination == null)
            return NotFound();

        ViewBag.Diseases = await GetDiseasesAsync(context, doctorExamination.DiseaseId);
        ViewBag.Doctors = await GetDoctorsAsync(context, doctorExamination.DoctorId);
        ViewBag.Examinations = await GetExaminationsAsync(context, doctorExamination.ExaminationId);
        ViewBag.Wards = await GetWardsAsync(context, doctorExamination.WardId);
        return View(doctorExamination.ToModel());
    }

    [HttpPost("[controller]/[action]/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Edit([FromRoute] int id, [FromForm] DoctorExaminationModel model)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateDoctorExaminationModelAsync(context, model);

        if (!ModelState.IsValid)
        {
            ViewBag.Diseases = await GetDiseasesAsync(context, model.DiseaseId);
            ViewBag.Doctors = await GetDoctorsAsync(context, model.DoctorId);
            ViewBag.Examinations = await GetExaminationsAsync(context, model.ExaminationId);
            ViewBag.Wards = await GetWardsAsync(context, model.WardId);
            return View(model);
        }

        var doctorExamination = await context.DoctorsExaminations
            .FirstOrDefaultAsync(doctorExamination => doctorExamination.Id == id);

        if (doctorExamination == null)
            return NotFound();

        doctorExamination.Date = model.Date.ToDateOnly();
        doctorExamination.DiseaseId = model.DiseaseId;
        doctorExamination.DoctorId = model.DoctorId;
        doctorExamination.ExaminationId = model.ExaminationId;
        doctorExamination.WardId = model.WardId;

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("[controller]/[action]/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleTokens.AdminRole)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();
        var doctorExamination = await context.DoctorsExaminations
            .FirstOrDefaultAsync(doctorExamination => doctorExamination.Id == id);

        if (doctorExamination == null)
            return NotFound();

        context.DoctorsExaminations.Remove(doctorExamination);

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private static async Task<SelectListItem[]> GetDiseasesAsync(
        ApplicationContext context,
        int? selectedDiseaseId = null)
    {
        return await context.Diseases
            .OrderBy(e => e.Id)
            .Select(disease => new SelectListItem
            {
                Value = disease.Id.ToString(),
                Text = $"{disease.Name} ({disease.Id})",
                Selected = disease.Id == selectedDiseaseId
            })
            .ToArrayAsync();
    }

    private static async Task<SelectListItem[]> GetDoctorsAsync(
        ApplicationContext context,
        int? selectedDoctorId = null)
    {
        return await context.Doctors
            .OrderBy(e => e.Id)
            .Select(doctor => new SelectListItem
            {
                Value = doctor.Id.ToString(),
                Text = $"{doctor.Name} ({doctor.Id})",
                Selected = doctor.Id == selectedDoctorId
            })
            .ToArrayAsync();
    }

    private static async Task<SelectListItem[]> GetExaminationsAsync(
        ApplicationContext context,
        int? selectedExaminationId = null)
    {
        return await context.Examinations
            .OrderBy(e => e.Id)
            .Select(examination => new SelectListItem
            {
                Value = examination.Id.ToString(),
                Text = $"{examination.Name} ({examination.Id})",
                Selected = examination.Id == selectedExaminationId
            })
            .ToArrayAsync();
    }

    private static async Task<SelectListItem[]> GetWardsAsync(
        ApplicationContext context,
        int? selectedWardId = null)
    {
        return await context.Wards
            .OrderBy(e => e.Id)
            .Select(ward => new SelectListItem
            {
                Value = ward.Id.ToString(),
                Text = $"{ward.Name} ({ward.Id})",
                Selected = ward.Id == selectedWardId
            })
            .ToArrayAsync();
    }

    private async Task ValidateDoctorExaminationModelAsync(
        ApplicationContext context,
        DoctorExaminationModel model)
    {
        ModelState.Remove(nameof(model.DiseaseName));
        ModelState.Remove(nameof(model.DoctorName));
        ModelState.Remove(nameof(model.ExaminationName));
        ModelState.Remove(nameof(model.WardName));

        if (model.Date > DateTime.Now)
            ModelState.AddModelError(nameof(model.Date), "Дата должна быть не больше текущей.");

        var isDiseaseExists = await context.Diseases.AnyAsync(disease =>
            disease.Id == model.DiseaseId);

        if (!isDiseaseExists)
            ModelState.AddModelError(nameof(model.DiseaseId), "Болезнь не найдена.");

        var isDoctorExists = await context.Doctors.AnyAsync(doctor =>
            doctor.Id == model.DoctorId);

        if (!isDoctorExists)
            ModelState.AddModelError(nameof(model.DoctorId), "Доктор не найден.");

        var isExaminationExists = await context.Examinations.AnyAsync(examination =>
            examination.Id == model.ExaminationId);

        if (!isExaminationExists)
            ModelState.AddModelError(nameof(model.ExaminationId), "Обследование не найдено.");

        var isWardExists = await context.Wards.AnyAsync(ward =>
            ward.Id == model.WardId);

        if (!isWardExists)
            ModelState.AddModelError(nameof(model.WardId), "Палата не найдена.");
    }
}