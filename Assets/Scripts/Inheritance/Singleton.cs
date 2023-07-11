using UnityEngine;

public abstract class Singleton<T> : InputBase where T : Component
{
    public static T instance;
    public abstract void AwakeFunction();
    protected new void Awake()
    {
        base.Awake();
        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
            AwakeFunction();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
