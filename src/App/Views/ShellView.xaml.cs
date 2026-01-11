using AegisLink.App.Models;
using AegisLink.App.Services;
using AegisLink.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly IConfigService _configService;
    private readonly List<string> _commandHistory = new();
    private int _historyIndex = -1;
    
    // Radar zoom/pan state
    private double _zoomLevel = 1.0;
    private Point _panOffset = new(0, 0);
    private Point _lastMousePos;
    private bool _isPanning = false;

    public ShellView(ShellViewModel viewModel, IConfigService configService)
    {
        InitializeComponent();
        _vm = viewModel;
        _configService = configService;
        DataContext = _vm;

        Loaded += ShellView_Loaded;
        SizeChanged += ShellView_SizeChanged;
        Closing += ShellView_Closing;
    }

    private void ShellView_Loaded(object sender, RoutedEventArgs e)
    {
        RestoreWindowPosition();
        DrawRadarGrid();
    }

    private void ShellView_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        SaveWindowPosition();
    }

    private void RestoreWindowPosition()
    {
        var config = _configService.Load();
        var bounds = config.LastWindowBounds;
        
        // Validate bounds are on-screen
        if (bounds.Width > 100 && bounds.Height > 100)
        {
            Left = bounds.X;
            Top = bounds.Y;
            Width = bounds.Width;
            Height = bounds.Height;
        }
    }

    private void SaveWindowPosition()
    {
        var config = _configService.Load();
        config.LastWindowBounds = new WindowBounds
        {
            X = Left,
            Y = Top,
            Width = Width,
            Height = Height
        };
        _configService.Save(config);
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
        // Handle Ctrl+C/V for clipboard
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            if (e.Key == Key.C)
            {
                CopyTerminalLogs();
                e.Handled = true;
                return;
            }
            if (e.Key == Key.V && CommandInput.IsFocused)
            {
                // Default paste behavior already handled by TextBox
                return;
            }
        }

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

    private void CopyTerminalLogs()
    {
        if (_vm.TerminalLog.Count > 0)
        {
            var lastLogs = string.Join(Environment.NewLine, _vm.TerminalLog.TakeLast(10));
            Clipboard.SetText(lastLogs);
            _vm.ShowToast("📋 Logs copied");
        }
    }

    // Radar Zoom/Pan Handlers
    private void RadarCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        var zoomDelta = e.Delta > 0 ? 1.1 : 0.9;
        _zoomLevel = Math.Clamp(_zoomLevel * zoomDelta, 0.5, 4.0);
        UpdateRadarTransform();
    }

    private void RadarCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isPanning = true;
        _lastMousePos = e.GetPosition(this);
        RadarCanvas.CaptureMouse();
    }

    private void RadarCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isPanning = false;
        RadarCanvas.ReleaseMouseCapture();
    }

    private void RadarCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isPanning)
        {
            var pos = e.GetPosition(this);
            var delta = pos - _lastMousePos;
            _panOffset.X += delta.X;
            _panOffset.Y += delta.Y;
            _lastMousePos = pos;
            UpdateRadarTransform();
        }
    }

    private void UpdateRadarTransform()
    {
        var matrix = new Matrix();
        matrix.Translate(_panOffset.X, _panOffset.Y);
        matrix.ScaleAt(_zoomLevel, _zoomLevel, RadarCanvas.ActualWidth / 2, RadarCanvas.ActualHeight / 2);
        RadarTransform.Matrix = matrix;
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
