using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using Overlap.Core;

namespace Overlap.App;

public partial class MainWindow : Window
{
    private const int GwlExStyle = -20;
    private const int WsExTransparent = 0x20;
    private const int WsExLayered = 0x80000;

    private readonly SettingsStore _settingsStore = new();
    private readonly OverlaySettings _settings;
    private readonly RadarProcessor _radarProcessor = new(maxCars: 6);

    private readonly Ellipse[] _carDots = new Ellipse[6];
    private readonly ScaleTransform _scaleTransform = new();

    private ITelemetryReader _telemetryReader;
    private TelemetryFrame _latestFrame = TelemetryFrame.Disconnected;
    private bool _moveMode;
    private long _lastPollTicks;

    public MainWindow()
    {
        InitializeComponent();

        _settings = _settingsStore.Load();
        Left = _settings.X;
        Top = _settings.Y;
        Opacity = Math.Clamp(_settings.Opacity, 0.2, 1.0);

        _scaleTransform.ScaleX = Math.Clamp(_settings.Scale, 0.6, 2.0);
        _scaleTransform.ScaleY = _scaleTransform.ScaleX;
        RadarCanvas.LayoutTransform = _scaleTransform;

        for (var i = 0; i < _carDots.Length; i++)
        {
            var dot = new Ellipse
            {
                Width = 14,
                Height = 14,
                Fill = Brushes.LightSteelBlue,
                Visibility = Visibility.Collapsed
            };
            _carDots[i] = dot;
            RadarCanvas.Children.Add(dot);
        }

        _telemetryReader = CreateTelemetryReader();

        Loaded += (_, _) =>
        {
            ApplyClickThrough(isLocked: true);
            CompositionTarget.Rendering += OnRendering;
        };

        Closed += (_, _) =>
        {
            CompositionTarget.Rendering -= OnRendering;
            _telemetryReader.Dispose();
            PersistSettings();
        };
    }

    private ITelemetryReader CreateTelemetryReader()
    {
        try
        {
            return new IracingSharedMemoryReader();
        }
        catch
        {
            return new MockTelemetryReader();
        }
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        // Render loop runs at display refresh; telemetry poll capped at ~20Hz.
        var now = Stopwatch.GetTimestamp();
        if (now - _lastPollTicks > Stopwatch.Frequency / 20)
        {
            _latestFrame = _telemetryReader.ReadFrame();
            _lastPollTicks = now;
        }

        DrawRadar(_latestFrame);
    }

    private void DrawRadar(TelemetryFrame frame)
    {
        StatusText.Text = frame.IsConnected ? "Overlap" : "Waiting for iRacing";
        ModeText.Text = _moveMode ? "MOVE (Ctrl+Shift+M)" : "LOCKED (Ctrl+Shift+M)";

        if (!frame.IsConnected)
        {
            for (var i = 0; i < _carDots.Length; i++)
            {
                _carDots[i].Visibility = Visibility.Collapsed;
            }
            return;
        }

        var cars = _radarProcessor.ComputeClosest(frame);
        var pulse = 0.7 + (0.3 * (Math.Sin(DateTime.UtcNow.TimeOfDay.TotalSeconds * 8) + 1) / 2);

        for (var i = 0; i < _carDots.Length; i++)
        {
            if (i >= cars.Length)
            {
                _carDots[i].Visibility = Visibility.Collapsed;
                continue;
            }

            var car = cars[i];
            var y = RadarCanvas.Height / 2 - (car.DeltaMeters / RadarMath.RadarRangeMeters) * (RadarCanvas.Height / 2 - 12);
            var x = RadarCanvas.Width / 2;

            var dot = _carDots[i];
            dot.Visibility = Visibility.Visible;
            dot.Fill = car.IsDanger
                ? new SolidColorBrush(Color.FromArgb((byte)(255 * pulse), 255, 60, 60))
                : Brushes.LightSteelBlue;

            Canvas.SetLeft(dot, x - dot.Width / 2);
            Canvas.SetTop(dot, y - dot.Height / 2);
        }
    }

    private void LockButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleMoveMode();
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.M
            && System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Control)
            && System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift))
        {
            ToggleMoveMode();
            e.Handled = true;
            return;
        }

        if (!_moveMode)
        {
            return;
        }

        if (e.Key == System.Windows.Input.Key.OemPlus || e.Key == System.Windows.Input.Key.Add)
        {
            ApplyScale(_scaleTransform.ScaleX + 0.05);
        }
        else if (e.Key == System.Windows.Input.Key.OemMinus || e.Key == System.Windows.Input.Key.Subtract)
        {
            ApplyScale(_scaleTransform.ScaleX - 0.05);
        }
    }

    private void Window_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
        if (!_moveMode)
        {
            return;
        }

        ApplyScale(_scaleTransform.ScaleX + Math.Sign(e.Delta) * 0.05);
    }

    private void ToggleMoveMode()
    {
        _moveMode = !_moveMode;

        MoveBorder.Visibility = _moveMode ? Visibility.Visible : Visibility.Collapsed;
        DragHint.Visibility = _moveMode ? Visibility.Visible : Visibility.Collapsed;
        LockButton.Visibility = _moveMode ? Visibility.Visible : Visibility.Collapsed;
        LockButton.Content = _moveMode ? "🔓" : "🔒";

        ApplyClickThrough(isLocked: !_moveMode);
    }

    private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_moveMode)
        {
            DragMove();
        }
    }

    private void ApplyScale(double scale)
    {
        var clamped = Math.Clamp(scale, 0.6, 2.0);
        _scaleTransform.ScaleX = clamped;
        _scaleTransform.ScaleY = clamped;
    }

    private void ApplyClickThrough(bool isLocked)
    {
        var windowInterop = new WindowInteropHelper(this);
        var hwnd = windowInterop.Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        var style = GetWindowLong(hwnd, GwlExStyle);
        style |= WsExLayered;
        style = isLocked ? style | WsExTransparent : style & ~WsExTransparent;
        SetWindowLong(hwnd, GwlExStyle, style);
    }

    private void PersistSettings()
    {
        _settings.X = Left;
        _settings.Y = Top;
        _settings.Scale = _scaleTransform.ScaleX;
        _settings.Opacity = Opacity;
        _settingsStore.Save(_settings);
    }

    [LibraryImport("user32.dll")]
    private static partial int GetWindowLong(IntPtr hWnd, int nIndex);

    [LibraryImport("user32.dll")]
    private static partial int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
}
