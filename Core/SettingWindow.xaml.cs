using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Windows.Win32;
using Color = System.Windows.Media.Color;

namespace CursorTail.Core
{
    public class ConvertLongNumberToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float f)
            {
                return f.ToString("0.00");
            }
            if (value is double d)
            {
                return d.ToString("0.00");
            }
            return 0;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Windows.Data.Binding.DoNothing;
        }
    }
    /// <summary>
    /// SettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : Window
    {
        public MainWindowViewModel ViewModel { get; set; }
        private VisualBrush? _visualBrush;
        private DpiScale _dpiScale;
        public SettingWindow(MainWindowViewModel viewModel, PainterVisionHost painter, DpiScale dpiScale)
        {
            InitializeComponent();
            _dpiScale = dpiScale;
            ViewModel = viewModel;
            DataContext = viewModel;
            ViewModel.IsStartUp = CheckStartUp(out bool IsEnabled);
            StartUpCheckBox.IsEnabled = IsEnabled;
            _visualBrush = new VisualBrush(painter)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                ViewportUnits = BrushMappingMode.Absolute,
            };
            _visualBrush.Viewport = new Rect(0, 0, ShowCanvas.ActualWidth, ShowCanvas.ActualHeight);
            ShowCanvas.Background = _visualBrush;
            DispatcherTimer? fpsTimer = new(TimeSpan.FromMilliseconds(1000), DispatcherPriority.Render, (s, e) => viewModel.RaisePropertyChanged("FPS"), Dispatcher);
            this.Closing += (s, e) =>
            {
                fpsTimer.Stop();
                fpsTimer = null;
                viewModel.RM?.SaveConfigs();
                CompositionTarget.Rendering -= UpdatePreview;
                _visualBrush = null;
            };
            BindingEvents();
            LoadGifFolders();
        }

        private void LoadGifFolders()
        {
            string[] folders= Directory.GetDirectories(System.IO.Path.Combine(AppContext.BaseDirectory, "GIFs"))
                .Select((f) => (new DirectoryInfo(f)).Name).ToArray();
            if(ViewModel.GifFolders==null ||ViewModel.GifFolders.Length!=folders.Length)
                ViewModel.GifFolders=folders;
        }
        private void BindingEvents()
        {
            CompositionTarget.Rendering += UpdatePreview;
            this.SizeChanged += (s, e) => _visualBrush.Viewport = new Rect(0, 0, ShowCanvas.ActualWidth, ShowCanvas.ActualHeight);
            LoadConfigButton.Click += (s, e) => LoadConfigFile();
            SaveAsButton.Click += (s, e) => SaveConfigAs();
            ResetButton.Click += (s, e) => ViewModel.LoadProps();
            GifFolderComBox.DropDownOpened += (s, e) => LoadGifFolders();
            RopeColorDia.Click += (s, e) => ViewModel.RopeColor = GetColor(ViewModel.RopeColor);
            StrokeColorDia.Click += (s, e) => ViewModel.StrokeColor = GetColor(ViewModel.StrokeColor);
            StartUpCheckBox.Click += (s, e) => SetStartUp();
        }

        private void LoadConfigFile()
        {
            System.Windows.Forms.OpenFileDialog openFile = new()
            {
                Title = "选择配置文本文件",
                InitialDirectory = System.IO.Path.Combine(AppContext.BaseDirectory, "Configs"),
                Filter = "文本文件 (*.txt)|*.txt",
                Multiselect = false,
            };
            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ViewModel.LoadProps(openFile.FileName);
            }
        }

        private void SaveConfigAs()
        {
            System.Windows.Forms.SaveFileDialog saveFile = new()
            {
                Title = "选择配置文本文件",
                InitialDirectory = System.IO.Path.Combine(AppContext.BaseDirectory, "Configs"),
                Filter = "文本文件 (*.txt)|*.txt",
            };
            saveFile.ShowDialog();
            ViewModel.RM.SaveConfigs(saveFile.FileName);
        }
        System.Drawing.Point _cursorPos = new(0, 0);
        private void UpdatePreview(object? s, EventArgs e)
        {
            PInvoke.GetCursorPos(out _cursorPos);
            _visualBrush.Viewbox = new Rect(_cursorPos.X / _dpiScale.DpiScaleX - ShowCanvas.ActualWidth / 2,
                _cursorPos.Y / _dpiScale.DpiScaleY - ShowCanvas.ActualHeight / 2,
                ShowCanvas.ActualWidth, ShowCanvas.ActualHeight);
        }

        private Color GetColor(Color defaultColor)
        {
            ColorDialog colorDialog = new ColorDialog();
            var oldColor = System.Drawing.Color.FromArgb(defaultColor.A, defaultColor.R, defaultColor.G, defaultColor.B);
            colorDialog.Color = oldColor;
            colorDialog.ShowDialog();
            if (colorDialog.Color != oldColor)
            {
                var newColor = colorDialog.Color;
                return Color.FromArgb(newColor.A, newColor.R, newColor.G, newColor.B);
            }
            else
            {
                return defaultColor;
            }
        }
        private bool CheckStartUp(out bool isEnable)
        {
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (key == null)
                {
                    isEnable = false;
                    return false;
                }
                isEnable = true;
                string appPath = $"\"{System.Windows.Forms.Application.ExecutablePath}\"";
                string appName = System.IO.Path.GetFileNameWithoutExtension(appPath);
                return key.GetSubKeyNames().Contains(appName) && key.GetValue(appName) == appPath;
            }
        }
        private bool SetStartUp()
        {
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (key == null)
                {
                    return false;
                }
                string appPath = $"\"{System.Windows.Forms.Application.ExecutablePath}\"";
                string appName = System.IO.Path.GetFileNameWithoutExtension(appPath);
                if (ViewModel.IsStartUp)
                {
                    key.SetValue(appName, appPath);
                    return true;
                }
                else
                {
                    try
                    {
                        key.DeleteValue(appName);
                        return false;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }

    }
}
