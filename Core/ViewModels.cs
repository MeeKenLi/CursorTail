using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ConfigRecorder;
using Point = System.Windows.Point;
using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;
using Size = System.Windows.Size;
using System.IO;

namespace CursorTail.Core
{
    public class ObservaleObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public RecordersManager? RM;
        public void RaisePropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public void RecordPropChanged<T>(T value, [CallerMemberName] string name = "")
        {
            RM?.RC[name].SetValue(value);
        }
    }
    public class MainWindowViewModel : ObservaleObject
    {
        public Rope rope;
        public PainterVisionHost painter;
        public FrameController frameController;
        public GIFLoder gifLoder;
        private readonly string SavePath = Path.Combine(AppContext.BaseDirectory, "Config.txt");

        public MainWindowViewModel(Rope rope, PainterVisionHost painter, FrameController frameController, GIFLoder gifLoder)
        {
            this.rope = rope;
            this.painter = painter;
            this.frameController = frameController;
            this.gifLoder = gifLoder;
            LoadProps();
        }

        public void LoadProps(string path)
        {
            RM = new RecordersManager(path);
            try
            {
                ReadProps();
                ConfigPath = path;
            }
            catch
            {
                if (path != SavePath)
                {
                    System.Windows.MessageBox.Show("目标存档读取失败，检查文本格式，重载最新存档文件", "错误", MessageBoxButton.OK);
                    LoadProps();
                }
                else
                {
                    System.Windows.MessageBox.Show("当前存档文件读取失败，重新创建默认参数并覆盖，\n！！！需要保存当前存档文件请在点击确认前创建副本！！！", "错误", MessageBoxButton.OK);
                    CreatRecords();
                    RM.SaveConfigs();
                }
                ConfigPath = SavePath;
            }
            ReadProps();
            CurrentConfig = path;
        }
        public void LoadProps()
        {
            LoadProps(SavePath);
        }

        public void CreatRecords()
        {
            RM.RC.Clear();
            RM.RC.Add(new(typeof(float), "Gravity", 1f));
            RM.RC.Add(new(typeof(float), "Stiffness", 1f));
            RM.RC.Add(new(typeof(float), "Damp", 1f));
            RM.RC.Add(new(typeof(float), "Mass", 0.5f));
            RM.RC.Add(new(typeof(int), "Iterations", 10));
            RM.RC.Add(new(typeof(int), "NodeLength", 8));
            RM.RC.Add(new(typeof(int), "NodeNums", 10));
            RM.RC.Add(new(typeof(int), "TargetFPS", 60));
            RM.RC.Add(new(typeof(int), "GIFPS", 10));
            RM.RC.Add(new(typeof(int), "GIFMaxLoop", 2));
            RM.RC.Add(new(typeof(int), "FastVelocity", 40));
            RM.RC.Add(new(typeof(double), "RopeWidth", 2));
            RM.RC.Add(new(typeof(double), "StrokeWidth", 0.5));
            RM.RC.Add(new(typeof(double), "ImgTransOffset_X", 0));
            RM.RC.Add(new(typeof(double), "ImgTransOffset_Y", 0));
            RM.RC.Add(new(typeof(double), "ImgAngleOffset", 0));
            RM.RC.Add(new(typeof(double), "ImgScale", 0.5));
            RM.RC.Add(new(typeof(double), "CursorOffset_X", 9));
            RM.RC.Add(new(typeof(double), "CursorOffset_Y", 5));
            RM.RC.Add(new(typeof(string), "GifFolder", "Hachimi"));
            RM.RC.Add(new(typeof(string), "RopeColor", "255,217,175,66"));
            RM.RC.Add(new(typeof(string), "StrokeColor", "255,156,123,35"));
            RM.RC.Add(new(typeof(bool), "IsFlipGIF", false));
            RM.RC.Add(new(typeof(bool), "IsFollowMode", false));

        }
        public void ReadProps()
        {
            Gravity = RM.RC["Gravity"].GetValue(Gravity);
            Stiffness = RM.RC["Stiffness"].GetValue(Stiffness);
            Damp = RM.RC["Damp"].GetValue(Damp);
            Mass = RM.RC["Mass"].GetValue(Mass);
            Iterations = RM.RC["Iterations"].GetValue(Iterations);
            NodeLength = RM.RC["NodeLength"].GetValue(NodeLength);
            NodeNums = RM.RC["NodeNums"].GetValue(NodeNums);
            FastVelocity = RM.RC["FastVelocity"].GetValue(FastVelocity);
            RopeWidth = RM.RC["RopeWidth"].GetValue(RopeWidth);
            StrokeWidth = RM.RC["StrokeWidth"].GetValue(StrokeWidth);
            var argb = RM.RC["RopeColor"].GetValue<string>().Split(',').Select((v) => Convert.ToByte(v)).ToArray();
            RopeColor = Color.FromArgb(argb[0], argb[1], argb[2], argb[3]);
            argb = RM.RC["StrokeColor"].GetValue<string>().Split(',').Select((v) => Convert.ToByte(v)).ToArray();
            StrokeColor = Color.FromArgb(argb[0], argb[1], argb[2], argb[3]);
            ImgTransOffset_X = RM.RC["ImgTransOffset_X"].GetValue(ImgTransOffset_X);
            ImgTransOffset_Y = RM.RC["ImgTransOffset_Y"].GetValue(ImgTransOffset_Y);
            ImgAngleOffset = RM.RC["ImgAngleOffset"].GetValue(ImgAngleOffset);
            ImgScale = RM.RC["ImgScale"].GetValue(ImgScale);
            CursorOffset_X = RM.RC["CursorOffset_X"].GetValue(CursorOffset_X);
            CursorOffset_Y = RM.RC["CursorOffset_Y"].GetValue(CursorOffset_Y);
            TargetFPS = RM.RC["TargetFPS"].GetValue(TargetFPS);
            GIFPS = RM.RC["GIFPS"].GetValue(GIFPS);
            GIFMaxLoop = RM.RC["GIFMaxLoop"].GetValue(GIFMaxLoop);
            GifFolder = RM.RC["GifFolder"].GetValue(GifFolder);
            IsFlipGIF = RM.RC["IsFlipGIF"].GetValue(IsFlipGIF);
            IsFollowMode = RM.RC["IsFollowMode"].GetValue(IsFollowMode);
        }

        #region 属性声明
        private string _currentConfig;
        public string CurrentConfig
        {
            get => _currentConfig;
            set
            {
                if (_currentConfig != value)
                {
                    _currentConfig = value;
                    RaisePropertyChanged();
                }
            }
        }
        public string GifFolder
        {
            get => gifLoder.GifFolder;
            set
            {
                if (gifLoder.GifFolder != value)
                {
                    gifLoder.GifFolder = value;
                    gifLoder.LoadAllImgs(value);
                    RaisePropertyChanged();
                    RecordPropChanged(gifLoder.GifFolder);
                    painter.ResetGeometryProps();
                }
            }
        }
        //public float Gravity;
        public float Gravity
        {
            get => rope.Gravity;
            set
            {
                if (rope.Gravity != value)
                {
                    rope.Gravity = value;
                    RaisePropertyChanged();
                    RecordPropChanged(value);
                }
            }
        }
        //public float Stiffness;
        public float Stiffness
        {
            get => rope.Stiffness;
            set
            {
                if (rope.Stiffness != value)
                {
                    rope.Stiffness = value;
                    RaisePropertyChanged();
                    RecordPropChanged(value);
                }
            }
        }
        //public float Damp;
        public float Damp
        {
            get => rope.Damp;
            set
            {
                if (rope.Damp != value)
                {
                    rope.Damp = value;
                    RaisePropertyChanged();
                    RecordPropChanged(value);
                }
            }
        }
        //public float Mass;
        public float Mass
        {
            get => rope.Mass; set
            {
                if (rope.Mass != value)
                {
                    rope.Mass = value;
                    RaisePropertyChanged();
                    RecordPropChanged(value);
                }
            }
        }
        //public int Iterations;
        public int Iterations
        {
            get => rope.Iterations;
            set
            {
                if (rope.Iterations != value)
                {
                    rope.Iterations = value;
                    RaisePropertyChanged();
                    RecordPropChanged(value);
                }
            }
        }
        //public int NodeLength;
        public int NodeLength
        {
            get => rope.NodeLength;
            set
            {
                if (rope.NodeLength != value)
                {
                    rope.NodeLength = value;
                    RaisePropertyChanged();
                    RecordPropChanged(value);
                }
            }
        }
        //public int NodeNums;
        public int NodeNums
        {
            get => rope.NodeNums;
            set
            {
                if (value < rope.NodeNums)
                {
                    for (int i = 0; i < rope.NodeNums - value; i++)
                    {
                        rope.DeleteNode();
                    }
                }
                if (value > rope.NodeNums)
                {
                    for (int i = 0; i < value - rope.NodeNums; i++)
                    {
                        rope.AddNode();
                    }
                }
                RaisePropertyChanged();
                RecordPropChanged(value);
            }
        }
        public int FastVelocity
        {
            get => rope.FastVelocity;
            set
            {
                if (rope.FastVelocity != value)
                {
                    rope.FastVelocity = value;
                    RaisePropertyChanged();
                    RecordPropChanged(value);
                }
            }
        }
        public double RopeWidth
        {
            get => painter.RopeWidth;
            set
            {
                if (painter.RopeWidth != value)
                {
                    painter.RopeWidth = value;
                    RaisePropertyChanged();
                    RecordPropChanged(value);
                    painter.ResetGeometryProps();
                }
            }
        }
        public double StrokeWidth
        {
            get => painter.StrokeWidth;
            set
            {
                if (painter.StrokeWidth != value)
                {
                    painter.StrokeWidth = value;
                    RaisePropertyChanged();
                    RecordPropChanged(value);
                    painter.ResetGeometryProps();
                }
            }
        }

        public Color RopeColor
        {
            get => painter.RopeColor;
            set
            {
                if (painter.RopeColor != value)
                {
                    painter.RopeColor = value;
                    RaisePropertyChanged();
                    RecordPropChanged($"{value.A},{value.R},{value.G},{value.B}");
                    painter.ResetGeometryProps();
                }
            }
        }
        public Color StrokeColor
        {
            get => painter.RopeStroke;
            set
            {
                if (painter.RopeStroke != value)
                {
                    painter.RopeStroke = value;
                    RaisePropertyChanged();
                    RecordPropChanged($"{value.A},{value.R},{value.G},{value.B}");
                    painter.ResetGeometryProps();
                }
            }
        }
        //public Point ImgTransOffset;
        public double ImgTransOffset_X
        {
            get => painter.ImgTransOffset.X;
            set
            {
                if (painter.ImgTransOffset.X != value)
                {
                    painter.ImgTransOffset.X = value;
                    RaisePropertyChanged();
                    RecordPropChanged(value);
                    painter.ResetGeometryProps();
                }
            }
        }
        public double ImgTransOffset_Y
        {
            get => painter.ImgTransOffset.Y;
            set
            {
                if (painter.ImgTransOffset.Y != value)
                {
                    painter.ImgTransOffset.Y = value;
                    RaisePropertyChanged();
                    RecordPropChanged(value);
                    painter.ResetGeometryProps();
                }
            }
        }
        public double ImgAngleOffset
        {
            get => painter.ImgAngleOffset;
            set
            {
                if (painter.ImgAngleOffset != value)
                {
                    painter.ImgAngleOffset = value;
                    RaisePropertyChanged();
                    RecordPropChanged(value);
                }
            }
        }
        public double ImgScale
        {
            get => painter.ImgScale;
            set
            {
                if (painter.ImgScale != value)
                {
                    painter.ImgScale = value;
                    RaisePropertyChanged();
                    RecordPropChanged(value);
                    painter.ResetGeometryProps();
                }
            }
        }

        private double _cursorOffset_X;
        public double CursorOffset_X
        {
            get => _cursorOffset_X;
            set
            {
                if (_cursorOffset_X != value)
                {
                    _cursorOffset_X = value;
                    RaisePropertyChanged();
                    RecordPropChanged(value);
                }
            }
        }
        private double _cursorOffset_Y;
        public double CursorOffset_Y
        {
            get => _cursorOffset_Y;
            set
            {
                if (_cursorOffset_Y != value)
                {
                    _cursorOffset_Y = value;
                    RaisePropertyChanged();
                    RecordPropChanged(value);
                }
            }
        }
        public int FPS { get => frameController.FPS; }
        public int TargetFPS
        {
            get => (int)(1 / (frameController.TargetFrameTime / 1000));
            set
            {
                var ft = 1.0 / value * 1000;
                if (frameController.TargetFrameTime != ft)
                {
                    frameController.TargetFrameTime = ft;
                    RaisePropertyChanged();
                    RecordPropChanged(value);
                }
            }
        }
        public int GIFPS
        {
            get => gifLoder.TargetFrame;
            set
            {
                if (gifLoder.TargetFrame != value)
                {
                    gifLoder.TargetFrame = value;
                    RaisePropertyChanged();
                    RecordPropChanged(value);
                }
            }
        }
        public int GIFMaxLoop
        {
            get => gifLoder.MaxLoopCount;
            set
            {
                if (gifLoder.MaxLoopCount != value)
                {
                    gifLoder.MaxLoopCount = value;
                    RaisePropertyChanged();
                    RecordPropChanged(value);
                }
            }
        }
        #endregion
        public bool IsFlipGIF
        {
            get => painter.IsFlipGif;
            set
            {
                if (painter.IsFlipGif != value)
                {
                    painter.IsFlipGif = value;
                    RaisePropertyChanged();
                    RecordPropChanged(value);
                }
            }
        }
        public bool IsFollowMode
        {
            get => painter.IsFollowMode;
            set
            {
                if (painter.IsFollowMode != value)
                {
                    painter.IsFollowMode = value;
                    RaisePropertyChanged();
                    RecordPropChanged(value);
                }
            }
        }

        private bool _isStartUp;
        public bool IsStartUp
        {
            get => _isStartUp;
            set
            {
                _isStartUp = value;
                RaisePropertyChanged();
            }
        }

        private string _configPath;
        public string ConfigPath
        {
            get => _configPath;
            set
            {
                if (_configPath != value)
                {
                    _configPath = value; RaisePropertyChanged();
                }
            }
        }

        private string[] _gifFolders;
        public string[] GifFolders
        {
            get => _gifFolders;
            set
            {
                _gifFolders = value;
                RaisePropertyChanged();
            }
        }

    }
}
