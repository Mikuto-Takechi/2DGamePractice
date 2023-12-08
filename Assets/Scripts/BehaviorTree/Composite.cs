using System.Collections.Generic;
using UnityEngine;

namespace Takechi.BT
{
    public abstract class Composite : Node
    {
        protected int activeChild;
        protected List<Node> children = new List<Node>();
        public Composite(List<Node> children = null)
        {
            if (children == null) return;
            for (var i = 0; i < children.Count; i++)
                this.children.Add(children[i]);
        }
        public virtual Composite OpenBranch(List<Node> children)
        {
            for (var i = 0; i < children.Count; i++)
                this.children.Add(children[i]);
            return this;
        }

        public List<Node> Children()
        {
            return children;
        }

        public int ActiveChild()
        {
            return activeChild;
        }

        public virtual void ResetChildren()
        {
            activeChild = 0;
            for (var i = 0; i < children.Count; i++)
            {
                Composite b = children[i] as Composite;
                if (b != null)
                {
                    b.ResetChildren();
                }
            }
        }
    }
    public class Sequence : Composite
    {
        public Sequence(List<Node> children) : base(children) { }
        public override BTState Tick()
        {
            var childState = children[activeChild].Tick();
            switch (childState)
            {
                case BTState.Success:
                    activeChild++;
                    if (activeChild == children.Count)
                    {
                        activeChild = 0;
                        return BTState.Success;
                    }
                    else
                        return BTState.Running;
                case BTState.Failure:
                    activeChild = 0;
                    return BTState.Failure;
                case BTState.Running:
                    return BTState.Running;
                case BTState.Abort:
                    activeChild = 0;
                    return BTState.Abort;
            }
            throw new System.Exception("Tick関数のswitch文を抜けてしまった。");
        }
    }
    /// <summary>
    /// 子が成功するまで各子を実行し、成功を返す。<br/>
    /// 成功した子プロセスがない場合は失敗を返す。
    /// </summary>
    public class Selector : Composite
    {
        public Selector(bool shuffle, List<Node> children = null) : base(children)
        {
            if (shuffle)
            {
                var n = children.Count;
                while (n > 1)
                {
                    n--;
                    var k = Mathf.FloorToInt(Random.value * (n + 1));
                    var value = children[k];
                    children[k] = children[n];
                    children[n] = value;
                }
            }
        }
        public Selector(List<Node> children) : base(children) { }

        public override BTState Tick()
        {
            var childState = children[activeChild].Tick();
            switch (childState)
            {
                case BTState.Success:
                    activeChild = 0;
                    return BTState.Success;
                case BTState.Failure:
                    activeChild++;
                    if (activeChild == children.Count)
                    {
                        activeChild = 0;
                        return BTState.Failure;
                    }
                    else
                        return BTState.Running;
                case BTState.Running:
                    return BTState.Running;
                case BTState.Abort:
                    activeChild = 0;
                    return BTState.Abort;
            }
            throw new System.Exception("Tick関数のswitch文を抜けてしまった。");
        }
    }
    public class Root : Composite
    {
        public bool isTerminated = false;
        public Root(List<Node> children) : base(children) { }
        public override BTState Tick()
        {
            if (isTerminated) return BTState.Abort;
            while (true)
            {
                switch (children[activeChild].Tick())
                {
                    case BTState.Running:
                        return BTState.Running;
                    case BTState.Abort:
                        isTerminated = true;
                        return BTState.Abort;
                    default:
                        activeChild++;
                        if (activeChild == children.Count)
                        {
                            activeChild = 0;
                            return BTState.Success;
                        }
                        continue;
                }
            }
        }
    }
    public class RandomSequence : Composite
    {
        int[] m_Weight = null;
        int[] m_AddedWeight = null;

        /// <summary>
        /// 再びトリガーされるたびに、ランダムな子供を1人選ぶ。
        /// </summary>
        /// <param name="weight">すべての子ノードが同じウェイトを持つように、nullのままにする。 
        /// 子ノードよりウェイトが少ない場合、後続の子ノードはすべてウェイト = 1になります。</param>
        public RandomSequence(int[] weight = null, List < Node > children = null) : base(children)
        {
            activeChild = -1;

            m_Weight = weight;
        }

        public override Composite OpenBranch(List<Node> children)
        {
            m_AddedWeight = new int[children.Count];

            for (int i = 0; i < children.Count; ++i)
            {
                int weight = 0;
                int previousWeight = 0;

                if (m_Weight == null || m_Weight.Length <= i)
                {//そのウェイトがなければ、ウェイトを1に設定する。
                    weight = 1;
                }
                else
                    weight = m_Weight[i];

                if (i > 0)
                    previousWeight = m_AddedWeight[i - 1];

                m_AddedWeight[i] = weight + previousWeight;
            }

            return base.OpenBranch(children);
        }

        public override BTState Tick()
        {
            if (activeChild == -1)
                PickNewChild();

            var result = children[activeChild].Tick();

            switch (result)
            {
                case BTState.Running:
                    return BTState.Running;
                default:
                    PickNewChild();
                    return result;
            }
        }

        void PickNewChild()
        {
            int choice = Random.Range(0, m_AddedWeight[m_AddedWeight.Length - 1]);

            for (int i = 0; i < m_AddedWeight.Length; ++i)
            {
                if (choice - m_AddedWeight[i] <= 0)
                {
                    activeChild = i;
                    break;
                }
            }
        }

        public override string ToString()
        {
            return "Random Sequence : " + activeChild + "/" + children.Count;
        }
    }
    /// <summary>
    /// 子ノードを並列実行します。<br/>
    /// 実行した子ノードのタスクが1つでもFailureを返したタイミングで他の子ノードのタスクを全て中断して、
    /// 自身もFailureを返します。
    /// </summary>
    public class Parallel : Composite
    {
        public Parallel(List<Node> children) :base(children) { }
        public override BTState Tick()
        {
            bool shouldWait = false;
            for (int i = 0; i < children.Count; ++i)
            {
                var childState = children[i].Tick();
                if (childState == BTState.Running)
                {
                    shouldWait = true;
                }
                else if (childState == BTState.Failure)
                {
                    return BTState.Failure;
                }
            }

            if(shouldWait)
                return BTState.Running;
            else
                return BTState.Success;
        }
    }
    /// <summary>
    /// 子ノードのタスクを並列実行します。<br/>
    /// 実行した子ノードのタスクが1つでもSuccessを返したタイミングで他の子ノードのタスクを全て中断して、
    /// 自身もSuccessを返します。
    /// </summary>
    public class ParallelSelector : Composite
    {
        public ParallelSelector(List<Node> children) : base(children) { }
        public override BTState Tick()
        {
            bool shouldWait = false;
            for (int i = 0; i < children.Count; ++i)
            {
                var childState = children[i].Tick();
                if (childState == BTState.Running)
                {
                    shouldWait = true;
                }
                else if (childState == BTState.Success)
                {
                    return BTState.Success;
                }
            }

            if (shouldWait)
                return BTState.Running;
            else
                return BTState.Failure;
        }
    }
}