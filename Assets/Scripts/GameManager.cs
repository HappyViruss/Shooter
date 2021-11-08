using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;    //파일 읽기를 위한 System.IO

// (소괄호), {중괄호}, [대괄호]
/*
Logic : 어떤 프로그램을 만들 때의 "논리적이 흐름" 
로직이 깨진다 == 논리적 흐름과 충돌한다. 
x)나 : 개발자에게 "날씨확인 기능츨 추가해주세요" 요청-> 개발자 : "로직이 충돌한다" 답변. 즉, 기능을 추가시 기존 로직이 붕괴될 수 있다는 얘기. ex) 1~9까지 곱

*/

public class GameManager : MonoBehaviour
{
    public int stage;
    public Animator stageAnim;
    public Animator clearAnim;
    public Animator fadeAnim;
    public Transform playerPos;

    public string[] enemyObjs;
    public Transform[] spawnPoints;

    public float nextSpawnDelay;
    public float curSpawnDelay;

    public GameObject player;

    //UI
    public Text scoreText;
    public Image[] lifeImage;
    public Image[] boomImage;       //Q. RGB +A에서 A를 0으로(완전 투명하게)하고 넣었는데 사용할 땐 다시 색이 돌아온다?  A.  new Color(1,1,1,1);  //Color(R,G,B,A);            
    public GameObject gameOverSet;
    public ObjectManager objectManager;

    //적 출현에 관련된 변수 생성
    public List<Spawn> spawnList;
    public int spawnIndex;
    public bool spawnEnd;

    void Awake()
    {
        spawnList = new List<Spawn>();
        enemyObjs = new string[]{ "EnemyS", "EnemyM", "EnemyL", "EnemyB"};
        StageStart();
    }

    public void StageStart()
    {
        //#. Stage UI Load
        stageAnim.SetTrigger("On");
        stageAnim.GetComponent<Text>().text = "Stage " + stage + "\nStart";
        clearAnim.GetComponent<Text>().text = "Stage " + stage + "\nClear!";
        //#. Enemy Spawn File Read
        ReadSpawnFile();

        //#. Fade In(밝아짐)
        fadeAnim.SetTrigger("In");
    }

    public void StageEnd()
    {
        //#. Clear UI Load
        clearAnim.SetTrigger("On");

        //#. Fade out(어두워짐)
        fadeAnim.SetTrigger("Out");

        //#.Player Repos
        player.transform.position = playerPos.position;

        //#. Stage Increment
        stage++;
        if(stage > 2)
            Invoke("GameOver", 6);
        else 
            Invoke("StageStart", 5);
    }

    void ReadSpawnFile()
    {
        //#1. 변수 초기화
        spawnList.Clear();
        spawnIndex = 0;
        spawnEnd = false;

        //#2. 리스폰 파일 읽기
        TextAsset  textFile =Resources.Load("Stage "+ stage.ToString()) as TextAsset; //.Tostring() 안해줘도 됨 : 묵시적 형변환됨
        StringReader stringReader = new StringReader(textFile.text);

        //while문으로 텍스트 데이터 끝에 다다를 때까지 반복
        while(stringReader != null)
        {
            string line = stringReader.ReadLine();
            Debug.Log(line);

            if(line == null)
                break;

            //#3. 리스폰 데이터 생성
            Spawn spawnData = new Spawn();
            spawnData.delay = float.Parse(line.Split(',')[0]);
            spawnData.type = line.Split(',')[1];
            spawnData.point = int.Parse(line.Split(',')[2]);
            spawnList.Add(spawnData);
        }

        //#. 작업이 끝난 후 StringReader로 열어둔 파일을 꼭 닫기
        stringReader.Close();

        //#. 첫번째 스폰 딜레이 적용
        nextSpawnDelay = spawnList[0].delay;

    }
  
    void Update()
    {
        //플레이어 죽음 테스트
        if(Input.GetKeyDown(KeyCode.U))
        {
            GameOver();
        }

        curSpawnDelay += Time.deltaTime;

        if (curSpawnDelay > nextSpawnDelay && !spawnEnd)
        {
            spawnEnemy();
            curSpawnDelay = 0; //이게 없으면 시간이 누적됨. 
        }

        Player playerLogic = player.GetComponent<Player>();
        scoreText.text = string.Format("{0:n0}", playerLogic.score);
    }

    void spawnEnemy()
    {
        // int ranEnemy = Random.Range(0, 3);  //0,1,2
        // int ranPoint = Random.Range(0, 9);  //0~8
        int enemyIndex = 0;
        switch(spawnList[spawnIndex].type)
        {
            case "S":
                enemyIndex = 0;
                break;
            case "M":
                enemyIndex = 1;
                break;
            case "L":
                enemyIndex = 2;
                break;
            case "B":
                enemyIndex = 3;
                break;
        }

        //GameObject enemy = objectManager.MakeObj();
        int enemyPoint = spawnList[spawnIndex].point;
        GameObject enemy = objectManager.MakeObj(enemyObjs[enemyIndex]);
        enemy.transform.position = spawnPoints[enemyPoint].position;

        Rigidbody2D rigid = enemy.GetComponent<Rigidbody2D>();
        Enemy enemyLogic = enemy.GetComponent<Enemy>();
        enemyLogic.player = player; //enemy가 생성이 된 후이기 때문에 player를 넘겨주는 게 가능하다. *중요*
        enemyLogic.gameManager = this;  //this : 클래스 자신을 일컫는 키워드
        enemyLogic.objectManager = objectManager;
    


        if (enemyPoint == 5 || enemyPoint == 6)
        {
            enemy.transform.Rotate(Vector3.back * 90);
            rigid.velocity = new Vector2(enemyLogic.speed * (-1), -1);
        }
        else if (enemyPoint == 7 || enemyPoint == 8)
        {
            enemy.transform.Rotate(Vector3.forward * 90);
            rigid.velocity = new Vector2(enemyLogic.speed, -1);
        }
        else
        {
            rigid.velocity = new Vector2(0, enemyLogic.speed * (-1));
        }

        //#. 리스폰 인덱스 증가
        spawnIndex++;
        if(spawnIndex == spawnList.Count)
        {
            spawnEnd = true;
            return;
        }

        //#. 다음 리스폰 딜레이 갱신 - 적 생성이 완료되면 다음 생성을 위한 시간 갱신
        nextSpawnDelay = spawnList[spawnIndex].delay;
    }


    public void RespawnPlayer()
    {
        //2초 뒤에 ""안의 함수를 실행하라.
        Invoke("RespawnPlayerExe", 2f);
    }

    void RespawnPlayerExe()
    {
        player.transform.position = Vector3.down * 3.5f;
        player.SetActive(true);

        Player playerLogic = player.GetComponent<Player>();
        playerLogic.isHit = false;
    }

    public void UpdateLifeIcon(int life)
    {
        //#.UI Life Init Disable
        for (int index = 0; index < 3; index++)
            lifeImage[index].color = new Color(1, 1, 1, 0);

        //#.UI Life Active
        for (int index = 0; index < life; index++)
            lifeImage[index].color = new Color(1, 1, 1, 1);
    }

    public void UpdateBoomIcon(int boom)
    {
        //#.UI boom Init Disable
        for (int index = 0; index < 3; index++)
            boomImage[index].color = new Color(1, 1, 1, 0);

        //#.UI boom Active
        for (int index = 0; index < boom; index++)
            boomImage[index].color = new Color(1, 1, 1, 1);  //Color(R,G,B,A);            
    }

    public void CallExplosion(Vector3 pos, string type)
    {
        GameObject explosion = objectManager.MakeObj("Explosion");
        Explosion explosionLogic = explosion.GetComponent<Explosion>();

        explosion.transform.position = pos;
        explosionLogic.StartExplosion(type);
    }

    public void GameOver()
    {
        gameOverSet.SetActive(true);
    }

    public void GameRetry()
    {
        SceneManager.LoadScene(0);
    }

}
