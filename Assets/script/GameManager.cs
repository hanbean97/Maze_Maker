using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("적유닛 전체 리스트 ")]
    [SerializeField] List<GameObject> EnemyList = new List<GameObject>();
    [Header("현 맵상에 있는 적")]
    List<Transform> nowenemytrs = new List<Transform>();
    public List<Transform> Nowenemytrs { get { return nowenemytrs; } }
    [Header("아군유닛 전체 리스트 ")]
    [SerializeField] List<GameObject> MonsterList = new List<GameObject>();
    public List<GameObject> MonsterLists { get { return MonsterList; } }
    [Header("현재 맵상에 있는 아군유닛 ")]
    List<Transform> nowMonstertrs = new List<Transform>();
    //Dictionary<string, (string, Vector3Int)> DungeonInMonster = new Dictionary<string, (string, Vector3Int)>();//Json정보저장용<키값(몬스터이름,위치)>
    public List<Transform> NowMonstertrs { get { return nowMonstertrs; } }
    Dictionary<string, string> InvenInMonster = new Dictionary<string, string>();//<인벤토리 위치,몬스터이름> 인벤토리안에있는 아군 정보 
    public Dictionary<string, string> InventoryMon { get { return InvenInMonster; } }
    [SerializeField, Header("?????? ?????? ????????????")] int maxMonster; 
    [SerializeField, Header("인벤토리 최대갯수 ")] int maxinvetory;
    public int MaxInventory { get { return maxinvetory; } }
    [SerializeField] List<List<byte>> goEnemyList;

    bool isgamestart = false;
    public bool IsGamStart { get { return isgamestart; } }
    bool isWaveClear = false;
    bool WaveEnd = false;
    bool isSpawn = false;
    bool spawntiming = false;
    [Range(-1, 2)] int waveLevel = 0;//?????? 3??????????
    int randompattern = 0;
    [SerializeField, Header("??????????????")] float spawnTimer;
    float timer;
    int nextspawnEnemy = 0;
    int[] spawnarrEnemy;
    [Header("????????")]
    [SerializeField] Transform spawnposition;
    [SerializeField] Transform endposition;
    public Transform EndPos { get { return endposition; } }
    [SerializeField] Button GameStartBT;
    [SerializeField] WallmakeSc wall;
    GiftChoice MonsterGift;
    [SerializeField] GameObject OpenSeletWindow;
    bool isitemcatching = false;
    public bool ItemCatching {get{ return isitemcatching; } set { isitemcatching = value; } }
    bool isNewGame = true;//???????? ???? true
    [SerializeField] InventorySc inventory;
    float score;
    public float Score { get { return score; } }
    [SerializeField] TMP_Text ScoreText;
    [SerializeField] TMP_Text LevelText;
    bool loadscene = false;
    float loadTimer;
    [SerializeField] Image fade;
    bool firstgame = true;
    public bool Firstgame { get { return firstgame; } set { firstgame = value; } }

    [SerializeField] GameObject GameoverPanel;
    [SerializeField] TMP_Text GameoverText;
    [SerializeField] GameObject ranktext;
    bool chartin= false;
    public bool ChartIn { get { return chartin; } }
    [Header("레벨마다 나올 몬스터")]
    [SerializeField] List<SpawnEnemy> spawnE;

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
        /*
        if(PlayerPrefs.GetInt("NewGame", 0) == 1)
        {
            firstgame = false;
        }*/
        NewGameStart();

    }
    void Start()
    {
        GameStartBT.onClick.AddListener( EnemyPatternSetting);
        MonsterGift = OpenSeletWindow.GetComponent<GiftChoice>();
        ScoreText.text = $"Score : {(int)score}";
        LevelText.text = $"Level : {waveLevel}";

        SoundManager.instance.PlayBgm(SoundManager.Bgm.fight);
    }
    void backmainmenu()
    {
        SceneManager.LoadSceneAsync(0);
    }
    void LoadSceneNow()
    {
        if(loadscene == false)
        {
            loadTimer += Time.deltaTime;
           
            fade.color = new Color(fade.color.r, fade.color.g, fade.color.b, Mathf.Lerp(0, 1,2-loadTimer));
            if(loadTimer>2)
            {
                fade.gameObject.SetActive(false);
                loadscene = true;
            }
        }
    } 
    void Update()
    {
        LoadSceneNow();
        GetScore();
        SpawnStart();
        WaveClear();
    }
    void GetScore()
    {
        if(isgamestart == true)
        {
            score += Time.deltaTime;
            ScoreText.text = $"Score : {(int)score}";
        }
    } 
    
    void EnemyPatternSetting()
    {
        AsrarAlgo.instance.Wallcheck();
        isgamestart = true;
        // -1 은 테스트용
        if(waveLevel >-1)
        {
            randompattern = Random.Range(0, spawnE[waveLevel].SpwanNumber.Length);
            //문자형인 데이터를 정수형으로 형변환
            char[] EnemyChar = spawnE[waveLevel].SpwanNumber[randompattern].ToString().ToCharArray();

            int[] enemyarray = new int[EnemyChar.Length];
            for(int i=0; i< EnemyChar.Length; i++)
            {
               if((int)char.GetNumericValue(EnemyChar[i]) < EnemyList.Count+1)
                {
                    enemyarray[i] = (int)char.GetNumericValue(EnemyChar[i])-1;
                }
            }
            //params를 이용한 가변 길이 적들 소환하는 함수
            Patterninstruct(enemyarray);
        }
        else
        {
            randompattern = 0;
            Patterninstruct(0);
        }

        /*
        switch (waveLevel, randompattern)
        {
            case (-1,0):
                Patterninstruct(0);
                break;

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
                Patterninstruct(0, 0, 0, 0, 0, 0, 1);
                break;
           case (2,0):
                Patterninstruct(0, 0, 0, 0, 0, 1, 1,1,1,1,0);
                break;
        }
        */
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
    /// ???? ?????? ???? ???????? ???? ?????????????? ????
    /// </summary>
    /// <param name="_transform"></param>
    public void DeathEnemy(Transform _transform)
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
    /// ???????? ?????? ???????????? ???? ???????? ?????? ????
    /// </summary>
    /// <param name="_transform"></param>
    public void DeathMonster(Transform _Mon)
    {
         nowMonstertrs.Remove(_Mon);
    }
    public GameObject[] LoadInInventory()
    {
        int count = InvenInMonster.Count;
        GameObject[] mon = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            if (InvenInMonster[$"{i}"] != "None")
            {
                mon[i] = MonsterList[FindMonsterList(InvenInMonster[$"{i}"])];
               
            }
            else if(InvenInMonster[$"{i}"] == "None")
            {
                mon[i] = null;
            }
        }
        return mon;
    }
  
   
    /// <summary>
    /// 스테이지가 끝날
    /// </summary>
    void WaveClear()
    {
        if(WaveEnd == true)
        {
            if (isgamestart == true && isWaveClear == true )
            {
                if (waveLevel < 3)
                {
                    switch (waveLevel)//스테이지가 끝날때 레벨에 따른 보
                    {
                        case 0:
                            waveLevel++;
                            wall.GiveWallcountUp(10);
                            score += 100;
                            break;
                        case 1:
                            waveLevel++;
                            wall.GiveWallcountUp(20);
                            score += 200;
                            break;
                        case 2:
                            score += 400;
                            break;
                    }
                }
            }
            else if (isgamestart == true && isWaveClear == false )
            {
                if(LoadDatas.instance.Rlists.Count < LoadDatas.instance.maxrank||score >= LoadDatas.instance.Rlists[LoadDatas.instance.maxrank-1].Item2)
                {
                    chartin = true;
                    ranktext.gameObject.SetActive(true);
                }


                GameoverPanel.SetActive(true);
                GameoverText.text = $"{(int)score}";
            }
            LevelText.text = $"Level : {waveLevel}";
            ScoreText.text = $"Score : {(int)score}";
            AllMonsterHeal();
            int count = nowMonstertrs.Count;
            for (int i = 0; i < count; i++)
            {
           //    nowMonstertrs[i].Item1.position = nowMonstertrs[i].Item2.MyPos;
            }

            OpenSeletWindow.SetActive(true);
            MonsterGift.SetImage();
            isWaveClear = true;
            isgamestart = false;
            WaveEnd = false;

        }
    }
  
    /// <summary>
    /// 적이 끝에 도착했을
    /// </summary>
    /// <param name="_transform"></param>
    public void EnemyFinshDungeon(Transform _transform)
    {
        DeathEnemy(_transform);
        isWaveClear = false;
    }
    
    public void GiftItem(int _monster)//보상 몬스
    {
        inventory.SetInvetory(MonsterList[_monster]);
    }
    int FindMonsterList(string _Monster)
    {
        int count = MonsterList.Count;
        int MonsterNum = -1; // -1 
        for (int i = 0; i < count; i++)//
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
            nowMonstertrs[i].GetComponent<Monster>().Heal();
        }
    }
  
    public void InvenOutDungeonMonster(Transform _Monster)
    {
        nowMonstertrs.Add(_Monster);
    }
    
     
    void NewGameStart()//최초 지급 몬스터 
    {
        if(isNewGame == true)
        {
            for (int i = 0; i < maxinvetory; i++)
            {
                InvenInMonster.Add($"{i}", "None");
            }

            InvenInMonster[$"{1}"] = "BigDemon"; 
            InvenInMonster[$"{2}"] = MonsterList[1].name;
            InvenInMonster[$"{5}"] = "BigDemon";
            InvenInMonster[$"{8}"] = MonsterList[0].name; ;
            InvenInMonster[$"{9}"] = MonsterList[1].name;
        }
    }

   

}
