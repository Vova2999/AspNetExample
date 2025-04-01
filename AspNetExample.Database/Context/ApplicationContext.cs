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
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Ward> Wards => Set<Ward>();

    private readonly ILogger<ApplicationContext>? _logger;

    public ApplicationContext(
        DbContextOptions<ApplicationContext> dbContextOptions,
        ILogger<ApplicationContext>? logger = null)
        : base(dbContextOptions)
    {
        _logger = logger;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

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
            .HasColumnType("decimal(18,2)")
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
                "\"Building\" BETWEEN 1 AND 5"));

        modelBuilder.Entity<Department>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Departments_Financing",
                "\"Financing\" >= 0"));

        modelBuilder.Entity<Department>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Departments_Name",
                "LENGTH(\"Name\") > 0"));

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
                "LENGTH(\"Name\") > 0"));

        #endregion

        #region Doctor

        modelBuilder.Entity<Doctor>()
            .Property(x => x.Salary)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Doctor>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Doctors_Name",
                "LENGTH(\"Name\") > 0"));

        modelBuilder.Entity<Doctor>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Doctors_Salary",
                "\"Salary\" > 0"));

        modelBuilder.Entity<Doctor>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Doctors_Surname",
                "LENGTH(\"Surname\") > 0"));

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
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<DoctorExamination>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_DoctorsExaminations_Date",
                "\"Date\" <= CURRENT_TIMESTAMP"));

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
                "LENGTH(\"Name\") > 0"));

        #endregion

        #region Intern

        modelBuilder.Entity<Intern>()
            .HasOne(x => x.Doctor)
            .WithMany(x => x.Interns)
            .HasForeignKey(x => x.DoctorId);

        modelBuilder.Entity<Intern>()
            .HasIndex(x => x.DoctorId)
            .IsUnique();

        #endregion

        #region Professor

        modelBuilder.Entity<Professor>()
            .HasOne(x => x.Doctor)
            .WithMany(x => x.Professors)
            .HasForeignKey(x => x.DoctorId);

        modelBuilder.Entity<Professor>()
            .HasIndex(x => x.DoctorId)
            .IsUnique();

        #endregion

        #region Role

        modelBuilder.Entity<Role>()
            .HasIndex(x => x.NormalizedName)
            .IsUnique();

        #endregion

        #region User

        modelBuilder.Entity<User>()
            .HasIndex(x => x.NormalizedName)
            .IsUnique();

        #endregion

        #region UserRole

        modelBuilder.Entity<UserRole>()
            .HasKey(x => new { x.UserId, x.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasIndex(x => x.UserId);

        modelBuilder.Entity<UserRole>()
            .HasIndex(x => x.RoleId);

        modelBuilder.Entity<UserRole>()
            .HasOne(x => x.User)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(x => x.Role)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.RoleId);

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
                "LENGTH(\"Name\") > 0"));

        modelBuilder.Entity<Ward>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Wards_Places",
                "\"Places\" > 0"));

        #endregion
    }
}