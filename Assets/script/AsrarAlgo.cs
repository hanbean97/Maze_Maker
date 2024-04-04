using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[System.Serializable]
public class Node
{
    public bool isWall;
    public Node ParentNode;

    public int x, y;// ��ġ
    public Vector2 nodePosition{ get { return new Vector2(x, -y); } }
    public int G, H;
    public int F { get { return G + H; } }
    public Node(bool _isWall, int _x, int _y) 
    {
        isWall = _isWall; x = _x; y = _y; 
    }
}

public class AsrarAlgo : MonoBehaviour
{
    [Header("Node����")]
    public Vector2Int startPos;
    public Vector2Int targetPos;
    [SerializeField] Vector2Int size;
    public List<Node> FinalNodeList;
    //public bool allowDiagonal, dontCrossCorner;
    Node[,] NodeArray;// �̵��� ���� ũ��
    Node StartNode, TargetNode, CurNode;
    List<Node> OpenList;
    List<Node> CloseList;

    [Header("������")]
    bool[,] wallPos;//����� �� ����
    [SerializeField]bool wallmakemode;
    [SerializeField]Tilemap walltile;
    [SerializeField]TileBase tilebase;
    TileBase emptytile;
    [SerializeField] bool wallmakedeletswitch = true;

    Grid grid;
    [SerializeField] bool dwd = false;
    private void Start()
    {
        NodeArray = new Node[size.x, size.y];
    }
    private void Update()
    {
        wallmode();
        raycheck();
    }

    void Wallcheck()//���� ��ġ�ϱ��� ��üũ
    {
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

    void wallmode()
    {
        if (wallmakemode == false) return;
        
        switch(wallmakedeletswitch)
        {
            case true :
                MakeWall();
                break;
            case false :
                DeletWall();
                break;
        }

    }

    void raycheck()
    {
        if(dwd== false) return;

        Vector2 mosPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D ray = Physics2D.Raycast(mosPos, Vector3.forward, 20);
        Debug.Log(ray.transform.name);

    }

    void MakeWall()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 mosPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D ray = Physics2D.Raycast(mosPos, Vector3.forward, 20);
            if(ray.transform.CompareTag("Ground"))
            {
                Vector3Int mousPostile = walltile.WorldToCell(mosPos);
                walltile.SetTile(mousPostile,tilebase);
            }
        }
    }
    void DeletWall()
    {

        if (Input.GetMouseButton(0))
        {
            Vector2 mosPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D ray = Physics2D.Raycast(mosPos, Vector3.forward, 20, LayerMask.GetMask("Wall"));
            if (ray && ray.transform.CompareTag("Wall"))
            {
                Vector3Int mousPostile = walltile.WorldToCell(mosPos);
                walltile.SetTile(mousPostile, null);
            }
        }
    }
    void FinishWallmake()// wall��尡 ������ üũ
    {
        Wallcheck();
    }

    public void PathFinding()
    {

        OpenList = new List<Node>() { StartNode};
        CloseList = new List<Node>();
        FinalNodeList = new List<Node>();
        while (OpenList.Count>0)// openList���� = ������ �ƿ�����
        {
            CurNode = OpenList[0];//��� ������
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
                    print(i + "��°�� " + FinalNodeList[i].x + ", " + FinalNodeList[i].y);
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
