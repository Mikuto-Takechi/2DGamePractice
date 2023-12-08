using UnityEngine;

namespace Takechi.BT
{
    /// <summary>
    /// �f�R���[�^�̊�{�N���X�A�����̎q�m�[�h��1�������ĂȂ�
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
    /// �q�m�[�h���w�肵���񐔌J��Ԃ��B<br/>
    /// �q�m�[�h�����Success�Ȃ�J��Ԃ������Success��Ԃ��B<br/>
    /// �q�m�[�h��Failure��Ԃ����ꍇ�̓��[�v�𒆒f���AFailure��Ԃ��B<br/>
    /// �q�m�[�h��Running�Ȃ炱�̃m�[�h��Running��Ԃ��A�J�E���g�A�b�v�����Ɏq�m�[�h�̏�����҂��܂��B
    /// </summary>
    public class Repeat : Decorator
    {
        public int limit = 1;
        int count = 0;
        /// <summary>�m�[�h�ƌJ��Ԃ��񐔂�n��</summary>
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
    /// �q�m�[�h��Failure��Ԃ������ɂ�Success��Ԃ��ASuccess��Ԃ����Ƃ��ɂ�Failure��Ԃ��B<br/>
    /// �q�m�[�h��Running��Ԃ������ɂ́A���̃m�[�h��Running��Ԃ��B
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
    /// �q�m�[�h��Running��Ԃ������ɂ́A���̃m�[�h��Running��Ԃ��B<br/>
    /// �����łȂ��ꍇ�́A���Success��Ԃ��B
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
    /// �q�m�[�h��Running��Ԃ������ɂ́A���̃m�[�h��Running��Ԃ��B<br/>
    /// �����łȂ��ꍇ�́A���Failure��Ԃ��B
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
    /// �w�肵���b��Running��Ԃ��ێ����Ă���q�m�[�h�̕]�����n�߂�f�R���[�^�B
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