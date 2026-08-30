using Microsoft.EntityFrameworkCore;
using QradarLogSystem.Api.Data;
using QradarLogSystem.Api.Services;
using QradarLogSystem.Api.Logging;
using QradarLogSystem.Api.Services.Parsers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// CORS Ayarı
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<EventParser>();
builder.Services.AddScoped<EventNormalizer>();
builder.Services.AddScoped<EventProcessingService>();

builder.Services.AddScoped<LogReaderService>();
builder.Services.AddScoped<CsvEventParser>();
builder.Services.AddScoped<JsonEventParser>();

builder.Services.AddScoped<DatasetFormatDetector>();
builder.Services.AddScoped<DatasetImportService>();
builder.Services.AddScoped<LeefEventParser>();

builder.Services.AddSingleton<IFileLogger, FileLogger>();

builder.Services.AddDbContext<QradarDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("QradarDatabase")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger Yapılandırması
app.UseSwagger(c =>
{
    c.RouteTemplate = "swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "QradarLogSystem API v1");
    c.RoutePrefix = "swagger";
});

app.UseRouting();

app.UseCors("FrontendPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
