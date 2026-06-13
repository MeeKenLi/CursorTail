using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace CursorTail.Core
{
    public class GIFLoder
    {
        private FrameController _gifFC;
        private StateMachine _stateMachine;
        public int TargetFrame
        {
            get => (int)(1 / (_gifFC.TargetFrameTime / 1000));
            set => _gifFC.TargetFrameTime = 1 / value * 1000;
        }
        private int _loopCount;
        private int _currentIndex;
        private BitmapSource[][] _bitmapSources;
        private int _currentState
        {
            get => (int)_stateMachine.CurrentState;
        }
        public int MaxLoopCount;

        public GIFLoder(StateMachine stateMachine, int loopCount, int targetFPS = 10, string GifFouder = "Hachimi")
        {
            _gifFC = new(GenerateToNextFrame, targetFPS);
            MaxLoopCount = loopCount;
            _stateMachine = stateMachine;
            _stateMachine.RaiseStateChange += RaiseStateChange;
            _stateMachine.RaiseStateKeep += RaiseStateKeep;
            LoadAllImgs(GifFouder);
        }

        public void LoadAllImgs(string GifFonder)
        {
            try
            {
                string[][] imgPath = new string[4][];
                _bitmapSources = new BitmapSource[4][];

                string[] stats = Enum.GetNames(typeof(States));
                for (int i = 0; i < stats.Length; i++)
                {
                    imgPath[i] = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, GifFonder + "/" + stats[i]));
                    _bitmapSources[i] = new BitmapSource[imgPath[i].Length];
                }
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < imgPath[i].Length; j++)
                    {
                        _bitmapSources[i][j] = new BitmapImage(new Uri(imgPath[i][j], UriKind.Absolute));
                    }
                }
            }
            catch
            {
                MessageBox.Show("Gif文件夹格式错误，请参照示例文件夹，程序退出", "错误", MessageBoxButtons.OK);
                App.Current.MainWindow.Close();
            }
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
