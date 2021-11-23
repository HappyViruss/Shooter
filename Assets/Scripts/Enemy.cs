using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public string enemyName;
    public int enemyScore;
    public float speed;   
    public int health;
    public Sprite[] sprites;    //SpriteRender를 사용해주기 위해 배열로 sprite를 담아줌
    
    public float maxShotDelay;  //실제 딜레이
    public float curShotDelay;  //한발 쏘고 충전되기 위한 딜레이

    public GameObject bulletObjA;
    public GameObject bulletObjB;
    public GameObject itemCoin;
    public GameObject itemPower;
    public GameObject itemBoom;
    public GameObject player;              //Enemy는 Prefab이기 때문에 바로 Player(gameObejct)를 끌고 올수 없다->gameManager에서 넘겨받는 작업 필요
    public GameManager gameManager;
    public ObjectManager objectManager;    //Enemy는 Prefab이기 때문에 바로 objectManager(script)를 끌고 올수 없다->gameManager에서 넘겨받는 작업 필요

    SpriteRenderer spriteRenderer;  //스프라이트를 바꿔주기 위해 사용
    Animator anim;

    //보스 패턴 변수
    public int patternIndex;
    public int curPatternCount;
    public int[] maxPatternCount;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if(enemyName =="B")
            anim = GetComponent<Animator>();
    }

    //OnEnable() : 컴포넌트가 활성화 될 때 호출되는 생명주기 함수 - health처럼 소모되는 것을 초기화해주기 위해 사용
    void OnEnable()
    {
        switch(enemyName)
        {
            case "B":
                health = 100;
                Invoke("Stop",2); // obj풀링때 1번, 호출때 1번 총 2번 Invoke호출-> 에러발생
                break;
            case "L":
                health = 40;
                break;
            case "M":
                health = 10;
                break;
            case "S":
                health = 3;
                break;
        }
    }


    void Update()
    {
        if(enemyName =="B")
            return;
        Fire();
        Reload();
    }

    void Fire()
    {
        if(curShotDelay < maxShotDelay)       
            return;

        if(enemyName == "S")
        {  
            //*중요*
            //플레이어에게 쏘기 위해서 플레이어 변수가 필요함 but, 
            //그럼 멤버변수에 Player를 만든다음에 넣어줄까? No! 왜냐하면, Eenmy는 Prefab(아직 Scene에 올라오지 못한 녀석들)이기 때문에 Player(Scene에 올라와 있음) 접근 불가능 -> 프리펩은 이미 Scene에 올라온 오브젝트에 접근이 불가능하다.
            //So, GameManager에서 Enemy를 Instantiate로 Scene에 생성 후 Player 변수를 넣어주면 된다.
            //GameManager와 Enemy에 모두 public GameObject player;를 넣어주자. 
            
            //GameObject bullet = Instantiate(bulletObjA, transform.position+Vector3, transform.rotation);
            GameObject bullet = objectManager.MakeObj("BulletEnemyA");
            bullet.transform.position = transform.position;
            
            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            Vector3 dirVec = player.transform.position - transform.position;
            rigid.AddForce(dirVec.normalized*3, ForceMode2D.Impulse);
        }
        else if(enemyName =="L")
        {
            GameObject bulletR = objectManager.MakeObj("BulletEnemyB");
            bulletR.transform.position = transform.position + Vector3.right * 0.3f;
            GameObject bulletL = objectManager.MakeObj("BulletEnemyB");
            bulletL.transform.position = transform.position + Vector3.left * 0.3f;

            Rigidbody2D rigidR = bulletR.GetComponent<Rigidbody2D>();
            Rigidbody2D rigidL = bulletL.GetComponent<Rigidbody2D>();

            Vector3 dirVecR = player.transform.position - (transform.position+Vector3.right*0.3f);
            Vector3 dirVecL = player.transform.position - (transform.position+Vector3.left*0.3f);

            rigidR.AddForce(dirVecR.normalized*3, ForceMode2D.Impulse);
            rigidL.AddForce(dirVecL.normalized*3, ForceMode2D.Impulse);
        }
            
        curShotDelay = 0;   //장전
    }

    void Reload()
    {
        curShotDelay += Time.deltaTime;
    }

    public void OnHit(int dmg)
    {
        //Destroy가 되기 전에 OnHit가 2번 맞으면 아이템을 2개 뱉을 수도 있다.
        if(health <=0)
            return;

        health -= dmg;
        
        if(enemyName == "B")
        {
            anim.SetTrigger("OnHit");
        }
        else
        {
            spriteRenderer.sprite = sprites[1];
            Invoke("ReturnSprite", 0.1f);;
        }

        if(health<=0)
        {
            Player playerLogic = player.GetComponent<Player>();
            playerLogic.score += enemyScore;


            //#.Random Ratio Item Drop
            int ran = enemyName == "B" ? 0 : Random.Range(0,10);
            if(ran <3)
            {
                Debug.Log("Not Item");
            }
            else if(ran<6)  //Coin 30%
            {
                //Instantiate(itemCoin, transform.position, itemCoin.transform.rotation);
                GameObject itemCoin = objectManager.MakeObj("ItemCoin");
                itemCoin.transform.position = transform.position;

                // 추가했다가 OnEnable때문에 다시 삭제
                // Rigidbody2D rigid = itemCoin.GetComponent<Rigidbody2D>();
                // rigid.velocity = Vector2.down * 0.1f;
                
            }
            else if(ran<8)  //Power 20%
            {
                GameObject itemPower = objectManager.MakeObj("ItemPower");
                itemPower.transform.position = transform.position;
            }
            else if(ran<10) //Boom 20%
            {
                GameObject itemBoom = objectManager.MakeObj("ItemBoom");
                itemBoom.transform.position = transform.position;
            }

            objectManager.DeleteAllObj("Boss");
            gameObject.SetActive(false);
            CancelInvoke(); 
            // Quaternion.identity : 기본 회전값 = 0
            transform.rotation = Quaternion.identity;
            gameManager.CallExplosion(transform.position, enemyName);

            //#. Boss Kill
            if(enemyName =="B")
                gameManager.StageEnd();

        }
    }
 
    void ReturnSprite()
    {
        spriteRenderer.sprite = sprites[0];
    }

    void Stop()
    {
        Debug.Log("Stop Out");

        if (gameObject.activeSelf)
        {
            Rigidbody2D rigid = GetComponent<Rigidbody2D>();
            rigid.velocity = Vector2.zero;

            Debug.Log("Think0");
            Invoke("Think", 2);
            Debug.Log("Stop In");

        }

    }

    void Think()
    {
        Debug.Log("패턴 인덱스 :" + patternIndex);

        patternIndex = patternIndex == 3 ? 0 : patternIndex + 1;
        curPatternCount = 0;
        Debug.Log("패턴 인덱스 :" + patternIndex);

        switch (patternIndex)
        {
            case 0:
                Debug.Log("패턴 인덱스0 :" + patternIndex);
                FireForward();
                break;
            case 1:
                Debug.Log("패턴 인덱스1 :" + patternIndex);
                FireShot();
                break;
            case 2:
                FireArc();
                break;
            case 3:
                FireAround();
                break;
        }
    }

    void FireForward()
    {
        if (health <= 0) return;

        Debug.Log("앞으로 4발 발사");
        //#. Fire 4 Bullet Forward
        GameObject bulletR = objectManager.MakeObj("BulletBossA");
        bulletR.transform.position = transform.position + Vector3.right * 0.3f;
        GameObject bulletRR = objectManager.MakeObj("BulletBossA");
        bulletRR.transform.position = transform.position + Vector3.right * 0.45f;
        GameObject bulletL = objectManager.MakeObj("BulletBossA");
        bulletL.transform.position = transform.position + Vector3.left * 0.3f;
        GameObject bulletLL = objectManager.MakeObj("BulletBossA");
        bulletLL.transform.position = transform.position + Vector3.left * 0.45f;

        Rigidbody2D rigidR = bulletR.GetComponent<Rigidbody2D>();
        Rigidbody2D rigidRR = bulletRR.GetComponent<Rigidbody2D>();
        Rigidbody2D rigidL = bulletL.GetComponent<Rigidbody2D>();
        Rigidbody2D rigidLL = bulletLL.GetComponent<Rigidbody2D>();

        rigidR.AddForce(Vector2.down * 8, ForceMode2D.Impulse);
        rigidRR.AddForce(Vector2.down * 8, ForceMode2D.Impulse);
        rigidL.AddForce(Vector2.down * 8, ForceMode2D.Impulse);
        rigidLL.AddForce(Vector2.down * 8, ForceMode2D.Impulse);


        //#. Pattern Counting
        curPatternCount++;
        if (curPatternCount < maxPatternCount[patternIndex])
            Invoke("FireForward", 2f);
        else
            Invoke("Think", 3);

    }

    void FireShot()
    {
        if (health <= 0) return;

        Debug.Log("플레이어 방향으로 샷건");

        for (int index = 0; index < 5; index++)
        {
            GameObject bullet = objectManager.MakeObj("BulletBossA");
            bullet.transform.position = transform.position;

            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            Vector2 dirVec = player.transform.position - transform.position;
            Vector2 ranVec = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(0f, 2f));
            dirVec += ranVec;
            rigid.AddForce(dirVec.normalized * 3, ForceMode2D.Impulse);
        }

        curPatternCount++;

        if (curPatternCount < maxPatternCount[patternIndex])
            Invoke("FireShot", 3.5f);
        else
            Invoke("Think", 3);
    }
    //todo : 부채모양으로 총알이 안 나감
    void FireArc()
    {
        if (health <= 0) return;

        Debug.Log("부채모양으로 발사");

        GameObject bullet = objectManager.MakeObj("BulletEnemyA");
        bullet.transform.position = transform.position;
        bullet.transform.rotation = Quaternion.identity;

        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
        Vector2 dirVec = new Vector2(Mathf.Sin(Mathf.PI * 2 * curPatternCount / maxPatternCount[patternIndex]), -1);
        rigid.AddForce(dirVec.normalized * 5, ForceMode2D.Impulse);

        curPatternCount++;

        if (curPatternCount < maxPatternCount[patternIndex])
            Invoke("FireArc", 0.15f);
        else
        {
            Invoke("Think", 3);
        }

    }
    void FireAround()
    {
        if (health <= 0) return;

        Debug.Log("원 형태로 전체 공격");
        //#. Fire Around
        int roundNumA = 50;
        int roundNumB = 40;
        int roundNum = curPatternCount % 2 == 0 ? roundNumA : roundNumB;

        for (int index = 0; index < roundNumA; index++)
        {
            GameObject bullet = objectManager.MakeObj("BulletBossB");
            bullet.transform.position = transform.position;
            bullet.transform.rotation = Quaternion.identity;

            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            Vector2 dirVec = new Vector2(Mathf.Cos(Mathf.PI * 2 * index / roundNumA),
                                         Mathf.Sin(Mathf.PI * 2 * index / roundNumA));

            rigid.AddForce(dirVec.normalized * 2, ForceMode2D.Impulse);

            Vector3 rotVec = Vector3.forward * 360 * index / roundNumA + Vector3.forward * 90;
            bullet.transform.Rotate(rotVec);
        }
        curPatternCount++;

        if (curPatternCount < maxPatternCount[patternIndex])
            Invoke("FireAround", 0.7f);
        else
        {
            Debug.Log("Think2");
            Invoke("Think", 3);

        }
    }

    void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.gameObject.tag == "BorderBullet" && enemyName != "B")
        {
            gameObject.SetActive(false);
            transform.rotation = Quaternion.identity;
        }
        else if(other.gameObject.tag == "PlayerBullet")
        {
            Bullet bulletC = other.gameObject.GetComponent<Bullet>();
            OnHit(bulletC.dmg);

            other.gameObject.SetActive(false);    
        }
    }
}
