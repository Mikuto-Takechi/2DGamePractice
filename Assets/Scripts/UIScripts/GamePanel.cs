using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamePanel : MonoBehaviour
{
    [SerializeField] CanvasGroup[] _panels;
    [SerializeField] Image _fadePanel;
    Tween _fadeTween;
    public void ChangePanel(int index)
    {
        EventSystem.current.SetSelectedGameObject(null);
        foreach (var panel in _panels)
        {
            panel.alpha = 0;
            panel.interactable = false;
            panel.blocksRaycasts = false;
            if (panel == _panels[index])
            {
                //panel.transform.localScale = Vector3.zero;
                //panel.transform.DOScale(1f, 1f).SetEase(Ease.OutBounce).SetLink(gameObject);
                panel.alpha = 1;
                panel.interactable = true;
                panel.blocksRaycasts = true;
                Button button = _panels[index].GetComponentInChildren<Button>();
                if (button != null) EventSystem.current.SetSelectedGameObject(button.gameObject);
            }
        }
    }
    public void SwitchPause()
    {
        if (GameManager.Instance._gameState == GameState.Pause)
        {
            GameManager.Instance._gameState = GameState.Idle;
            ChangePanel(0);//メインUI
            AudioManager.Instance.PauseBGM(false);
        }
        else
        {
            GameManager.Instance._gameState = GameState.Pause;
            ChangePanel(2);//ポーズUI
            EaseText[] ease = _panels[2].transform.GetComponentsInChildren<EaseText>();
            foreach (var e in ease)
            {
                if (e != null) e.EaseStart();
            }
            AudioManager.Instance.PauseBGM(true);
            AudioManager.Instance.PlaySound(2);
        }
    }
    public void Clear()
    {
        EaseText[] ease = _panels[1].transform.GetComponentsInChildren<EaseText>();
        foreach (var e in ease)
        {
            if (e != null) e.EaseStart();
        }
        _panels[1].transform.GetComponentInChildren<Button>().Select();
    }
    public void LoadScene(string sceneName)
    {
        SceneChanger.Instance.LoadScene(sceneName);
    }
    /// <summary>
    /// ゲーム開始
    /// </summary>
    public void StartGame(string stageName)
    {
        SceneChanger.Instance.StartGame(stageName);
    }
    /// <summary>
    /// 次のステージへ移動させる
    /// </summary>
    public void NextGame()
    {
        SceneChanger.Instance.NextGame();
    }
    public void QuitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//Unityエディター上ならこっち
    #else
        Application.Quit();//アプリ上ならこっち
    #endif
    }
    public void ResetGame()
    {
        GameManager.Instance.ResetGame();
        ChangePanel(0);
        AudioManager.Instance.PlayBGM(2);
        GameManager.Instance._gameState = GameState.Idle;
    }
}
