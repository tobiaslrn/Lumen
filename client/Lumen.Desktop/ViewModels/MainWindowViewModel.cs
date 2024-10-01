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
        var saveFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Lumen/app.json");

        _settingsManager = new SettingsManager(saveFile);
        try
        {
            _settings = _settingsManager.LoadSettings();
        }
        catch (JsonException jsonException)
        {
            Console.WriteLine(jsonException.ToString());
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

        _activeTask?.Wait();

        var layout = _settings.StripSettings.StripLayoutSettings.AsLayout();

        var effectSettings = _settings.StripSettings.GetCurrentEffectSettings();
        if (effectSettings is null)
            return;

        _stripRunner?.Dispose();
        _stripRunner = new StripRunner(new UdpConnection(
            _settings.StripSettings.ConnectionSettings.ControllerAddress,
            _settings.StripSettings.ConnectionSettings.ControllerPort)
        );


        _cts = new CancellationTokenSource();
        _activeTask = Task.Run(async () =>
        {
            using var effect = effectSettings.ToEffect(layout);
            if (effect is null)
                return;

            try
            {
                await _stripRunner.RunWithEffect(effect, _cts.Token);
            }
            catch (OperationCanceledException)
            {
                // ignored
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