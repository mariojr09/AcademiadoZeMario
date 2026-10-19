
using AcademiaDoZe.Presentation.Pages;

namespace AcademiaDoZe.Presentation;

// Mario Cesar Alves Júnior
public partial class AppShell : Shell
{
    public AppShell(
        DashboardPage dashboardPage,
        LogradourosPage logradourosPage,
        ColaboradorListPage colaboradorListPage,
        AlunoListPage alunoListPage,
        ConfigPage configPage)
    {
        InitializeComponent();

        Routing.RegisterRoute(
            LogradouroFormPage.Route,
            typeof(LogradouroFormPage));
        Routing.RegisterRoute(ColaboradorPage.Route, typeof(ColaboradorPage));
        Routing.RegisterRoute(AlunoPage.Route, typeof(AlunoPage));

        DashboardShellContent.Content = dashboardPage;
        LogradourosShellContent.Content = logradourosPage;
        ColaboradoresShellContent.Content = colaboradorListPage;
        AlunosShellContent.Content = alunoListPage;
        ConfigShellContent.Content = configPage;
    }
}
