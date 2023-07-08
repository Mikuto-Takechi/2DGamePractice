using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    GameObject[] blockPoints;
    GameObject[] blocks;
    void Start()
    {
        blockPoints = GameObject.FindGameObjectsWithTag("BlockPoint");
        blocks = GameObject.FindGameObjectsWithTag("Moveable");
    }

    void Update()
    {
        int pointCount = blockPoints.Length;
        int blockOnPoints = 0;
        foreach (var point in blockPoints)
        {
            foreach (var block in blocks)
            {
                if (point.transform.position == block.transform.position)
                {
                    ++blockOnPoints;
                }
            }
        }
        if(blockOnPoints == pointCount && GManager.instance._gameState != GManager.GameState.Clear)
        {
            GManager.instance._gameState = GManager.GameState.Clear;
            Debug.Log("Clear!!!");
        }
    }
}
