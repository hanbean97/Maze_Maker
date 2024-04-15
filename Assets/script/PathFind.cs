using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFind : MonoBehaviour
{
    protected List<Node> FinalNodeList;
    Node StartNode, TargetNode, CurNode;
    List<Node> OpenList;
    List<Node> CloseList;
    public bool PathFinding(Vector2Int nowPos,Vector2Int targetPos)
    {
        StartNode =AsrarAlgo.instance.NodeArray[nowPos.x, nowPos.y];
        TargetNode = AsrarAlgo.instance.NodeArray[targetPos.x, targetPos.y];
        OpenList = new List<Node>() { StartNode };
        CloseList = new List<Node>();
        FinalNodeList = new List<Node>();
        while (OpenList.Count > 0)// openList���� = ������ �ƿ�����
        {
            CurNode = OpenList[0];//��� ������
            for (int i = 1; i < OpenList.Count; i++)
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
                return true;
            }
            OpenListAdd(CurNode.x, CurNode.y + 1);
            OpenListAdd(CurNode.x + 1, CurNode.y);
            OpenListAdd(CurNode.x, CurNode.y - 1);
            OpenListAdd(CurNode.x - 1, CurNode.y);
        }
       return false;

    }
    private void OpenListAdd(int checkX, int checkY)
    {
        if (checkX >= 0 && checkX <AsrarAlgo.instance.size.x && checkY >= 0 && checkY < AsrarAlgo.instance.size.y && AsrarAlgo.instance.NodeArray[checkX, checkY].isWall && !CloseList.Contains(AsrarAlgo.instance.NodeArray[checkX, checkY]))
        {
            Node NeighborNode = AsrarAlgo.instance.NodeArray[checkX, checkY];
            int MoveCost = CurNode.G + (CurNode.x - checkX == 0 || CurNode.y - checkY == 0 ? 10 : 14);
            if (MoveCost < NeighborNode.G || !OpenList.Contains(NeighborNode))//�̺κ� ����
            {
                NeighborNode.G = CurNode.G;
                NeighborNode.H = (Mathf.Abs(NeighborNode.x - TargetNode.x) + Mathf.Abs(NeighborNode.y - TargetNode.y)) * 10;//�̿������ ���±� ���
                NeighborNode.ParentNode = CurNode;
                OpenList.Add(NeighborNode);
            }
        }
    }
}
