using AcademiadoZE.Domain.ValueObjects;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AcademiaDoZe.Presentation.Pages;

public partial class AlunoPage : ContentPage, IQueryAttributable
{
    public const string Route = "aluno-form";
    private readonly IAlunoService _alunoService;
    private readonly ILogradouroService _logradouroService;
    private readonly ILogger<AlunoPage> _logger;
    private int _id;
    private bool _carregado;
    private bool _salvando;
    private byte[]? _fotoBytes;

    public AlunoPage(
        IAlunoService alunoService,
        ILogradouroService logradouroService,
        ILogger<AlunoPage> logger)
    {
        InitializeComponent();
        _alunoService = alunoService;
        _logradouroService = logradouroService;
        _logger = logger;
        NascimentoPicker.MaximumDate = DateTime.Today.AddYears(-14);
        NascimentoPicker.Date = DateTime.Today.AddYears(-14);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query) =>
        _id = query.TryGetValue("id", out var value) && int.TryParse(value?.ToString(), out var id) ? id : 0;

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_carregado) return;
        try
        {
            var logradouros = (await _logradouroService.ObterTodosAsync()).ToList();
            LogradouroPicker.ItemsSource = logradouros;
            LogradouroPicker.ItemDisplayBinding = new Binding(nameof(LogradouroDto.Nome));
            if (_id > 0) await CarregarAlunoAsync();
            _carregado = true;
        }
        catch (Exception ex) { await DisplayAlertAsync("Erro", AppErrorMessages.From(ex, _logger, "abrir formulário de aluno"), "OK"); }
    }

    private async Task CarregarAlunoAsync()
    {
        var aluno = await _alunoService.ObterPorIdAsync(_id) ?? throw new InvalidOperationException("ALUNO_NAO_ENCONTRADO");
        Title = "Editar aluno";
        TituloLabel.Text = "EDITAR ALUNO";
        NomeEntry.Text = aluno.Nome;
        CpfEntry.Text = aluno.Cpf;
        NascimentoPicker.Date = aluno.DataNascimento.ToDateTime(TimeOnly.MinValue);
        TelefoneEntry.Text = aluno.Telefone;
        EmailEntry.Text = aluno.Email;
        NumeroEntry.Text = aluno.Numero;
        ComplementoEntry.Text = aluno.Complemento;
        LogradouroPicker.SelectedItem = (LogradouroPicker.ItemsSource as IEnumerable<LogradouroDto>)?
            .FirstOrDefault(item => item.Id == aluno.Endereco?.Id);
        _fotoBytes = aluno.Foto?.Conteudo;
        AtualizarFoto();
        SenhaEntry.Placeholder = "Nova senha (deixe em branco para manter)";
        ConfirmarSenhaEntry.Placeholder = "Confirmar nova senha";
    }

    private async void CameraButton_Clicked(object? sender, EventArgs e) => await SelecionarFotoAsync(true);
    private async void GaleriaButton_Clicked(object? sender, EventArgs e) => await SelecionarFotoAsync(false);

    private async Task SelecionarFotoAsync(bool capturar)
    {
        try
        {
            FileResult? arquivo = capturar
                ? await MediaPicker.Default.CapturePhotoAsync()
                : (await MediaPicker.Default.PickPhotosAsync()).FirstOrDefault();
            if (arquivo is null) return;
            await using var stream = await arquivo.OpenReadAsync();
            using var memoria = new MemoryStream();
            await stream.CopyToAsync(memoria);
            if (memoria.Length > 10 * 1024 * 1024) throw new InvalidOperationException("A fotografia deve ter no máximo 10 MB.");
            _fotoBytes = memoria.ToArray();
            AtualizarFoto();
        }
        catch (FeatureNotSupportedException) { await DisplayAlertAsync("Câmera indisponível", "Use a galeria neste dispositivo.", "OK"); }
        catch (PermissionException) { await DisplayAlertAsync("Permissão necessária", "Autorize o acesso à câmera ou à galeria.", "OK"); }
        catch (Exception ex) { await DisplayAlertAsync("Fotografia", AppErrorMessages.From(ex, _logger, "selecionar fotografia de aluno"), "OK"); }
    }

    private void AtualizarFoto() => FotoImage.Source = _fotoBytes is { Length: > 0 }
        ? ImageSource.FromStream(() => new MemoryStream(_fotoBytes)) : null;

    private async void SalvarButton_Clicked(object? sender, EventArgs e)
    {
        if (_salvando) return;
        var erro = ValidarFormulario();
        if (erro is not null) { await DisplayAlertAsync("Verifique os dados", erro, "OK"); return; }
        try
        {
            _salvando = true;
            SalvarButton.IsEnabled = false;
            var dto = new AlunoDto
            {
                Id = _id, Nome = NomeEntry.Text!.Trim(), Cpf = CpfEntry.Text!.Trim(),
                DataNascimento = DateOnly.FromDateTime(NascimentoPicker.Date ?? DateTime.Today), Telefone = TelefoneEntry.Text!.Trim(),
                Email = EmailEntry.Text!.Trim(), Endereco = (LogradouroDto)LogradouroPicker.SelectedItem!,
                Numero = NumeroEntry.Text!.Trim(), Complemento = string.IsNullOrWhiteSpace(ComplementoEntry.Text) ? null : ComplementoEntry.Text.Trim(),
                Senha = string.IsNullOrWhiteSpace(SenhaEntry.Text) ? null : SenhaEntry.Text,
                Foto = _fotoBytes is { Length: > 0 } ? new ArquivoDto { Conteudo = _fotoBytes } : null,
                Responsavel = null
            };
            if (_id == 0) await _alunoService.AdicionarAsync(dto); else await _alunoService.AtualizarAsync(dto);
            await DisplayAlertAsync("Sucesso", _id == 0 ? "Aluno cadastrado." : "Aluno atualizado.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex) { await DisplayAlertAsync("Não foi possível salvar", AppErrorMessages.From(ex, _logger, "salvar aluno"), "OK"); }
        finally { _salvando = false; SalvarButton.IsEnabled = true; }
    }

    private string? ValidarFormulario()
    {
        if (string.IsNullOrWhiteSpace(NomeEntry.Text)) return "Informe o nome completo.";
        if (Cpf.Criar(CpfEntry.Text).IsFailure) return "Informe um CPF válido.";
        if (Telefone.Criar(TelefoneEntry.Text).IsFailure) return "Informe um telefone com DDD e 11 dígitos.";
        if (AcademiadoZE.Domain.ValueObjects.Email.Criar(EmailEntry.Text).IsFailure) return "Informe um e-mail válido.";
        if (LogradouroPicker.SelectedItem is null) return "Selecione um logradouro.";
        if (string.IsNullOrWhiteSpace(NumeroEntry.Text)) return "Informe o número do endereço.";
        if (_id == 0 && string.IsNullOrWhiteSpace(SenhaEntry.Text)) return "Informe uma senha.";
        if (!string.IsNullOrWhiteSpace(SenhaEntry.Text) && PoliticaSenha.Validar(SenhaEntry.Text).IsFailure)
            return "A senha deve ter no mínimo 8 caracteres, uma letra maiúscula e um número.";
        if (SenhaEntry.Text != ConfirmarSenhaEntry.Text) return "A confirmação da senha não confere.";
        return null;
    }

    private async void CancelarButton_Clicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("..");
}
