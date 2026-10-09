using System.Text.Json;
using Application.Catalogs.Commands;
using Application.Catalogs.Validators;
using Domain.Catalogs.Interfaces;
using FluentValidation;
using Infrastructure.Catalogs.Auditing;
using Infrastructure.Catalogs.Persistence;
using Infrastructure.Catalogs.Repositories;
using Microsoft.EntityFrameworkCore;
using Presentation.Common;
using Presentation.Catalogs.Middleware;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddAuthorization();

        builder.Services.AddDbContext<CatalogDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogDatabase")));

        builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<CreateCompanyCommand>());
        builder.Services.AddValidatorsFromAssemblyContaining<CreateCompanyCommandValidator>();

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
        builder.Services.AddScoped<IFormatRepository, FormatRepository>();
        builder.Services.AddScoped<IDisciplineRepository, DisciplineRepository>();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        builder.Services.AddScoped<CatalogChangeAuditedHandler>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<CatalogExceptionMiddleware>();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
