using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputBase : MonoBehaviour
{
    public GameInputs _gameInputs;
    protected void Awake()
    {
        _gameInputs = new GameInputs();
        _gameInputs.Enable();
    }
    protected void OnDestroy()
    {
        _gameInputs?.Dispose();
    }
}