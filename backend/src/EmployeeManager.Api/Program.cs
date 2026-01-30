using EmployeeManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using EmployeeManager.Application.Employees;
using EmployeeManager.Application.Employees.Ports;
using EmployeeManager.Infrastructure.Employees;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<CreateEmployee>();

builder.Services.AddScoped<GetEmployee>();
builder.Services.AddScoped<ListEmployees>();

builder.Services.AddScoped<UpdateEmployee>();
builder.Services.AddScoped<DeleteEmployee>();

// EF Core + Postgres
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("Default");
    opt.UseNpgsql(cs);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();