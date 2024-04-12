using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Node
{
    public bool isWall;
    public Node ParentNode;

    public int x, y;// 위치
    public int G, H;
    public int F { get { return G + H; } }
    public Vector2Int nodePosition { get { return new Vector2Int(x, -y); } }
    public Node(bool _isWall, int _x, int _y) 
    {
        isWall = _isWall; x = _x; y = _y; 
    }
}

public class AsrarAlgo : MonoBehaviour
{
    public static AsrarAlgo instance;

    [Header("Node정보 ")]
    public Vector2Int startPos;
    public Vector2Int targetPos;
    public Vector2Int size;
    //public bool allowDiagonal, dontCrossCorner;
    public Node[,] NodeArray;// 이동할 구역 크기
    bool[,] wallPos;//사용자 벽 정보
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
    }
    private void Start()
    {
        NodeArray = new Node[size.x, size.y];
        wallPos = new bool[size.x, size.y];
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
                NodeArray[x, y] = new Node(false, x, y);
                if (transform.CompareTag("Wall") ==Physics2D.Raycast(new Vector2(x, -y), Vector2.up, 0.2f,LayerMask.GetMask("Wall")))
                {
                    NodeArray[x, y].isWall = true;
                    wallPos[x, y] = true;
                }
            }
        }
    }
    
    //public void PathFinding()
    //{
    //    StartNode = NodeArray[startPos.x,startPos.y];
    //    TargetNode = NodeArray[targetPos.x, targetPos.y];
    //    OpenList = new List<Node>() { StartNode};
    //    CloseList = new List<Node>();
    //    FinalNodeList = new List<Node>();
    //    while (OpenList.Count>0)// openList없다 = 갈곳이 아예없다
    //    {
    //        CurNode = OpenList[0];//노드 시작점
    //        for(int i =1; i < OpenList.Count; i ++)
    //        {
    //            if (OpenList[i].F <= CurNode.F && OpenList[i].H < CurNode.H && OpenList[i].G < CurNode.G)
    //            {
    //                CurNode = OpenList[i];
    //            }
    //        }
    //        OpenList.Remove(CurNode);
    //        CloseList.Add(CurNode);
    //        if (CurNode == TargetNode)
    //        {
    //            Node TatgetCurNode = TargetNode;
    //            while (TatgetCurNode != StartNode)
    //            {
    //                FinalNodeList.Add(TatgetCurNode);
    //                TatgetCurNode = TatgetCurNode.ParentNode;
    //            }
    //            FinalNodeList.Add(StartNode);
    //            FinalNodeList.Reverse();
    //           return;
    //        }
    //        OpenListAdd(CurNode.x, CurNode.y + 1);
    //        OpenListAdd(CurNode.x + 1, CurNode.y);
    //        OpenListAdd(CurNode.x, CurNode.y - 1);
    //        OpenListAdd(CurNode.x - 1, CurNode.y);
    //    }
    //    if (FinalNodeList.Count == 0)
    //    {
    //        Debug.Log("길찾기 실패");
    //    }

    //}
    //private void OpenListAdd(int checkX, int checkY)
    //{
    //    if(checkX >= 0 && checkX<size.x &&checkY >= 0 && checkY <size.y && NodeArray[checkX,checkY].isWall && !CloseList.Contains(NodeArray[checkX,checkY]))
    //    {
    //        Node NeighborNode = NodeArray[checkX,checkY];   
    //        int MoveCost = CurNode.G + (CurNode.x - checkX == 0 || CurNode.y - checkY == 0 ? 10 : 14);
    //        if(MoveCost<NeighborNode.G || !OpenList.Contains(NeighborNode))//이부분 질문
    //        {
    //            NeighborNode.G = CurNode.G;
    //            NeighborNode.H = (Mathf.Abs(NeighborNode.x - TargetNode.x) + Mathf.Abs(NeighborNode.y - TargetNode.y)) * 10;//이웃노드의 가는길 계산
    //             NeighborNode.ParentNode = CurNode;
    //            OpenList.Add(NeighborNode);
    //        }
    //    }
    //}
}
