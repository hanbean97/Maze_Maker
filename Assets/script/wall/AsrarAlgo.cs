using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node
{
    public bool isWall;
    public Node ParentNode;

    public int x, y;// ��ġ
    public int G, H;
    public int F { get { return G + H; } }
    public Vector3Int nodePosition { get { return new Vector3Int(x, -y,0); } }
    public Node(bool _isWall, int _x, int _y) 
    {
        isWall = _isWall; x = _x; y = _y; 
    }
}

public class AsrarAlgo : MonoBehaviour
{
    public static AsrarAlgo instance;

    [Header("Node���� ")]
    [SerializeField]Vector2Int startPos;
    public Vector2Int StartPos { get { return startPos; } }
    [SerializeField]Vector2Int targetPos;
    public Vector2Int TargetPos { get { return targetPos; } }
    [SerializeField]Vector2Int size;
    public Vector2Int Size { get { return size; } }
    //public bool allowDiagonal, dontCrossCorner;
    Node[,] nodeArray;// �̵��� ���� ũ��
    public Node[,] NodeArray { get { return nodeArray; } }
    bool[,] wallPos;//����� �� ����
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
    public void Wallcheck()//���� ��ġ�ϱ��� ��üũ
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                wallPos[x, y] = false;
                nodeArray[x, y] = new Node(false, x, y);
                if (transform.CompareTag("Wall") ==Physics2D.Raycast(new Vector2(x, -y), Vector2.up, 0.2f,LayerMask.GetMask("Wall")))
                {
                    nodeArray[x, y].isWall = true;
                    wallPos[x, y] = true;
                }
            }
        }
    }
}
