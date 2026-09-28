using AcademiaDoZe.Application.Interfaces;

namespace AcademiaDoZe.Presentation.Pages;
//Mario Cesar Alves Júnior

public partial class DashboardPage : ContentPage
{
    private readonly ILogradouroService _logradouroService;
    private bool _isLoading;

    public DashboardPage(ILogradouroService logradouroService)
    {
        InitializeComponent();
        _logradouroService = logradouroService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarTotalLogradourosAsync();
    }

    private async Task CarregarTotalLogradourosAsync()
    {
        if (_isLoading)
            return;

        try
        {
            _isLoading = true;
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            TotalLogradourosLabel.IsVisible = false;
            StatusLabel.IsVisible = false;

            var logradouros = await _logradouroService.ObterTodosAsync();
            TotalLogradourosLabel.Text = logradouros.Count().ToString();
        }
        catch (Exception)
        {
            TotalLogradourosLabel.Text = "--";
            StatusLabel.Text = "Não foi possível consultar o MySQL.";
            StatusLabel.IsVisible = true;
        }
        finally
        {
            _isLoading = false;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            TotalLogradourosLabel.IsVisible = true;
        }
    }

    private async void OnAcessarLogradourosClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//logradouros/logradouros-page");
    }
}
