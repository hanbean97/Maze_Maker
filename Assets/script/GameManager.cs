using System.Collections;
using System.Collections.Generic;
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
    public Dictionary<string, string> InventoryMon { get { return InvenInMonster; } }
    [SerializeField, Header("던전내 몬스터 최대소환개수")] int maxMonster;
    [SerializeField, Header("인벤토리 최대공간")] int maxinvetory;
    bool isgamestart = false;
    bool isWaveClear = false;
    bool WaveEnd = false;
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
    GiftChoice MonsterGift;
    [SerializeField] GameObject OpenSeletWindow;
    bool isNewGame = true;//바꿔야함 임시 true
   void TestMonsterinf()
    {
        DungeonInMonster["0"] = ("BigDemon", new Vector3Int(10, -7, 0));
        DungeonInMonster["1"] = ("BigDemon", new Vector3Int(5, -5, 0));
        DungeonInMonster["2"] = ("BigDemon", new Vector3Int(3, -3, 0));
        DungeonInMonster["3"] = ("None", new Vector3Int(0, 0, 0));
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
        NewGameStart();
        TestMonsterinf();// 테스트 나중에 지우기
        SetLoadMonster();
        GameStartBT.onClick.AddListener( EnemyPatternSetting);
        MonsterGift = OpenSeletWindow.GetComponent<GiftChoice>();
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
            if (timer > spawnTimer)//소환도중에 죽어서 trs의 몬스터가 사라짐 =>인덱스 오류가일어남
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
    public void DeathEnemy(Transform _transform)// 소환도중에 죽으면 인덱스 정보가 변환해서 액티브를 꺼주는 방향으로
    {
        int count = nowenemytrs.Count;
        int allEnemyactive = 0;
        for (int i = 0; i < count; i++)
        {
            if (nowenemytrs[i].gameObject.activeSelf == false)
            {
                allEnemyactive++;
            }
        }
        if(allEnemyactive == count)
        {
            for (int i = 0; i < count; i++)
            {
                Destroy(nowenemytrs[i].gameObject);
            }
            nowenemytrs.Clear();
           WaveEnd = true;
        }
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
        if(WaveEnd == true)
        {
            if (isgamestart == true && isWaveClear == true )//디펜스 성공시
            {
                if (waveLevel < 2)
                {
                    waveLevel++;
                    switch (waveLevel)//레벨에 따른 보상
                    {
                        case 1:
                            wall.GiveWallcountUp(5);

                            break;
                        case 2:
                            wall.GiveWallcountUp(10);
                            break;
                    }
                }
            }
            else if (isgamestart == true && isWaveClear == false )//디펜스 실패시 
            {
              
            }
            AllMonsterHeal();
            OpenSeletWindow.SetActive(true);
            MonsterGift.SetImage();
            isWaveClear = false;
            isgamestart = false;
            WaveEnd = false;
        }
        
    }
    /// <summary>
    /// 적이 끝지점에 도착했을때 실행
    /// </summary>
    /// <param name="_transform"></param>
    public void EnemyFinshDungeon(Transform _transform)
    {
        DeathEnemy(_transform);
        isWaveClear = false;
    }
    
    public void GiftItem(int _monster)//인벤토리안에 선택된 몬스터를 넣어준다.
    {

        for (int i = 0; i < maxinvetory; i++)
        {
            if (InvenInMonster[$"{i}"] == "None")
            {
                InvenInMonster[$"{i}"] = MonsterList[_monster].name;
                break;
            }
        }
    }
    int FindMonsterList(string _Monster)
    {
        int count = MonsterList.Count;
        int MonsterNum = -1; // -1 이면 찾기 실패
        for (int i = 0; i < count; i++)//소환해야할 몬스터 넘버링찾기
        {
            if (_Monster == MonsterList[i].name)
            {
                MonsterNum = i;
                break;
            }
        }
        return MonsterNum;
    }
    void AllMonsterHeal()
    {
        int count = nowenemytrs.Count;
        for (int i = 0;i < count; i++)
        {
           nowenemytrs[i].GetComponent<Monster>().Heal();
        }
    }
    public void ChangeInvenPosMonster(string _Monster)
    {

    }

    public void InvenOutDungeonMonster(string _Monster , Vector2Int _vec)// 몬스터를 소환하고 저장사전에 저장
    {
        int findnum =FindMonsterList(_Monster);

        GameObject monsterspawn = Instantiate(MonsterList[findnum], new Vector3( _vec.x,_vec.y,0),Quaternion.identity);
        //List<string> listkey = InvenInMonster.Keys.ToList(); 사전형식을 리스트 형식으로 바꿈
        int count = DungeonInMonster.Count;
        for (int i = 0;i < count;i++)//사전에서 빈공간을 찾고 넣기
        {
            if (DungeonInMonster.Count == 0 || DungeonInMonster[$"{i}"].Item1 == "None")
            {
               DungeonInMonster[$"{i}"] =( MonsterList[findnum].name, new Vector3Int( _vec.x,_vec.y,0));
               break;
            }
        }
    }
   
    void NewGameStart()//인벤토리 공간과 기타 시작정보 정의
    {
        if(isNewGame == true)
        {
            for (int i = 0; i < maxMonster; i++)
            {
                DungeonInMonster.Add($"{i}", ("None", Vector3Int.zero));
            }
            for (int i = 0; i < maxinvetory; i++)
            {
                InvenInMonster.Add($"{i}", "None");
            }

        }
    }
}
