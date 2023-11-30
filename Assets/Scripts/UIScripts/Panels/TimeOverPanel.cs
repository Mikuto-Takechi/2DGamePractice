using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class TimeOverPanel : PanelBase
{
    [SerializeField] Button _titleButton;
    [SerializeField] Button _retryButton;
    new void OnEnable()
    {
        base.OnEnable();
        GameManager.Instance.TimeOver += TweenSetActive;
    }
    void OnDisable()
    {
        GameManager.Instance.TimeOver -= TweenSetActive;
    }
    protected override void Subscribe()
    {
        Subscriptions.Add(_titleButton.OnClickAsObservable()
            .Subscribe(_ => SceneChanger.Instance.LoadScene("Title")).AddTo(this));
        Subscriptions.Add(_retryButton.OnClickAsObservable()
            .Subscribe(_ => 
            {
                GameManager.Instance.ResetGame();
                TweenDeletePanel();
            }).AddTo(this));
    }
}
