using Core.Services;
using GwasClusteringApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using UI.ViewModels;

namespace clustering_by_the_k_means_algorithm_ON_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Регистрация ClusteringService
            services.AddTransient<ClusteringService>();
            // Другие регистрации: ViewModels, окна и т.д.
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainWindow>();
        }
    }

}
