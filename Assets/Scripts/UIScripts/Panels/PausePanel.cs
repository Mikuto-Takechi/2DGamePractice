using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class PausePanel : PanelBase
{
    [SerializeField] Button _returnButton;
    [SerializeField] Button _settingsButton;
    [SerializeField] PanelBase _settingsPanel;
    [SerializeField] Button _titleButton;
    new void OnEnable()
    {
        base.OnEnable();
        GameManager.Instance.PauseAction += TweenSetActive;
    }
    void OnDisable()
    {
        GameManager.Instance.PauseAction -= TweenSetActive;
    }
    protected override void Subscribe()
    {
        Subscriptions.Add(Observable.EveryUpdate()
            .Where(_ => _gameInputs.Player.Pause.triggered || _gameInputs.UI.Return.triggered)
            .Subscribe(_ => TweenDeletePanel(GameManager.Instance.UnPause)).AddTo(this));
        Subscriptions.Add(_returnButton.OnClickAsObservable()
            .Subscribe(_ => TweenDeletePanel(GameManager.Instance.UnPause)).AddTo(this));
        Subscriptions.Add(_settingsButton.OnClickAsObservable()
            .Subscribe(_ => TweenChangePanel(_settingsPanel, false, PauseLink)).AddTo(this));
        Subscriptions.Add(_titleButton.OnClickAsObservable()
            .Subscribe(_ => SceneChanger.Instance.LoadScene("Title")).AddTo(this));
    }
    void PauseLink()
    {
        _settingsPanel.Subscriptions.Add(Observable.EveryUpdate()
         .Where(_ => _gameInputs.Player.Pause.triggered)
         .Subscribe(_ => _settingsPanel.TweenDeletePanel(GameManager.Instance.UnPause)).AddTo(this));
    }
}
