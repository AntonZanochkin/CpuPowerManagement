using System.Configuration;
using System.Data;
using System.Windows;
using System;
using System.Windows;
using CpuPowerManagement.Models;
using CpuPowerManagement.Services;
using CpuPowerManagement.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CpuPowerManagement
{
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
      // Configure Logging
      services.AddLogging();

      // Register Services
      services.AddSingleton<IUserService, UserService>();

      // Register ViewModels
      services.AddSingleton<IMainViewModel, MainViewModel>();

      // Register Views
      services.AddSingleton<MainWindow>();
    }

    private void OnExit(object sender, ExitEventArgs e)
    {
      // Dispose of services if needed
      if (_serviceProvider is IDisposable disposable)
      {
        disposable.Dispose();
      }
    }
  }
}


