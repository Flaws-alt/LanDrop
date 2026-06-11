using System;
using System.Windows;
using System.Windows.Input;
using LanDrop.ViewModels;

namespace LanDrop;

public partial class MainWindow : Window
{
    private MainViewModel VM => (MainViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        VM.Shutdown();
    }

    private void SendFileButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is DeviceItemViewModel device)
        {
            VM.SelectedDevice = device;
            VM.SendFileCommand.Execute(null);
        }
    }

    private void SendClipboardButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is DeviceItemViewModel device)
        {
            VM.SelectedDevice = device;
            VM.SendClipboardCommand.Execute(null);
        }
    }

    private void DeviceList_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;

            if (sender is FrameworkElement element)
            {
                var hitElement = element.InputHitTest(e.GetPosition(element));
                while (hitElement is FrameworkElement fe)
                {
                    if (fe.DataContext is DeviceItemViewModel device)
                    {
                        VM.SelectedDevice = device;
                        break;
                    }
                    hitElement = (fe.Parent ?? fe.TemplatedParent) as FrameworkElement;
                }
            }

            VM.HandleFileDrop(files);
        }
    }
}
