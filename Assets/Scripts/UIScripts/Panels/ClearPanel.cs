using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ClearPanel : PanelBase
{
    [SerializeField] Button _titleButton;
    [SerializeField] Button _nextStageButton;
    new void OnEnable()
    {
        base.OnEnable();
        GameManager.Instance.GameClear += TweenSetActive;
    }
    void OnDisable()
    {
        GameManager.Instance.GameClear -= TweenSetActive;
    }
    protected override void Subscribe()
    {
        Subscriptions.Add(_titleButton.OnClickAsObservable()
            .Subscribe(_ => SceneChanger.Instance.LoadScene("Title")).AddTo(this));
        Subscriptions.Add(_nextStageButton.OnClickAsObservable()
            .Subscribe(_ => SceneChanger.Instance.NextGame()).AddTo(this));
    }
}
