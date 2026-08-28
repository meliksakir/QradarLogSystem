using Microsoft.EntityFrameworkCore;
using QradarLogSystem.Api.Data;
using QradarLogSystem.Api.Services;
using QradarLogSystem.Api.Logging;
using QradarLogSystem.Api.Services.Parsers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173"
            )
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS yönlendirmesinden sonra CORS
app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();