using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int dmg;
    public bool isRotate;

    void Update()   
    {
        if(isRotate)
            transform.Rotate(Vector3.forward*10);
    }

    void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.gameObject.tag =="BorderBullet")
        {
            //Deroy(gameObject);
            gameObject.SetActive(false);
        }
    }
}
