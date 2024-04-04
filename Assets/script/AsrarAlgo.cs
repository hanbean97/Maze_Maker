using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[System.Serializable]
public class Node
{
    public bool isWall;
    public Node ParentNode;
    
    public int x, y;// 위치
    public int G, H;
    public int F { get { return G + H; } }
    public Node(bool _isWall, int _x, int _y) 
    {
        isWall = _isWall; x = _x; y = _y; 
    }
}

public class AsrarAlgo : MonoBehaviour
{
    [Header("Node정보")]
    public Vector2Int startPos;
    public Vector2Int targetPos;
    [SerializeField] Vector2Int size;
    public List<Node> FinalNodeList;
    //public bool allowDiagonal, dontCrossCorner;
    Node[,] NodeArray;// 이동할 구역 크기
    Node StartNode, TargetNode, CurNode;
    List<Node> OpenList;
    List<Node> CloseList;

    [Header("벽관련")]
    int[,] wallPos;//사용자 벽 정보
    [SerializeField]bool wallmakemode;
    [SerializeField]TileBase walltile;


    Grid grid;
    private void Start()
    {
      
    }
    private void Update()
    {
        MakeWall();
    }

    void Wallcheck()//몬스터 배치하기전 벽체크
    {
        NodeArray = new Node[size.x, size.y];

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                NodeArray[x, y] = new Node(false, x, -y);
                if (transform.CompareTag("Wall") ==Physics2D.Raycast(new Vector2(x, -y), Vector2.up, 0.2f))
                {
                    NodeArray[x, y].isWall = true;
                }
            }
        }
    }

    void MakeWall()
    {
        if (wallmakemode == false) return;
        //타일 넣는곳
        //  ruletile
        //  walltile
        if (Input.GetMouseButton(0))
        {
            Vector2 mosPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D ray = Physics2D.Raycast(mosPos, Vector3.forward, 20);
            if(ray.transform.CompareTag("Ground"))
            {
                //walltile.


                //if()
                //{

                //}
            }
        }
    }
    void DeletWall()
    {

    }
    

    public void PathFinding()
    {

        OpenList = new List<Node>() { StartNode};
        CloseList = new List<Node>();
        FinalNodeList = new List<Node>();
        while (OpenList.Count>0)// openList없다 = 갈곳이 아예없다
        {
            CurNode = OpenList[0];//노드 시작점
            for(int i =1; i < OpenList.Count; i ++)
            {
                if (OpenList[i].F <= CurNode.F && OpenList[i].H < CurNode.H)
                {
                    CurNode = OpenList[i];
                }
            }
            OpenList.Remove(CurNode);
            CloseList.Add(CurNode);
            if (CurNode == TargetNode)
            {
                Node TatgetCurNode = TargetNode;
                while (TatgetCurNode != StartNode)
                {
                    FinalNodeList.Add(TatgetCurNode);
                    TatgetCurNode = TatgetCurNode.ParentNode;
                }
                FinalNodeList.Add(StartNode);
                FinalNodeList.Reverse();
                int count = FinalNodeList.Count;
                for (int i = 0; i < count; i++)
                {
                    print(i + "번째는 " + FinalNodeList[i].x + ", " + FinalNodeList[i].y);
                    return;
                }
            }
            OpenListAdd(CurNode.x, CurNode.y + 1);
            OpenListAdd(CurNode.x + 1, CurNode.y);
            OpenListAdd(CurNode.x, CurNode.y - 1);
            OpenListAdd(CurNode.x - 1, CurNode.y);

        }

      
    }
    void OpenListAdd(int checkX, int checkY)
    {
        int MoveCost = CurNode.G + (CurNode.x - checkX == 0 || CurNode.y - checkY == 0 ? 10 : 14);

    }
}
