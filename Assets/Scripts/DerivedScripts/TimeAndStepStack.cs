using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// éûä‘Ç∆ï‡êîÇStackÇ≈ï€ë∂ÇµÇƒÇ®Ç≠
/// </summary>
public class TimeAndStepStack : MonoBehaviour/*, IReload, IPushUndo, IPopUndo*/
{
    Stack<int> _stepStack = new Stack<int>();
    Stack<float> _timeStack = new Stack<float>();
    void OnEnable()
    {
        GameManager.instance.PushData += PushUndo;
        GameManager.instance.PopData += PopUndo;
        GameManager.instance.ReloadData += Reload;
    }
    void OnDisable()
    {
        GameManager.instance.PushData -= PushUndo;
        GameManager.instance.PopData -= PopUndo;
        GameManager.instance.ReloadData -= Reload;
    }
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
