using Microsoft.EntityFrameworkCore;

namespace EmployeeManager.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


// Depois a gente coloca:
// public DbSet<Employee> Employees => Set<Employee>();
}