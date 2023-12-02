using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
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
                Selectable selectable = _panels[index].GetComponentInChildren<Selectable>();
                if (selectable != null) EventSystem.current.SetSelectedGameObject(selectable.gameObject);
            }
        }
    }
    public void TweenPanel(int index)
    {
        _panels[index].transform.localScale = Vector3.zero;
        _panels[index].transform.DOScale(1, 0.5f).SetEase(Ease.OutBounce).SetLink(gameObject);
    }
    public void SwitchPause()
    {
        if (GameManager.Instance._gameState == GameState.Pause)
        {
            GameManager.Instance._gameState = GameState.Idle;
            ChangePanel(0);//���C��UI
            AudioManager.Instance.PauseBGM(false);
        }
        else
        {
            GameManager.Instance._gameState = GameState.Pause;
            ChangePanel(2);//�|�[�YUI
            TweenPanel(2);
            //EaseText[] ease = _panels[2].transform.GetComponentsInChildren<EaseText>();
            //foreach (var e in ease)
            //{
            //    if (e != null) e.EaseStart();
            //}
            AudioManager.Instance.PauseBGM(true);
            AudioManager.Instance.PlaySound(2);
        }
    }
    public void Clear()
    {
        //EaseText[] ease = _panels[1].transform.GetComponentsInChildren<EaseText>();
        //foreach (var e in ease)
        //{
        //    if (e != null) e.EaseStart();
        //}
        TweenPanel(1);
        _panels[1].transform.GetComponentInChildren<Button>().Select();
    }
    public void LoadScene(string sceneName)
    {
        SceneChanger.Instance.LoadScene(sceneName);
    }
    /// <summary>
    /// �Q�[���J�n
    /// </summary>
    public void StartGame(string stageName)
    {
        SceneChanger.Instance.StartGame(stageName);
    }
    /// <summary>
    /// ���̃X�e�[�W�ֈړ�������
    /// </summary>
    public void NextGame()
    {
        SceneChanger.Instance.NextGame();
    }
    public void QuitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//Unity�G�f�B�^�[��Ȃ炱����
    #else
        Application.Quit();//�A�v����Ȃ炱����
    #endif
    }
    public void ResetGame()
    {
        GameManager.Instance.ResetGame();
        ChangePanel(0);
    }
}
