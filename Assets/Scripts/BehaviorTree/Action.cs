using System.Collections.Generic;
using UnityEngine;

namespace Takechi.BT
{
    /// <summary>
    /// メソッドを呼び出すか、コルーチンを実行する。
    /// </summary>
    public class Action : Node
    {
        System.Action fn;
        System.Func<IEnumerator<BTState>> coroutineFactory;
        IEnumerator<BTState> coroutine;
        public Action(System.Action fn)
        {
            this.fn = fn;
        }
        public Action(System.Func<IEnumerator<BTState>> coroutineFactory)
        {
            this.coroutineFactory = coroutineFactory;
        }
        public override BTState Tick()
        {
            if (fn != null)
            {
                fn();
                return BTState.Success;
            }
            else
            {
                if (coroutine == null)
                    coroutine = coroutineFactory();
                if (!coroutine.MoveNext())
                {
                    coroutine = null;
                    return BTState.Success;
                }
                var result = coroutine.Current;
                if (result == BTState.Running)
                    return BTState.Running;
                else
                {
                    coroutine = null;
                    return result;
                }
            }
        }

        public override string ToString()
        {
            return "Action : " + fn.Method.ToString();
        }
    }
    /// <summary>
    /// メソッドを呼び出し、メソッドが真を返せば成功を返し、そうでなければ失敗を返す。
    /// </summary>
    public class Condition : Node
    {
        public System.Func<bool> fn;

        public Condition(System.Func<bool> fn)
        {
            this.fn = fn;
        }
        public override BTState Tick()
        {
            return fn() ? BTState.Success : BTState.Failure;
        }

        public override string ToString()
        {
            return "Condition : " + fn.Method.ToString();
        }
    }
    /// <summary>
    /// 指定秒数待ってからSuccessを返す。
    /// </summary>
    public class Wait : Node
    {
        public float seconds = 0;
        float future = -1;
        public Wait(float seconds)
        {
            this.seconds = seconds;
        }

        public override BTState Tick()
        {
            if (future < 0)
                future = Time.time + seconds;

            if (Time.time >= future)
            {
                future = -1;
                return BTState.Success;
            }
            else
                return BTState.Running;
        }

        public override string ToString()
        {
            return "Wait : " + (future - Time.time) + " / " + seconds;
        }
    }
    public class Terminate : Node
    {
        public override BTState Tick()
        {
            return BTState.Abort;
        }
    }

    public class Log : Node
    {
        string msg;

        public Log(string msg)
        {
            this.msg = msg;
        }

        public override BTState Tick()
        {
            Debug.Log(msg);
            return BTState.Success;
        }
    }
}