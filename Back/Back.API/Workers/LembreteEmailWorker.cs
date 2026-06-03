using Back.Application.UseCases.LembreteEmail;

namespace Back.API.Workers;

/// <summary>
/// Verifica diariamente os lembretes agendados e dispara os e-mails de pendência
/// para os alunos cujos lembretes já venceram. A idempotência é garantida pela
/// flag <c>Enviado</c> do lembrete.
/// </summary>
public class LembreteEmailWorker : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromHours(24);
    private static readonly TimeSpan AtrasoInicial = TimeSpan.FromMinutes(1);

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LembreteEmailWorker> _logger;

    public LembreteEmailWorker(IServiceProvider serviceProvider, ILogger<LembreteEmailWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Pequeno atraso inicial para não concorrer com migrations/seed no boot.
        try
        {
            await Task.Delay(AtrasoInicial, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        using var timer = new PeriodicTimer(Intervalo);
        try
        {
            do
            {
                await DispararAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            // Encerramento normal da aplicação.
        }
    }

    private async Task DispararAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var useCase = scope.ServiceProvider.GetRequiredService<EnviarLembretesPendentesUseCase>();

            var enviados = await useCase.ExecuteAsync();
            if (enviados > 0)
                _logger.LogInformation("Lembretes de pendência enviados: {Quantidade}.", enviados);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Encerramento normal da aplicação.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao processar lembretes de pendência.");
        }
    }
}
