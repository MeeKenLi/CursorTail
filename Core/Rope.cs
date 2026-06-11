using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Windows;

namespace CursorTail.Core
{
    public class Rope
    {
        private List<Vector2> _oldNodes;
        private List<Vector2> _newNodes;
        //private Vector2 _oldCursorPos;
        public float Gravity;
        public float Stiffness;
        public float Damp;
        public float Mass;
        public int Iterations;
        public int NodeLength;
        public int NodeNums;
        public Vector2 CollideBox;
        //public Vector2 CenterPoint;
        //public double BoxSize => NodeNums * NodeLength / Stiffness * 3 * 2;
        public Point[] _dPoints;

        public Rope(
            Vector2 collideBox,
            float gravity = 1f,//0-2
            float stiffness = 1f,
            float mass = 0.5f,//0.3-2
            float damp = 0.95f, //0.85-1.0
            int iterations = 10,//1-15
            int nodeLength = 10,//1-10
            int nodeNums = 9)
        {
            Gravity = gravity;
            Stiffness = stiffness;
            Damp = damp;
            Mass = mass;
            Iterations = iterations;
            NodeLength = nodeLength;
            NodeNums = nodeNums;
            //CenterPoint = new Vector2((float)BoxSize/2);
            CollideBox = collideBox;
            _newNodes = new List<Vector2>();
            _oldNodes = new List<Vector2>();
            InitNodes();
        }

        public void UpdateGravity(Vector2 newCursorPos)
        {

            //var cursorVelocity = newCursorPos - _oldCursorPos;
            //_oldCursorPos = newCursorPos;

            //左闭右开
            Parallel.For(0, NodeNums + 1, (i) =>
            {
                var temp = _newNodes[i];
                if (i == 0)
                {
                    _newNodes[0] = newCursorPos;
                }
                else
                {
                    var velocity = _newNodes[i] - _oldNodes[i] + new Vector2(0, Gravity);
                    _newNodes[i] += velocity * Damp;
                }
                #region 边界碰撞
                //边界碰撞处理
                bool isCrashed = false;
                var Crashed = _newNodes[i];
                if (_newNodes[i].X < 0)
                {
                    isCrashed = true;
                    Crashed.X = 0;
                }
                if (_newNodes[i].Y < 0)
                {
                    isCrashed = true;
                    Crashed.Y = 0;
                }
                if (_newNodes[i].X > CollideBox.X)
                {
                    isCrashed = true;
                    Crashed.X = CollideBox.X;
                }
                if (_newNodes[i].Y > CollideBox.Y)
                {
                    isCrashed = true;
                    Crashed.Y = CollideBox.Y;
                }
                if (isCrashed)
                {
                    temp = _newNodes[i];
                    _newNodes[i] = Crashed;
                }
                #endregion

                _oldNodes[i] = temp;
            });
        }

        public void ConstraintIteration()
        {
            for (int _ = 0; _ < Iterations; _++)
            {
                for (int i = 0; i < NodeNums; i++)
                {
                    //每次迭代从前往后，将每个点移动半个距离差，
                    //第一个点i=0不移动，第二个点移动整个offset
                    //最后一个点不用单独迭代 i<N-1

                    //向量使用B-A 即从A指向B
                    Vector2 A2B;
                    A2B = _newNodes[i + 1] - _newNodes[i];
                    var len_A2B = A2B.Length();
                    if (len_A2B < 0.001f)
                        continue;
                    var dir_A2B = A2B / len_A2B;
                    //差值使用长减短(大多数情况)，当前长度-目标长度
                    var difference = len_A2B - NodeLength;
                    //这样得到的差值向量则上加下减
                    var offset = difference * Stiffness * dir_A2B;
                    if (i == 0)
                    {
                        _newNodes[i + 1] -= offset;
                    }
                    else if (i == NodeNums - 1)
                    {
                        _newNodes[i] += offset * Mass;
                        _newNodes[i + 1] -= offset * (1 - Mass);
                    }
                    else
                    {
                        _newNodes[i] += offset * 0.5f;
                        _newNodes[i + 1] -= offset * 0.5f;
                    }
                }
            }
        }

        public void InitNodes()
        {
            for (int i = 0; i < NodeNums + 1; i++)
            {
                _newNodes.Add(new Vector2(10, i * NodeLength));
                _oldNodes.Add(new Vector2(10, i * NodeLength));
            }
            _dPoints = new Point[NodeNums + 1];
            //SetCenterPoint();
            UpdatePoints();
        }

        //public void SetCenterPoint()
        //{
        //    _newNodes[0] = CenterPoint;
        //    _oldNodes[0] = CenterPoint;
        //    _dPoints[0].X = CenterPoint.X;
        //    _dPoints[0].Y = CenterPoint.Y;
        //}

        public void AddNode()
        {
            NodeNums++;
            _newNodes.Add(new Vector2(0, 0));
            _oldNodes.Add(new Vector2(0, 0));
            _dPoints = new Point[NodeNums + 1];
        }

        public void DeleteNode()
        {
            NodeNums--;
            _newNodes.RemoveAt(_newNodes.Count - 1);
            _oldNodes.RemoveAt(_oldNodes.Count - 1);
            _dPoints = new Point[NodeNums + 1];
        }

        public Vector2 GetNodeByIndex(int i) => _newNodes[i];
        public Point[] RopeNodes => _dPoints;
        public float MaxRopeLength => NodeNums * NodeLength / Stiffness;

        public void Update(Vector2 cursorPos)
        {
            UpdateGravity(cursorPos);
            ConstraintIteration();
            UpdatePoints();
        }
        public void UpdatePoints()
        {
            for (int i = 0; i < NodeNums + 1; i++)
            {
                _dPoints[i].X = _newNodes[i].X;
                _dPoints[i].Y = _newNodes[i].Y;
            }
        }
    }
}
