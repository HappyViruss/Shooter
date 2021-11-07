using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    public float speed;
    public int startIndex;
    public int endIndex;
    public Transform[] sprites;

    float viewHeight;

    void Awake()
    {
        viewHeight = Camera.main.orthographicSize * 2;
    }

    void Update()
    {
        Vector3 curPos = transform.position;
        Vector3 nextPos = Vector3.down * speed * Time.deltaTime;
        transform.position = curPos + nextPos;

        if(sprites[endIndex].position.y < viewHeight *(-1))
        {
            //front <-> back || start <-> finish(사람) || begin <-> end(사물)
            // startIndex : 2 || endIndex : 0
            Vector3 backSpritesPos = sprites[startIndex].localPosition;  
            Vector3 frontSpritesPos = sprites[endIndex].localPosition;
            sprites[endIndex].transform.localPosition = backSpritesPos + Vector3.up*10;


            int startIndexSave = startIndex;
            startIndex = endIndex;
            endIndex = startIndexSave - 1 == -1? sprites.Length-1 : startIndexSave -1;;
        }
    }
}
