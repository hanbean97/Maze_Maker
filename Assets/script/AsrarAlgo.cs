using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class Node
{
    public bool isWall;
    public Node ParentNode;
    
    public int x, y;
    public int G, H;
    public int F { get { return G + H; } }
    public Node(bool _isWall, int _x, int _y) 
    {
        isWall = _isWall; x = _x; y = _y; 
    }
}

public class AsrarAlgo : MonoBehaviour
{
    public Vector2Int startPos;
    public Vector2Int targetPos;

    public List<Node> FinalNodeList;
    //public bool allowDiagonal, dontCrossCorner;

    Node[,] NodeArray;// 이동할 구역 크기
    Node StartNode, TargetNode, CurNode;
    List<Node> OpenList;
    List<Node> CloseList;

    [SerializeField]Tilemap tile;
    [SerializeField] TilemapCollider2D walltile;
    Grid grid;
   

    private void Start()
    {
        BoundsInt bounds = tile.cellBounds;
        TileBase[] allTiles = tile.GetTilesBlock(bounds);
        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if (tile != null)
                {
                    Debug.Log("x:" + x + " y:" + y + " tile:" + tile.name);
                }
                else
                {
                    Debug.Log("x:" + x + " y:" + y + " tile: (null)");
                }
            }
        }
        //Debug.Log(tile.GetTilesBlock(new BoundsInt ));
        //NodeArray[tile.size.x, tile.size.y];
    }
    public void PathFinding()
    {
        //StartNode = NodeArray[,];
       
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
