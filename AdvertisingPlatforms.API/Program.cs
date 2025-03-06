using System.Reflection;
using AdvertisingPlatforms.API.Interfaces;
using AdvertisingPlatforms.API.Utils;
using AdvertisingPlatforms.API.Validation;
using AdvertisingPlatforms.Application.Interfaces;
using AdvertisingPlatforms.Application.Services;
using AdvertisingPlatforms.Domain.Interfaces;
using AdvertisingPlatforms.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Контроллеры
builder.Services.AddControllers();

builder.Services.AddValidatorsFromAssemblyContaining<UploadFileRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

// Включаем Swagger (для тестирования API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Advertising Platforms API",
        Version = "v1",
        Description = "API для загрузки и поиска рекламных площадок по регионам."
    });

    // Включаем XML-документацию
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// DI
builder.Services.AddSingleton<IFileHelper, FileHelper>();
builder.Services.AddSingleton<IAdvertisingPlatformRepository, AdvertisingPlatformStorage>();
builder.Services.AddScoped<IAdvertisingPlatformService, AdvertisingPlatformService>();

var app = builder.Build();

// Включаем Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Advertising Platforms API v1"); });
}

// Маршрутизация и контроллеры
app.MapControllers();

app.Run();