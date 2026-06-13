using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Windows.Win32;

namespace CursorTail.Core
{
    /// <summary>
    /// SettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : Window
    {
        public MainWindowViewModel ViewModel { get; set; }
        private VisualBrush _visualBrush;
        private DpiScale _dpiScale;
        public SettingWindow(MainWindowViewModel viewModel, PainterVisionHost painter, DpiScale dpiScale)
        {
            InitializeComponent();
            _dpiScale = dpiScale;
            ViewModel = viewModel;
            DataContext = viewModel;
            _visualBrush = new VisualBrush(painter)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                ViewportUnits = BrushMappingMode.Absolute,
            };
            _visualBrush.Viewport = new Rect(0, 0, ShowCanvas.ActualWidth, ShowCanvas.ActualHeight);
            ShowCanvas.Background = _visualBrush;
            CompositionTarget.Rendering += (s, e) => UpdatePreview();
            this.SizeChanged += (s, e) => _visualBrush.Viewport = new Rect(0, 0, ShowCanvas.ActualWidth, ShowCanvas.ActualHeight);
        }
        public SettingWindow()
        {
            InitializeComponent();
        }

        System.Drawing.Point _cursorPos = new(0, 0);
        private void UpdatePreview()
        {
            PInvoke.GetCursorPos(out _cursorPos);
            _visualBrush.Viewbox = new Rect(_cursorPos.X / _dpiScale.DpiScaleX - ShowCanvas.ActualWidth / 2, 
                _cursorPos.Y / _dpiScale.DpiScaleY - ShowCanvas.ActualHeight / 2, 
                ShowCanvas.ActualWidth, ShowCanvas.ActualHeight);
        }
    }
}
