using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Player : MonoBehaviour
{
    float _horizontal = 0;
    float _vertical = 0;
    bool _delayFlag = false;
    void Update()
    {
        _horizontal = Input.GetAxisRaw("Horizontal");
        _vertical = Input.GetAxisRaw("Vertical");
        if (_horizontal != 0 && !_delayFlag)
        {
            Vector2 start = transform.position;
            Vector2 direction = new Vector2(_horizontal, 0);
            RaycastHit2D hit = PointCast(start + direction);
            if (hit.collider == null)
            {
                transform.position += (Vector3)direction;
            }
            else
            {
                PushBlock(hit.collider, direction);
            }
            StartCoroutine(DelayMove(0.2f));
        }
        if (_vertical != 0 && !_delayFlag)
        {
            Vector2 start = transform.position;
            Vector2 direction = new Vector2(0, _vertical);
            RaycastHit2D hit = PointCast(start + direction);
            if (hit.collider == null) transform.position += (Vector3)direction;
            else PushBlock(hit.collider, direction);
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
    void PushBlock(Collider2D col, Vector2 direction)
    {
        if (col.CompareTag("Moveable"))
        {
            Vector2 start = col.transform.position;
            RaycastHit2D hit = PointCast(start + direction);
            if (hit.collider == null) col.transform.position += (Vector3)direction;
        }
    }
    RaycastHit2D PointCast(Vector2 pos)
    {
        return Physics2D.Linecast(pos, pos);
    }
}