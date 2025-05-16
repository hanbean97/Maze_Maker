using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFind : MonoBehaviour
{
    protected List<Node> FinalNodeList;//������Ʈ
    Node StartNode, TargetNode, CurNode;//Ž��������, Ž�� ������ , Ž������ ���
    List<Node> OpenList;// �����ִ� ��� 
    List<Node> CloseList;// Ž���� ���� ���
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
                //�����ִ������� ���������� F,H,G�� ���峷�� ������ ���� ����
                if (OpenList[i].F <= CurNode.F && OpenList[i].H < CurNode.H && OpenList[i].G < CurNode.G)
                {
                    CurNode = OpenList[i];
                }
            }
            OpenList.Remove(CurNode);//Ž���� ���� ���� ���¸���Ʈ���� ����
            CloseList.Add(CurNode); // Ž���� ���� ��带 ��������Ʈ�� ����
            if (CurNode == TargetNode) //�����尡 �������϶�
            {
                Node TatgetCurNode = TargetNode;
                //Ÿ�ٳ�尡 ��ŸƮ���� ������������
                while (TatgetCurNode != StartNode)
                {
                    //Ÿ�� ��� �������� ������Ʈ�� �ְ�
                    FinalNodeList.Add(TatgetCurNode);
                    // Ÿ�ٳ�带 ���Ե� ������带 Ÿ�ٳ��� ���� -> ���������� �ݺ�
                    TatgetCurNode = TatgetCurNode.ParentNode;
                }
                FinalNodeList.Add(StartNode);//������ ������������ �ְ�
                FinalNodeList.Reverse();// ������ ���� ������������ �������
                return true;// ��ã�� ���� 
            }
            OpenListAdd(CurNode.x, CurNode.y + 1);//�������(������ġ Ž��)
            OpenListAdd(CurNode.x + 1, CurNode.y);
            OpenListAdd(CurNode.x, CurNode.y - 1);
            OpenListAdd(CurNode.x - 1, CurNode.y);
        }
       return false;

    }
    private void OpenListAdd(int checkX, int checkY)
    {
        //����+ �� üũ
        if (checkX >= 0 && checkX <AsrarAlgo.instance.Size.x && checkY >= 0 && checkY 
            < AsrarAlgo.instance.Size.y && AsrarAlgo.instance.NodeArray[checkX, checkY].isWall 
            && !CloseList.Contains(AsrarAlgo.instance.NodeArray[checkX, checkY]))
        {
            //�����忡 �̿�����
            Node NeighborNode = AsrarAlgo.instance.NodeArray[checkX, checkY];
            // �밢�� üũ(��������Ʈ���� �ʿ����)
            int MoveCost = CurNode.G + (CurNode.x - checkX == 0 || CurNode.y - checkY == 0 ? 10 : 14);
            //�̿���尡 ���¸���Ʈ�� ������ 
            if (MoveCost < NeighborNode.G || !OpenList.Contains(NeighborNode))
            {
                // ������+�̵�����ġ�� �ְ�
                NeighborNode.G = MoveCost;
                //Ÿ�ٳ����� ���±��� ��� 
                NeighborNode.H = (Mathf.Abs(NeighborNode.x - TargetNode.x) + Mathf.Abs(NeighborNode.y - TargetNode.y)) * 10;
                // �� �̿������ �θ��带 ������ �ٲ�
                NeighborNode.ParentNode = CurNode;
                //�����ִ� ��忡 �߰�
                OpenList.Add(NeighborNode);
            }
        }
    }
}
