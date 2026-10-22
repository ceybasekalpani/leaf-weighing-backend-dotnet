using System.Globalization;
using System.Reflection;
using System.Text.Json;
using LeafWeighing.Application.Interfaces.Repositories;
using LeafWeighing.Application.Services;
using LeafWeighing.Domain.Calculations;
using LeafWeighing.Domain.Entities;
using LeafWeighing.Infrastructure.Data;
using LeafWeighing.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var cases = JsonSerializer.Deserialize<List<WeightCase>>(
    File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "net-weight-cases.json")),
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
var previousCulture = CultureInfo.CurrentCulture;
try
{
    // API string fields must retain a decimal point even on other server locales.
    CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
    foreach (var test in cases)
    {
        var originalGross = test.Rows.Sum(x => x.Gross ?? 0);
        Equal(test.ExpectedGross, LeafWeightCalculator.CalculateAdjustedGrossTotal(test.Rows), test.Name + " adjusted gross");
        Equal(test.Expected, LeafWeightCalculator.CalculateTotal(test.Rows), test.Name);
        foreach (var row in test.Rows) row.RegNo = 42;
        var repository = DispatchProxy.Create<ITrLeafCollectionRepository, CollectionProxy>();
        ((CollectionProxy)(object)repository).Rows = test.Rows;
        var service = new CollectionService(repository);
        var today = (await service.GetTodayCollectionsAsync()).ToList();
        var dated = (await service.GetCollectionsByDateAsync(new DateTime(2026, 10, 20))).ToList();
        foreach (var summaries in new[] { today, dated })
        {
            if (test.Rows.Count == 0)
            {
                if (summaries.Count != 0) throw new Exception("Empty selection returned a summary");
                continue;
            }
            var summary = summaries.Single();
            Equal(test.ExpectedGross, decimal.Parse(summary.Gross, CultureInfo.InvariantCulture), test.Name + " summary gross");
            Equal(originalGross, decimal.Parse(summary.RawGross, CultureInfo.InvariantCulture), test.Name + " raw gross");
            Equal(test.Expected, decimal.Parse(summary.NetWeight, CultureInfo.InvariantCulture), test.Name + " summary");
            Equal(test.Rows.Sum(x => x.SuperDeduction ?? 0), decimal.Parse(summary.TotalSuperDeduction, CultureInfo.InvariantCulture), "super total");
            Equal(test.Rows.Sum(x => x.NormalDeduction ?? 0), decimal.Parse(summary.TotalNormalDeduction, CultureInfo.InvariantCulture), "normal total");
        }
        var details = (await service.GetCollectionDetailsAsync(42)).ToList();
        Equal(test.ExpectedGross, details.Sum(x => x.Gross ?? 0), test.Name + " detail gross");
        Equal(test.Expected, details.Sum(x => x.NetWeight ?? 0), test.Name + " details");
        Equal(originalGross, test.Rows.Sum(x => x.Gross ?? 0), "Source gross must not be overwritten");
    }
}
finally { CultureInfo.CurrentCulture = previousCulture; }

using var context = new BoughtLeafDbContext(new DbContextOptionsBuilder<BoughtLeafDbContext>()
    .UseSqlServer("Server=localhost;Database=Unused;Integrated Security=True;TrustServerCertificate=True;").Options);
var sql = context.TrLeafCollectionTemp.ToQueryString();
if (!sql.Contains("[SuperDeduction]") || !sql.Contains("[NormalDeduction]"))
    throw new Exception("Missing deduction column mappings");
foreach (var name in new[] { "SuperDeduction", "NormalDeduction" })
{
    var property = context.Model.FindEntityType(typeof(TrLeafCollectionTemp))!.FindProperty(name)!;
    if (property.ClrType != typeof(int?)) throw new Exception("SQL int type mismatch: " + name);
}
Console.WriteLine($"Passed {cases.Count} shared cases, today/date summaries, details, culture and EF mappings.");

// Optional read-only integration check. Pass credentials through the environment;
// never write them to the test project or print connection strings.
var liveConnection = Environment.GetEnvironmentVariable("LEAF_WEIGHT_CHECK_CONNECTION");
if (!string.IsNullOrWhiteSpace(liveConnection))
{
    using var liveContext = new BoughtLeafDbContext(new DbContextOptionsBuilder<BoughtLeafDbContext>()
        .UseSqlServer(liveConnection).Options);
    var latestDate = await liveContext.TrLeafCollectionTemp.AsNoTracking()
        .OrderByDescending(x => x.LogTime).Select(x => x.LogTime).FirstOrDefaultAsync();
    if (latestDate.HasValue)
    {
        var liveService = new CollectionService(new TrLeafCollectionRepository(liveContext));
        var summaries = (await liveService.GetCollectionsByDateAsync(latestDate.Value.Date)).ToList();
        var db = liveContext.Database.GetDbConnection();
        await db.OpenAsync();
        using var command = db.CreateCommand();
        command.CommandText = """
            SELECT COALESCE(SUM(COALESCE(Gross,0)),0),
                COALESCE(SUM(COALESCE(Gross,0) - COALESCE(SuperDeduction,0) - COALESCE(NormalDeduction,0)),0),
                COALESCE(SUM(COALESCE(Gross,0) - COALESCE(BagWeight,0)
                - COALESCE(Water,0) - COALESCE(Coarse,0) - COALESCE(Rejected,0)
                - COALESCE(Boild,0) - COALESCE(SuperDeduction,0) - COALESCE(NormalDeduction,0)),0)
            FROM dbo.Tr_LeafCollection_Temp WHERE LogTime >= @start AND LogTime < @end
            """;
        var start = command.CreateParameter();
        start.ParameterName = "@start";
        start.Value = latestDate.Value.Date;
        command.Parameters.Add(start);
        var end = command.CreateParameter();
        end.ParameterName = "@end";
        end.Value = latestDate.Value.Date.AddDays(1);
        command.Parameters.Add(end);
        using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        var sqlRawGross = reader.GetDecimal(0);
        var sqlGross = reader.GetDecimal(1);
        var sqlNet = reader.GetDecimal(2);
        Equal(sqlRawGross, summaries.Sum(x => decimal.Parse(x.RawGross, CultureInfo.InvariantCulture)), "Live SQL raw gross");
        Equal(sqlGross, summaries.Sum(x => decimal.Parse(x.Gross, CultureInfo.InvariantCulture)), "Live SQL adjusted gross");
        Equal(sqlNet, summaries.Sum(x => decimal.Parse(x.NetWeight, CultureInfo.InvariantCulture)), "Live SQL vs API summary");
        Console.WriteLine($"Read-only TFMS check passed for {summaries.Count} supplier summaries; no data changed.");
    }
    else Console.WriteLine("Read-only TFMS connection passed; no collection rows to check.");
}

static void Equal(decimal expected, decimal actual, string name)
{
    if (expected != actual) throw new Exception($"{name}: expected {expected}, got {actual}");
}

public record WeightCase(string Name, decimal Expected, decimal ExpectedGross, List<TrLeafCollectionTemp> Rows);

public class CollectionProxy : DispatchProxy
{
    public List<TrLeafCollectionTemp> Rows { get; set; } = [];
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => targetMethod!.Name switch
    {
        "GetTodayGroupedCollectionsAsync" or "GetGroupedCollectionsByDateAsync" =>
            Task.FromResult<IEnumerable<IGrouping<int, TrLeafCollectionTemp>>>(Rows.GroupBy(x => x.RegNo ?? 0)),
        "FindAsync" => Task.FromResult<IEnumerable<TrLeafCollectionTemp>>(Rows),
        _ => throw new NotSupportedException(targetMethod.Name)
    };
}
