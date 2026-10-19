using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AcademiaDoZe.Presentation.Pages;

public partial class AlunoListPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AlunoListPage> _logger;
    private CancellationTokenSource? _pesquisaCts;
    private bool _carregando;

    public AlunoListPage(
        IServiceProvider serviceProvider,
        ILogger<AlunoListPage> logger)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        if (_carregando) return;
        try
        {
            _carregando = true;
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IAlunoService>();
            AlunosCollection.ItemsSource = (await service.ObterTodosAsync()).ToList();
        }
        catch (Exception ex) { await DisplayAlertAsync("Erro ao carregar", AppErrorMessages.From(ex, _logger, "listar alunos"), "OK"); }
        finally { _carregando = false; }
    }

    private async void PesquisaEntry_TextChanged(object? sender, TextChangedEventArgs e)
    {
        _pesquisaCts?.Cancel();
        _pesquisaCts?.Dispose();
        _pesquisaCts = new CancellationTokenSource();
        var token = _pesquisaCts.Token;
        try
        {
            await Task.Delay(350, token);
            var cpf = e.NewTextValue?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(cpf)) { await CarregarAsync(); return; }
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IAlunoService>();
            var aluno = await service.ObterPorCpfAsync(cpf, token);
            AlunosCollection.ItemsSource = aluno is null ? [] : new[] { aluno };
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { await DisplayAlertAsync("Erro na pesquisa", AppErrorMessages.From(ex, _logger, "pesquisar aluno por CPF"), "OK"); }
    }

    private async void NovoButton_Clicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync(AlunoPage.Route);

    private async void EditarButton_Clicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: int id }) await Shell.Current.GoToAsync($"{AlunoPage.Route}?id={id}");
    }

    private async void ExcluirButton_Clicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: AlunoDto aluno }) return;
        if (!await DisplayAlertAsync("Excluir aluno", $"Deseja excluir {aluno.Nome}?", "Excluir", "Cancelar")) return;
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IAlunoService>();
            if (!await service.RemoverAsync(aluno.Id)) throw new InvalidOperationException("ALUNO_NAO_ENCONTRADO");
            await CarregarAsync();
        }
        catch (Exception ex) { await DisplayAlertAsync("Não foi possível excluir", AppErrorMessages.From(ex, _logger, "excluir aluno"), "OK"); }
    }
}
