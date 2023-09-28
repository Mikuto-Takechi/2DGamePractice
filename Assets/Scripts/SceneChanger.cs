using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : Singleton<SceneChanger>
{
    [SerializeField] Image _fadePanel;
    [SerializeField] Sprite _loadSceneSprite;
    [SerializeField] Sprite _resetGameSprite;
    Tween _tween;
    public override void AwakeFunction()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    /// <summary>
    /// ゲーム開始
    /// </summary>
    public void StartGame(string stageName)
    {
        if (_tween != null) return;
        _fadePanel.sprite = _loadSceneSprite;
        _tween = DOVirtual.Float(0, 1, 1.5f, value => _fadePanel.material.SetFloat("_Alpha", value));
        _tween.OnComplete(() => 
        {
            if (GameManager.Instance.MapEditor.BuildMapData(stageName))
                SceneManager.LoadSceneAsync("CSVTest");
            DOVirtual.Float(1, 0, 1.5f, value => _fadePanel.material.SetFloat("_Alpha", value))
                     .OnComplete(() => _tween = null);
        });
    }
    /// <summary>
    /// 次のステージへ移動させる
    /// </summary>
    public void NextGame()
    {
        if (GameManager.Instance.MapEditor._stageData.next != null)
        {
            if (GameManager.Instance.MapEditor.BuildMapData(GameManager.Instance.MapEditor._stageData.next))
                SceneManager.LoadSceneAsync("CSVTest");
        }
        else
        {
            Debug.Log("次のマップの読み込みに失敗しました");
        }
    }
    public void ResetGame()
    {
        _fadePanel.sprite = _resetGameSprite;
        AudioManager.Instance.PlaySound(7);
        DOVirtual.Float(0, 1, 1, value => _fadePanel.material.SetFloat("_Alpha", value)).OnComplete(() => 
        {
            GameManager.Instance.ResetGame();
            AudioManager.Instance.PlaySound(8);
            AudioManager.Instance.PlaySound(9);
            DOVirtual.Float(1, 0, 1, value => _fadePanel.material.SetFloat("_Alpha", value))
                     .OnComplete(() => GameManager.Instance._gameState = GameState.Idle);
        });
    }
}
