using System.Windows;

namespace GamingTweaksManager
{
    /// <summary>
    /// Lógica de interação para App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Verificar se está executando como administrador
            if (!IsRunningAsAdministrator())
            {
                MessageBox.Show(
                    "⚠️ AVISO: O aplicativo não está sendo executado como Administrador.\n\n" +
                    "Para aplicar os tweaks, é necessário executar como Administrador.\n" +
                    "Clique com o botão direito no executável e selecione 'Executar como administrador'.",
                    "Permissões Insuficientes",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Verifica se está executando como administrador
        /// </summary>
        private bool IsRunningAsAdministrator()
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
    }
}
