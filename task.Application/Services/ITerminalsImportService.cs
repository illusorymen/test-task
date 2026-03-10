namespace task.Application.Services;

public interface ITerminalsImportService
{
    Task ImportFromFileAsync(string filePath, CancellationToken cancellationToken = default);
}
