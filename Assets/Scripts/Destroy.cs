using UnityEngine;

public class Destroy : MonoBehaviour
{
    [SerializeField] float _destroyTime = 0.5f;
    void Start()
    {
        Destroy(gameObject, _destroyTime);
    }
}
