using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFind : MonoBehaviour
{
    protected List<Node> FinalNodeList;//최종루트
    Node StartNode, TargetNode, CurNode;//탐색시작점, 탐색 목적지 , 탐색중인 노드
    List<Node> OpenList;// 갈수있는 노드 
    List<Node> CloseList;// 탐색이 끝난 노드
    public bool PathFinding(Vector2Int nowPos,Vector2Int targetPos)
    {
        StartNode =AsrarAlgo.instance.NodeArray[nowPos.x, nowPos.y];
        TargetNode = AsrarAlgo.instance.NodeArray[targetPos.x, targetPos.y];
        OpenList = new List<Node>() { StartNode };
        CloseList = new List<Node>();
        FinalNodeList = new List<Node>();
        while (OpenList.Count > 0)// openList없다 = 갈곳이 아예없다
        {
            CurNode = OpenList[0];//노드 시작점
            for (int i = 1; i < OpenList.Count; i++)
            {
                if (OpenList[i].F <= CurNode.F && OpenList[i].H < CurNode.H && OpenList[i].G < CurNode.G)//갈수있는지점중 현지점에서 F,H,G가 가장낮은 지점을 먼저 선택
                {
                    CurNode = OpenList[i];
                }
            }
            OpenList.Remove(CurNode);//탐색이 끝난 노드는 오픈리스트에서 삭제
            CloseList.Add(CurNode); // 탐색이 끝난 노드를 닫힌리스트에 넣음
            if (CurNode == TargetNode) //현재노드가 도착지일때
            {
                Node TatgetCurNode = TargetNode;
                while (TatgetCurNode != StartNode)//타겟노드가 스타트노드와 같아질때까지
                {
                    FinalNodeList.Add(TatgetCurNode);//타겟 노드 지점부터 최종루트에 넣고
                    TatgetCurNode = TatgetCurNode.ParentNode;// 타겟노드를 오게된 이전노드를 타겟노드로 변경 -> 시작점까지 반복
                }
                FinalNodeList.Add(StartNode);//마지막 시작지점까지 넣고
                FinalNodeList.Reverse();// 리버스 시켜 시작지점부터 순서대로
                return true;// 길찾기 성공
            }
            OpenListAdd(CurNode.x, CurNode.y + 1);//현노드의(다음위치 탐색)
            OpenListAdd(CurNode.x + 1, CurNode.y);
            OpenListAdd(CurNode.x, CurNode.y - 1);
            OpenListAdd(CurNode.x - 1, CurNode.y);
        }
       return false;

    }
    private void OpenListAdd(int checkX, int checkY)
    {
        if (checkX >= 0 && checkX <AsrarAlgo.instance.Size.x && checkY >= 0 && checkY < AsrarAlgo.instance.Size.y && AsrarAlgo.instance.NodeArray[checkX, checkY].isWall && !CloseList.Contains(AsrarAlgo.instance.NodeArray[checkX, checkY]))
        {//길이+ 벽 체크
            Node NeighborNode = AsrarAlgo.instance.NodeArray[checkX, checkY];//현재노드에 이웃노드들
            int MoveCost = CurNode.G + (CurNode.x - checkX == 0 || CurNode.y - checkY == 0 ? 10 : 14);// 대각선 체크(현프로젝트에선 필요없음)
            if (MoveCost < NeighborNode.G || !OpenList.Contains(NeighborNode))//이웃노드가 오픈리스트에 없으면 
            {
                NeighborNode.G = MoveCost;// 현재노드+이동가중치를 넣고
                NeighborNode.H = (Mathf.Abs(NeighborNode.x - TargetNode.x) + Mathf.Abs(NeighborNode.y - TargetNode.y)) * 10;//타겟노드까지 가는길이 계산 
                NeighborNode.ParentNode = CurNode;// 이 이웃노드의 부모노드를 현노드로 바꿈
                OpenList.Add(NeighborNode);//갈수있는 노드에 추가
            }
        }
    }
}
