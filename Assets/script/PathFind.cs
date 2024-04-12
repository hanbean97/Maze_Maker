using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFind : MonoBehaviour
{
    AsrarAlgo Astar = AsrarAlgo.instance;
    List<Node> FinalNodeList;
    Node StartNode, TargetNode, CurNode;
    List<Node> OpenList;
    List<Node> CloseList;
   
    public void PathFinding(int targetx, int targety,int nowx,int nowy)
    {
        StartNode =Astar.NodeArray[nowx, nowy];
        TargetNode = Astar.NodeArray[targetx, targety];
        OpenList = new List<Node>() { StartNode };
        CloseList = new List<Node>();
        FinalNodeList = new List<Node>();
        while (OpenList.Count > 0)// openList없다 = 갈곳이 아예없다
        {
            CurNode = OpenList[0];//노드 시작점
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
                return;
            }
            OpenListAdd(CurNode.x, CurNode.y + 1);
            OpenListAdd(CurNode.x + 1, CurNode.y);
            OpenListAdd(CurNode.x, CurNode.y - 1);
            OpenListAdd(CurNode.x - 1, CurNode.y);
        }
        if (FinalNodeList.Count == 0)
        {
            Debug.Log("길찾기 실패");
        }

    }
    private void OpenListAdd(int checkX, int checkY)
    {
        if (checkX >= 0 && checkX <Astar.size.x && checkY >= 0 && checkY < Astar.size.y && Astar.NodeArray[checkX, checkY].isWall && !CloseList.Contains(Astar.NodeArray[checkX, checkY]))
        {
            Node NeighborNode = Astar.NodeArray[checkX, checkY];
            int MoveCost = CurNode.G + (CurNode.x - checkX == 0 || CurNode.y - checkY == 0 ? 10 : 14);
            if (MoveCost < NeighborNode.G || !OpenList.Contains(NeighborNode))//이부분 질문
            {
                NeighborNode.G = CurNode.G;
                NeighborNode.H = (Mathf.Abs(NeighborNode.x - TargetNode.x) + Mathf.Abs(NeighborNode.y - TargetNode.y)) * 10;//이웃노드의 가는길 계산
                NeighborNode.ParentNode = CurNode;
                OpenList.Add(NeighborNode);
            }
        }
    }
}
