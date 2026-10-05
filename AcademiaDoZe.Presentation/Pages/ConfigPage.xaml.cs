namespace AcademiaDoZe.Presentation.Pages;

public partial class ConfigPage : ContentPage
{
    public ConfigPage()
    {
        InitializeComponent();

        CarregarConfiguracoes();
    }

    private void CarregarConfiguracoes()
    {
        ServidorEntry.Text = Preferences.Default.Get(
            "DbServidor",
            "127.0.0.1");

        BancoEntry.Text = Preferences.Default.Get(
            "DbBanco",
            "db_academia_do_ze");

        UsuarioEntry.Text = Preferences.Default.Get(
            "DbUsuario",
            "mario");

        TemaPicker.SelectedItem = Preferences.Default.Get(
            "AppTema",
            "Sistema");
    }

    private async void SalvarButton_Clicked(object sender, EventArgs e)
    {
        Preferences.Default.Set(
            "DbServidor",
            ServidorEntry.Text?.Trim() ?? "");

        Preferences.Default.Set(
            "DbBanco",
            BancoEntry.Text?.Trim() ?? "");

        Preferences.Default.Set(
            "DbUsuario",
            UsuarioEntry.Text?.Trim() ?? "");

        if (!string.IsNullOrWhiteSpace(SenhaEntry.Text))
        {
            Preferences.Default.Set(
                "DbSenha",
                SenhaEntry.Text);
        }

        var tema = TemaPicker.SelectedItem?.ToString() ?? "Sistema";

        Preferences.Default.Set(
            "AppTema",
            tema);

        if (Microsoft.Maui.Controls.Application.Current != null)
        {
            Microsoft.Maui.Controls.Application.Current.UserAppTheme = tema switch
            {
                "Claro" => AppTheme.Light,
                "Escuro" => AppTheme.Dark,
                _ => AppTheme.Unspecified
            };
        }
        await DisplayAlert(
            "Configurações",
            "Configurações salvas com sucesso.",
            "OK");

    
    }
}