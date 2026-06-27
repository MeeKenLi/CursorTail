using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media.Imaging;
using Windows.Win32;

namespace CursorTail.Core
{
    public class GIFLoder
    {
        private FrameController _gifFC;
        private StateMachine _stateMachine;
        private int _loopCount;
        private int _currentIndex;
        private BitmapSource[][] _bitmapSources;
        private int _currentState
        {
            get => (int)_stateMachine.CurrentState;
        }

        public string GifFolder;
        public int MaxLoopCount;
        public int TargetFrame
        {
            get => (int)Math.Round(1000 / _gifFC.TargetFrameTime);
            set => _gifFC.TargetFrameTime = 1.0 / value * 1000;
        }

        public GIFLoder(StateMachine stateMachine, int loopCount, string gifFolder="", int targetFPS = 10)
        {
            _gifFC = new(GenerateToNextFrame, targetFPS);
            MaxLoopCount = loopCount;
            _stateMachine = stateMachine;
            _stateMachine.RaiseStateChange += RaiseStateChange;
            _stateMachine.RaiseStateKeep += RaiseStateKeep;
            GifFolder = gifFolder;
        }
        public bool LoadAllImgs(string gifFonder)
        {
            try
            {
                string[][] imgPath = new string[4][];
                var temp = _bitmapSources;
                _bitmapSources = new BitmapSource[4][];

                string[] stats = Enum.GetNames(typeof(States));
                for (int i = 0; i < stats.Length; i++)
                {
                    var currentDir = Path.Combine(AppContext.BaseDirectory, "GIFs\\" + gifFonder + "\\" + stats[i]);
                    if (!Directory.Exists(currentDir) || Directory.GetFiles(currentDir).Length == 0)
                    {
                        if (i == 0)
                        {
                            throw new Exception("Can't find any image.");
                        }
                        imgPath[i] = imgPath[0];
                        _bitmapSources[i] = _bitmapSources[0];
                    }
                    else
                    {
                        imgPath[i] = Directory.GetFiles(currentDir);
                        Array.Sort(imgPath[i], new NatureSortCompare());
                        _bitmapSources[i] = new BitmapSource[imgPath[i].Length] ;
                        for (int j = 0; j < imgPath[i].Length; j++)
                        {
                            _bitmapSources[i][j] = new BitmapImage(new Uri(imgPath[i][j], UriKind.Absolute))
                            {
                                CacheOption = BitmapCacheOption.OnLoad
                            };
                        }
                    }
                    //Array.Sort(imgPath[i], StrCmpLogicalW);
                }
                RaiseStateChange(States.Stopped);
                _stateMachine.SwitchTo(States.Stopped);
            }
            catch (Exception e)
            {
                var defaultPath = "Hachimi";
                if (GifFolder != defaultPath)
                {
                    GifFolder = defaultPath;
                    LoadAllImgs(GifFolder);
                    MessageBox.Show("Gif文件夹格式错误，尝试加载示例文件夹，请参照示例文件结构。\n需要保证至少有Stoped文件夹即至少一个图片", "错误", MessageBoxButtons.OK);
                    return false;
                }
                else
                {
                    MessageBox.Show("Gif示例文件夹怎么也没了？程序退出。\n ! ? 删删 ? ! ", "错误", MessageBoxButtons.OK);
                    App.Current.MainWindow.Close();
                    return false;
                }
            }
            return true;
        }
        public BitmapSource GetCurrentFrame => _bitmapSources[_currentState][_currentIndex];
        public Action LoadFrameControler => _gifFC.UpdateFrame;
        public void GenerateToNextFrame()
        {
            if (_loopCount >= MaxLoopCount)
            {
                _stateMachine.ResetState();
                return;
            }
            if (_currentIndex < _bitmapSources[_currentState].Length - 1)
            {
                _currentIndex++;
            }
            else
            {
                _loopCount++;
                _currentIndex = 0;
            }
        }
        public void RaiseStateChange(States state)
        {
            _loopCount = 0;
            _currentIndex = 0;
        }
        public void RaiseStateKeep()
        {
            _loopCount = 0;
        }
        private class NatureSortCompare : IComparer<string>
        {
            public int Compare(string? a, string? b)
            {
                //a、b都是格式化的文件名
                //从第一个数字进行累计，直到出现第一个不同的数字列或字符
                //此处提供两个变量实际是为了方便独立遍历
                for (int x = 0, y = 0; x < a.Length && y < b.Length; x++, y++)
                {
                    if (char.IsDigit(a[x]) && char.IsDigit(b[y]))
                    {
                        long vx = 0, vy = 0;
                        //每次迭代为数字进位，即*10
                        for (; x < a.Length; x++)
                        {
                            vx = vx * 10 + (a[x] - '0');
                        }
                        for (; y < b.Length; y++)
                        {
                            vy = vy * 10 + (b[y] - '0');
                        }
                        //仅不相等时返回
                        if (vx != vy)
                        {
                            return vx > vy ? 1 : -1;
                        }
                    }

                    //对于非字母情况使用unicode码表排序
                    if (x < a.Length && y < b.Length && a[x] != b[y])
                    {
                        return a[x] > b[y] ? 1 : -1;
                    }
                }
                return 0;
            }
        }
    }
}
