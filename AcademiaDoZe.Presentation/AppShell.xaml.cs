using AcademiaDoZe.Presentation.Pages;

namespace AcademiaDoZe.Presentation;

public partial class AppShell : Shell
{
    public AppShell(
        DashboardPage dashboardPage,
        LogradourosPage logradourosPage)
    {
        InitializeComponent();
        Routing.RegisterRoute(LogradouroFormPage.Route, typeof(LogradouroFormPage));
        DashboardShellContent.Content = dashboardPage;
        LogradourosShellContent.Content = logradourosPage;
    }
}
