using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Lumen.Desktop.Models;
using Lumen.Desktop.ViewModels.TabPages;
using Lumen.Service;
using Lumen.Service.Connection;
using Lumen.Service.Effect;
using Microsoft.Extensions.Logging;

namespace Lumen.Desktop.ViewModels;

public partial class MainWindowViewModel : StorageBasedViewModel
{
    [ObservableProperty] private StripViewModel _stripViewModel;
    [ObservableProperty] private ApplicationViewModel _applicationViewModel;

    private readonly SettingsManager _settingsManager;
    private readonly LumenSettings? _settings;

    // Connection should be reused.
    private Task? _activeTask;
    private CancellationTokenSource _cts = new();
    private StripRunner? _stripRunner;

    public MainWindowViewModel()
    {
        var baseAppPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Lumen");

        foreach (var file in Directory.GetFiles(baseAppPath, "*.log"))
        {
            File.Delete(file);
        }

        var logFile = Path.Combine(baseAppPath, "lumen.log");
        ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole().AddFile(logFile));
        ILogger logger = factory.CreateLogger("Lumen");

        Logging.Logger = logger;

        logger.LogInformation("Starting Lumen");

        var saveFile = Path.Combine(baseAppPath, "app.json");
        logger.LogInformation("Loading settings from {0}", saveFile);
        _settingsManager = new SettingsManager(saveFile);
        try
        {
            _settings = _settingsManager.LoadSettings();
        }
        catch (JsonException jsonException)
        {
            logger.LogError(jsonException, "Failed to load settings. Using default settings.");
        }
        finally
        {
            _settings ??= new LumenSettings(
                new StripSettings(
                    null,
                    new StripLayoutSettings(20, 40, 20, 40),
                    new ConnectionSettings("192.168.0.50", 34254),
                    new EffectSettingsCache()
                ),
                new ApplicationSettings(
                    false
                )
            );
        }

        StripViewModel = new StripViewModel(_settings.StripSettings);
        ApplicationViewModel = new ApplicationViewModel(_settings.ApplicationSettings);

        logger.LogInformation("Settings default worker.");
        SetLedStripWorker();

        WeakReferenceMessenger.Default.Register<SaveStateMessage>(this, SettingsChangedHandler);
    }


    private void SettingsChangedHandler(object o, SaveStateMessage saveStateMessage)
    {
        if (_settings is null)
            return;

        SetLedStripWorker();
        _settingsManager.SaveSettings(_settings);
    }

    private void SetLedStripWorker()
    {
        if (_settings is null)
            return;

        if (!_cts.IsCancellationRequested)
            _cts.Cancel();

        Logging.Logger?.LogInformation("Waiting for previous task to finish.");
        _activeTask?.Wait();

        var layout = _settings.StripSettings.StripLayoutSettings.AsLayout();
        var effectSettings = _settings.StripSettings.GetCurrentEffectSettings();
        if (effectSettings is null)
        {
            Logging.Logger?.LogError("No effect settings found. Fallback to Off effect.");
            effectSettings = new EffectOffSettings();
        }


        Logging.Logger?.LogInformation("Creating new StripRunner.");
        _stripRunner?.Dispose();
        _stripRunner = new StripRunner(new UdpConnection(
            _settings.StripSettings.ConnectionSettings.ControllerAddress,
            _settings.StripSettings.ConnectionSettings.ControllerPort)
        );


        _cts = new CancellationTokenSource();
        _activeTask = Task.Run(async () =>
        {
            Logging.Logger?.LogInformation("Starting effect");
            if (!effectSettings.TryToEffect(layout, out var effect))
            {
                Logging.Logger?.LogError("Failed to build effect. Removing entry from cache.");
                _settings.StripSettings.DeleteCurrentEffectSettings();
                _settingsManager.SaveSettings(_settings);
                Logging.Logger?.LogInformation("Fallback to Off effect.");
                effect = SolidColorEffect.Off(layout);
            }

            Logging.Logger?.LogInformation("Running effect {0}", effect.GetType().Name);
            try
            {
                await _stripRunner.RunWithEffect(effect, _cts.Token);
            }
            catch (OperationCanceledException)
            {
                Logging.Logger?.LogInformation("Task was cancelled.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        });
    }

    protected override void UpdateStorageValues()
    {
    }
}

public class SaveStateMessage : RequestMessage<bool>;