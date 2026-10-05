using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Presentation;

public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();

        _serviceProvider = serviceProvider;

        AplicarTemaSalvo();
    }

    private void AplicarTemaSalvo()
    {
        var tema = Preferences.Default.Get(
            "AppTema",
            "Sistema");

        UserAppTheme = tema switch
        {
            "Claro" => AppTheme.Light,
            "Escuro" => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(
            _serviceProvider.GetRequiredService<AppShell>());
    }
}