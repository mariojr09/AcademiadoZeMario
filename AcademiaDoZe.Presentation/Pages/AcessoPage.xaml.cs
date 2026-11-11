using AcademiadoZE.Domain.Enums;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Presentation.Pages;

public partial class AcessoPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;
    private AcessoPessoaTipo _tipo = AcessoPessoaTipo.Aluno;
    private bool _carregando;

    public AcessoPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await CarregarPessoasAsync();
            await CarregarAcessosAsync();
        }
        catch (Exception ex)
        {
            MostrarMensagem(TraduzirErro(ex), false);
        }
    }

    private async Task CarregarPessoasAsync(int? pessoaIdSelecionada = null)
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IAcessoService>();
        var pessoas = (_tipo == AcessoPessoaTipo.Aluno
            ? await service.ListarAlunosAsync()
            : await service.ListarColaboradoresAsync()).ToList();

        PessoaPicker.ItemsSource = pessoas;

        if (pessoas.Count > 0)
        {
            int indice = pessoaIdSelecionada.HasValue
                ? pessoas.FindIndex(pessoa => pessoa.Id == pessoaIdSelecionada.Value)
                : 0;

            PessoaPicker.SelectedIndex = indice >= 0 ? indice : 0;
            await AtualizarSituacaoAsync();
        }
        else
        {
            PessoaPicker.SelectedItem = null;
            RegistrarButton.IsEnabled = false;
            SituacaoLabel.Text = _tipo == AcessoPessoaTipo.Aluno
                ? "Nenhum aluno cadastrado. Cadastre um aluno antes de registrar o acesso."
                : "Nenhum colaborador cadastrado. Cadastre um colaborador antes de registrar o acesso.";
        }
    }

    private async Task CarregarAcessosAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IAcessoService>();
        AcessosCollection.ItemsSource = (await service.ListarAcessosDoDiaAsync(DateOnly.FromDateTime(DateTime.Now))).ToList();
    }

    private async Task AtualizarSituacaoAsync()
    {
        if (PessoaPicker.SelectedItem is not AcessoPessoaDto pessoa)
        {
            RegistrarButton.IsEnabled = false;
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IAcessoService>();
        var aberto = _tipo == AcessoPessoaTipo.Aluno
            ? await service.ObterAbertoAlunoAsync(pessoa.Id)
            : await service.ObterAbertoColaboradorAsync(pessoa.Id);

        bool dentro = aberto is not null;
        SituacaoLabel.Text = dentro
            ? $"{pessoa.Nome} está dentro da academia desde {aberto!.DataHoraChegada:HH:mm:ss}."
            : $"{pessoa.Nome} está fora da academia.";
        RegistrarButton.Text = dentro ? "Registrar saída" : "Registrar entrada";
        RegistrarButton.IsEnabled = true;

        if (_tipo == AcessoPessoaTipo.Colaborador)
        {
            var resumo = await service.ObterResumoDiarioColaboradorAsync(pessoa.Id, DateTime.Now);
            HorasTrabalhadasLabel.Text = resumo.HorasTrabalhadasDescricao;
            JornadaPrevistaLabel.Text = $"{resumo.JornadaPrevistaDescricao} ({resumo.VinculoDescricao})";
            SaldoHorasLabel.Text = resumo.SaldoHorasDescricao;
            SaldoHorasLabel.TextColor = Color.FromArgb(resumo.SaldoHoras < TimeSpan.Zero ? "#C05621" : "#147D92");
            ResumoColaboradorBorder.IsVisible = true;
        }
        else
        {
            ResumoColaboradorBorder.IsVisible = false;
        }
    }

    private async Task AlterarTipoAsync(AcessoPessoaTipo tipo)
    {
        _tipo = tipo;
        bool aluno = tipo == AcessoPessoaTipo.Aluno;
        AlunoButton.BackgroundColor = Color.FromArgb(aluno ? "#102A43" : "#DCE5EF");
        AlunoButton.TextColor = Color.FromArgb(aluno ? "#FFFFFF" : "#102A43");
        ColaboradorButton.BackgroundColor = Color.FromArgb(aluno ? "#DCE5EF" : "#102A43");
        ColaboradorButton.TextColor = Color.FromArgb(aluno ? "#102A43" : "#FFFFFF");
        SelecaoLabel.Text = aluno ? "Selecione o aluno" : "Selecione o colaborador";
        ResumoColaboradorBorder.IsVisible = !aluno;
        MensagemBorder.IsVisible = false;
        await CarregarPessoasAsync();
    }

    private async void AlunoButton_Clicked(object? sender, EventArgs e) =>
        await AlterarTipoAsync(AcessoPessoaTipo.Aluno);

    private async void ColaboradorButton_Clicked(object? sender, EventArgs e) =>
        await AlterarTipoAsync(AcessoPessoaTipo.Colaborador);

    private async void PessoaPicker_SelectedIndexChanged(object? sender, EventArgs e)
    {
        try { await AtualizarSituacaoAsync(); }
        catch (Exception ex) { MostrarMensagem(TraduzirErro(ex), false); }
    }

    private async void RegistrarButton_Clicked(object? sender, EventArgs e)
    {
        if (_carregando || PessoaPicker.SelectedItem is not AcessoPessoaDto pessoa)
            return;

        try
        {
            _carregando = true;
            RegistrarButton.IsEnabled = false;
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IAcessoService>();
            DateTime agora = DateTime.Now;

            AcessoResultadoDto resultado;
            if (_tipo == AcessoPessoaTipo.Aluno)
            {
                resultado = await service.ObterAbertoAlunoAsync(pessoa.Id) is null
                    ? await service.RegistrarEntradaAlunoAsync(pessoa.Id, agora)
                    : await service.RegistrarSaidaAlunoAsync(pessoa.Id, agora);
            }
            else
            {
                resultado = await service.ObterAbertoColaboradorAsync(pessoa.Id) is null
                    ? await service.RegistrarEntradaColaboradorAsync(pessoa.Id, agora)
                    : await service.RegistrarSaidaColaboradorAsync(pessoa.Id, agora);
            }

            MostrarMensagem(resultado.Mensagem, true);
            await AtualizarSituacaoAsync();
            await CarregarAcessosAsync();
        }
        catch (Exception ex)
        {
            MostrarMensagem(TraduzirErro(ex), false);
        }
        finally
        {
            _carregando = false;
            RegistrarButton.IsEnabled = PessoaPicker.SelectedItem is not null;
        }
    }

    private async void AtualizarButton_Clicked(object? sender, EventArgs e)
    {
        if (_carregando) return;

        try
        {
            _carregando = true;
            int? pessoaId = (PessoaPicker.SelectedItem as AcessoPessoaDto)?.Id;
            await CarregarPessoasAsync(pessoaId);
            await CarregarAcessosAsync();
            MostrarMensagem("Dados atualizados com sucesso.", true);
        }
        catch (Exception ex) { MostrarMensagem(TraduzirErro(ex), false); }
        finally { _carregando = false; }
    }

    private void MostrarMensagem(string mensagem, bool sucesso)
    {
        MensagemLabel.Text = mensagem;
        MensagemBorder.BackgroundColor = Color.FromArgb(sucesso ? "#DDF3F5" : "#FFF0EB");
        MensagemLabel.TextColor = Color.FromArgb(sucesso ? "#102A43" : "#C53030");
        MensagemBorder.IsVisible = true;
    }

    private static string TraduzirErro(Exception exception) => exception.Message switch
    {
        "ALUNO_SEM_MATRICULA_ATIVA" => "Acesso negado: o aluno não possui matrícula ativa.",
        "ALUNO_JA_ESTA_NA_ACADEMIA" => "O aluno já possui uma entrada sem saída.",
        "ALUNO_NAO_ESTA_NA_ACADEMIA" => "Não existe entrada aberta para este aluno.",
        "COLABORADOR_JA_ESTA_NA_ACADEMIA" => "O colaborador já possui uma entrada sem saída.",
        "COLABORADOR_NAO_ESTA_NA_ACADEMIA" => "Não existe entrada aberta para este colaborador.",
        "JORNADA_DIARIA_JA_CUMPRIDA" => "A jornada diária deste colaborador já foi cumprida.",
        _ => $"Não foi possível concluir a operação: {exception.Message}"
    };
}
