using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class TitlePanel : PanelBase
{
    [SerializeField] Button _difficultySelectButton;
    [SerializeField] PanelBase _difficultySelectPanel;
    [SerializeField] Button _settingsButton;
    [SerializeField] PanelBase _settingsPanel;
    protected override void Subscribe()
    {
        Subscriptions.Add(_difficultySelectButton.OnClickAsObservable()
            .Subscribe(_ => TweenChangePanel(_difficultySelectPanel)).AddTo(this));
        Subscriptions.Add(_settingsButton.OnClickAsObservable()
            .Subscribe(_ => TweenChangePanel(_settingsPanel)).AddTo(this));
    }
}
