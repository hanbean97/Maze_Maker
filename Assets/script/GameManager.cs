using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("적유닛 리스트")]
    [SerializeField] List<GameObject> EnemyList = new List<GameObject>();
    [Header("맵상에 적 유닛 리스트")]
    List<Transform> nowenemytrs = new List<Transform>();
    public List<Transform> Nowenemytrs { get { return nowenemytrs; } }
    [Header("아군 유닛 전체 리스트")]
    [SerializeField] List<GameObject> MonsterList = new List<GameObject>();
    [Header("맵상에 아군 유닛 리스트")]
    List<Transform> nowMonstertrs = new List<Transform>();// 저장데이터
    Dictionary<string, (string, Vector3Int)> SaveMonsterData = new Dictionary<string, (string, Vector3Int)>();//<키값,(저장몬스터이름,배치위치)>

    public List<Transform> NowMonstertrs { get { return nowMonstertrs; } }
    [SerializeField] bool isgamestart = false;
    bool isWaveClear = false;
    bool isWaveFail = false;
    bool isSpawn = false;
    [Range(0,2)] int waveLevel = 0;//구현은 3레벨까지만
    int randompattern = 0;
    [SerializeField,Header("적유닛소환공백")]float spawnTimer;
    float timer;
    int nextspawnEnemy =0;
    int[] spawnarrEnemy;
    [Header("소환위치")]
    [SerializeField] Transform spawnposition;
    [SerializeField] Transform endposition;
    public Transform EndPos { get { return endposition; } }
    [SerializeField] Button GameStartBT;
    [SerializeField] WallmakeSc wall;
   void TestMonsterinf()
    {
        SaveMonsterData.Add("0", ("BigDemon", new Vector3Int(10, -7, 0)));
        SaveMonsterData.Add("1", ("BigDemon", new Vector3Int(5, -5, 0)));
        SaveMonsterData.Add("2", ("BigDemon", new Vector3Int(3, -3, 0)));
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
        }
    }
    void Start()
    {
        TestMonsterinf();// 테스트 나중에 지우기
        SetLoadMonster();
        GameStartBT.onClick.AddListener(Gamestart);
    }
    void Update()
    {
        EnemyPatternSetting();
        SpawnStart();
        WaveClear();
    }
    
    void EnemyPatternSetting()
    {
        if(isgamestart == false) return;

        switch (waveLevel)//레벨에 따른 패턴
        {
            case 0:
                randompattern = Random.Range(0, 3);
              
                break;
            case 1:
                randompattern = Random.Range(0, 2);
                break;
            case 2:
                randompattern = Random.Range(0,1);
                break;
        }
        switch (waveLevel, randompattern)
        {
           case (0,0):
                Patterninstruct(1,1,0,0);
                break;
           case (0,1):
                 Patterninstruct(1, 1, 1, 1);
                break;
           case (0,2):
                Patterninstruct(1, 1, 1, 1);
                break;
           case (1,0):
                Patterninstruct(0, 0, 1, 1,1,1,1);
                break;
           case (1,1):
                break;
           case (2,0):
                break;
        }

        isgamestart = false;
    }
    void Patterninstruct(params int[] enemynumber)
    {
        isSpawn = true;
        spawnarrEnemy = enemynumber;
    }
    void SpawnStart()
    {
        if (isSpawn == true)
        {
            timer += Time.deltaTime;
            if (timer > spawnTimer)
            {
                SpawnEnmys(spawnarrEnemy[nextspawnEnemy]);
                nextspawnEnemy++;
                timer = 0;
                if (nextspawnEnemy == spawnarrEnemy.Length)
                {
                    nextspawnEnemy = 0;
                    spawnarrEnemy = null;
                    isSpawn = false;
                    isWaveClear = true;
                }
            }
        }
    }
    void SpawnEnmys(int enemyIndex)
    {
        GameObject enemyGo = Instantiate(EnemyList[enemyIndex], spawnposition.position,Quaternion.identity);
        nowenemytrs.Add(enemyGo.transform);
    }
    /// <summary>
    /// 적이 죽을때 관리 리스트의 죽은 오브젝트정보를 삭제
    /// </summary>
    /// <param name="_transform"></param>
    public void DeathEnemy(Transform _transform)
    {
      nowenemytrs.Remove(_transform);
    }
    /// <summary>
    /// 몬스터가 죽을때 관리리스트의 죽은 오브젝트 정보를 삭제
    /// </summary>
    /// <param name="_transform"></param>
    public void DeathMonster(Transform _transform)
    {
        nowMonstertrs.Remove(_transform);
    }
    void SetLoadMonster()
    {
        int count = SaveMonsterData.Count;
        int count2 = MonsterList.Count;
        for(int i=0;i< count; i++)//n*n =n^2 계산식 나중에 한번 2n으로 바꿔보기 가능하면
        {
            (string, Vector3Int) Data = (SaveMonsterData[ $"{i}" ].Item1, SaveMonsterData[$"{i}"].Item2);
            for(int j=0;j<count2; j++)
            {
                if (MonsterList[j].gameObject.name == Data.Item1)
                {
                    GameObject gam = Instantiate(MonsterList[j].gameObject ,Data.Item2 ,Quaternion.identity);
                    nowMonstertrs.Add(gam.transform);//몬스터의 데이터를 넣고 + 소환된몬스터는 받은 위치정보를 기존위치로 설정
                    SaveMonsterData.Remove($"{i}");
                }
            }
        }
    }
    void Gamestart()
    {
       isgamestart = true;
    }
    /// <summary>
    /// 소환된몬스터가 전부 죽을때 나오는 함수
    /// </summary>
    void WaveClear()
    {
         if(isWaveClear ==true && isWaveFail == false && nowenemytrs.Count==0)
         {
            if(waveLevel < 2)
            {
                waveLevel++;
                switch(waveLevel)
                {
                    case 1:
                        wall.GiveWallcountUp(5);
                        break;
                    case 2:
                        wall.GiveWallcountUp(10);
                        break;

                }
            }
            isWaveClear = false;
         }
         else if (isWaveFail == true)
        {
            isWaveClear = false;
            isWaveFail = false;
        }
    }
    /// <summary>
    /// 적이 끝지점에 도착했을때 실행
    /// </summary>
    /// <param name="_transform"></param>
    public void EnemyFinshDungeon(Transform _transform)
    {
        nowenemytrs.Remove(_transform);
        isWaveFail = true;
    }
  
}
