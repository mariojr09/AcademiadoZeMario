using System.Collections.ObjectModel;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.Helpers;

namespace AcademiaDoZe.Presentation.Pages;
//Mario Cesar Alves Júnior

public partial class LogradourosPage : ContentPage
{
    private readonly ILogradouroService _logradouroService;
    private readonly ObservableCollection<LogradouroDto> _logradourosExibidos = [];
    private List<LogradouroDto> _todosLogradouros = [];
    private bool _isLoading;

    public LogradourosPage(ILogradouroService logradouroService)
    {
        InitializeComponent();
        _logradouroService = logradouroService;
        LogradourosCollection.ItemsSource = _logradourosExibidos;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarLogradourosAsync();
    }

    private async Task CarregarLogradourosAsync()
    {
        if (_isLoading)
            return;

        try
        {
            DefinirCarregamento(true);
            var logradouros = await _logradouroService.ObterTodosAsync();
            _todosLogradouros = logradouros
                .OrderBy(logradouro => logradouro.Nome, StringComparer.OrdinalIgnoreCase)
                .ToList();

            AplicarFiltro(PesquisaSearchBar.Text);
        }
        catch (Exception ex)
        {
            _todosLogradouros = [];
            AplicarFiltro(null);
            await DisplayAlertAsync(
                "Não foi possível carregar",
                LogradouroErrorMessages.From(
                    ex,
                    "Não foi possível consultar os logradouros. Verifique a conexão com o MySQL."),
                "Entendi");
        }
        finally
        {
            DefinirCarregamento(false);
        }
    }

    private void AplicarFiltro(string? termo)
    {
        IEnumerable<LogradouroDto> resultado = _todosLogradouros;
        var pesquisa = termo?.Trim();

        if (!string.IsNullOrWhiteSpace(pesquisa))
        {
            var pesquisaCep = pesquisa.All(caractere =>
                char.IsDigit(caractere) || caractere == '-' || char.IsWhiteSpace(caractere))
                ? string.Concat(pesquisa.Where(char.IsDigit))
                : string.Empty;

            resultado = resultado.Where(logradouro =>
                Contem(logradouro.Cep, pesquisa) ||
                (!string.IsNullOrEmpty(pesquisaCep) && Contem(logradouro.Cep, pesquisaCep)) ||
                Contem(logradouro.Nome, pesquisa) ||
                Contem(logradouro.Bairro, pesquisa) ||
                Contem(logradouro.Cidade, pesquisa));
        }

        _logradourosExibidos.Clear();
        foreach (var logradouro in resultado)
            _logradourosExibidos.Add(logradouro);

        QuantidadeLabel.Text = _logradourosExibidos.Count == 1
            ? "1 registro encontrado"
            : $"{_logradourosExibidos.Count} registros encontrados";
    }

    private static bool Contem(string valor, string termo) =>
        valor.Contains(termo, StringComparison.OrdinalIgnoreCase);

    private void DefinirCarregamento(bool carregando)
    {
        _isLoading = carregando;
        LoadingIndicator.IsVisible = carregando;
        LoadingIndicator.IsRunning = carregando;
    }

    private void OnPesquisaTextChanged(object? sender, TextChangedEventArgs e)
    {
        AplicarFiltro(e.NewTextValue);
    }

    private async void OnAtualizarClicked(object? sender, EventArgs e)
    {
        await CarregarLogradourosAsync();
    }

    private async void OnNovoLogradouroClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(LogradouroFormPage.Route);
    }

    private async void OnEditarClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: LogradouroDto logradouro })
            return;

        await Shell.Current.GoToAsync($"{LogradouroFormPage.Route}?id={logradouro.Id}");
    }

    private async void OnExcluirClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: LogradouroDto logradouro })
            return;

        var confirmado = await DisplayAlertAsync(
            "Excluir Logradouro",
            $"Deseja realmente excluir \"{logradouro.Nome}\" (CEP {logradouro.Cep})?",
            "Excluir",
            "Cancelar");

        if (!confirmado)
            return;

        try
        {
            DefinirCarregamento(true);
            var removido = await _logradouroService.RemoverAsync(logradouro.Id);

            if (!removido)
            {
                await DisplayAlertAsync(
                    "Logradouro não encontrado",
                    "O logradouro não foi encontrado. A lista será atualizada.",
                    "Entendi");
            }
            else
            {
                await DisplayAlertAsync(
                    "Exclusão concluída",
                    "O logradouro foi excluído com sucesso.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Não foi possível excluir",
                LogradouroErrorMessages.From(
                    ex,
                    "Não foi possível excluir o logradouro. Verifique a conexão com o MySQL."),
                "Entendi");
        }
        finally
        {
            DefinirCarregamento(false);
            await CarregarLogradourosAsync();
        }
    }
}
