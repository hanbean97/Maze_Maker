using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("������ ��ü ����Ʈ")]
    [SerializeField] List<GameObject> EnemyList = new List<GameObject>();
    [Header("�ʻ� �� ���� ����Ʈ")]
    List<Transform> nowenemytrs = new List<Transform>();
    public List<Transform> Nowenemytrs { get { return nowenemytrs; } }
    [Header("�Ʊ� ���� ��ü ����Ʈ")]
    [SerializeField] List<GameObject> MonsterList = new List<GameObject>();
    public List<GameObject> MonsterLists { get { return MonsterList; } }
    [Header("�ʻ� �Ʊ� ���� ����Ʈ")]
    List<Transform> nowMonstertrs = new List<Transform>();// ���嵥����
    Dictionary<string, (string, Vector3Int)> DungeonInMonster = new Dictionary<string, (string, Vector3Int)>();//��ȯ�Ǿ��ִ� ���� <Ű��,(��������̸�,��ġ��ġ)>
    public List<Transform> NowMonstertrs { get { return nowMonstertrs; } }
    Dictionary<string, string> InvenInMonster = new Dictionary<string, string>();//�κ��丮�� ����
    public Dictionary<string, string> InventoryMon { get { return InvenInMonster; } }
    [SerializeField, Header("������ ���� �ִ��ȯ����")] int maxMonster;
    [SerializeField, Header("�κ��丮 �ִ����")] int maxinvetory;
    bool isgamestart = false;
    bool isWaveClear = false;
    bool WaveEnd = false;
    bool isSpawn = false;
    bool spawntiming =false;
    [Range(0,2)] int waveLevel = 0;//������ 3����������
    int randompattern = 0;
    [SerializeField,Header("�����ּ�ȯ����")]float spawnTimer;
    float timer;
    int nextspawnEnemy =0;
    int[] spawnarrEnemy;
    [Header("��ȯ��ġ")]
    [SerializeField] Transform spawnposition;
    [SerializeField] Transform endposition;
    public Transform EndPos { get { return endposition; } }
    [SerializeField] Button GameStartBT;
    [SerializeField] WallmakeSc wall;
    GiftChoice MonsterGift;
    [SerializeField] GameObject OpenSeletWindow;
    bool isNewGame = true;//�ٲ���� �ӽ� true
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
        TestMonsterinf();// �׽�Ʈ ���߿� �����
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

        switch (waveLevel)//������ ���� ����
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
            if (timer > spawnTimer)//��ȯ���߿� �׾ trs�� ���Ͱ� ����� =>�ε��� �������Ͼ
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
    /// ���� ������ ���� ����Ʈ�� ���� ������Ʈ������ ����
    /// </summary>
    /// <param name="_transform"></param>
    public void DeathEnemy(Transform _transform)// ��ȯ���߿� ������ �ε��� ������ ��ȯ�ؼ� ��Ƽ�긦 ���ִ� ��������
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
    /// ���Ͱ� ������ ��������Ʈ�� ���� ������Ʈ ������ ����
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
        for(int i=0;i< count; i++)//n*n =n^2 ���� ���߿� �ѹ� 2n���� �ٲ㺸�� �����ϸ�
        {
            (string, Vector3Int) Data = (DungeonInMonster[ $"{i}" ].Item1, DungeonInMonster[$"{i}"].Item2);
            for(int j=0;j<count2; j++)
            {
                if (MonsterList[j].gameObject.name == Data.Item1)
                {
                    GameObject gam = Instantiate(MonsterList[j].gameObject ,Data.Item2 ,Quaternion.identity);
                    nowMonstertrs.Add(gam.transform);//������ �����͸� �ְ� + ��ȯ�ȸ��ʹ� ���� ��ġ������ ������ġ�� ����
                    DungeonInMonster.Remove($"{i}");
                }
            }
        }
    }
    /// <summary>
    /// ��ȯ�ȸ��Ͱ� ���� ������ ������ �Լ�
    /// </summary>
    void WaveClear()
    {
        if(WaveEnd == true)
        {
            if (isgamestart == true && isWaveClear == true )//���潺 ������
            {
                if (waveLevel < 2)
                {
                    waveLevel++;
                    switch (waveLevel)//������ ���� ����
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
            else if (isgamestart == true && isWaveClear == false )//���潺 ���н� 
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
    /// ���� �������� ���������� ����
    /// </summary>
    /// <param name="_transform"></param>
    public void EnemyFinshDungeon(Transform _transform)
    {
        DeathEnemy(_transform);
        isWaveClear = false;
    }
    
    public void GiftItem(int _monster)//�κ��丮�ȿ� ���õ� ���͸� �־��ش�.
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
        int MonsterNum = -1; // -1 �̸� ã�� ����
        for (int i = 0; i < count; i++)//��ȯ�ؾ��� ���� �ѹ���ã��
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

    public void InvenOutDungeonMonster(string _Monster , Vector2Int _vec)// ���͸� ��ȯ�ϰ� ��������� ����
    {
        int findnum =FindMonsterList(_Monster);

        GameObject monsterspawn = Instantiate(MonsterList[findnum], new Vector3( _vec.x,_vec.y,0),Quaternion.identity);
        //List<string> listkey = InvenInMonster.Keys.ToList(); ���������� ����Ʈ �������� �ٲ�
        int count = DungeonInMonster.Count;
        for (int i = 0;i < count;i++)//�������� ������� ã�� �ֱ�
        {
            if (DungeonInMonster.Count == 0 || DungeonInMonster[$"{i}"].Item1 == "None")
            {
               DungeonInMonster[$"{i}"] =( MonsterList[findnum].name, new Vector3Int( _vec.x,_vec.y,0));
               break;
            }
        }
    }
   
    void NewGameStart()//�κ��丮 ������ ��Ÿ �������� ����
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
