using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.Helpers;

namespace AcademiaDoZe.Presentation.Pages;
//Mario Cesar Alves Júnior

[QueryProperty(nameof(LogradouroId), "id")]
public partial class LogradouroFormPage : ContentPage
{
    public const string Route = "logradouro-formulario";

    private readonly ILogradouroService _logradouroService;
    private int _logradouroId;
    private bool _isInitialized;
    private bool _isBusy;
    private bool _isFormattingCep;

    public string? LogradouroId
    {
        set => _logradouroId = int.TryParse(value, out var id) ? id : 0;
    }

    public LogradouroFormPage(ILogradouroService logradouroService)
    {
        InitializeComponent();
        _logradouroService = logradouroService;
        PaisEntry.Text = "Brasil";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isInitialized)
            return;

        _isInitialized = true;
        if (_logradouroId > 0)
            await CarregarLogradouroAsync();
    }

    private async Task CarregarLogradouroAsync()
    {
        try
        {
            DefinirCarregamento(true);
            var logradouro = await _logradouroService.ObterPorIdAsync(_logradouroId);

            if (logradouro is null)
            {
                await DisplayAlertAsync(
                    "Logradouro não encontrado",
                    "O logradouro não foi encontrado.",
                    "Entendi");
                await Shell.Current.GoToAsync("..");
                return;
            }

            TituloLabel.Text = "Editar Logradouro";
            DescricaoLabel.Text = "Atualize os dados do endereço selecionado.";
            SalvarButton.Text = "Salvar Alterações";
            CepEntry.Text = FormatarCep(logradouro.Cep);
            NomeEntry.Text = logradouro.Nome;
            BairroEntry.Text = logradouro.Bairro;
            CidadeEntry.Text = logradouro.Cidade;
            EstadoEntry.Text = logradouro.Estado;
            PaisEntry.Text = logradouro.Pais;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Não foi possível carregar",
                LogradouroErrorMessages.From(
                    ex,
                    "Não foi possível carregar o logradouro. Verifique a conexão com o MySQL."),
                "Entendi");
            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            DefinirCarregamento(false);
        }
    }

    private async void OnSalvarClicked(object? sender, EventArgs e)
    {
        if (_isBusy)
            return;

        var cep = ApenasDigitos(CepEntry.Text);
        if (!ValidarFormulario(cep))
            return;

        var logradouro = new LogradouroDto
        {
            Id = _logradouroId,
            Cep = cep,
            Nome = NomeEntry.Text!.Trim(),
            Bairro = BairroEntry.Text!.Trim(),
            Cidade = CidadeEntry.Text!.Trim(),
            Estado = EstadoEntry.Text!.Trim(),
            Pais = PaisEntry.Text!.Trim()
        };

        try
        {
            DefinirCarregamento(true);

            if (_logradouroId == 0)
                await _logradouroService.AdicionarAsync(logradouro);
            else
                await _logradouroService.AtualizarAsync(logradouro);

            var mensagem = _logradouroId == 0
                ? "Logradouro cadastrado com sucesso."
                : "Logradouro atualizado com sucesso.";

            await DisplayAlertAsync("Operação concluída", mensagem, "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ValidationLabel.Text = LogradouroErrorMessages.From(
                ex,
                "Não foi possível salvar o logradouro. Verifique a conexão com o MySQL.");
            ValidationLabel.IsVisible = true;
        }
        finally
        {
            DefinirCarregamento(false);
        }
    }

    private bool ValidarFormulario(string cep)
    {
        ValidationLabel.IsVisible = true;

        if (cep.Length != 8)
        {
            ValidationLabel.Text = "Informe um CEP válido com 8 dígitos.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(NomeEntry.Text) ||
            string.IsNullOrWhiteSpace(BairroEntry.Text) ||
            string.IsNullOrWhiteSpace(CidadeEntry.Text) ||
            string.IsNullOrWhiteSpace(EstadoEntry.Text) ||
            string.IsNullOrWhiteSpace(PaisEntry.Text))
        {
            ValidationLabel.Text = "Preencha todos os campos obrigatórios.";
            return false;
        }

        ValidationLabel.IsVisible = false;
        return true;
    }

    private void DefinirCarregamento(bool carregando)
    {
        _isBusy = carregando;
        LoadingIndicator.IsVisible = carregando;
        LoadingIndicator.IsRunning = carregando;
        SalvarButton.IsEnabled = !carregando;
    }

    private void OnCepTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isFormattingCep)
            return;

        var formatado = FormatarCep(e.NewTextValue);
        if (formatado == e.NewTextValue)
            return;

        _isFormattingCep = true;
        CepEntry.Text = formatado;
        CepEntry.CursorPosition = formatado.Length;
        _isFormattingCep = false;
    }

    private static string FormatarCep(string? valor)
    {
        var digitos = ApenasDigitos(valor);
        if (digitos.Length > 8)
            digitos = digitos[..8];

        return digitos.Length > 5
            ? $"{digitos[..5]}-{digitos[5..]}"
            : digitos;
    }

    private static string ApenasDigitos(string? valor) =>
        string.Concat((valor ?? string.Empty).Where(char.IsDigit));

    private async void OnCancelarClicked(object? sender, EventArgs e)
    {
        if (!_isBusy)
            await Shell.Current.GoToAsync("..");
    }
}
