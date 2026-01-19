using System;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;
using ClassroomManagement.Services;

namespace ClassroomManagement;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        // Handle unhandled exceptions
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogService.Instance.Error("App", "Unhandled UI exception", e.Exception);

        MessageBox.Show(
            $"Đã xảy ra lỗi:\n{e.Exception.Message}\n\nVui lòng thử lại hoặc khởi động lại ứng dụng.",
            "Lỗi ứng dụng",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        e.Handled = true; // Prevent app from crashing
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        LogService.Instance.Error("App", "Unhandled domain exception", exception);

        if (e.IsTerminating)
        {
            MessageBox.Show(
                $"Lỗi nghiêm trọng:\n{exception?.Message}\n\nỨng dụng sẽ đóng.",
                "Lỗi nghiêm trọng",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
