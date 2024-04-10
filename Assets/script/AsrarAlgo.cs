using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

[System.Serializable]
public class Node
{
    public bool isWall;
    public Node ParentNode;

    public int x, y;// ��ġ
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
    [Header("Node���� ��������2�� �Ͻ� ���������� �Է�")]
    public Vector2Int startPos;
    public Vector2Int targetPos;
    [SerializeField] Vector2Int size;
    public List<Node> FinalNodeList;
    //public bool allowDiagonal, dontCrossCorner;
    Node[,] NodeArray;// �̵��� ���� ũ��
    Node StartNode, TargetNode, CurNode;
    List<Node> OpenList;
    List<Node> CloseList;
    bool[,] wallPos;//����� �� ����

    [SerializeField] Button wallcheck;
    
    private void Start()
    {
        NodeArray = new Node[size.x, size.y];
        wallPos = new bool[size.x, size.y];
        wallcheck.onClick.AddListener(Wallcheck);
        wallcheck.onClick.AddListener(PathFinding);
    }
    private void Update()
    {
    }
    public void Wallcheck()//���� ��ġ�ϱ��� ��üũ
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
    void FinishWallmake()// wall��尡 ������ üũ
    {
        Wallcheck();
    }
    public void PathFinding()
    {
        StartNode = NodeArray[startPos.x,startPos.y];
        TargetNode = NodeArray[targetPos.x, targetPos.y];
        OpenList = new List<Node>() { StartNode};
        CloseList = new List<Node>();
        FinalNodeList = new List<Node>();
        while (OpenList.Count>0)// openList���� = ������ �ƿ�����
        {
            CurNode = OpenList[0];//��� ������
            for(int i =1; i < OpenList.Count; i ++)
            {
                if (OpenList[i].F <= CurNode.F && OpenList[i].H < CurNode.H && OpenList[i].G < CurNode.G)
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
               return;
            }
            OpenListAdd(CurNode.x, CurNode.y + 1);
            OpenListAdd(CurNode.x + 1, CurNode.y);
            OpenListAdd(CurNode.x, CurNode.y - 1);
            OpenListAdd(CurNode.x - 1, CurNode.y);
        }
    }
    void OpenListAdd(int checkX, int checkY)
    {
        if(checkX >= 0 && checkX<size.x &&checkY >= 0 && checkY <size.y && NodeArray[checkX,checkY].isWall && !CloseList.Contains(NodeArray[checkX,checkY]))
        {
            Node NeighborNode = NodeArray[checkX,checkY];   
            int MoveCost = CurNode.G + (CurNode.x - checkX == 0 || CurNode.y - checkY == 0 ? 10 : 14);
            if(MoveCost<NeighborNode.G || !OpenList.Contains(NeighborNode))
            {
                NeighborNode.G = CurNode.G;
                NeighborNode.H = (Mathf.Abs(NeighborNode.x - TargetNode.x) + Mathf.Abs(NeighborNode.y - TargetNode.y)) * 10;//�̿������ ���±� ���
                 NeighborNode.ParentNode = CurNode;
                OpenList.Add(NeighborNode);
            }
        }
    }
    private void OnDrawGizmos()
    {
            if(FinalNodeList.Count != 0)
        {
            for(int i =0; i < FinalNodeList.Count-1; i++) 
            {
                Gizmos.DrawLine( (Vector2)FinalNodeList[i].nodePosition, (Vector2)FinalNodeList[i+1].nodePosition);
            }
        }
    }
}
