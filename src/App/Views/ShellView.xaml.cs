using AegisLink.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AegisLink.App.Views;

public partial class ShellView : Window
{
    private readonly ShellViewModel _vm;
    private readonly List<string> _commandHistory = new();
    private int _historyIndex = -1;

    public ShellView(ShellViewModel viewModel)
    {
        InitializeComponent();
        _vm = viewModel;
        DataContext = _vm;

        Loaded += ShellView_Loaded;
        SizeChanged += ShellView_SizeChanged;
    }

    private void ShellView_Loaded(object sender, RoutedEventArgs e)
    {
        DrawRadarGrid();
    }

    private void ShellView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        DrawRadarGrid();
    }

    private void DrawRadarGrid()
    {
        RadarCanvas.Children.Clear();
        var centerX = RadarCanvas.ActualWidth / 2;
        var centerY = RadarCanvas.ActualHeight / 2;
        var maxRadius = Math.Min(centerX, centerY) - 30;

        if (maxRadius <= 0) return;

        // Freeze brushes for performance
        var gridBrush = new SolidColorBrush(Color.FromArgb(40, 0, 255, 0));
        gridBrush.Freeze();
        var centerBrush = new SolidColorBrush(Colors.Cyan);
        centerBrush.Freeze();

        // Concentric circles
        for (int i = 1; i <= 5; i++)
        {
            var radius = maxRadius * i / 5;
            var circle = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Stroke = gridBrush,
                StrokeThickness = 1
            };
            Canvas.SetLeft(circle, centerX - radius);
            Canvas.SetTop(circle, centerY - radius);
            RadarCanvas.Children.Add(circle);
        }

        // Crosshairs
        RadarCanvas.Children.Add(new Line
        {
            X1 = 30, Y1 = centerY,
            X2 = RadarCanvas.ActualWidth - 30, Y2 = centerY,
            Stroke = gridBrush, StrokeThickness = 1
        });
        RadarCanvas.Children.Add(new Line
        {
            X1 = centerX, Y1 = 30,
            X2 = centerX, Y2 = RadarCanvas.ActualHeight - 30,
            Stroke = gridBrush, StrokeThickness = 1
        });

        // Center dot
        var dot = new Ellipse { Width = 8, Height = 8, Fill = centerBrush };
        Canvas.SetLeft(dot, centerX - 4);
        Canvas.SetTop(dot, centerY - 4);
        RadarCanvas.Children.Add(dot);
    }

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            if (e.ClickCount == 2)
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            else
                DragMove();
        }
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Maximize_Click(object sender, RoutedEventArgs e) => 
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.F11:
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                break;
            case Key.Oem3: // Tilde ~
                _vm.ToggleTerminalCommand.Execute(null);
                if (_vm.IsTerminalVisible)
                {
                    Dispatcher.BeginInvoke(() => CommandInput.Focus(), DispatcherPriority.Input);
                }
                break;
            case Key.Escape:
                if (_vm.IsTerminalVisible)
                    _vm.ToggleTerminalCommand.Execute(null);
                break;
        }
    }

    private void CommandInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var command = CommandInput.Text.Trim();
            if (!string.IsNullOrEmpty(command))
            {
                _commandHistory.Add(command);
                _historyIndex = _commandHistory.Count;
                _vm.ExecuteCommandCommand.Execute(command);
                CommandInput.Clear();
            }
        }
        else if (e.Key == Key.Up && _commandHistory.Count > 0)
        {
            if (_historyIndex > 0) _historyIndex--;
            CommandInput.Text = _commandHistory[_historyIndex];
            CommandInput.CaretIndex = CommandInput.Text.Length;
        }
        else if (e.Key == Key.Down && _commandHistory.Count > 0)
        {
            if (_historyIndex < _commandHistory.Count - 1)
            {
                _historyIndex++;
                CommandInput.Text = _commandHistory[_historyIndex];
            }
            else
            {
                CommandInput.Clear();
                _historyIndex = _commandHistory.Count;
            }
        }
    }
}
