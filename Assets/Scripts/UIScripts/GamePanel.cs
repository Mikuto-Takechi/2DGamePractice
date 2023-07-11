using UnityEngine;
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
        foreach (var panel in _panels)
        {
            panel.alpha = 0;
            panel.interactable = false;
            panel.blocksRaycasts = false;
            if(panel == _panels[index])
            {
                panel.alpha = 1;
                panel.interactable = true;
                panel.blocksRaycasts = true;
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
            _panels[2].transform.GetComponentInChildren<Button>().Select();
            AudioManager.instance.PauseBGM(true);
            AudioManager.instance.PlaySound(2);
        }
    }
    public void Clear()
    {
        _panels[1].transform.GetComponentInChildren<Button>().Select();
    }
    public void ChangeScene(int index)
    {
        if(index < 0) index = 0;
        if(index > SceneManager.sceneCountInBuildSettings - 1) index = SceneManager.sceneCountInBuildSettings - 1;
        SceneManager.LoadScene(index);
    }
}
