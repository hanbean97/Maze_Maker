using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("������ ����Ʈ")]
    [SerializeField] List<GameObject> EnemyList = new List<GameObject>();
    [Header("�ʻ� �� ���� ����Ʈ")]
    List<Transform> nowenemytrs = new List<Transform>();
    public List<Transform> Nowenemytrs { get { return nowenemytrs; } }
    [Header("�Ʊ� ���� ��ü ����Ʈ")]
    [SerializeField] List<GameObject> MonsterList = new List<GameObject>();
    [Header("�ʻ� �Ʊ� ���� ����Ʈ")]
    List<Transform> nowMonstertrs = new List<Transform>();// ���嵥����
    public List<Transform> NowMonstertrs { get { return nowMonstertrs; } }
    [SerializeField]bool isgamestart = false;
    bool isSpawn = false;
    int waveLevel = 0;
    int randompattern = 0;
    [SerializeField,Header("�����ּ�ȯ����")]float spawnTimer;
    float timer;
    int nextspawnEnemy =0;
    int[] spawnarrEnemy;
    [Header("��ȯ��ġ")]
    [SerializeField] Transform spawnposition;
   
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
        
    }
    void Update()
    {
        EnemyPatternSetting();
        SpawnStart();
    }
    void GoGameStart()
    {
       
    }
    void EnemyPatternSetting()
    {
        if(isgamestart == false) return;

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
                Patterninstruct(1, 1, 1, 1);
                break;
           case (1,0):
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
                }
            }
        }
    }

    void SpawnEnmys(int enemyIndex)
    {
        GameObject enemyGo = Instantiate(EnemyList[enemyIndex], spawnposition.position,Quaternion.identity);
        nowenemytrs.Add(enemyGo.transform);
    }
    public void DeathEnemy(Transform _transform)
    {
      nowenemytrs.Remove(_transform);
    }
    public void DeathMonster(Transform _transform)
    {
        nowMonstertrs.Remove(_transform);
    }
    public void WaveStartCommand()
    {
        int count = nowMonstertrs.Count;
        for(int i=0; i < count; i++)
        {
            //nowMonstertrs[i].GetComponent<>();
        }
    }
}
