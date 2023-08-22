using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class InputBase : MonoBehaviour//, GameInputs.IPlayerActions
{
    public GameInputs _gameInputs;
    //public GameInputs.PlayerActions _playerActions = default;
    //protected virtual void InputDown(InputAction.CallbackContext context) { }
    //public void OnDown(InputAction.CallbackContext context) { InputDown(context); }
    //public void OnPause(InputAction.CallbackContext context) { }
    //public void OnReset(InputAction.CallbackContext context) { }
    //public void OnRight(InputAction.CallbackContext context) { }
    //public void OnLeft(InputAction.CallbackContext context) { }
    //public void OnUndo(InputAction.CallbackContext context) { }
    //public void OnUp(InputAction.CallbackContext context) { }
    public void OnEnable()
    {
        _gameInputs.Enable();
        //_playerActions.Enable();
    }
    public void Awake()
    {
        _gameInputs = new GameInputs();
        //_playerActions = new GameInputs.PlayerActions(new GameInputs());
        //_playerActions.SetCallbacks(this);
    }
    public void OnDestroy()
    {
        _gameInputs?.Dispose();
        //_playerActions.Disable();
    }
}