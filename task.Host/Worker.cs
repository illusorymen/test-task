using System.Runtime.InteropServices;
using task.Application.Services;

namespace task.Host;

public class Worker : BackgroundService
{
    private static readonly TimeZoneInfo Msk = TimeZoneInfo.FindSystemTimeZoneById(
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "Russian Standard Time"
            : "Europe/Moscow"
    );

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Worker> _logger;
    private readonly string _terminalsPath;

    public Worker(
        IServiceScopeFactory scopeFactory,
        ILogger<Worker> logger,
        IConfiguration configuration
    )
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        var filesDir = configuration["Terminals:FilesPath"];
        _terminalsPath = Path.Combine(AppContext.BaseDirectory, filesDir, "terminals.json");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Служба справочника терминалов запущена. Ожидание 02:00 MSK для первого импорта."
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Msk);
                var nextRun = GetNextRunAt02Msk(now);
                var delay = nextRun - now;
                if (delay > TimeSpan.Zero)
                {
                    _logger.LogInformation(
                        "Следующий импорт в {NextRun:yyyy-MM-dd HH:mm} MSK (через {Delay})",
                        nextRun,
                        delay
                    );
                    await Task.Delay(delay, stoppingToken);
                }

                if (stoppingToken.IsCancellationRequested)
                    break;

                await RunImportAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка импорта: {Message}", ex.Message);
            }
        }

        _logger.LogInformation("Служба справочника терминалов остановлена.");
    }

    private static DateTime GetNextRunAt02Msk(DateTime nowMsk)
    {
        var today02 = nowMsk.Date.AddHours(2);
        return nowMsk < today02 ? today02 : today02.AddDays(1);
    }

    private async Task RunImportAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Запуск импорта справочника терминалов из {Path}", _terminalsPath);
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var importService = scope.ServiceProvider.GetRequiredService<ITerminalsImportService>();
            await importService.ImportFromFileAsync(_terminalsPath, cancellationToken);
            sw.Stop();
            _logger.LogInformation("Импорт завершён за {ElapsedMs} мс", sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Ошибка импорта: {Exception}", ex.ToString());
        }
    }
}
