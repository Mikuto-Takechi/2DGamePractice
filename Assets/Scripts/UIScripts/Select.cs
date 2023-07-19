using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class Select : MonoBehaviour, ISelectHandler,IDeselectHandler, ISubmitHandler, IPointerClickHandler
{
    Transform _arrow;
    Image _image;
    Coroutine _coroutine;
    private void Awake()
    {
        _arrow = transform.Find("Arrow");
        _image = _arrow.GetComponent<Image>();
        if(_image) _image.enabled = false;
    }
    //選択状態のときに呼ばれる
    public void OnSelect(BaseEventData eventData)
    {
        AudioManager.instance.PlaySound(4);
        _coroutine = StartCoroutine(Selected());
    }
    //選択解除のときに呼ばれる
    public void OnDeselect(BaseEventData eventData)
    {
        StopCoroutine(_coroutine);
        if (_image) _image.enabled = false;
    }
    IEnumerator Selected()
    {
        if(_arrow == null) yield break;
        if (_image) _image.enabled = true;
        var wait = new WaitForEndOfFrame();
        float count = 0;
        while (true)
        {
            _arrow.localScale = new Vector3(_arrow.localScale.x, Mathf.Sin(count), _arrow.localScale.z);
            count += Time.deltaTime*4;
            yield return wait;
        }
    }
    //Navigationでボタンを押したときに呼ばれる
    public void OnSubmit(BaseEventData eventData)
    {
        AudioManager.instance.PlaySound(5);
    }
    //マウスでクリックしたときに呼ばれる
    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.instance.PlaySound(5);
    }
}
