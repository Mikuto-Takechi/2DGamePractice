using UnityEngine;

namespace Takechi.BT
{
    /// <summary>
    /// デコレータの基本クラス、直下の子ノードを1つしか持てない
    /// </summary>
    public abstract class Decorator : Node
    {
        protected Node child;
        public Decorator(Node child)
        {
            this.child = child;
        }
    }

    /// <summary>
    /// 子ノードを指定した回数繰り返す。<br/>
    /// 子ノードが常にSuccessなら繰り返した後にSuccessを返す。<br/>
    /// 子ノードがFailureを返した場合はループを中断し、Failureを返す。<br/>
    /// 子ノードがRunningならこのノードもRunningを返し、カウントアップせずに子ノードの処理を待ちます。
    /// </summary>
    public class Repeat : Decorator
    {
        public int limit = 1;
        int count = 0;
        /// <summary>ノードと繰り返し回数を渡す</summary>
        public Repeat(int count, Node child) : base(child)
        {
            this.limit = count;
        }
        public override BTState Tick()
        {
            if (limit > 0 && count < limit)
            {
                switch (child.Tick())
                {
                    case BTState.Running:
                        return BTState.Running;
                    case BTState.Failure:
                        count = 0;
                        return BTState.Failure;
                    default:
                        count++;
                        if (count == limit)
                        {
                            count = 0;
                            return BTState.Success;
                        }
                        return BTState.Running;
                }
            }
            count = 0;
            return BTState.Failure;
        }

        public override string ToString()
        {
            return "Repeat : " + count + " / " + limit;
        }
    }
    /// <summary>
    /// 子ノードがFailureを返した時にはSuccessを返し、Successを返したときにはFailureを返す。<br/>
    /// 子ノードがRunningを返した時には、このノードもRunningを返す。
    /// </summary>
    public class Inverter : Decorator
    {
        public Inverter(Node child) : base(child) { }

        public override BTState Tick()
        {
            switch (child.Tick())
            {
                case BTState.Running:
                    return BTState.Running;
                case BTState.Failure:
                    return BTState.Success;
                case BTState.Success:
                    return BTState.Failure;
                default:
                    return BTState.Running;
            }
        }

        public override string ToString()
        {
            return "Inverter :";
        }
    }
    /// <summary>
    /// 子ノードがRunningを返した時には、このノードもRunningを返す。<br/>
    /// そうでない場合は、常にSuccessを返す。
    /// </summary>
    public class ForceSuccess : Decorator
    {
        public ForceSuccess(Node child) : base(child) { }

        public override BTState Tick()
        {
            switch (child.Tick())
            {
                case BTState.Running:
                    return BTState.Running;
                default:
                    return BTState.Success;
            }
        }

        public override string ToString()
        {
            return "ForceSuccess :";
        }
    }
    /// <summary>
    /// 子ノードがRunningを返した時には、このノードもRunningを返す。<br/>
    /// そうでない場合は、常にFailureを返す。
    /// </summary>
    public class ForceFailure : Decorator
    {
        public ForceFailure(Node child) : base(child) { }

        public override BTState Tick()
        {
            switch (child.Tick())
            {
                case BTState.Running:
                    return BTState.Running;
                default:
                    return BTState.Failure;
            }
        }

        public override string ToString()
        {
            return "ForceFailure :";
        }
    }
    /// <summary>
    /// 指定した秒数Running状態を維持してから子ノードの評価を始めるデコレータ。
    /// </summary>
    public class Delay : Decorator
    {
        public float seconds = 0;
        float future = -1;
        public Delay(float seconds, Node child) : base(child)
        {
            this.seconds = seconds;
        }

        public override BTState Tick()
        {
            if (future < 0)
                future = Time.time + seconds;

            if (Time.time >= future)
            {
                switch (child.Tick())
                {
                    case BTState.Running:
                        return BTState.Running;
                    case BTState.Failure:
                        future = -1;
                        return BTState.Failure;
                    case BTState.Success:
                        future = -1;
                        return BTState.Success;
                    default:
                        return BTState.Running;
                }
            }
            else
                return BTState.Running;
        }

        public override string ToString()
        {
            return "Delay : " + (future - Time.time) + " / " + seconds;
        }
    }
}