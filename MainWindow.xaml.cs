using CursorTail.Core;
using System.Diagnostics;
using System.Numerics;
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
        public MainWindow()
        {
            InitializeComponent();
            _dpiScale = VisualTreeHelper.GetDpi(this);
            SourceInitialized += OnSourceInitialized;
            this.SnapsToDevicePixels = true;

            //临时测试部分
            rope = new Rope(new Vector2((float)SystemParameters.PrimaryScreenWidth, (float)SystemParameters.PrimaryScreenHeight));
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
            painter = new PainterVisionHost(rope, Color.FromRgb(255, 0, 0), Color.FromRgb(0, 0, 0), new Point(0, 20), 0, new Uri("D:\\Learn\\Useful C3\\CursorTail\\TailImg\\sayori.png", UriKind.RelativeOrAbsolute));
            MainCanvas.Children.Add(painter);
            frameController = new FrameController(UpdatePerFrame, 60);

            //CompositionTarget.Rendering += CompositionTarget_Rendering;
            DispatcherTimer timer = new DispatcherTimer(TimeSpan.FromMicroseconds(1), DispatcherPriority.Render, CompositionTarget_Rendering, Dispatcher);
        }
        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            frameController.UpdateFrame();
        }

        System.Drawing.Point cursorPos = new(0, 0);
        private void UpdatePerFrame()
        {
            PInvoke.GetCursorPos(out cursorPos);
            rope.Update(new((float)(cursorPos.X / _dpiScale.DpiScaleX), (float)(cursorPos.Y / _dpiScale.DpiScaleY)));
            painter.Update();
        }

        private void UpdateWinPos(object? s, EventArgs? e)
        {
            PInvoke.GetCursorPos(out cursorPos);
            MoveWindow(cursorPos.X / _dpiScale.DpiScaleX, cursorPos.Y / _dpiScale.DpiScaleY);
        }

        private void MoveWindow(double x, double y)
        {
            if (Left != x)
                this.Left = x - this.ActualWidth / 2;
            if (Top != y)
                this.Top = y - this.ActualHeight / 2;
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
    }
}