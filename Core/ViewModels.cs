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
        public Uri SavePath = App.Current.StartupUri;
        public void CreatRecords()
        {
            RM.RC.Add(new(typeof(string), "StrokeColor", "255,0,0,0"));
            RM.RC.Add(new(typeof(double), "Gravity", 1));
            RM.RC.Add(new(typeof(double), "Stiffness", 1));
            RM.RC.Add(new(typeof(double), "Damp", 1));
            RM.RC.Add(new(typeof(double), "Mass", 1));
            RM.RC.Add(new(typeof(int), "Iterations", 10));
            RM.RC.Add(new(typeof(int), "NodeLength", 5));
            RM.RC.Add(new(typeof(int), "NodeNums", 18));
            RM.RC.Add(new(typeof(double), "RopeWidth", 3));
            RM.RC.Add(new(typeof(double), "StrokeWidth", 3));
            RM.RC.Add(new(typeof(string), "RopeColor", "255,0,0,0"));
            RM.RC.Add(new(typeof(double), "ImgTransOffset_X", 1));
            RM.RC.Add(new(typeof(double), "ImgTransOffset_Y", 1));
            RM.RC.Add(new(typeof(double), "ImgAngleOffset", 1));
            RM.RC.Add(new(typeof(double), "ImgScale", 1));
            RM.RC.Add(new(typeof(double), "CursorOffset_X", 1));
            RM.RC.Add(new(typeof(double), "CursorOffset_Y", 1));
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
        }

        #region 属性声明
        private Uri _currentConfig;

        public Uri CurrentConfig
        {
            get => _currentConfig;
            set
            {
                if (_currentConfig != value)
                {
                    _currentConfig = value;
                    RaisePropertyChanged();
                    RecordPropChanged(value.ToString);
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
        public int FPS { get=>frameController.FPS;  }
        #endregion
    }
}
