using EmployeeManager.Api.Middleware;
using EmployeeManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using EmployeeManager.Application.Employees;
using EmployeeManager.Application.Employees.Ports;
using EmployeeManager.Infrastructure.Employees;
using System.Text;
using EmployeeManager.Api.Security;
using EmployeeManager.Application.Security;
using EmployeeManager.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using EmployeeManager.Application.Auth;
using EmployeeManager.Api.Seed;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "EmployeeManager API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Cole somente o token (sem 'Bearer ') ou cole no formato: Bearer {seu_token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<CreateEmployee>();

builder.Services.AddScoped<GetEmployee>();
builder.Services.AddScoped<ListEmployees>();

builder.Services.AddScoped<UpdateEmployee>();
builder.Services.AddScoped<DeleteEmployee>();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<IPasswordHasher, PasswordHasherAdapter>();
builder.Services.AddScoped<IAccessTokenGenerator, JwtTokenGenerator>();

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddScoped<Login>();

builder.Services.AddAuthorization();

// EF Core + Postgres
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("Default");
    opt.UseNpgsql(cs);
});

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("frontend", p =>
    p.WithOrigins("http://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod());
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await DirectorSeeder.SeedAsync(app);

app.Run();