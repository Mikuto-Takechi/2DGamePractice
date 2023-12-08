using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Takechi.BT
{
    public enum BTState
    {
        Failure,
        Success,
        Running,
        Abort
    }

    public static class BT
    {
        public static Root Root(List<Node> children = null) => new Root(children);
        public static Sequence Sequence(List<Node> children = null) => new Sequence(children);
        public static Parallel Parallel(List<Node> children = null) => new Parallel(children);
        public static Selector Selector(bool shuffle = false) => new Selector(shuffle);
        public static Selector Selector(List<Node> children = null) => new Selector(children);
        public static ParallelSelector ParallelSelector(List<Node> children = null) => new ParallelSelector(children);
        public static Action Action(System.Func<IEnumerator<BTState>> coroutine) => new Action(coroutine);
        public static Action Action(System.Action fn) => new Action(fn);
        public static Inverter Inverter(Node child) => new Inverter(child);
        public static ConditionalBranch If(System.Func<bool> fn) => new ConditionalBranch(fn);
        public static While While(System.Func<bool> fn) => new While(fn);
        /// <summary>ノードの一種。条件を満たしていればtrueを出力する。Tick()では成功を出力する</summary>
        public static Condition Condition(System.Func<bool> fn) => new Condition(fn);   //  Node
        public static Repeat Repeat(int count, Node child) => new Repeat(count, child);    //  Decorator
        /// <summary>ノードの一種。指定した秒数待ってから成功を出力する。待ち時間中のTick()ではContinueを出力し続ける</summary>
        public static Wait Wait(float seconds) => new Wait(seconds);    //  Node
        public static Trigger Trigger(Animator animator, string name, bool set = true) => new Trigger(animator, name, set); //  Node
        public static WaitForAnimatorState WaitForAnimatorState(Animator animator, string name, int layer = 0) => new WaitForAnimatorState(animator, name, layer);  //  Node
        public static SetBool SetBool(Animator animator, string name, bool value) => new SetBool(animator, name, value);    //  Node
        public static SetActive SetActive(GameObject gameObject, bool active) => new SetActive(gameObject, active); //  Node
        //public static WaitForAnimatorSignal WaitForAnimatorSignal(Animator animator, string name, string state, int layer = 0) => new WaitForAnimatorSignal(animator, name, state, layer);
        public static Terminate Terminate() => new Terminate(); //  Node
        /// <summary>ノードの一種。ログを出力する。失敗することが無いのでTick()では必ず成功を出力する</summary>
        public static Log Log(string msg) => new Log(msg);  //  Node
        public static RandomSequence RandomSequence(int[] weights = null) => new RandomSequence(weights);   //  Block

    }

    public abstract class Node
    {
        public abstract BTState Tick();
    }
    public class ConditionalBranch : Block
    {
        public System.Func<bool> fn;
        bool tested = false;
        public ConditionalBranch(System.Func<bool> fn)
        {
            this.fn = fn;
        }
        public override BTState Tick()
        {
            if (!tested)
            {
                tested = fn();
            }
            if (tested)
            {
                var result = base.Tick();
                if (result == BTState.Running)
                    return BTState.Running;
                else
                {
                    tested = false;
                    return result;
                }
            }
            else
            {
                return BTState.Failure;
            }
        }

        public override string ToString()
        {
            return "ConditionalBranch : " + fn.Method.ToString();
        }
    }

    /// <summary>
    /// メソッドがtrueを返す間、すべての子を実行する。
    /// </summary>
    public class While : Block
    {
        public System.Func<bool> fn;

        public While(System.Func<bool> fn)
        {
            this.fn = fn;
        }

        public override BTState Tick()
        {
            if (fn())
                base.Tick();
            else
            {
                //if we exit the loop
                ResetChildren();
                return BTState.Failure;
            }

            return BTState.Running;
        }

        public override string ToString()
        {
            return "While : " + fn.Method.ToString();
        }
    }

    public abstract class Block : Composite
    {
        public override BTState Tick()
        {
            switch (children[activeChild].Tick())
            {
                case BTState.Running:
                    return BTState.Running;
                default:
                    activeChild++;
                    if (activeChild == children.Count)
                    {
                        activeChild = 0;
                        return BTState.Success;
                    }
                    return BTState.Running;
            }
        }
    }
    /// <summary>
    /// アニメーターのトリガーをアクティブにする。
    /// </summary>
    public class Trigger : Node
    {
        Animator animator;
        int id;
        string triggerName;
        bool set = true;

        //もし set == false なら、トリガーをセットする代わりにリセットする。
        public Trigger(Animator animator, string name, bool set = true)
        {
            this.id = Animator.StringToHash(name);
            this.animator = animator;
            this.triggerName = name;
            this.set = set;
        }

        public override BTState Tick()
        {
            if (set)
                animator.SetTrigger(id);
            else
                animator.ResetTrigger(id);

            return BTState.Success;
        }

        public override string ToString()
        {
            return "Trigger : " + triggerName;
        }
    }

    /// <summary>
    /// アニメーターにブール値を設定する。
    /// </summary>
    public class SetBool : Node
    {
        Animator animator;
        int id;
        bool value;
        string triggerName;

        public SetBool(Animator animator, string name, bool value)
        {
            this.id = Animator.StringToHash(name);
            this.animator = animator;
            this.value = value;
            this.triggerName = name;
        }

        public override BTState Tick()
        {
            animator.SetBool(id, value);
            return BTState.Success;
        }

        public override string ToString()
        {
            return "SetBool : " + triggerName + " = " + value.ToString();
        }
    }

    /// <summary>
    /// アニメーターがある状態に達するのを待つ。
    /// </summary>
    public class WaitForAnimatorState : Node
    {
        Animator animator;
        int id;
        int layer;
        string stateName;

        public WaitForAnimatorState(Animator animator, string name, int layer = 0)
        {
            this.id = Animator.StringToHash(name);
            if (!animator.HasState(layer, this.id))
            {
                Debug.LogError("The animator does not have state: " + name);
            }
            this.animator = animator;
            this.layer = layer;
            this.stateName = name;
        }

        public override BTState Tick()
        {
            var state = animator.GetCurrentAnimatorStateInfo(layer);
            if (state.fullPathHash == this.id || state.shortNameHash == this.id)
                return BTState.Success;
            return BTState.Running;
        }

        public override string ToString()
        {
            return "Wait For State : " + stateName;
        }
    }

    /// <summary>
    /// ゲームオブジェクトのアクティブフラグを設定する。
    /// </summary>
    public class SetActive : Node
    {

        GameObject gameObject;
        bool active;

        public SetActive(GameObject gameObject, bool active)
        {
            this.gameObject = gameObject;
            this.active = active;
        }

        public override BTState Tick()
        {
            gameObject.SetActive(this.active);
            return BTState.Success;
        }

        public override string ToString()
        {
            return "Set Active : " + gameObject.name + " = " + active;
        }
    }

    /// <summary>
    /// アニメーターの SendSignal ステートマシン動作からシグナルを受信するのを待つ。
    /// </summary>
    //public class WaitForAnimatorSignal : BTNode
    //{
    //    internal bool isSet = false;
    //    string name;
    //    int id;

    //    public WaitForAnimatorSignal(Animator animator, string name, string state, int layer = 0)
    //    {
    //        this.name = name;
    //        this.id = Animator.StringToHash(name);
    //        if (!animator.HasState(layer, this.id))
    //        {
    //            Debug.LogError("The animator does not have state: " + name);
    //        }
    //        else
    //        {
    //            SendSignal.Register(animator, name, this);
    //        }
    //    }

    //    public override BTState Tick()
    //    {
    //        if (!isSet)
    //            return BTState.Continue;
    //        else
    //        {
    //            isSet = false;
    //            return BTState.Success;
    //        }

    //    }

    //    public override string ToString()
    //    {
    //        return "Wait For Animator Signal : " + name;
    //    }
    //}
}

#if UNITY_EDITOR
namespace Takechi.BT
{
    public interface IBTDebugable
    {
        Root GetAIRoot();
    }
}
#endif