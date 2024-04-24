using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField, Header("�κ��丮 �ִ����")] int maxinvetory;
    bool isgamestart = false;
    bool isWaveClear = false;
    bool isWaveFail = false;
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
        TestMonsterinf();// �׽�Ʈ ���߿� �����
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
    /// ���� ������ ���� ����Ʈ�� ���� ������Ʈ������ ����
    /// </summary>
    /// <param name="_transform"></param>
    public void DeathEnemy(Transform _transform)
    {
      nowenemytrs.Remove(_transform);
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
         if(isgamestart == true && isWaveClear ==true && isWaveFail == false && nowenemytrs.Count==0)//���潺 ������
         {
            if(waveLevel < 2)
            {
                waveLevel++;
                switch(waveLevel)//������ ���� ����
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
         else if (isgamestart==true && isWaveClear == false && nowenemytrs.Count == 0)//���潺 ���н� 
        {
            isWaveClear = false;
            isgamestart = false;
        }
    }
    /// <summary>
    /// ���� �������� ���������� ����
    /// </summary>
    /// <param name="_transform"></param>
    public void EnemyFinshDungeon(Transform _transform)
    {
        nowenemytrs.Remove(_transform);
        isWaveFail = true;
    }
    void inventorySpaceMake()//ó���� ���� �κ��丮�� ����� �����
    {
        for(int i = 0; i < maxinvetory; i++) 
        {
            InvenInMonster.Add($"{i}","None");
        }
    }
    public void GiftItem(int monsterNumber)//�κ��丮�ȿ� ���õ� ���͸� �־��ش�.
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
