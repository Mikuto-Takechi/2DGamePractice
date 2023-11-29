using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;


public class AudioManager : Singleton<AudioManager>
{
    public AudioSource _se;
    public AudioSource _loop;
    [SerializeField] List<AudioClip> _audioList;
    [SerializeField] List<AudioClip> _bgmList;
    public event Action DeleteSetting;
    public override void AwakeFunction()
    {
        float bgmVol = PlayerPrefs.GetFloat("BGM", 999);
        float seVol = PlayerPrefs.GetFloat("SE", 999);
        if (bgmVol != 999) _loop.volume = bgmVol;
        if (seVol != 999) _se.volume = seVol;
        SceneManager.sceneLoaded += SceneLoaded;
    }
    public void PlaySound(int num)
    {
        _se.PlayOneShot(_audioList[num]);
    }
    public void PlayBGM(int num)
    {
        _loop.clip = _bgmList[num];
        _loop.Play();
    }
    public void PauseBGM(bool flag)
    {
        if (flag)
        {
            _loop.Pause();
        }
        else
        {
            _loop.UnPause();
        }
    }
    public void StopBGM()
    {
        _loop.Stop();
    }
    void SceneLoaded(Scene nextScene, LoadSceneMode mode)
    {
        if (nextScene.name == "Title")
        {
            StopBGM();
            PlayBGM(3);
        }
        if (nextScene.name == "CSVTest")
        {
            StopBGM();
            PlayBGM(2);
        }
    }
    private void OnApplicationQuit()
    {
        Save();
    }
    public void Save()
    {
        //Debug.Log("オーディオデータセーブ");
        PlayerPrefs.SetFloat("BGM", _loop.volume);
        PlayerPrefs.SetFloat("SE", _se.volume);
    }
    public void DeleteSave()
    {
        DeleteSetting();
        PlayerPrefs.DeleteKey("BGM");
        PlayerPrefs.DeleteKey("SE");
        _loop.volume = 1;
        _se.volume = 1;
    }
}
