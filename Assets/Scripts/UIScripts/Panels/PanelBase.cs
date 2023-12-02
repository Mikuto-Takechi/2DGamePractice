using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
[RequireComponent(typeof(CanvasGroup))]
public abstract class PanelBase : InputBase
{
    [SerializeField] bool _setActiveOnStart = false;
    public List<IDisposable> Subscriptions { get; private set; } = new();
    public CanvasGroup CanvasGroup { get; private set; }
    public RectTransform RectT { get; private set; }
    public Vector2 InitPos{ get; private set; }
    float _moveDistance;
    /// <summary>çwì«ÇÇ∑ÇÈÇΩÇﬂÇÃèàóùÇèëÇ≠èÍèäÅB</summary>
    protected abstract void Subscribe();
    protected void Start()
    {
        CanvasGroup = GetComponent<CanvasGroup>();
        RectT = GetComponent<RectTransform>();
        _moveDistance = Screen.width;
        InitPos = RectT.anchoredPosition;
        if(_setActiveOnStart) 
            SetActive();
        else 
            SetInactive();
    }
    public void SetInactive(bool visible = false)
    {
        if (Subscriptions.Count > 0)
        {
            Subscriptions.ForEach(s => s.Dispose());
            Subscriptions.Clear();
        }
        CanvasGroup.alpha = visible ? 1 : 0;
        CanvasGroup.blocksRaycasts = false;
        CanvasGroup.interactable = false;
    }
    public void SetActive()
    {
        Subscribe();
        CanvasGroup.alpha = 1;
        CanvasGroup.blocksRaycasts = true;
        CanvasGroup.interactable = true;
        Selectable selectable = GetComponentInChildren<Selectable>();
        if (selectable != null) EventSystem.current.SetSelectedGameObject(selectable.gameObject);
    }
    public void TweenSetActive()
    {
        transform.localScale = Vector3.zero;
        CanvasGroup.alpha = 1;
        transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).OnComplete(() => 
        {
            Subscribe();
            CanvasGroup.blocksRaycasts = true;
            CanvasGroup.interactable = true;
            Selectable selectable = GetComponentInChildren<Selectable>();
            if (selectable != null) EventSystem.current.SetSelectedGameObject(selectable.gameObject);
        }).SetLink(gameObject);
    }
    protected void ChangePanel(PanelBase panel)
    {
        SetInactive();
        panel.SetActive();
    }
    protected void TweenChangePanel(PanelBase panel, bool reverse = false, Action callback = null)
    {
        var moveToPosition = RectT.anchoredPosition;
        moveToPosition.x -= reverse ? -_moveDistance : _moveDistance;
        SetInactive(true);
        EventSystem.current.SetSelectedGameObject(null);
        RectT.DOAnchorPos(moveToPosition, 0.5f).SetEase(Ease.InOutExpo).OnComplete(() => 
        {
            CanvasGroup.alpha = 0;
            RectT.anchoredPosition = InitPos;
        }).SetLink(gameObject);

        var offsetPosition = panel.RectT.anchoredPosition;
        offsetPosition.x += reverse ? -_moveDistance : _moveDistance;
        panel.RectT.anchoredPosition = offsetPosition;
        panel.CanvasGroup.alpha = 1;
        panel.RectT.DOAnchorPos(panel.InitPos, 0.5f).SetEase(Ease.InOutExpo)
            .OnComplete(() => 
            {
                panel.SetActive();
                if(callback != null) callback();
            }).SetLink(panel.gameObject);
    }
    public void DeletePanel()
    {
        SetInactive();
        EventSystem.current.SetSelectedGameObject(null);
    }
    public void TweenDeletePanel(Action callback = null)
    {
        SetInactive(true);
        EventSystem.current.SetSelectedGameObject(null);
        transform.DOScale(Vector3.zero, 0.25f).OnComplete(() =>
        {
            CanvasGroup.alpha = 0;
            transform.localScale = Vector3.one;
            if (callback != null) callback();
        }).SetLink(gameObject);
    }
}
