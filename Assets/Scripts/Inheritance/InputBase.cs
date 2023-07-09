using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputBase : MonoBehaviour
{
    public GameInputs _gameInputs;
    private void Awake()
    {
        _gameInputs = new GameInputs();
        _gameInputs.Enable();
    }
    private void OnDestroy()
    {
        _gameInputs?.Dispose();
    }
}
