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

        public GIFLoder(StateMachine stateMachine, int loopCount, string gifFolder, int targetFPS = 10)
        {
            _gifFC = new(GenerateToNextFrame, targetFPS);
            MaxLoopCount = loopCount;
            _stateMachine = stateMachine;
            _stateMachine.RaiseStateChange += RaiseStateChange;
            _stateMachine.RaiseStateKeep += RaiseStateKeep;
            GifFolder = gifFolder;
            LoadAllImgs(GifFolder);
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
                    var currentDir = Path.Combine(AppContext.BaseDirectory, "GIFs\\"+gifFonder+"\\"+stats[i]);
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
                        _bitmapSources[i] = new BitmapSource[imgPath[i].Length];
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
    }
}
