using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ŠÔ‚Æ•à”‚ğStack‚Å•Û‘¶‚µ‚Ä‚¨‚­
/// </summary>
public class TimeAndStepStack : MonoBehaviour, IReload, IPushUndo, IPopUndo
{
    Stack<int> _stepStack = new Stack<int>();
    Stack<float> _timeStack = new Stack<float>();
    public void Reload()
    {
        GManager.instance._steps = 0;
        GManager.instance._stageTime = 0;
        _stepStack.Clear();
        _timeStack.Clear();
    }
    public void PushUndo()
    {
        _stepStack.Push(GManager.instance._steps);
        _timeStack.Push(GManager.instance._stageTime);
    }

    public void PopUndo()
    {
        if (_stepStack.TryPop(out int step))
        {
            GManager.instance._steps = step;
        }
        if (_timeStack.TryPop(out float time))
        {
            GManager.instance._stageTime = time;
        }
    }
}
