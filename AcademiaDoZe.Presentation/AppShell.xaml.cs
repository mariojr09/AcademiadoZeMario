using AcademiaDoZe.Presentation.Pages;

namespace AcademiaDoZe.Presentation;

public partial class AppShell : Shell
{
    public AppShell(
        DashboardPage dashboardPage,
        LogradourosPage logradourosPage,
        ConfigPage configPage)
    {
        InitializeComponent();

        Routing.RegisterRoute(
            LogradouroFormPage.Route,
            typeof(LogradouroFormPage));

        DashboardShellContent.Content = dashboardPage;
        LogradourosShellContent.Content = logradourosPage;
        ConfigShellContent.Content = configPage;
    }
}