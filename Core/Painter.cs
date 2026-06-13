using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using Point = System.Windows.Point;
using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;
using Size = System.Windows.Size;

namespace CursorTail.Core
{
    public class PainterVisionHost : FrameworkElement
    {
        private Rope _rope;
        private Pen _ropeFullPen;
        private Pen _ropeStrokePen;

        public double RopeWidth;
        public double StrokeWidth;
        public Color RopeColor;
        public Color RopeStroke;
        public Point ImgTransOffset;
        public double ImgAngleOffset;
        public double ImgScale;
        public Point? StartPoint;

        private StreamGeometry _geometry;
        private TransformGroup _imgTrans;
        private TranslateTransform _imgTranslate;
        private RotateTransform _imgRotate;

        private DrawingVisual _geoVisual;
        private DrawingVisual _imgVisual;
        private DrawingVisual[] _visuals;

        private GIFLoder _gifLoder;
        //private BitmapSource _imgSource;
        
        /// <summary>
        /// 缩放尺寸
        /// </summary>
        private Size _rectScale;
        /// <summary>
        /// 定位Rect
        /// </summary>
        private Rect _imgRect;

        protected override int VisualChildrenCount => 2;
        protected override Visual GetVisualChild(int index) => _visuals[index];

        public PainterVisionHost(Rope rope, Color fill, Color stroke, Point imgOffset, double imgAngle, GIFLoder loder, double ropeWidth = 2, double strokeWidth = 0.5, double scale = 0.5, Point? startPoint = null)
        {
            //同步变量
            _rope = rope;
            RopeColor = fill;
            RopeStroke = stroke;
            RopeWidth = ropeWidth;
            StrokeWidth = strokeWidth;
            ImgTransOffset = imgOffset;
            ImgAngleOffset = imgAngle;
            ImgScale = scale;
            //实例化变量
            _geometry = new StreamGeometry();
            _geoVisual = new DrawingVisual();
            _imgVisual = new DrawingVisual();
            _visuals = [_geoVisual, _imgVisual];
            AddVisualChild(_geoVisual);
            AddVisualChild(_imgVisual);

            _imgTranslate = new TranslateTransform();
            _imgRotate = new RotateTransform();
            _imgTrans = new TransformGroup();
            _imgTrans.Children.Add(_imgTranslate);
            _imgTrans.Children.Add(_imgRotate);

            _gifLoder = loder;
            ResetGeometryProps();
            StartPoint = startPoint;
        }

        /// <summary>
        /// 该函数内仅发生Color、Width、Scale、变化时触发
        /// </summary>
        public void ResetGeometryProps()
        {
            _ropeFullPen = new Pen(new SolidColorBrush(RopeColor), RopeWidth);
            _ropeStrokePen = new Pen(new SolidColorBrush(RopeStroke), StrokeWidth * 2 + RopeWidth);
            _rectScale = new Size(_gifLoder.GetCurrentFrame.Width * ImgScale, _gifLoder.GetCurrentFrame.Height * ImgScale);
            //平移：平移至实时中心
            _imgTranslate.X = -_rectScale.Width / 2 + ImgTransOffset.X;
            _imgTranslate.Y = -_rectScale.Height / 2 + ImgTransOffset.Y;
        }
        private const double at = 180 / MathF.PI;
        public void UpdatePrepare()
        {
            var ropeNodes = _rope.RopeNodes;
            _geometry.Clear();

            int secondLastIndex = ropeNodes.Length - 2;
            //定位矩形：左上角定位+偏移+缩放
            _imgRect = new Rect(ropeNodes[secondLastIndex + 1].X, ropeNodes[secondLastIndex + 1].Y, _rectScale.Width, _rectScale.Height);

            //旋转：旋转中心+角度偏移
            _imgRotate.CenterX = ropeNodes[secondLastIndex + 1].X;
            _imgRotate.CenterY = ropeNodes[secondLastIndex + 1].Y;
            Vector2 A2B = _rope.GetNodeByIndex(secondLastIndex + 1) - _rope.GetNodeByIndex(secondLastIndex);
            if (A2B.Y > 0 && A2B.Y < 0.0001f)
            {
                _imgRotate.Angle = 90 + ImgAngleOffset;
            }
            else if (A2B.Y < 0 && A2B.Y > -0.0001f)
            {
                _imgRotate.Angle = -90 + ImgAngleOffset;
            }
            else
            {
                _imgRotate.Angle = -Math.Atan(A2B.X / A2B.Y) * at + ImgAngleOffset;
                if (A2B.Y < 0)
                {
                    _imgRotate.Angle += 180;
                }
            }
            using (StreamGeometryContext sgc = _geometry.Open())
            {
                if (StartPoint != null)
                {
                    Parallel.For(0, ropeNodes.Length, (i) =>
                    {
                        ropeNodes[i] = new(ropeNodes[i].X - StartPoint.Value.X, ropeNodes[i].Y - StartPoint.Value.Y);
                    });
                }
                sgc.BeginFigure(ropeNodes[0], false, false);
                for (int i = 1; i < ropeNodes.Length; i++)
                {
                    sgc.LineTo(ropeNodes[i], true, true);
                }
            }
        }

        public void Update()
        {
            _gifLoder.LoadFrameControler.Invoke();
            UpdatePrepare();
            UpdateGeo();
            UpdateImg();
        }

        public void UpdateGeo()
        {
            using (DrawingContext gdc = _geoVisual.RenderOpen())
            {
                if (StrokeWidth >= 0)
                {
                    gdc.DrawGeometry(null, _ropeStrokePen, _geometry);
                }
                gdc.DrawGeometry(null, _ropeFullPen, _geometry);
            }
        }

        public void UpdateImg()
        {
            using (DrawingContext idc = _imgVisual.RenderOpen())
            {
                idc.PushTransform(_imgTrans);
                idc.DrawImage(_gifLoder.GetCurrentFrame, _imgRect);
                idc.Pop();
            }
        }
    }
}
