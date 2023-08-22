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
        GameManager.instance._steps = 0;
        GameManager.instance._stageTime = 0;
        _stepStack.Clear();
        _timeStack.Clear();
    }
    public void PushUndo()
    {
        _stepStack.Push(GameManager.instance._steps);
        _timeStack.Push(GameManager.instance._stageTime);
    }

    public void PopUndo()
    {
        if (_stepStack.TryPop(out int step))
        {
            GameManager.instance._steps = step;
        }
        if (_timeStack.TryPop(out float time))
        {
            GameManager.instance._stageTime = time;
        }
    }
}
