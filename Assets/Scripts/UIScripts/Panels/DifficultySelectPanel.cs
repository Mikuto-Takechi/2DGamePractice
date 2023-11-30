using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class DifficultySelectPanel : ReturnOnlyPanel
{
    [SerializeField] Button _easyStageSelectButton;
    [SerializeField] PanelBase _easyStageSelectPanel;
    [SerializeField] Button _hardStageSelectButton;
    [SerializeField] PanelBase _hardStageSelectPanel;
    protected override void Subscribe()
    {
        base.Subscribe();
        Subscriptions.Add(_easyStageSelectButton.OnClickAsObservable()
            .Subscribe(_ => TweenChangePanel(_easyStageSelectPanel)).AddTo(this));
        Subscriptions.Add(_hardStageSelectButton.OnClickAsObservable()
            .Subscribe(_ => TweenChangePanel(_hardStageSelectPanel)).AddTo(this));
    }
}
