using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Player : MonoBehaviour
{
    float _horizontal = 0;
    float _vertical = 0;
    bool _delayFlag = false;
    Animator _animator;
    GameObject[] _allObjects;
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _allObjects = GameObject.FindObjectsOfType<GameObject>();
    }
    void Update()
    {
        if (GManager.instance._gameState != GManager.GameState.Play) return;
        _horizontal = Input.GetAxisRaw("Horizontal");
        _vertical = Input.GetAxisRaw("Vertical");
        if (_horizontal != 0 && !_delayFlag)
        {
            Vector2 start = transform.position;
            Vector2 direction = new Vector2(_horizontal, 0);
            PushBlock(start, start + direction);
            StartCoroutine(DelayMove(0.2f));
        }
        if (_vertical != 0 && !_delayFlag)
        {
            Vector2 start = transform.position;
            Vector2 direction = new Vector2(0, _vertical);
            PushBlock(start, start + direction);
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
    bool PushBlock(Vector2 from, Vector2 to)
    {
        RaycastHit2D hit = PointCast(to);
        if (hit.collider && !hit.collider.CompareTag("Moveable")) return false;//移動先が壁なら処理を抜ける
        Vector2 direction = to - from;
        if (hit.collider && hit.collider.CompareTag("Moveable"))//判定が取れたオブジェクトがブロックなら再帰処理
        {
            bool success = PushBlock(to, to + direction);
            if (!success)
                return false;
        }
        GameObject targetObject = null;
        foreach (GameObject ob in _allObjects)//座標から移動させるオブジェクトを探し出す
        {
            if(ob != null && (Vector2)ob.transform.position == from && (ob.CompareTag("Player") || ob.CompareTag("Moveable")))
            {
                targetObject = ob;
                break;
            }
        }
        targetObject.transform.position = to;//移動
        if(targetObject.CompareTag("Player"))
        {
            GManager.instance.Steps += 1;
            AudioManager.instance.PlaySound(1);
            _animator.Play("PlayerRun");
        }
        if(targetObject.CompareTag("Moveable"))
        {
            AudioManager.instance.PlaySound(0);
        }
        return true;
    }
    RaycastHit2D PointCast(Vector2 pos)
    {
        return Physics2D.Linecast(pos, pos);
    }
}