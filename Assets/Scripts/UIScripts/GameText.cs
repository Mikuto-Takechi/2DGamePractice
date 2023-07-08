using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Text))]
public class GameText : MonoBehaviour
{
    Text _text;
    [SerializeField] TextType _type = TextType.None;
    void Start()
    {
        _text = GetComponent<Text>();
    }

    void Update()
    {
        if(_type == TextType.Steps) _text.text = $"ï‡êîÅF{GManager.instance.Steps}";
        if (_type == TextType.Time) _text.text = $"éûä‘ÅF{GManager.instance.StageTime.ToString("F2")}";
    }
}
enum TextType
{
    Steps,
    Time,
    None,
}