using CursorTail.Core;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using CL = CursorTail.Core.CursorLocation;

namespace CursorTail
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Rope rope;
        public PainterVisionHost painter;
        public FrameController frameController;
        private DpiScale _dpiScale;
        private System.Drawing.Rectangle[] _screens;
        public MainWindowViewModel ViewModel;
        public StateMachine stateMachine;
        public GIFLoder gifLoder;
        public MainWindow(StateMachine stateMachine, Rope rope, GIFLoder gifLoder, PainterVisionHost painter, FrameController frameController, MainWindowViewModel viewModel)
        {
            InitializeComponent();

            //窗口属性
            _dpiScale = VisualTreeHelper.GetDpi(this);
            _screens = Screen.AllScreens.Select(s => s.WorkingArea).ToArray();
            SourceInitialized += OnSourceInitialized;
            this.SnapsToDevicePixels = true;
            this.WindowState = WindowState.Normal;

            //实例化
            this.stateMachine = stateMachine;
            this.rope = rope;
            this.gifLoder = gifLoder;
            this.painter = painter;
            this.frameController = frameController;
            this.ViewModel = viewModel;

            //绑定事件
            MainCanvas.Children.Add(painter);
            CL.RaiseDeskTopChange += MouseCrossScreen;
            this.frameController.UpdatePerFrame += UpdatePerFrame;
            //CompositionTarget.Rendering += (s,e)=> frameController.UpdateFrame();
            var reTopAction = GetReTopAction();
            frameController.UpdatePerFrame += reTopAction;
            DispatcherTimer timer = new DispatcherTimer(TimeSpan.FromMicroseconds(6), DispatcherPriority.Render, (s, e) => frameController.UpdateFrame(), Dispatcher);

            //解绑事件
            this.Closing += (s, e) =>
            {
                MainCanvas.Children.Clear();
                CL.RaiseDeskTopChange -= MouseCrossScreen;
                this.frameController.UpdatePerFrame -= UpdatePerFrame;
                this.frameController.UpdatePerFrame -= reTopAction;
                timer.Stop();
                CL.ResetCursor();
            };
        }

        private void MouseCrossScreen(System.Drawing.Rectangle bound)
        {
            this.Top = bound.Top;
            this.Left = bound.Left;
            this.Width = bound.Width / _dpiScale.DpiScaleX;
            this.Height = bound.Height / _dpiScale.DpiScaleY;
            rope.CollideBox = new(0, 0, (float)Width, (float)Height);
        }

        private void UpdatePerFrame()
        {
            CL.FrushCursorPos(_dpiScale);
            rope.Update(new(CL.RelatviCursorPos.X + ViewModel.CursorOffset_X, CL.RelatviCursorPos.Y + ViewModel.CursorOffset_Y));
            painter.Update();
        }

        /// <summary>
        /// 源初始化完毕函数，在构造函数完成后运行，窗口已有句柄，此时定义鼠标穿透和工具窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSourceInitialized(object? sender, EventArgs e)
        {
            const int WS_EX_TRANSPARENT = 0x00000020;
            const int WS_EX_TOOLWINDOW = 0x00000080;
            const WINDOW_LONG_PTR_INDEX GWL_EXSTYLE = (WINDOW_LONG_PTR_INDEX)(-20);
            var hwnd = new WindowInteropHelper(this).Handle;
            int currentExStyle = PInvoke.GetWindowLong(new HWND(hwnd), GWL_EXSTYLE);
            PInvoke.SetWindowLong(new HWND(hwnd), GWL_EXSTYLE, currentExStyle | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW);
        }

        private FrameController.UpdatePerFrameHandler GetReTopAction()
        {
            FrameController fc = new FrameController(() =>
            {
                this.Topmost = false;
                this.Topmost = true;
            })
            {
                TargetFrameTime = 10 * 1000,
            };
            return fc.UpdateFrame;
        }
    }
}