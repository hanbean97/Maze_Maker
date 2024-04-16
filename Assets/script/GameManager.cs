using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class GameManager : MonoBehaviour
{
    static GameManager instance;
    [Header("������ ����Ʈ")]
    public List<GameObject> EnemyList = new List<GameObject>();
    private List<Transform> nowenemytrs = new List<Transform>();
    bool isgamestart = false;
    int waveLevel = 0;
    int randompattern = 0;
    [SerializeField]float spawnTimer;
    float timer;
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
        
    }
    void GoGameStart()
    {
       if(isgamestart == false) { return; }
    }
    void EnemyPatternSetting()
    {
        switch(waveLevel)//������ ���� ����
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
      
    }
    void SpawnPattern()
    {
        timer += Time.deltaTime;
        if (timer > spawnTimer)
        {
            timer = 0;
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
