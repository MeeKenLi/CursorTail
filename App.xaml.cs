using CursorTail.Core;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Application = System.Windows.Application;

namespace CursorTail
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public Rope rope;
        public PainterVisionHost painter;
        public MainWindowViewModel ViewModel;
        public StateMachine stateMachine;
        public GIFLoder gifLoder;
        public FrameController frameController;
        public MainWindow? mainWindow;
        public HotKeyManager hotKeyManager;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            //实例化
            stateMachine = new();
            rope = new Rope(stateMachine);
            gifLoder = new(stateMachine, 4);
            painter = new PainterVisionHost(rope, System.Windows.Media.Color.FromRgb(255, 255, 0), System.Windows.Media.Color.FromRgb(0, 0, 0), new(0, 0), 0, gifLoder);
            frameController = new FrameController(60);
            ViewModel = new MainWindowViewModel(rope, painter, frameController, gifLoder);
            LoadTaskIcon();
            hotKeyManager = new HotKeyManager();
            ViewModel.IsSwitHKOn = hotKeyManager.SetHotKey(HotKeyVk.X, SwitchMainWindow);
            ViewModel.IsExitHKOn = hotKeyManager.SetHotKey(HotKeyVk.Z, ExitApp);
            CreatMainWindow();
        }
        SettingWindow? _settingWindow;
        NotifyIcon? _notifyIcon;
        private void LoadTaskIcon()
        {
            _notifyIcon = new NotifyIcon()
            {
                Text = "CursorTail",
                Icon = new System.Drawing.Icon(System.IO.Path.Combine(AppContext.BaseDirectory, "Core/icon.ico")),
            };
            EventHandler openSetting = (s, e) =>
            {
                if (_settingWindow == null)
                {
                    _settingWindow = new SettingWindow(ViewModel, painter);
                    _settingWindow.Closed += (s, e) => _settingWindow = null;
                    _settingWindow.Show();
                }
                else
                {
                    _settingWindow.Topmost = true;
                    _settingWindow.Topmost = false;
                }
            };
            _notifyIcon.DoubleClick += openSetting;
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem setting = new("设置");
            setting.Click += openSetting;
            ToolStripMenuItem switchMW = new("切换开关");
            switchMW.Click += (s, e) => SwitchMainWindow();
            ToolStripMenuItem exit = new("退出");
            exit.Click += (s, e) => ExitApp();

            contextMenu.Items.Add(setting);
            contextMenu.Items.Add(switchMW);
            contextMenu.Items.Add(exit);
            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.Visible = true;
        }
        public void ExitApp()
        {
            _settingWindow?.Close();
            mainWindow?.Close();
            _notifyIcon?.Dispose();
            Application.Current.Shutdown();
        }
        public void CreatMainWindow()
        {
            if (mainWindow == null)
            {
                mainWindow = new(stateMachine, rope, gifLoder, painter, frameController, ViewModel);
                mainWindow.Show();
                _notifyIcon.ContextMenuStrip.Items[1].Text = "关闭挂件";
            }
        }
        public void CloseMainWindow()
        {
            if (mainWindow != null)
            {
                var t = mainWindow;
                mainWindow = null;
                t.Close();
                _notifyIcon.ContextMenuStrip.Items[1].Text = "开启挂件";
            }
        }
        public void SwitchMainWindow()
        {
            if (mainWindow != null)
                CloseMainWindow();
            else
                CreatMainWindow();
        }
    }
}