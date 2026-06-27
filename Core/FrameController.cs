using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace CursorTail.Core
{
    public class FrameController
    {
        public delegate void UpdatePerFrameHandler();
        public UpdatePerFrameHandler UpdatePerFrame;

        public double TargetFrameTime;
        public int FPS;
        private int _frameCount;
        private long _previousSecond;
        private readonly Stopwatch _frameTimer;
        private long _previousFrame;
        private double _accrumulateTime;
        private double _maxDelta;
        public FrameController(UpdatePerFrameHandler updatePerFrame, int targetFps = 60)
        {
            UpdatePerFrame = updatePerFrame;
            TargetFrameTime = 1.0 / (double)targetFps * 1000;
            _frameTimer = Stopwatch.StartNew();
            _previousFrame = 0;
            _accrumulateTime = 0;
            _frameCount = 0;
            _previousSecond = 0;
            _maxDelta = 500;
        }

        public void UpdateFrame()
        {
            if (AccumulateUpdateTime())
            {
                UpdatePerFrame.Invoke();
            }
            AccumulateSecondTime();
        }

        private bool AccumulateUpdateTime()
        {
            long currentFrame = _frameTimer.ElapsedMilliseconds;
            double delta = currentFrame - _previousFrame;
            _accrumulateTime += delta < _maxDelta ? delta : _maxDelta;
            _previousFrame = currentFrame;
            if (_accrumulateTime >= TargetFrameTime)
            {
                _accrumulateTime -= TargetFrameTime;
                _frameCount++;
                return true;
            }
            return false;
        }
        private bool AccumulateSecondTime()
        {
            long currentFrame = _frameTimer.ElapsedMilliseconds;
            if (currentFrame - _previousSecond >= 1000)
            {
                _previousSecond = currentFrame;
                FPS = _frameCount;
                _frameCount = 0;
                return true;
            }
            return false;
        }
    }
}
