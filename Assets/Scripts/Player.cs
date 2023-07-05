using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Player : MonoBehaviour
{
    float _horizontal = 0;
    float _vertical = 0;
    bool _delayFlag = false;
    void Start()
    {
    }

    void Update()
    {
        _horizontal = Input.GetAxisRaw("Horizontal");
        _vertical = Input.GetAxisRaw("Vertical");
        Vector2 start = transform.position;
        Debug.DrawLine(start, start + Vector2.right);
        Debug.DrawLine(start, start + Vector2.left);
        Debug.DrawLine(start, start + Vector2.up);
        Debug.DrawLine(start, start + Vector2.down);
        
        //RaycastHit2D hit = Physics2D.CircleCast(start, 1, Vector2.zero);
        //if (hit.collider) Debug.Log("a");
        if (_horizontal != 0 && !_delayFlag)
        {
            RaycastHit2D hit = new RaycastHit2D();
            if (_horizontal > 0) hit = Physics2D.Linecast(start, start + Vector2.right);
            if (_horizontal < 0) hit = Physics2D.Linecast(start, start + Vector2.left);
            if(hit.collider == null) transform.position += new Vector3(_horizontal, 0, 0);
            StartCoroutine(DelayMove(0.2f));
        }
        if (_vertical != 0 && !_delayFlag)
        {
            RaycastHit2D hit = new RaycastHit2D();
            if (_vertical > 0) hit = Physics2D.Linecast(start, start + Vector2.up);
            if (_vertical < 0) hit = Physics2D.Linecast(start, start + Vector2.down);
            if (hit.collider == null) transform.position += new Vector3(0,_vertical,0);
            StartCoroutine(DelayMove(0.2f));
        }
    }
    IEnumerator DelayMove(float time)
    {
        _delayFlag = true;
        yield return new WaitForSeconds(time);
        _delayFlag = false;
        yield break;
    }
}
