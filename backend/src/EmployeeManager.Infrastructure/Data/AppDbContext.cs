using EmployeeManager.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManager.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<EmployeeEntity> Employees => Set<EmployeeEntity>();
    public DbSet<EmployeePhoneEntity> EmployeePhones => Set<EmployeePhoneEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmployeeEntity>(b =>
        {
            b.ToTable("employees");
            b.HasKey(x => x.Id);

            b.Property(x => x.FirstName).HasColumnName("first_name").IsRequired();
            b.Property(x => x.LastName).HasColumnName("last_name").IsRequired();
            b.Property(x => x.Email).HasColumnName("email").IsRequired();
            b.Property(x => x.DocNumber).HasColumnName("doc_number").IsRequired();
            b.HasIndex(x => x.DocNumber).IsUnique(); // doc number unique

            b.Property(x => x.BirthDate).HasColumnName("birth_date").IsRequired();
            b.Property(x => x.Role).HasColumnName("role").IsRequired();

            b.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

            b.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

            b.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

            b.Property(x => x.DeactivatedAt)
            .HasColumnName("deactivated_at");

            b.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();

            b.Property(x => x.ManagerEmployeeId).HasColumnName("manager_employee_id");
            b.Property(x => x.ManagerName).HasColumnName("manager_name");

            b.HasMany(x => x.Phones)
            .WithOne(p => p.Employee)
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EmployeePhoneEntity>(b =>
        {
            b.ToTable("employee_phones");
            b.HasKey(x => x.Id);

            b.Property(x => x.EmployeeId).HasColumnName("employee_id").IsRequired();
            b.Property(x => x.Number).HasColumnName("number").IsRequired();
            b.Property(x => x.Type).HasColumnName("type");
        });
    }
}