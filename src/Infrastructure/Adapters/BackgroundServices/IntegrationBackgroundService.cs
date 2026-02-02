using Application.Features.DatabaseManagement.Queries.TableControls;
using Application.Features.WindchillIntegration.EPMDocumentCancelled.Commands.ErrorProcess;
using Application.Features.WindchillIntegration.EPMDocumentCancelled.Commands.Process;
using Application.Features.WindchillIntegration.EPMDocumentReleased.Commands.ErrorProcess;
using Application.Features.WindchillIntegration.EPMDocumentReleased.Commands.Process;
using Application.Features.WindchillIntegration.WTPartAlternateLink.Commands.ErrorProcess;
using Application.Features.WindchillIntegration.WTPartAlternateLink.Commands.Process;
using Application.Features.WindchillIntegration.WTPartAlternateLinkRemoved.Commands.ErrorProcess;
using Application.Features.WindchillIntegration.WTPartAlternateLinkRemoved.Commands.Process;
using Application.Features.WindchillIntegration.WTPartCancelled.Commands;
using Application.Features.WindchillIntegration.WTPartCancelled.Commands.ErrorProcess;
using Application.Features.WindchillIntegration.WTPartCancelled.Commands.Process;
using Application.Features.WindchillIntegration.WTPartReleased.Commands.ErrorProcess;
using Application.Features.WindchillIntegration.WTPartReleased.Commands.Process;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.BackgroundServices
{
	public class IntegrationBackgroundService : BackgroundService
	{
		private readonly ILogger<IntegrationBackgroundService> _logger;
		private readonly IServiceProvider _serviceProvider;
		private bool _uyariMesajiGosterildi = false;
		private bool _kurulumTamamlandiMesajiGosterildi = false;

		// Deadlock önlemi için semaphore
		private readonly SemaphoreSlim _databaseSemaphore = new(2, 2); // Aynı anda max 2 DB işlemi

		public IntegrationBackgroundService(ILogger<IntegrationBackgroundService> logger,
											IServiceProvider serviceProvider)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					// Tabloların hazır olup olmadığını kontrol et
					bool tablolarHazir = await CheckTablesReadyAsync(stoppingToken);

					if (!tablolarHazir)
					{
						if (!_uyariMesajiGosterildi)
						{
							_logger.LogWarning("Veritabanı kurulumu tamamlanmamış. Entegrasyon işlemleri başlamadan önce lütfen arayüzden kurulumu tamamlayın.");
							_uyariMesajiGosterildi = true;
							_kurulumTamamlandiMesajiGosterildi = false;
						}

						await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
						continue;
					}

					if (_uyariMesajiGosterildi && !_kurulumTamamlandiMesajiGosterildi)
					{
						_logger.LogInformation("Veritabanı kurulumu başarıyla tamamlandı. Entegrasyon işlemleri başlıyor.");
						_kurulumTamamlandiMesajiGosterildi = true;
						_uyariMesajiGosterildi = false;
					}

					var now = DateTime.Now;

					if (now.Hour >= 20 && now.Hour < 24)
					{
						// PARALEL + DEADLOCK KORUNMASI
						_logger.LogInformation("Hata işleme modunda - paralel işleme (deadlock korumalı)");

						await ProcessErrorTasksParallelSafe(stoppingToken);
					}
					else
					{
						// Normal işlemler
						await ProcessNormalTasksParallel(stoppingToken);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "IntegrationBackgroundService çalışırken hata oluştu.");

					// Hata durumunda kısa bekleme
					await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
				}

				// Normal döngü bekleme
				await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
			}
		}

		private async Task ProcessErrorTasksParallelSafe(CancellationToken stoppingToken)
		{
			// Paralel ama kontrollü - Deadlock koruması ile
			var errorReleasedTask = ExecuteTaskWithDeadlockProtection(
				async (mediator) => await mediator.Send(new ErrorProcessWTPartReleasedCommand(), stoppingToken),
				"ErrorProcessWTPartReleased", stoppingToken);

			var errorCancelledTask = ExecuteTaskWithDeadlockProtection(
				async (mediator) => await mediator.Send(new ErrorProcessWTPartCancelledCommand(), stoppingToken),
				"ErrorProcessWTPartCancelled", stoppingToken);

			var errorAlternateTask = ExecuteTaskWithDeadlockProtection(
				async (mediator) => await mediator.Send(new ErrorProcessWTPartAlternateLinkCommand(), stoppingToken),
				"ErrorProcessWTPartAlternateLink", stoppingToken);

			var errorAlternateRemovedTask = ExecuteTaskWithDeadlockProtection(
				async (mediator) => await mediator.Send(new ErrorProcessWTPartAlternateLinkRemovedCommand(), stoppingToken),
				"ErrorProcessWTPartAlternateLinkRemoved", stoppingToken);

			//EMPDocument
			var errorEpmReleasedTask = ExecuteTaskWithDeadlockProtection(
		async (mediator) => await mediator.Send(new ErrorProcessEPMDocumentReleasedCommand(), stoppingToken),
		"ErrorProcessEPMDocumentReleased", stoppingToken);

			var errorEpmCancelledTask = ExecuteTaskWithDeadlockProtection(
		async (mediator) => await mediator.Send(new ErrorProcessEPMDocumentCancelledCommand(), stoppingToken),
		"ErrorProcessEPMDocumentReleased", stoppingToken);


			// Hala paralel çalışıyor ama güvenli
			await Task.WhenAll(errorReleasedTask, errorCancelledTask, errorAlternateTask, errorAlternateRemovedTask, errorEpmReleasedTask, errorEpmCancelledTask);
		}

		private async Task ProcessNormalTasksParallel(CancellationToken stoppingToken)
		{
			// Normal işlemler tam paralel (deadlock riski düşük)
			var releasedTask = ExecuteTaskSafely(
				async (mediator) => await mediator.Send(new ProcessWTPartReleasedCommand(), stoppingToken),
				"ProcessWTPartReleased", stoppingToken);

			var cancelledTask = ExecuteTaskSafely(
				async (mediator) => await mediator.Send(new ProcessWTPartCancelledCommand(), stoppingToken),
				"ProcessWTPartCancelled", stoppingToken);

			var alternateTask = ExecuteTaskSafely(
				async (mediator) => await mediator.Send(new ProcessWTPartAlternateLinkCommand(), stoppingToken),
				"ProcessWTPartAlternateLink", stoppingToken);

			var alternateRemovedTask = ExecuteTaskSafely(
				async (mediator) => await mediator.Send(new ProcessWTPartAlternateLinkRemovedCommand(), stoppingToken),
				"ProcessWTPartAlternateLinkRemoved", stoppingToken);


			var epmDocumentReleasedTask = ExecuteTaskSafely(
				async (mediator) => await mediator.Send(new ProcessEPMDocumentReleasedCommand(), stoppingToken),
				"ProcessEPMDocumentReleased", stoppingToken);


			var epmDocumentCancelledTask = ExecuteTaskSafely(
				async (mediator) => await mediator.Send(new ProcessEPMDocumentCancelledCommand(), stoppingToken),
				"ProcessEPMDocumentCancelled", stoppingToken);


			await Task.WhenAll(releasedTask, cancelledTask, alternateTask, alternateRemovedTask, epmDocumentReleasedTask ,epmDocumentCancelledTask);
		}

		private async Task ExecuteTaskWithDeadlockProtection(Func<IMediator, Task> taskFunc, string taskName, CancellationToken stoppingToken)
		{
			var maxRetries = 3;
			var baseDelay = TimeSpan.FromMilliseconds(500);

			for (int attempt = 0; attempt < maxRetries; attempt++)
			{
				try
				{
					stoppingToken.ThrowIfCancellationRequested();

					// Semaphore ile eş zamanlı DB erişimini sınırla
					await _databaseSemaphore.WaitAsync(stoppingToken);

					try
					{
						using var scope = _serviceProvider.CreateScope();
						var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

						_logger.LogDebug("Başlatılıyor: {TaskName} (Deneme: {Attempt})", taskName, attempt + 1);

						await taskFunc(mediator);

						_logger.LogDebug("Tamamlandı: {TaskName}", taskName);
						return; // Başarılı, retry'dan çık
					}
					finally
					{
						_databaseSemaphore.Release();
					}
				}
				catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 1205 && attempt < maxRetries - 1) // Deadlock
				{
					var delay = TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(2, attempt));
					_logger.LogWarning("Deadlock tespit edildi: {TaskName} - Yeniden deneniyor {Attempt}/{MaxRetries} - Bekleme: {Delay}ms",
						taskName, attempt + 1, maxRetries, delay.TotalMilliseconds);

					await Task.Delay(delay, stoppingToken);
				}
				catch (Exception ex) when (attempt < maxRetries - 1)
				{
					var delay = TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(2, attempt));
					_logger.LogWarning(ex, "Geçici hata: {TaskName} - Yeniden deneniyor {Attempt}/{MaxRetries} - Bekleme: {Delay}ms",
						taskName, attempt + 1, maxRetries, delay.TotalMilliseconds);
					 
					await Task.Delay(delay, stoppingToken);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Görev başarısız (son deneme): {TaskName}", taskName);
					return; // Son deneme de başarısız, çık
				}
			}
		}

		private async Task ExecuteTaskSafely(Func<IMediator, Task> taskFunc, string taskName, CancellationToken stoppingToken)
		{
			try
			{
				stoppingToken.ThrowIfCancellationRequested();

				using var scope = _serviceProvider.CreateScope();
				var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

				await taskFunc(mediator);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Görev başarısız: {TaskName}", taskName);
			}
		}

		private async Task<bool> CheckTablesReadyAsync(CancellationToken stoppingToken)
		{
			try
			{
				using var scope = _serviceProvider.CreateScope();
				var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

				var tableControls = await mediator.Send(new TableControlsDatabaseQuery(), stoppingToken);

				return tableControls != null && tableControls.Count > 0 && tableControls.All(t => t.IsActive);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Tablo kontrolü sırasında hata oluştu.");
				return false;
			}
		}

		// Düzeltilmiş Dispose
		public override void Dispose()
		{
			_databaseSemaphore?.Dispose();
			base.Dispose();
		}
	}
}

