using System;
using System.Collections.Generic;
using System.Text;
using static System.Windows.Forms.AxHost;

namespace CursorTail.Core
{
    public enum States
    {
        Stopped,
        SlowMoved,
        FastMoved,
        Collided,
    }
    public class StateMachine
    {
        public States CurrentState;
        public delegate void StateChange(States state);
        public delegate void StateKeep();
        public StateChange? RaiseStateChange;
        public StateKeep? RaiseStateKeep;
        public StateMachine(StateChange stateChange, StateKeep stateKeep)
        {
            CurrentState = States.SlowMoved;
            RaiseStateChange += stateChange;
            RaiseStateKeep += stateKeep;
        }
        public StateMachine()
        {
            CurrentState = States.SlowMoved;
        }
        public void ResetState()
        {
            RaiseStateChange?.Invoke(States.Stopped);
            CurrentState = States.Stopped;
        }
        public void SwitchTo(States state)
        {

            if (CurrentState != state && (int)state > (int)CurrentState)
            {
                //处于stop立刻切换
                RaiseStateChange?.Invoke(state);
                CurrentState = state;
            }
            else if (CurrentState != States.Stopped && CurrentState == state)
            {
                //处于state，状态保持
                RaiseStateKeep?.Invoke();
            }
            else
            {
                //处于非normal,等待复位
            }
        }
    }
}
