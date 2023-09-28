using UnityEngine;

public abstract class Singleton<T> : InputBase where T : Component
{
    public static T Instance { get; set; }
    public abstract void AwakeFunction();
    protected new void Awake()
    {
        base.Awake();
        if (Instance == null)
        {
            Instance = this as T;
            DontDestroyOnLoad(gameObject);
            AwakeFunction();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
