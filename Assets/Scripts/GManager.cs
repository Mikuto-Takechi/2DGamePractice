using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GManager : MonoBehaviour
{
    public static GManager instance;
    int _steps = 0;
    float _stageTime = 0;
    public GameState _gameState = GameState.Title;
    public Dictionary<string, float> _timeRecords = new Dictionary<string, float>();
    public Dictionary<string, int> _stepsRecords = new Dictionary<string, int>();
    public enum GameState
    {
        Title,
        Play,
        Move,
        Pause,
        Clear,
    }
    public int Steps
    {
        get { return _steps; }
        set { _steps = value; }
    }
    public float StageTime
    {
        get { return _stageTime; }
        set { _stageTime = value; } 
    }
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
    private void Update()
    {
        if(_gameState == GameState.Play || _gameState == GameState.Move) _stageTime += Time.deltaTime;
    }
    void SceneLoaded(Scene nextScene, LoadSceneMode mode)
    {
        if (!_timeRecords.ContainsKey(nextScene.name) && nextScene.name.Contains("Stage")) _timeRecords.Add(nextScene.name, 99999.99f);
        if (!_stepsRecords.ContainsKey(nextScene.name) && nextScene.name.Contains("Stage")) _stepsRecords.Add(nextScene.name, 99999);
        _stageTime = 0;
        if (nextScene.name.Contains("Stage")) _gameState = GameState.Play;
    }
}