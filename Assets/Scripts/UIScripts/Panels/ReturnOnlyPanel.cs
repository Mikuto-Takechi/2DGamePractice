using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class ReturnOnlyPanel : PanelBase
{
    [SerializeField] Button _returnButton;
    [SerializeField] PanelBase _returnPanel;
    protected override void Subscribe()
    {
        Subscriptions.Add(Observable.EveryUpdate().Where(_ => _gameInputs.UI.Return.triggered)
            .Subscribe(_ => TweenChangePanel(_returnPanel, true)).AddTo(this));
        Subscriptions.Add(_returnButton.OnClickAsObservable()
            .Subscribe(_ => TweenChangePanel(_returnPanel, true)).AddTo(this));
    }
}
