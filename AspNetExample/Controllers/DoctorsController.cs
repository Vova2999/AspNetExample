using AspNetExample.Common.Extensions;
using AspNetExample.Database.Context;
using AspNetExample.Database.Context.Factory;
using AspNetExample.Domain.Entities;
using AspNetExample.Extensions.Models;
using AspNetExample.Models.Doctors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetExample.Controllers;

public class DoctorsController : Controller
{
    private readonly IApplicationContextFactory _applicationContextFactory;
    private readonly ILogger<DoctorsController> _logger;

    public DoctorsController(
        IApplicationContextFactory applicationContextFactory,
        ILogger<DoctorsController> logger)
    {
        _applicationContextFactory = applicationContextFactory;
        _logger = logger;
    }

    public async Task<IActionResult> Index([FromQuery] DoctorsIndexModel? model)
    {
        await using var context = _applicationContextFactory.Create();

        var doctorsQuery = context.Doctors
            .AsNoTracking();

        var names = model?.Names?.Split(';');
        var salaryFrom = model?.SalaryFrom;
        var salaryTo = model?.SalaryTo;
        var surnames = model?.Surnames?.Split(';');

        if (names?.Any() == true)
            doctorsQuery = doctorsQuery.Where(doctor => names.Contains(doctor.Name));
        if (salaryFrom != null)
            doctorsQuery = doctorsQuery.Where(doctor => doctor.Salary >= salaryFrom);
        if (salaryTo != null)
            doctorsQuery = doctorsQuery.Where(doctor => doctor.Salary <= salaryTo);
        if (surnames?.Any() == true)
            doctorsQuery = doctorsQuery.Where(doctor => surnames.Contains(doctor.Surname));

        doctorsQuery = model?.SortBy switch
        {
            nameof(DoctorModel.Id) => doctorsQuery.OrderBy(doctor => doctor.Id),
            nameof(DoctorModel.Id) + Constants.DescSuffix => doctorsQuery.OrderByDescending(doctor => doctor.Id),
            nameof(DoctorModel.Name) => doctorsQuery.OrderBy(doctor => doctor.Name),
            nameof(DoctorModel.Name) + Constants.DescSuffix => doctorsQuery.OrderByDescending(doctor => doctor.Name),
            nameof(DoctorModel.Salary) => doctorsQuery.OrderBy(doctor => doctor.Salary),
            nameof(DoctorModel.Salary) + Constants.DescSuffix => doctorsQuery.OrderByDescending(doctor => doctor.Salary),
            nameof(DoctorModel.Surname) => doctorsQuery.OrderBy(doctor => doctor.Surname),
            nameof(DoctorModel.Surname) + Constants.DescSuffix => doctorsQuery.OrderByDescending(doctor => doctor.Surname),
            _ => doctorsQuery.OrderBy(doctor => doctor.Id)
        };

        var page = Math.Max(Constants.FirstPage, model?.Page ?? Constants.FirstPage);
        var totalCount = doctorsQuery.Count();
        var doctors = await doctorsQuery
            .Skip((page - Constants.FirstPage) * Constants.PageSize)
            .Take(Constants.PageSize)
            .Select(doctor => doctor.ToModel())
            .ToArrayAsync();

        return View(new DoctorsIndexModel
        {
            Doctors = doctors,
            SortBy = model?.SortBy,
            Page = page,
            TotalCount = totalCount
        });
    }

    public IActionResult Create()
    {
        return View(new DoctorModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] DoctorModel model)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateDoctorModelAsync(context, model);

        if (!ModelState.IsValid)
            return View(model);

        var doctor = new Doctor
        {
            Name = model.Name,
            Salary = model.Salary,
            Surname = model.Surname
        };

        context.Doctors.Add(doctor);

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("[controller]/[action]/{id:int}")]
    public async Task<IActionResult> Edit([FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();

        var doctor = await context.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(doctor => doctor.Id == id);

        if (doctor == null)
            return NotFound();

        return View(doctor.ToModel());
    }

    [HttpPost("[controller]/[action]/{id:int}")]
    public async Task<IActionResult> Edit([FromRoute] int id, [FromForm] DoctorModel model)
    {
        await using var context = _applicationContextFactory.Create();

        await ValidateDoctorModelAsync(context, model, id);

        if (!ModelState.IsValid)
            return View(model);

        var doctor = await context.Doctors
            .FirstOrDefaultAsync(doctor => doctor.Id == id);

        if (doctor == null)
            return NotFound();

        doctor.Name = model.Name;
        doctor.Salary = model.Salary;
        doctor.Surname = model.Surname;

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("[controller]/[action]/{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        await using var context = _applicationContextFactory.Create();
        var doctor = await context.Doctors
            .FirstOrDefaultAsync(doctor => doctor.Id == id);

        if (doctor == null)
            return NotFound();

        context.Doctors.Remove(doctor);

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateDoctorModelAsync(
        ApplicationContext context,
        DoctorModel model,
        int? currentId = null)
    {
        if (model.Salary <= 0)
            ModelState.AddModelError(nameof(model.Salary), "Зарплата должна должно быть больше 0.");

        if (model.Name.IsSignificant())
        {
            var hasConflictedName = await context.Doctors.AnyAsync(doctor =>
                (!currentId.HasValue || doctor.Id != currentId.Value) &&
                EF.Functions.Like(model.Name, doctor.Name) &&
                EF.Functions.Like(model.Surname, doctor.Surname));

            if (hasConflictedName)
            {
                ModelState.AddModelError(nameof(model.Name), "Имя и фамилия должны быть уникальными.");
                ModelState.AddModelError(nameof(model.Surname), "Имя и фамилия должны быть уникальными.");
            }
        }
    }
}