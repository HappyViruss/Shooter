using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool isTouchTop;
    public bool isTouchBottom;
    public bool isTouchRight;
    public bool isTouchLeft;

    public int life;
    public int score;
    public float speed;
    public int power;
    public int maxPower;
    public int boom;
    public int maxBoom;
    public float maxShotDelay;  //실제 딜레이
    public float curShotDelay;  //한발 쏘고 충전되기 위한 딜레이


    public GameObject bulletObjA;   //프리팹
    public GameObject bulletObjB;
    public GameObject boomEffect;
    
    public GameManager gameManager;     //씬에 항상 존재하는 GameObject가 가지고 있는 Script Component
    public ObjectManager objectManager;
    public bool isHit;
    public bool isBoomTime;

    public GameObject[] followers;

    Animator anim;                  //본 오브젝트 안에 존재하는 Component

    void Awake()
    {
        anim = GetComponent<Animator>();
    }


    void Update()
    {
        Move();
        Fire();
        Boom();
        Reload();
    }

    public void LButtonDown()
    {
        transform.Translate(-1,0,0);
    }
    public void RButtonDown()
    {
        transform.Translate(1,0,0);
    }

    void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        if((isTouchRight&&h==1)||(isTouchLeft&&h==-1))
        h=0;
        float v = Input.GetAxisRaw("Vertical");
        if((isTouchTop&&v==1)||(isTouchBottom&&v==-1))
        v=0;
        Vector3 curPos = transform.position; //(0,-3,0)
        Vector3 nextPos = new Vector3(h,v,0)*speed*Time.deltaTime;

        transform.position = curPos + nextPos;

        if(Input.GetButtonDown("Horizontal")||Input.GetButtonUp("Horizontal"))
        {
            anim.SetInteger("Input",(int)h);
        }
    }

    void Fire()
    {
        if(!Input.GetButton("Fire1"))
            return;

        if(curShotDelay < maxShotDelay)       
            return;
            
        switch(power)
        {
            case 1:
                //Instantiate->object풀링으로 변경시켜주기
                //GameObject bullet = Instantiate(bulletObjA, transform.position, transform.rotation);
                GameObject bullet = objectManager.MakeObj("BulletPlayerA");
                bullet.transform.position = transform.position;


                Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
                rigid.AddForce(Vector2.up*10, ForceMode2D.Impulse);
                break;
            case 2:
                GameObject bulletR = objectManager.MakeObj("BulletPlayerA");
                bulletR.transform.position = transform.position;

                GameObject bulletL = objectManager.MakeObj("BulletPlayerA");
                bulletL.transform.position = transform.position;

                Rigidbody2D rigidL = bulletL.GetComponent<Rigidbody2D>();
                Rigidbody2D rigidR = bulletR.GetComponent<Rigidbody2D>();
                rigidL.AddForce(Vector2.up*10, ForceMode2D.Impulse);
                rigidR.AddForce(Vector2.up*10, ForceMode2D.Impulse);
                break;
            default:
                GameObject bulletLL = objectManager.MakeObj("BulletPlayerA");
                bulletLL.transform.position = transform.position + Vector3.left * 0.25f;

                GameObject bulletCC = objectManager.MakeObj("BulletPlayerB");
                bulletCC.transform.position = transform.position;

                GameObject bulletRR = objectManager.MakeObj("BulletPlayerA");
                bulletRR.transform.position = transform.position+ Vector3.right * 0.25f;

                Rigidbody2D rigidLL = bulletLL.GetComponent<Rigidbody2D>();
                Rigidbody2D rigidCC = bulletCC.GetComponent<Rigidbody2D>();
                Rigidbody2D rigidRR = bulletRR.GetComponent<Rigidbody2D>();
                rigidLL.AddForce(Vector2.up*10, ForceMode2D.Impulse);
                rigidCC.AddForce(Vector2.up*10, ForceMode2D.Impulse);
                rigidRR.AddForce(Vector2.up*10, ForceMode2D.Impulse);
                break;
        }

        curShotDelay = 0;   //장전
    }
    
    void Boom()
    {
        if(!Input.GetButton("Fire2"))
            return;  

        if(isBoomTime)
            return;
        
        if(boom == 0)
            return;
        
        boom--;
        isBoomTime = true;
        gameManager.UpdateBoomIcon(boom);
        
        //#1. Effect visible
        boomEffect.SetActive(true);
        Invoke("OffBoomEffect", 4f);
        //#2. Remove Enemy
        //FindGameObjectsWithTag - 오브젝트를 직접 찾는 "Find 계열" 함수도 성능 부하를 유발
        //모~든 오브젝트를 찾아가며 tag를 찾던 것에서 Pool 안에 있는 오브젝트만 찾으면 되니 더 빨라짐!
        //(+) 활성화 오브젝트를 따로 관리하는 배열도 있다면 금상첨화..!
        //GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] enemiesL = objectManager.GetPool("EnemyL");
        GameObject[] enemiesM = objectManager.GetPool("EnemyM");
        GameObject[] enemiesS = objectManager.GetPool("EnemyS");

        for(int index = 0; index < enemiesL.Length; index++)
        {
            if(enemiesL[index].activeSelf)
            {
                Enemy enemyLogic = enemiesL[index].GetComponent<Enemy>();
                enemyLogic.OnHit(1000);
            }
        }
        for(int index = 0; index < enemiesM.Length; index++)
        {
            if(enemiesM[index].activeSelf)
            {
                Enemy enemyLogic = enemiesM[index].GetComponent<Enemy>();
                enemyLogic.OnHit(1000);
            }
        }
        for(int index = 0; index < enemiesS.Length; index++)
        {
            if(enemiesS[index].activeSelf)
            {
                Enemy enemyLogic = enemiesS[index].GetComponent<Enemy>();
                enemyLogic.OnHit(1000);
            }
        }

        //#3. Remove Enemy Bullet
        //뷸렛도 위와 마찬가지다
        //GameObject[] bullets = GameObject.FindGameObjectsWithTag("EnemyBullet");
        GameObject[] bulletsA = objectManager.GetPool("BulletEnemyA");
        GameObject[] bulletsB = objectManager.GetPool("BulletEnemyA");
        for(int index = 0; index < bulletsA.Length; index++)
        {
            if(bulletsA[index].activeSelf)
            {
                bulletsA[index].SetActive(false);
            }
        }
        for(int index = 0; index < bulletsB.Length; index++)
        {
            if(bulletsB[index].activeSelf)
            {
                bulletsB[index].SetActive(false);
            }
        }
    }

    void Reload()
    {
        curShotDelay += Time.deltaTime;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "Border")
        {
            switch(other.gameObject.name)
            {
                case "Top":
                isTouchTop = true;
                break;
                case "Bottom":
                isTouchBottom = true;
                break;
                case "Right": 
                isTouchRight = true;
                break;
                case "Left":
                isTouchLeft = true;
                break;
            }
        }
        else if(other.gameObject.tag =="Enemy" || other.gameObject.tag == "EnemyBullet")
        {
            if(isHit)
                return;

            isHit = true;
            life--;
            gameManager.UpdateLifeIcon(life);

            if(life == 0)
            {
                gameManager.GameOver();
            }
            else
            {
                gameManager.RespawnPlayer();
            }
            //비활성화! -> gameManager에서 실려주자
            gameObject.SetActive(false);
        }
        else if(other.gameObject.tag =="Item")
        {
            Item item = other.gameObject.GetComponent<Item>();
            switch(item.type)
            {
                case "Coin":
                    score += 100;
                    break;
                case "Power":
                    if(power == maxPower)
                        score += 500;
                    else
                        power++;
                        AddFollower();
                    break;
                case "Boom":
                    if(boom == maxBoom)
                        score += 1000;
                    else
                        boom++;
                        gameManager.UpdateBoomIcon(boom);
                    break;
            }
           //Destroy(other.gameObject);
           other.gameObject.SetActive(false);
        }
    }

    void OffBoomEffect()
    {
        boomEffect.SetActive(false);
        isBoomTime = false;
    }

    void AddFollower()
    {
        if(power ==4)
            followers[0].SetActive(true);
        else if(power ==5)
            followers[1].SetActive(true);
        else if(power ==6)
            followers[2].SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        if(other.gameObject.tag == "Border")
        {
            switch(other.gameObject.name)
            {
                case "Top":
                isTouchTop = false;
                break;
                case "Bottom":
                isTouchBottom = false;
                break;
                case "Right":
                isTouchRight = false;
                break;
                case "Left":
                isTouchLeft = false;
                break;
            }
        }
    }
}
