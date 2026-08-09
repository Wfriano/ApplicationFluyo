using FluyoV2.Features.Assets.Repositories;
using FluyoV2.Features.Commitments.Repositories;
using FluyoV2.Features.Liabilities.Repositories;
using FluyoV2.Features.Notifications.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FluyoV2.BackgroundServices;

public class NotificationsProcessorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationsProcessorService> _logger;

    public NotificationsProcessorService(
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationsProcessorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationsProcessorService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var today = now.Date;

                using var scope = _scopeFactory.CreateScope();

                var notificationsService = scope.ServiceProvider.GetRequiredService<NotificationsService>();
                var commitmentsRepository = scope.ServiceProvider.GetRequiredService<CommitmentsRepository>();
                var assetsRepository = scope.ServiceProvider.GetRequiredService<AssetsRepository>();
                var liabilitiesRepository = scope.ServiceProvider.GetRequiredService<LiabilitiesRepository>();

                var commitments = await commitmentsRepository.GetAllAsync();
                foreach (var item in commitments.Where(x => x.IsActive && x.PaymentDate.HasValue && x.PaymentDate.Value.Date == today))
                {
                    var dedupKey = $"commitment:{item.Id}:{today:yyyyMMdd}";

                    await notificationsService.CreatePaymentNotificationIfNotExistsAsync(
                        item.UserId,
                        "Pago pendiente hoy",
                        $"Hoy vence: {item.Name} por {item.Amount:N0}",
                        "Commitment",
                        item.Id,
                        item.PaymentDate!.Value,
                        dedupKey);
                }

                var assets = await assetsRepository.GetAllAsync();
                foreach (var item in assets.Where(x => x.IsActive && x.IsStillPaying && x.NextPaymentDate.HasValue && x.NextPaymentDate.Value.Date == today))
                {
                    var amount = item.InstallmentAmount ?? 0m;
                    var dedupKey = $"asset:{item.Id}:{today:yyyyMMdd}";

                    await notificationsService.CreatePaymentNotificationIfNotExistsAsync(
                        item.UserId,
                        "Cuota de bien pendiente hoy",
                        $"Hoy vence cuota de {item.Name} por {amount:N0}",
                        "Asset",
                        item.Id,
                        item.NextPaymentDate!.Value,
                        dedupKey);
                }

                var liabilities = await liabilitiesRepository.GetAllAsync();
                foreach (var item in liabilities.Where(x => x.IsActive && x.IsStillPaying && x.NextPaymentDate.HasValue && x.NextPaymentDate.Value.Date == today))
                {
                    var amount = item.InstallmentAmount ?? 0m;
                    var dedupKey = $"liability:{item.Id}:{today:yyyyMMdd}";

                    await notificationsService.CreatePaymentNotificationIfNotExistsAsync(
                        item.UserId,
                        "Cuota de deuda pendiente hoy",
                        $"Hoy vence cuota de {item.Name} por {amount:N0}",
                        "Liability",
                        item.Id,
                        item.NextPaymentDate!.Value,
                        dedupKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notifications");
            }

            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }
}
