using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;

namespace AcademiaDoZe.Presentation.Pages;

// Mario Cesar Alves Júnior
public partial class MatriculaListPage : ContentPage
{
    private readonly IMatriculaService _matriculaService;
    private List<MatriculaDto> _matriculas = [];
    private CancellationTokenSource? _pesquisaCts;

    public MatriculaListPage(IMatriculaService matriculaService)
    {
        InitializeComponent();
        _matriculaService = matriculaService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        try
        {
            DefinirCarregando(true);
            _matriculas = (await _matriculaService.ObterTodosAsync())
                .OrderByDescending(item => item.DataInicio)
                .ThenBy(item => item.Aluno?.Nome)
                .ToList();
            AplicarFiltro(PesquisaEntry.Text);
        }
        catch (Exception ex)
        {
            _matriculas = [];
            AplicarFiltro(null);
            await DisplayAlertAsync(
                "Não foi possível carregar",
                $"Não foi possível consultar as matrículas. {TraduzirErro(ex.Message)}",
                "Entendi");
        }
        finally
        {
            DefinirCarregando(false);
        }
    }

    private void AplicarFiltro(string? filtro)
    {
        IEnumerable<MatriculaDto> resultado = _matriculas;
        var termo = filtro?.Trim();

        if (!string.IsNullOrWhiteSpace(termo))
        {
            var cpf = ApenasDigitos(termo);
            resultado = resultado.Where(item =>
                Contem(item.Aluno?.Nome, termo) ||
                Contem(item.Aluno?.Cpf, termo) ||
                (!string.IsNullOrWhiteSpace(cpf) && Contem(item.Aluno?.Cpf, cpf)) ||
                Contem(item.Objetivo, termo) ||
                Contem(ObterRestricoes(item.Restricoes), termo));
        }

        var itens = resultado.ToList();
        MatriculasCollection.ItemsSource = itens;
        QuantidadeLabel.Text = itens.Count == 1
            ? "1 matrícula encontrada"
            : $"{itens.Count} matrículas encontradas";
    }

    private async void OnPesquisaTextChanged(object? sender, TextChangedEventArgs e)
    {
        _pesquisaCts?.Cancel();
        _pesquisaCts?.Dispose();
        _pesquisaCts = new CancellationTokenSource();

        try
        {
            await Task.Delay(250, _pesquisaCts.Token);
            AplicarFiltro(e.NewTextValue);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async void OnAtualizarClicked(object? sender, EventArgs e) => await CarregarAsync();

    private async void OnNovaClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(MatriculaPage.Route);

    private async void OnEditarClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: MatriculaDto matricula })
            await Shell.Current.GoToAsync($"{MatriculaPage.Route}?id={matricula.Id}");
    }

    private async void OnExcluirClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: MatriculaDto matricula })
            return;

        var aluno = matricula.Aluno?.Nome ?? $"ID {matricula.AlunoId}";
        var confirmar = await DisplayAlertAsync(
            "Excluir matrícula",
            $"Deseja excluir a matrícula de {aluno}? Esta ação não pode ser desfeita.",
            "Excluir",
            "Cancelar");

        if (!confirmar)
            return;

        try
        {
            DefinirCarregando(true);
            if (!await _matriculaService.RemoverAsync(matricula.Id))
                await DisplayAlertAsync("Matrícula", "A matrícula já não existe.", "OK");
            await CarregarAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Não foi possível excluir", TraduzirErro(ex.Message), "OK");
        }
        finally
        {
            DefinirCarregando(false);
        }
    }

    private void DefinirCarregando(bool carregando)
    {
        LoadingIndicator.IsVisible = carregando;
        LoadingIndicator.IsRunning = carregando;
    }

    private static bool Contem(string? valor, string termo) =>
        valor?.Contains(termo, StringComparison.OrdinalIgnoreCase) == true;

    private static string ApenasDigitos(string? valor) =>
        string.Concat((valor ?? string.Empty).Where(char.IsDigit));

    private static string ObterRestricoes(AppMatriculaRestricoes restricoes) =>
        string.Join(" ", Enum.GetValues<AppMatriculaRestricoes>()
            .Where(item => item != AppMatriculaRestricoes.None && restricoes.HasFlag(item)));

    private static string TraduzirErro(string codigo) => codigo switch
    {
        "MATRICULA_NAO_ENCONTRADA" => "Matrícula não encontrada.",
        _ => codigo
    };
}
