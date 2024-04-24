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
                if (OpenList[i].F <= CurNode.F && OpenList[i].H < CurNode.H && OpenList[i].G < CurNode.G)//�����ִ������� ���������� F,H,G�� ���峷�� ������ ���� ����
                {
                    CurNode = OpenList[i];
                }
            }
            OpenList.Remove(CurNode);//Ž���� ���� ���� ���¸���Ʈ���� ����
            CloseList.Add(CurNode); // Ž���� ���� ��带 ��������Ʈ�� ����
            if (CurNode == TargetNode) //�����尡 �������϶�
            {
                Node TatgetCurNode = TargetNode;
                while (TatgetCurNode != StartNode)//Ÿ�ٳ�尡 ��ŸƮ���� ������������
                {
                    FinalNodeList.Add(TatgetCurNode);//Ÿ�� ��� �������� ������Ʈ�� �ְ�
                    TatgetCurNode = TatgetCurNode.ParentNode;// Ÿ�ٳ�带 ���Ե� ������带 Ÿ�ٳ��� ���� -> ���������� �ݺ�
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
        if (checkX >= 0 && checkX <AsrarAlgo.instance.Size.x && checkY >= 0 && checkY < AsrarAlgo.instance.Size.y && AsrarAlgo.instance.NodeArray[checkX, checkY].isWall && !CloseList.Contains(AsrarAlgo.instance.NodeArray[checkX, checkY]))
        {//����+ �� üũ
            Node NeighborNode = AsrarAlgo.instance.NodeArray[checkX, checkY];//�����忡 �̿�����
            int MoveCost = CurNode.G + (CurNode.x - checkX == 0 || CurNode.y - checkY == 0 ? 10 : 14);// �밢�� üũ(��������Ʈ���� �ʿ����)
            if (MoveCost < NeighborNode.G || !OpenList.Contains(NeighborNode))//�̿���尡 ���¸���Ʈ�� ������ 
            {
                NeighborNode.G = MoveCost;// ������+�̵�����ġ�� �ְ�
                NeighborNode.H = (Mathf.Abs(NeighborNode.x - TargetNode.x) + Mathf.Abs(NeighborNode.y - TargetNode.y)) * 10;//Ÿ�ٳ����� ���±��� ��� 
                NeighborNode.ParentNode = CurNode;// �� �̿������ �θ��带 ������ �ٲ�
                OpenList.Add(NeighborNode);//�����ִ� ��忡 �߰�
            }
        }
    }
}
