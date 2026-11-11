
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
        MatriculaListPage matriculaListPage,
        AcessoPage acessoPage,
        ConfigPage configPage)
    {
        InitializeComponent();

        Routing.RegisterRoute(
            LogradouroFormPage.Route,
            typeof(LogradouroFormPage));
        Routing.RegisterRoute(ColaboradorPage.Route, typeof(ColaboradorPage));
        Routing.RegisterRoute(AlunoPage.Route, typeof(AlunoPage));
        Routing.RegisterRoute(MatriculaPage.Route, typeof(MatriculaPage));

        DashboardShellContent.Content = dashboardPage;
        LogradourosShellContent.Content = logradourosPage;
        ColaboradoresShellContent.Content = colaboradorListPage;
        AlunosShellContent.Content = alunoListPage;
        MatriculasShellContent.Content = matriculaListPage;
        AcessosShellContent.Content = acessoPage;
        ConfigShellContent.Content = configPage;
    }
}
