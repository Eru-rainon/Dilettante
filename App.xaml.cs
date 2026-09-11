using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Data;
using System.Windows;
using System.IO;

namespace Dilettante;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Catch any unhandled exception and write to a log file
        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            File.WriteAllText(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log"),
                ex?.ToString() ?? "Unknown error");
        };

        DispatcherUnhandledException += (s, args) =>
        {
            File.WriteAllText(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log"),
                args.Exception.ToString());
            args.Handled = true;
        };

        using var db = new Dilettante.Data.AppDbContext();
        db.Database.Migrate();
    }
}

