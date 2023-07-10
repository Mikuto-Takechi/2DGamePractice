using UnityEngine;

public class GamePanel : MonoBehaviour
{
    [SerializeField] CanvasGroup[] _panels;
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
}
