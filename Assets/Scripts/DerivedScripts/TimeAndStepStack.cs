using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// éûä‘Ç∆ï‡êîÇStackÇ≈ï€ë∂ÇµÇƒÇ®Ç≠
/// </summary>
public class TimeAndStepStack : MonoBehaviour/*, IReload, IPushUndo, IPopUndo*/
{
    Stack<int> _stepStack = new Stack<int>();
    //Stack<float> _timeStack = new Stack<float>();
    void OnEnable()
    {
        GameManager.Instance.PushData += PushUndo;
        GameManager.Instance.PopData += PopUndo;
        GameManager.Instance.ReloadData += Reload;
    }
    void OnDisable()
    {
        GameManager.Instance.PushData -= PushUndo;
        GameManager.Instance.PopData -= PopUndo;
        GameManager.Instance.ReloadData -= Reload;
    }
    public void Reload()
    {
        GameManager.Instance._steps = 0;
        //GameManager.instance._stageTime = 0;
        _stepStack.Clear();
        //_timeStack.Clear();
    }
    public void PushUndo()
    {
        _stepStack.Push(GameManager.Instance._steps);
        //_timeStack.Push(GameManager.instance._stageTime);
    }

    public void PopUndo()
    {
        if (_stepStack.TryPop(out int step))
        {
            GameManager.Instance._steps = step;
        }
        //if (_timeStack.TryPop(out float time))
        //{
        //    GameManager.instance._stageTime = time;
        //}
    }
}
