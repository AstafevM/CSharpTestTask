using Library;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var dbContextOption = new DbContextOptionsBuilder<WebApiDbContext>()
    .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
    .Options;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Test Task API", Version = "v1" });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Test Task API V1"));
}

// ������ ����� � CSV ������
List<string> csvFilePaths =
[
    @".\CSVs\10001rows.csv",
    @".\CSVs\5000rows.csv",
    @".\CSVs\7000rows.csv",
    @".\CSVs\invalid.csv",
    @".\CSVs\invalid_date.csv",
    @".\CSVs\invalid_exec_time.csv",
    @".\CSVs\invalid_value.csv",
    @".\CSVs\empty_file.csv"
];

//1. ������ ������������� SaveDataFromCsvToDbAsync
foreach (string csvFilePath in csvFilePaths)
{
    using WebApiDbContext dbContext = new WebApiDbContext(dbContextOption);
    string csvFileName = Path.GetFileName(csvFilePath);

    try
    {
        await dbContext.SaveDataFromCsvToDbAsync(csvFilePath);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"�� ������� ���������� {csvFileName}: {ex.Message}");
    }
}

// 2. ������ ������������� GetFilteredResultsAsync (� �����������)
using (var dbContext = new WebApiDbContext(dbContextOption))
{
    try
    {
        var filteredResults = await dbContext.GetFilteredResultsAsync(fileName: "7000rows.csv");
        Console.WriteLine($"������� {filteredResults.Count} ��������������� �����������.");
        foreach (var result in filteredResults)
        {
            Console.WriteLine($"  FileName: {result.FileName}, AverageValue: {result.AverageValue}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"������ ��� ��������� ��������������� �����������: {ex.Message}");
    }
}

// 3. ������ ������������� GetLastTenResultsByFileNameAsync
using (var dbContext = new WebApiDbContext(dbContextOption))
{
    try
    {
        var latestResults = await dbContext.GetLastTenResultsByFileNameAsync("7000rows.csv");
        Console.WriteLine($"������� {latestResults.Count} ��������� ����������� ��� 5000rows.csv.");
        foreach (var result in latestResults)
        {
            Console.WriteLine($"  FileName: {result.FileName}, MinDate: {result.Date}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"������ ��� ��������� ��������� �����������: {ex.Message}");
    }
}

app.MapGet("/", () => "Hello World!");

app.Run();
