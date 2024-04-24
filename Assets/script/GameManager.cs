using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("적유닛 전체 리스트")]
    [SerializeField] List<GameObject> EnemyList = new List<GameObject>();
    [Header("맵상에 적 유닛 리스트")]
    List<Transform> nowenemytrs = new List<Transform>();
    public List<Transform> Nowenemytrs { get { return nowenemytrs; } }
    [Header("아군 유닛 전체 리스트")]
    [SerializeField] List<GameObject> MonsterList = new List<GameObject>();
    public List<GameObject> MonsterLists { get { return MonsterList; } }
    [Header("맵상에 아군 유닛 리스트")]
    List<Transform> nowMonstertrs = new List<Transform>();// 저장데이터
    Dictionary<string, (string, Vector3Int)> DungeonInMonster = new Dictionary<string, (string, Vector3Int)>();//소환되어있는 몬스터 <키값,(저장몬스터이름,배치위치)>
    public List<Transform> NowMonstertrs { get { return nowMonstertrs; } }
    Dictionary<string, string> InvenInMonster = new Dictionary<string, string>();//인벤토리안 몬스터
    [SerializeField, Header("인벤토리 최대공간")] int maxinvetory;
    bool isgamestart = false;
    bool isWaveClear = false;
    bool isWaveFail = false;
    bool isSpawn = false;
    bool spawntiming =false;
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

    bool isNewGame = false;
   void TestMonsterinf()
    {
        DungeonInMonster.Add("0", ("BigDemon", new Vector3Int(10, -7, 0)));
        DungeonInMonster.Add("1", ("BigDemon", new Vector3Int(5, -5, 0)));
        DungeonInMonster.Add("2", ("BigDemon", new Vector3Int(3, -3, 0)));
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
        inventorySpaceMake();
        TestMonsterinf();// 테스트 나중에 지우기
        SetLoadMonster();
        GameStartBT.onClick.AddListener( EnemyPatternSetting);
    }
    void Update()
    {
        SpawnStart();
        WaveClear();
    }
    
    void EnemyPatternSetting()
    {
        isgamestart = true;

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
                Patterninstruct(0,0,0,0);
                break;
           case (0,1):
                 Patterninstruct(0, 0, 1, 1);
                break;
           case (0,2):
                Patterninstruct(0, 1, 1, 1);
                break;
           case (1,0):
                Patterninstruct(0, 0, 1, 1,1,1,1);
                break;
           case (1,1):
                break;
           case (2,0):
                break;
        }
    }
    void Patterninstruct(params int[] enemynumber)
    {
        isSpawn = true;
        spawntiming = true;
       spawnarrEnemy = enemynumber;
    }
    void SpawnStart()
    {
        if (isSpawn == true && spawntiming == true)
        {
            for (int i = 0; i < spawnarrEnemy.Length; i++)
            {
                SpawnEnmys(spawnarrEnemy[i]);
            }
            isWaveClear = true;
            spawntiming = false;
        }
        if (isSpawn == true )
        {
            timer += Time.deltaTime;
            if (timer > spawnTimer)
            {
                nowenemytrs[nextspawnEnemy].gameObject.SetActive(true);
                nextspawnEnemy++;
                timer = 0;
                if (nextspawnEnemy == spawnarrEnemy.Length)
                {
                    nextspawnEnemy = 0;
                    spawnarrEnemy = null;
                    isSpawn = false;
                }
            }
        }
    }
    void SpawnEnmys(int enemyIndex)
    {
        GameObject enemyGo = Instantiate(EnemyList[enemyIndex], spawnposition.position,Quaternion.identity);
        nowenemytrs.Add(enemyGo.transform);
        enemyGo.SetActive(false);

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
        int count = DungeonInMonster.Count;
        int count2 = MonsterList.Count;
        for(int i=0;i< count; i++)//n*n =n^2 계산식 나중에 한번 2n으로 바꿔보기 가능하면
        {
            (string, Vector3Int) Data = (DungeonInMonster[ $"{i}" ].Item1, DungeonInMonster[$"{i}"].Item2);
            for(int j=0;j<count2; j++)
            {
                if (MonsterList[j].gameObject.name == Data.Item1)
                {
                    GameObject gam = Instantiate(MonsterList[j].gameObject ,Data.Item2 ,Quaternion.identity);
                    nowMonstertrs.Add(gam.transform);//몬스터의 데이터를 넣고 + 소환된몬스터는 받은 위치정보를 기존위치로 설정
                    DungeonInMonster.Remove($"{i}");
                }
            }
        }
    }
    /// <summary>
    /// 소환된몬스터가 전부 죽을때 나오는 함수
    /// </summary>
    void WaveClear()
    {
         if(isgamestart == true && isWaveClear ==true && isWaveFail == false && nowenemytrs.Count==0)//디펜스 성공시
         {
            if(waveLevel < 2)
            {
                waveLevel++;
                switch(waveLevel)//레벨에 따른 보상
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
            isgamestart = false;
         }
         else if (isgamestart==true && isWaveClear == false && nowenemytrs.Count == 0)//디펜스 실패시 
        {
            isWaveClear = false;
            isgamestart = false;
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
    void inventorySpaceMake()//처음에 시작 인벤토리의 빈공간 만들기
    {
        for(int i = 0; i < maxinvetory; i++) 
        {
            InvenInMonster.Add($"{i}","None");
        }
    }
    public void GiftItem(int monsterNumber)//인벤토리안에 선택된 몬스터를 넣어준다.
    {
        for(int i = 0; i < InvenInMonster.Count; i++)
        {
            if (InvenInMonster[$"{i}"] == "None")
            {
                InvenInMonster.Add($"{i}", MonsterList[monsterNumber].name);
            }
        }
    }
    void AllMonsterHeal()
    {
        int count = nowenemytrs.Count;
        for (int i = 0;i < count; i++)
        {

        }
    }
    public void InventorySelectMonster()
    {

    }
    public void InvenOutDungeonMonster(int _Monster , Vector2 _vec)
    {
        GameObject monsterspawn = Instantiate(MonsterList[_Monster],_vec,Quaternion.identity);

        List<string> listkey = InvenInMonster.Keys.ToList();

        for (int i = 0;i < MonsterList.Count;i++)
        {
            if (InvenInMonster.Count == 0 || InvenInMonster[$"{i}"] == "None")
            {
               // DungeonInMonster.Add($"{i}", MonsterList[_Monster], _vec);
                
            }
        }

    }
   
    void NewGameStart()
    {
        if(isNewGame == true)
        {

        }

    }
}
