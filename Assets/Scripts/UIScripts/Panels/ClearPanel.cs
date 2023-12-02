using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ClearPanel : PanelBase
{
    [SerializeField] Button _titleButton;
    [SerializeField] Button _nextStageButton;
    [SerializeField] Text _clearSteps;
    [SerializeField] Text _clearTime;
    [SerializeField] Text _stepsNewRecord;
    [SerializeField] Text _timeNewRecord;
    new void OnEnable()
    {
        base.OnEnable();
        GameManager.Instance.GameClear += SetRecords;
        GameManager.Instance.NewRecord += NewRecord;
    }
    void OnDisable()
    {
        GameManager.Instance.GameClear -= SetRecords;
        GameManager.Instance.NewRecord -= NewRecord;
    }
    void NewRecord(TextType type)
    {
        Text newRecord = type switch
        {
            TextType.ClearSteps => _stepsNewRecord,
            TextType.ClearTime => _timeNewRecord,
            _ => throw new InvalidOperationException()
        };
        if (newRecord == null) return;
        newRecord.text = "�L�^�X�V�I�I";
        newRecord.rectTransform.DOLocalMoveY(20f, 0.4f)
                               .SetRelative(true).SetEase(Ease.OutQuad)
                               .SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
        newRecord.DOColor(Color.white, 1f).SetEase(Ease.Flash, 8).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
    }
    void SetRecords()
    {
        var gm = GameManager.Instance;
        _clearSteps.text = "�N���A�����F" + gm._steps.ToString();
        _clearTime.text = "�N���A���ԁF" + (gm.MapEditor._stageData.timeLimit - gm._stageTime).ToString("0.00");
        TweenSetActive();
    }
    protected override void Subscribe()
    {
        Subscriptions.Add(_titleButton.OnClickAsObservable()
            .Subscribe(_ => SceneChanger.Instance.LoadScene("Title")).AddTo(this));
        Subscriptions.Add(_nextStageButton.OnClickAsObservable()
            .Subscribe(_ => SceneChanger.Instance.NextGame()).AddTo(this));
    }
}
