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
        if (GameManager.instance._gameState == GameState.Pause)
        {
            GameManager.instance._gameState = GameState.Idle;
            ChangePanel(0);//メインUI
            AudioManager.instance.PauseBGM(false);
        }
        else
        {
            GameManager.instance._gameState = GameState.Pause;
            ChangePanel(2);//ポーズUI
            EaseText[] ease = _panels[2].transform.GetComponentsInChildren<EaseText>();
            foreach (var e in ease)
            {
                if (e != null) e.EaseStart();
            }
            AudioManager.instance.PauseBGM(true);
            AudioManager.instance.PlaySound(2);
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
    public void ChangeScene(int index)
    {
        if (index < 0) index = 0;
        if (index > SceneManager.sceneCountInBuildSettings - 1) index = SceneManager.sceneCountInBuildSettings - 1;
        SceneManager.LoadScene(index);
    }
    /// <summary>
    /// ゲーム開始
    /// </summary>
    public void StartGame(string stageName)
    {
        MapEditor mapEditor = FindObjectOfType<MapEditor>();
        if (_fadeTween != null) return;
        _fadeTween = _fadePanel.DOFade(1, 1).OnComplete(() => 
        {
            if (mapEditor.BuildMapData(stageName))
                SceneManager.LoadScene("CSVTest");
        });
    }
    /// <summary>
    /// 次のステージへ移動させる
    /// </summary>
    public void NextGame()
    {
        MapEditor mapEditor = FindObjectOfType<MapEditor>();
        if(mapEditor._stageData.next != null)
        {
            if (mapEditor.BuildMapData(mapEditor._stageData.next))
                SceneManager.LoadScene("CSVTest");
        }
        else
        {
            Debug.Log("次のマップの読み込みに失敗しました");
        }
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
        GameManager.instance.ResetGame();
        ChangePanel(0);
        AudioManager.instance.PlayBGM(2);
        GameManager.instance._gameState = GameState.Idle;
    }
}
