using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public string type;
    Rigidbody2D rigid;

    void Awake()
    {   
        // 아이템 속도 Enemy로 이동 -> 적 만들 때 넣어주는 걸로다가~! -> OnEnable사용으로 다시 이쪽으로 이동
        // rigid = GetComponent<Rigidbody2D>();
        // rigid.velocity = Vector2.down * 0.1f;

        rigid = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        rigid.velocity = Vector2.down * 1.5f;
    }
}
