using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using task.Application.Services;
using task.Domain.Entities;
using task.Infrastructure.Persistence;

namespace task.Infrastructure.Import;

public class TerminalsImportService : ITerminalsImportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly DellinDictionaryDbContext _db;
    private readonly ILogger<TerminalsImportService> _logger;

    public TerminalsImportService(
        DellinDictionaryDbContext db,
        ILogger<TerminalsImportService> logger
    )
    {
        _db = db;
        _logger = logger;
    }

    public async Task ImportFromFileAsync(
        string filePath,
        CancellationToken cancellationToken = default
    )
    {
        if (!File.Exists(filePath))
        {
            _logger.LogError("Файл не найден: {FilePath}", filePath);
            throw new FileNotFoundException("Файл справочника не найден", filePath);
        }

        string json;
        try
        {
            json = await File.ReadAllTextAsync(filePath, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка чтения файла {FilePath}", filePath);
            throw;
        }

        TerminalsFileDto? data;
        try
        {
            data = JsonSerializer.Deserialize<TerminalsFileDto>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка десериализации JSON");
            throw;
        }

        var offices = FlattenToOffices(data);
        _logger.LogInformation("Загружено {Count} терминалов из JSON", offices.Count);

        var oldCount = await _db.Offices.CountAsync(cancellationToken);
        await _db.Phones.ExecuteDeleteAsync(cancellationToken);
        await _db.Offices.ExecuteDeleteAsync(cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Удалено {OldCount} старых записей", oldCount);

        if (offices.Count == 0)
        {
            _logger.LogInformation("Нет данных для сохранения");
            return;
        }

        await _db.Offices.AddRangeAsync(offices, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Сохранено {NewCount} новых терминалов", offices.Count);
    }

    private static List<Office> FlattenToOffices(TerminalsFileDto? data)
    {
        var result = new List<Office>();
        if (data?.City == null)
            return result;

        foreach (var city in data.City)
        {
            var terminals = city.Terminals?.Terminal;
            if (terminals == null)
                continue;

            foreach (var t in terminals)
            {
                var office = new Office
                {
                    Code = t.Id,
                    CityCode = city.CityId,
                    CountryCode = "RU",
                    Coordinates = new Coordinates
                    {
                        Latitude = ParseDouble(t.Latitude),
                        Longitude = ParseDouble(t.Longitude),
                    },
                    AddressCity = city.Name,
                    AddressStreet = t.Address ?? t.FullAddress,
                    WorkTime = GetWorkTime(t),
                    Type = MapType(t),
                };

                if (t.Phones != null)
                {
                    foreach (var p in t.Phones)
                    {
                        if (string.IsNullOrWhiteSpace(p.Number))
                            continue;
                        office.Phones.Add(
                            new Phone
                            {
                                PhoneNumber = p.Number,
                                Additional = string.IsNullOrWhiteSpace(p.Type)
                                    ? p.Comment
                                    : $"{p.Type}: {p.Comment}".TrimEnd(' ', ':'),
                            }
                        );
                    }
                }

                if (office.Phones.Count == 0 && !string.IsNullOrWhiteSpace(t.Address))
                    office.Phones.Add(new Phone { PhoneNumber = "-", Additional = null });

                result.Add(office);
            }
        }

        return result;
    }

    private static double ParseDouble(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return 0;
        return double.TryParse(
            s.Replace(',', '.'),
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out var v
        )
            ? v
            : 0;
    }

    private static string GetWorkTime(TerminalDto t)
    {
        var derival = t.CalcSchedule?.Derival;
        var arrival = t.CalcSchedule?.Arrival;
        if (!string.IsNullOrWhiteSpace(derival) || !string.IsNullOrWhiteSpace(arrival))
            return string.Join(
                "; ",
                new[] { derival, arrival }.Where(x => !string.IsNullOrWhiteSpace(x))!
            );

        var wt = t.Worktables?.Worktable?.FirstOrDefault()?.Timetable;
        return string.IsNullOrWhiteSpace(wt) ? "" : wt;
    }

    private static OfficeType? MapType(TerminalDto t)
    {
        if (t.IsPvz)
            return OfficeType.PVZ;
        if (t.IsOffice)
            return OfficeType.WAREHOUSE;
        return OfficeType.POSTAMAT;
    }
}
