using System.ComponentModel.DataAnnotations;
using System.Reflection;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;

namespace AcademiaDoZe.Presentation.Pages;

// Mario Cesar Alves Júnior
[QueryProperty(nameof(MatriculaId), "id")]
public partial class MatriculaPage : ContentPage
{
    public const string Route = "matricula-formulario";

    private readonly IMatriculaService _matriculaService;
    private readonly IAlunoService _alunoService;
    private readonly Dictionary<AppMatriculaRestricoes, CheckBox> _restricoes = [];
    private int _matriculaId;
    private bool _inicializada;
    private bool _ocupada;
    private AlunoDto? _aluno;
    private byte[]? _laudoBytes;

    public string? MatriculaId
    {
        set => _matriculaId = int.TryParse(value, out var id) ? id : 0;
    }

    public MatriculaPage(IMatriculaService matriculaService, IAlunoService alunoService)
    {
        InitializeComponent();
        _matriculaService = matriculaService;
        _alunoService = alunoService;

        PlanoPicker.ItemsSource = Enum.GetValues<AppMatriculaPlano>()
            .Select(valor => new OpcaoPlano(valor, ObterDescricao(valor)))
            .ToList();
        PlanoPicker.ItemDisplayBinding = new Binding(nameof(OpcaoPlano.Descricao));
        PlanoPicker.SelectedIndex = 0;
        InicioPicker.Date = DateTime.Today;
        CriarRestricoes();
        CalcularFim();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_inicializada)
            return;

        _inicializada = true;
        if (_matriculaId > 0)
            await CarregarAsync();
    }

    private void CriarRestricoes()
    {
        foreach (var restricao in Enum.GetValues<AppMatriculaRestricoes>()
                     .Where(item => item != AppMatriculaRestricoes.None))
        {
            var checkBox = new CheckBox
            {
                Color = Color.FromArgb("#147D92"),
                VerticalOptions = LayoutOptions.Center
            };
            checkBox.CheckedChanged += OnRestricaoAlterada;

            var linha = new HorizontalStackLayout { Spacing = 8 };
            linha.Children.Add(checkBox);
            linha.Children.Add(new Label
            {
                Text = ObterDescricao(restricao),
                VerticalOptions = LayoutOptions.Center
            });

            _restricoes[restricao] = checkBox;
            RestricoesContainer.Children.Add(linha);
        }
    }

    private async Task CarregarAsync()
    {
        try
        {
            DefinirOcupada(true);
            var matricula = await _matriculaService.ObterPorIdAsync(_matriculaId);
            if (matricula is null)
            {
                await DisplayAlertAsync("Matrícula", "Cadastro não encontrado.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            TituloLabel.Text = "EDITAR MATRÍCULA";
            SalvarButton.Text = "Salvar alterações";
            _aluno = matricula.Aluno ?? await _alunoService.ObterPorIdAsync(matricula.AlunoId);
            AtualizarAluno();
            PlanoPicker.SelectedItem = ((IEnumerable<OpcaoPlano>)PlanoPicker.ItemsSource)
                .First(item => item.Valor == matricula.Plano);
            InicioPicker.Date = matricula.DataInicio.ToDateTime(TimeOnly.MinValue);
            FimPicker.Date = matricula.DataFinal.ToDateTime(TimeOnly.MinValue);
            ObjetivoEntry.Text = matricula.Objetivo;
            ObservacoesEditor.Text = matricula.ObservacoesRestricoes;

            foreach (var item in _restricoes)
                item.Value.IsChecked = matricula.Restricoes.HasFlag(item.Key);

            _laudoBytes = matricula.LaudoMedico?.Conteudo;
            AtualizarLaudo();
            AtualizarObrigatoriedadeLaudo();
        }
        catch (Exception ex)
        {
            ExibirErro(TraduzirErro(ex.Message));
        }
        finally
        {
            DefinirOcupada(false);
        }
    }

    private async void OnBuscarCpfClicked(object? sender, EventArgs e)
    {
        var cpf = ApenasDigitos(CpfEntry.Text);
        if (cpf.Length != 11)
        {
            ExibirErro("Informe um CPF válido com 11 dígitos.");
            return;
        }

        try
        {
            DefinirOcupada(true);
            _aluno = await _alunoService.ObterPorCpfAsync(cpf);
            if (_aluno is null)
            {
                AtualizarAluno();
                ExibirErro("Aluno não encontrado. Cadastre o aluno antes de criar a matrícula.");
                return;
            }

            ErroLabel.IsVisible = false;
            AtualizarAluno();
            AtualizarObrigatoriedadeLaudo();
        }
        catch (Exception ex)
        {
            ExibirErro(TraduzirErro(ex.Message));
        }
        finally
        {
            DefinirOcupada(false);
        }
    }

    private void AtualizarAluno()
    {
        AlunoCard.IsVisible = _aluno is not null;
        AlunoAjudaLabel.IsVisible = _aluno is null;

        if (_aluno is null)
            return;

        CpfEntry.Text = _aluno.Cpf;
        AlunoNomeLabel.Text = _aluno.Nome;
        AlunoCpfLabel.Text = $"CPF: {_aluno.Cpf}";
        AlunoNascimentoLabel.Text = $"Nascimento: {_aluno.DataNascimento:dd/MM/yyyy}";
        AlunoFotoImage.Source = _aluno.Foto?.Conteudo is { Length: > 0 } foto
            ? ImageSource.FromStream(() => new MemoryStream(foto))
            : null;
    }

    private void OnPlanoOuInicioAlterado(object? sender, EventArgs e) => CalcularFim();

    private void CalcularFim()
    {
        if (PlanoPicker.SelectedItem is not OpcaoPlano plano)
            return;

        var inicio = DateOnly.FromDateTime(InicioPicker.Date ?? DateTime.Today);
        var fim = plano.Valor switch
        {
            AppMatriculaPlano.Mensal => inicio.AddMonths(1),
            AppMatriculaPlano.Trimestral => inicio.AddMonths(3),
            AppMatriculaPlano.Semestral => inicio.AddMonths(6),
            AppMatriculaPlano.Anual => inicio.AddYears(1),
            _ => inicio
        };
        FimPicker.Date = fim.ToDateTime(TimeOnly.MinValue);
        AtualizarObrigatoriedadeLaudo();
    }

    private void OnRestricaoAlterada(object? sender, CheckedChangedEventArgs e) =>
        AtualizarObrigatoriedadeLaudo();

    private AppMatriculaRestricoes ObterRestricoes()
    {
        var resultado = AppMatriculaRestricoes.None;
        foreach (var item in _restricoes.Where(item => item.Value.IsChecked))
            resultado |= item.Key;
        return resultado;
    }

    private bool ExigeLaudoPorIdade()
    {
        if (_aluno is null)
            return false;

        var referencia = DateOnly.FromDateTime(InicioPicker.Date ?? DateTime.Today);
        var idade = referencia.Year - _aluno.DataNascimento.Year;
        if (_aluno.DataNascimento > referencia.AddYears(-idade))
            idade--;
        return idade is >= 12 and <= 16;
    }

    private void AtualizarObrigatoriedadeLaudo()
    {
        var obrigatorio = ExigeLaudoPorIdade() || ObterRestricoes() != AppMatriculaRestricoes.None;
        LaudoStatusLabel.Text = _laudoBytes is { Length: > 0 }
            ? "Laudo anexado"
            : obrigatorio ? "Obrigatório" : "Opcional";
        LaudoStatusLabel.TextColor = _laudoBytes is { Length: > 0 }
            ? Color.FromArgb("#147D92")
            : obrigatorio ? Color.FromArgb("#C53030") : Color.FromArgb("#71869A");
    }

    private async void OnGaleriaLaudoClicked(object? sender, EventArgs e)
    {
        try
        {
            var arquivo = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Selecione a imagem do laudo médico",
                FileTypes = FilePickerFileType.Images
            });
            await LerLaudoAsync(arquivo);
        }
        catch (Exception ex)
        {
            ExibirErro($"Não foi possível selecionar o laudo: {ex.Message}");
        }
    }

    private async void OnCameraLaudoClicked(object? sender, EventArgs e)
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                ExibirErro("A câmera não está disponível neste dispositivo.");
                return;
            }
            await LerLaudoAsync(await MediaPicker.Default.CapturePhotoAsync());
        }
        catch (Exception ex)
        {
            ExibirErro($"Não foi possível fotografar o laudo: {ex.Message}");
        }
    }

    private async Task LerLaudoAsync(FileResult? arquivo)
    {
        if (arquivo is null)
            return;

        await using var stream = await arquivo.OpenReadAsync();
        using var memoria = new MemoryStream();
        await stream.CopyToAsync(memoria);
        if (memoria.Length > 5 * 1024 * 1024)
            throw new InvalidOperationException("O laudo deve ter no máximo 5 MB.");

        _laudoBytes = memoria.ToArray();
        AtualizarLaudo();
    }

    private void OnRemoverLaudoClicked(object? sender, EventArgs e)
    {
        _laudoBytes = null;
        AtualizarLaudo();
    }

    private void AtualizarLaudo()
    {
        LaudoImage.Source = _laudoBytes is { Length: > 0 } laudo
            ? ImageSource.FromStream(() => new MemoryStream(laudo))
            : null;
        RemoverLaudoButton.IsVisible = _laudoBytes is { Length: > 0 };
        AtualizarObrigatoriedadeLaudo();
    }

    private async void OnSalvarClicked(object? sender, EventArgs e)
    {
        if (_ocupada)
            return;

        var erro = ValidarFormulario();
        if (erro is not null)
        {
            ExibirErro(erro);
            return;
        }

        var plano = (OpcaoPlano)PlanoPicker.SelectedItem;
        var dto = new MatriculaDto
        {
            Id = _matriculaId,
            AlunoId = _aluno!.Id,
            Aluno = _aluno,
            Plano = plano.Valor,
            DataInicio = DateOnly.FromDateTime(InicioPicker.Date ?? DateTime.Today),
            DataFinal = DateOnly.FromDateTime(FimPicker.Date ?? DateTime.Today),
            Objetivo = ObjetivoEntry.Text!.Trim(),
            Restricoes = ObterRestricoes(),
            ObservacoesRestricoes = string.IsNullOrWhiteSpace(ObservacoesEditor.Text)
                ? null
                : ObservacoesEditor.Text.Trim(),
            LaudoMedico = _laudoBytes is null ? null : new ArquivoDto { Conteudo = _laudoBytes }
        };

        try
        {
            DefinirOcupada(true);
            if (_matriculaId == 0)
                await _matriculaService.AdicionarAsync(dto);
            else
                await _matriculaService.AtualizarAsync(dto);

            await DisplayAlertAsync(
                "Operação concluída",
                _matriculaId == 0 ? "Matrícula cadastrada com sucesso." : "Matrícula atualizada com sucesso.",
                "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ExibirErro(TraduzirErro(ex.Message));
        }
        finally
        {
            DefinirOcupada(false);
        }
    }

    private string? ValidarFormulario()
    {
        if (_aluno is null)
            return "Busque e selecione um aluno válido pelo CPF.";
        if (PlanoPicker.SelectedItem is null)
            return "Selecione um plano.";
        if (string.IsNullOrWhiteSpace(ObjetivoEntry.Text))
            return "Informe o objetivo da matrícula.";
        if ((ExigeLaudoPorIdade() || ObterRestricoes() != AppMatriculaRestricoes.None) &&
            _laudoBytes is not { Length: > 0 })
            return "Anexe o laudo médico obrigatório para a idade ou restrição informada.";
        return null;
    }

    private async void OnCancelarClicked(object? sender, EventArgs e)
    {
        if (!_ocupada)
            await Shell.Current.GoToAsync("..");
    }

    private void DefinirOcupada(bool ocupada)
    {
        _ocupada = ocupada;
        LoadingIndicator.IsVisible = ocupada;
        LoadingIndicator.IsRunning = ocupada;
        SalvarButton.IsEnabled = !ocupada;
    }

    private void ExibirErro(string mensagem)
    {
        ErroLabel.Text = mensagem;
        ErroLabel.IsVisible = true;
    }

    private static string ApenasDigitos(string? valor) =>
        string.Concat((valor ?? string.Empty).Where(char.IsDigit));

    private static string ObterDescricao(Enum valor) =>
        valor.GetType().GetMember(valor.ToString()).First()
            .GetCustomAttribute<DisplayAttribute>()?.GetName() ?? valor.ToString();

    private static string TraduzirErro(string codigo) => codigo switch
    {
        "ALUNO_NAO_ENCONTRADO" => "O aluno selecionado não foi encontrado.",
        "ALUNO_JA_POSSUI_MATRICULA_VIGENTE" => "O aluno já possui uma matrícula ativa.",
        "LAUDO_MEDICO_OBRIGATORIO_PARA_RESTRICAO" => "Anexe um laudo médico para as restrições selecionadas.",
        "LAUDO_MEDICO_OBRIGATORIO_ENTRE_12_E_16_ANOS" => "Alunos de 12 a 16 anos precisam apresentar laudo médico.",
        "LAUDO_MEDICO_INVALIDO" => "O arquivo do laudo médico é inválido.",
        "DADOS_MATRICULA_INVALIDOS" => "Revise os dados da matrícula.",
        "MATRICULA_NAO_ENCONTRADA" => "Matrícula não encontrada.",
        _ => codigo
    };

    private sealed record OpcaoPlano(AppMatriculaPlano Valor, string Descricao);
}
