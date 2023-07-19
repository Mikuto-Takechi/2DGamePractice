using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamePanel : InputBase
{
    [SerializeField] CanvasGroup[] _panels;

    private void Update()
    {
        if (_gameInputs.Player.Pause.triggered)
        {
            SwitchPause();
        }
    }
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
        if (GManager.instance._gameState == GManager.GameState.Pause)
        {
            GManager.instance._gameState = GManager.GameState.Idle;
            ChangePanel(0);//メインUI
            AudioManager.instance.PauseBGM(false);
        }
        else
        {
            GManager.instance._gameState = GManager.GameState.Pause;
            ChangePanel(2);//ポーズUI
            Ease[] ease = _panels[2].transform.GetComponentsInChildren<Ease>();
            foreach (var e in ease)
            {
                if (e != null) e._timer = 0;
            }
            //_panels[2].transform.GetComponentInChildren<Button>().Select();
            AudioManager.instance.PauseBGM(true);
            AudioManager.instance.PlaySound(2);
        }
    }
    public void Clear()
    {
        Ease[] ease = _panels[1].transform.GetComponentsInChildren<Ease>();
        foreach (var e in ease)
        {
            if(e != null) e._timer = 0;
        }
        _panels[1].transform.GetComponentInChildren<Button>().Select();
    }
    public void ChangeScene(int index)
    {
        if (index < 0) index = 0;
        if (index > SceneManager.sceneCountInBuildSettings - 1) index = SceneManager.sceneCountInBuildSettings - 1;
        SceneManager.LoadScene(index);
    }
    public void QuitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//Unityエディター上ならこっち
    #else
        Application.Quit();//アプリ上ならこっち
    #endif
    }
}
