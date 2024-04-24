using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node
{
    public bool isWall;//이노드에 벽이 있는지
    public Node ParentNode; // 부모노드 = 이노드로 도착하기전 이전노드

    public int x, y;// 위치
    public int G, H;// G = 여기까지 오는데 사용한 거리 H = 도착지까지 벽을 무시하고 최단거리
    public int F { get { return G + H; } }// F가 가장 짧은 노드먼저 탐색
    public Vector3Int nodePosition { get { return new Vector3Int(x, -y,0); } }//맵상에 노드가 존재하는 위치
    public Node(bool _isWall, int _x, int _y) // 생성할때 셋팅
    {
        isWall = _isWall; x = _x; y = _y; 
    }
}

public class AsrarAlgo : MonoBehaviour
{
    public static AsrarAlgo instance;//싱글톤

    [Header("Node정보 ")]
    [SerializeField]Vector2Int startPos;//적이 게임에서 처음으로 시작할 위치
    public Vector2Int StartPos { get { return startPos; } }
    [SerializeField]Vector2Int targetPos; //적이 도착해야할 최종 위치
    public Vector2Int TargetPos { get { return targetPos; } }
    [SerializeField]Vector2Int size;// 맵 최대 길이
    public Vector2Int Size { get { return size; } }
    //public bool allowDiagonal, dontCrossCorner;
    Node[,] nodeArray;// 이동할 구역
    public Node[,] NodeArray { get { return nodeArray; } }
    bool[,] wallPos;//사용자 벽 정보를 저장하기 위해 사용
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
        }
        nodeArray = new Node[size.x, size.y];
        wallPos = new bool[size.x, size.y];
    }
    private void Start()
    {
       
        Wallcheck();
    }
    private void Update()
    {
    }
    public void Wallcheck()//몬스터 배치하기전 벽체크
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                wallPos[x, y] = false;
                nodeArray[x, y] = new Node(false, x, y);
                if (transform.CompareTag("Wall") ==Physics2D.Raycast(new Vector2(x, -y), Vector2.up, 0.2f,LayerMask.GetMask("Wall")))// 각위치에 레이캐스트를 쏴 확인
                {
                    nodeArray[x, y].isWall = true;
                    wallPos[x, y] = true;
                }
            }
        }
    }
}
