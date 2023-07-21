using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class GameSlider : MonoBehaviour
{
    [SerializeField] SliderType _sliderType = SliderType.None;
    Slider _slider;
    Text _sliderText;
    void Start()
    {
        _slider = GetComponent<Slider>();
        _sliderText = transform.GetComponentInChildren<Text>();
        if (_slider == null) return;
        _slider.onValueChanged.AddListener(SliderSound);
        if (_sliderType == SliderType.BGM)
        {
            _slider.value = AudioManager.instance._loop.volume;//�X���C�_�[��AudioSource�̒l����
            if (_sliderText) _sliderText.text = $"BGM: {_slider.value.ToString("F1")}";
        }
        if (_sliderType == SliderType.SE)
        {
            _slider.value = AudioManager.instance._se.volume;//�X���C�_�[��AudioSource�̒l����
            if (_sliderText) _sliderText.text = $"SE: {_slider.value.ToString("F1")}";
        }
    }

    void Update()
    {
        if (_slider == null) return;
        if (_sliderType == SliderType.BGM)
        {
            AudioManager.instance._loop.volume = _slider.value;//AudioSource�ɃX���C�_�[�̒l����
            if (_sliderText) _sliderText.text = $"BGM: {_slider.value.ToString("F1")}";
        }
        if (_sliderType == SliderType.SE)
        {
            AudioManager.instance._se.volume = _slider.value;//AudioSource�ɃX���C�_�[�̒l����
            if (_sliderText) _sliderText.text = $"SE: {_slider.value.ToString("F1")}";
        }
    }
    void SliderSound(float num)
    {
        AudioManager.instance.PlaySound(4);
    }
}
enum SliderType
{
    BGM,
    SE,
    None,
}