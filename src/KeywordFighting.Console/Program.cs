using KeywordFighting;
using KeywordFighting.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

Console.WriteLine("Ładowanie...");

var builder = Host.CreateApplicationBuilder();

builder.Services
    .AddSerilog(configuration => configuration.ReadFrom.Configuration(builder.Configuration))
    .Configure<AppOptions>(builder.Configuration)
    .AddKeywordFightingApplication()
    .AddKeywordFighting();

using var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation(" =================== NEW GAME =================== ");

var cts = host.Services.GetRequiredService<ICancellationTokenSource>();
Console.CancelKeyPress += (_, args) =>
{
    const string? message = "Anulowanie...";
    logger.LogInformation(message);
    Console.WriteLine(message);
    cts.Cancel();
    args.Cancel = true;
};

Console.WriteLine("Gra rozpoczęta! Naciśnij ctrl+C żeby przerwać.");
Console.WriteLine();

var nextGame = true;
while (nextGame)
{
    cts.Reset();

    await using var scope = host.Services.CreateAsyncScope();

    var engine = scope.ServiceProvider.GetRequiredService<IGameEngine>();
    var context = await scope.ServiceProvider.GetRequiredService<IGameContextProvider>()
        .CreateAsync(cts.Token);
    await RunEngineLoop(engine, context, cts.Token);

    var message = (cts.Token.IsCancellationRequested ? "Gra przerwana. " : "")
        + "Czy chcesz zagrać ponownie? [y/n]";
    logger.LogInformation(message);
    Console.WriteLine(message);
    Console.WriteLine();
    if (!ConsoleReadLineUntilBinaryDecision())
        nextGame = false;
}

Console.WriteLine("Gra skończona. Do zobaczenia później!");
Console.WriteLine();

return;

bool ConsoleReadLineUntilBinaryDecision()
{
    string? action;
    do
    {
        action = Console.ReadLine();
        Console.WriteLine();
    } while (string.IsNullOrWhiteSpace(action)
             || !(action.ToLower().StartsWith("t") || action.ToLower().StartsWith("y") || action.StartsWith("1")
                  || action.ToLower().StartsWith("n") || action.StartsWith("0")));

    return action.ToLower().StartsWith("t") || action.ToLower().StartsWith("y") || action.StartsWith("1");
}

async Task RunEngineLoop(IGameEngine engine, IGameContext context, CancellationToken cancellationToken)
{
    var action = 0.ToString();
    while (!context.IsFinished && !cancellationToken.IsCancellationRequested)
    {
        try
        {
            if (action == null)
            {
                await Task.Delay(100, cancellationToken);
            }
            else
            {
                var index = GetIndex(action);
                if (index == null || index < 0 || index >= context.AvailableActions.Length)
                {
                    Console.WriteLine("[Błąd] Akcja nieprawidłowa. Podaj numer wybranej akcji!");
                }
                else
                {
                    await engine.DoActionAsync(context, index.Value, cancellationToken);
                    Console.Clear();
                    if (context.IsInitialized)
                    {
                        Console.WriteLine(context.GetDescription());
                        Console.WriteLine();
                    }

                    Console.WriteLine(context.LastAction);
                    Console.WriteLine();
                    Console.WriteLine(context.NextAction);
                    foreach (var (i, actionDescription) in context.AvailableActions.Index())
                        Console.WriteLine($" [{i}] {actionDescription}");
                    Console.WriteLine();
                }
            }

            if (!context.IsFinished && !cancellationToken.IsCancellationRequested)
            {
                action = Console.ReadLine();
                Console.WriteLine();
            }
        }
        catch (OperationCanceledException)
        {

        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An unexpected error occurred!");
            Console.WriteLine(
                "[Błąd] Wystąpił nieoczekiwany błąd, przepraszamy. Aby wyjaśnić sytuację, skontaktuj się z administratorem załączając plik logów.");
        }
    }
}
static int? GetIndex(string action)
{
    foreach (var s in action.Split())
        if (int.TryParse(s, out var idx))
            return idx;

    return null;
}
