using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioSource _se;
    public AudioSource _loop;
    [SerializeField] List<AudioClip> _audioList;
    [SerializeField] List<AudioClip> _bgmList;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += SceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
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
        if(nextScene.name == "Title")
        {
            StopBGM();
            PlayBGM(0);
        }
        if(nextScene.name == "Stage1")
        {
            StopBGM();
            PlayBGM(1);
        }
    }
}
