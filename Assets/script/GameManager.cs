using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public  class GameManager : MonoBehaviour
{
    static GameManager instance;
    [Header("적유닛 리스트")]
    public List<GameObject> EnemyList = new List<GameObject>();
    private List<Transform> nowenemytrs = new List<Transform>();
    [SerializeField]bool isgamestart = false;
    bool isSpawn = false;
    int waveLevel = 0;
    int randompattern = 0;
    [SerializeField]float spawnTimer;
    float timer;
    int nextspawnEnemy =0;
    int[] spawnarrEnemy;
    [Header("소환위치")]
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
               // Patterninstruct(0, 0, 1, 1);
                break;
           case (0,2):
                //Patterninstruct(1, 1, 1, 1);
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
                    isSpawn = false;
                }
            }
        }
    }


    void SpawnEnmys(int enemyIndex)
    {
        GameObject enemyGo = Instantiate(EnemyList[enemyIndex], spawnposition.position,Quaternion.identity);
    }
    void SpawnTimer( )
    {

    }

}
