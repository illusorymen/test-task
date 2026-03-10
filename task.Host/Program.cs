using Microsoft.EntityFrameworkCore;
using task.Application.Services;
using task.Host;
using task.Infrastructure.Import;
using task.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<DellinDictionaryDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(cs);
});

builder.Services.AddScoped<ITerminalsImportService, TerminalsImportService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DellinDictionaryDbContext>();
    db.Database.EnsureCreated();
}

await host.RunAsync();
