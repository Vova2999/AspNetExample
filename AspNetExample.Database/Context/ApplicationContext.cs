using AspNetExample.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AspNetExample.Database.Context;

public class ApplicationContext : DbContext
{
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Disease> Diseases => Set<Disease>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<DoctorExamination> DoctorsExaminations => Set<DoctorExamination>();
    public DbSet<Examination> Examinations => Set<Examination>();
    public DbSet<Intern> Interns => Set<Intern>();
    public DbSet<Professor> Professors => Set<Professor>();
    public DbSet<Ward> Wards => Set<Ward>();

    private readonly ILogger<ApplicationContext>? _logger;

    public ApplicationContext(ILogger<ApplicationContext>? logger = null)
    {
        _logger = logger;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Hospital;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");
        optionsBuilder.LogTo(LogMessage, LogLevel.Information);
    }

    private void LogMessage(string message)
    {
        _logger?.LogInformation(message);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region Department

        modelBuilder.Entity<Department>()
            .Property(x => x.Financing)
            .HasColumnType("money")
            .HasDefaultValue(0);

        modelBuilder.Entity<Department>()
            .Property(x => x.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<Department>()
            .HasIndex(x => x.Name)
            .IsUnique();

        modelBuilder.Entity<Department>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Departments_Building",
                "Building BETWEEN 1 AND 5"));

        modelBuilder.Entity<Department>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Departments_Financing",
                "Financing >= 0"));

        modelBuilder.Entity<Department>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Departments_Name",
                "LEN(Name) > 0"));

        #endregion

        #region Disease

        modelBuilder.Entity<Disease>()
            .Property(x => x.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<Disease>()
            .HasIndex(x => x.Name)
            .IsUnique();

        modelBuilder.Entity<Disease>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Diseases_Name",
                "LEN(Name) > 0"));

        #endregion

        #region Doctor

        modelBuilder.Entity<Doctor>()
            .Property(x => x.Salary)
            .HasColumnType("money");

        modelBuilder.Entity<Doctor>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Doctors_Name",
                "LEN(Name) > 0"));

        modelBuilder.Entity<Doctor>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Doctors_Salary",
                "Salary > 0"));

        modelBuilder.Entity<Doctor>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Doctors_Surname",
                "LEN(Surname) > 0"));

        #endregion

        #region DoctorExamination

        modelBuilder.Entity<DoctorExamination>()
            .HasOne(x => x.Disease)
            .WithMany(x => x.DoctorsExaminations)
            .HasForeignKey(x => x.DiseaseId);

        modelBuilder.Entity<DoctorExamination>()
            .HasOne(x => x.Doctor)
            .WithMany(x => x.DoctorsExaminations)
            .HasForeignKey(x => x.DoctorId);

        modelBuilder.Entity<DoctorExamination>()
            .HasOne(x => x.Examination)
            .WithMany(x => x.DoctorsExaminations)
            .HasForeignKey(x => x.ExaminationId);

        modelBuilder.Entity<DoctorExamination>()
            .HasOne(x => x.Ward)
            .WithMany(x => x.DoctorsExaminations)
            .HasForeignKey(x => x.WardId);

        modelBuilder.Entity<DoctorExamination>()
            .Property(x => x.Date)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<DoctorExamination>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_DoctorsExaminations_Date",
                "Date <= GETDATE()"));

        #endregion

        #region Examination

        modelBuilder.Entity<Examination>()
            .Property(x => x.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<Examination>()
            .HasIndex(x => x.Name)
            .IsUnique();

        modelBuilder.Entity<Examination>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Examinations_Name",
                "LEN(Name) > 0"));

        #endregion

        #region Intern

        modelBuilder.Entity<Intern>()
            .HasOne(x => x.Doctor)
            .WithMany(x => x.Interns)
            .HasForeignKey(x => x.DoctorId);

        #endregion

        #region Professor

        modelBuilder.Entity<Professor>()
            .HasOne(x => x.Doctor)
            .WithMany(x => x.Professors)
            .HasForeignKey(x => x.DoctorId);

        #endregion

        #region Ward

        modelBuilder.Entity<Ward>()
            .HasOne(x => x.Department)
            .WithMany(x => x.Wards)
            .HasForeignKey(x => x.DepartmentId);

        modelBuilder.Entity<Ward>()
            .Property(x => x.Name)
            .HasMaxLength(20);

        modelBuilder.Entity<Ward>()
            .HasIndex(x => x.Name)
            .IsUnique();

        modelBuilder.Entity<Ward>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Wards_Name",
                "LEN(Name) > 0"));

        modelBuilder.Entity<Ward>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Wards_Places",
                "Places > 0"));

        #endregion
    }
}