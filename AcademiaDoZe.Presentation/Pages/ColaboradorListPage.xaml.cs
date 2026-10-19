using System.Threading;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AcademiaDoZe.Presentation.Pages;

public partial class ColaboradorListPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ColaboradorListPage> _logger;
    private CancellationTokenSource? _pesquisaCts;
    private bool _carregando;

    public ColaboradorListPage(
        IServiceProvider serviceProvider,
        ILogger<ColaboradorListPage> logger)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarColaboradoresAsync();
    }

    private async Task CarregarColaboradoresAsync()
    {
        if (_carregando) return;
        try
        {
            _carregando = true;
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IColaboradorService>();
            ColaboradoresCollection.ItemsSource = (await service.ObterTodosAsync()).ToList();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro ao carregar", AppErrorMessages.From(ex, _logger, "listar colaboradores"), "OK");
        }
        finally
        {
            _carregando = false;
        }
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
            if (string.IsNullOrWhiteSpace(cpf))
            {
                await CarregarColaboradoresAsync();
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IColaboradorService>();
            var colaborador = await service.ObterPorCpfAsync(cpf, token);
            ColaboradoresCollection.ItemsSource = colaborador is null ? [] : new[] { colaborador };
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro na pesquisa", AppErrorMessages.From(ex, _logger, "pesquisar colaborador por CPF"), "OK");
        }
    }

    private async void NovoButton_Clicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(ColaboradorPage.Route);

    private async void EditarButton_Clicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: int id })
            await Shell.Current.GoToAsync($"{ColaboradorPage.Route}?id={id}");
    }

    private async void ExcluirButton_Clicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: ColaboradorDto colaborador }) return;
        var confirmar = await DisplayAlertAsync("Excluir colaborador", $"Deseja excluir {colaborador.Nome}?", "Excluir", "Cancelar");
        if (!confirmar) return;
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IColaboradorService>();
            if (!await service.RemoverAsync(colaborador.Id))
                throw new InvalidOperationException("COLABORADOR_NAO_ENCONTRADO");
            await CarregarColaboradoresAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Não foi possível excluir", AppErrorMessages.From(ex, _logger, "excluir colaborador"), "OK");
        }
    }
}
