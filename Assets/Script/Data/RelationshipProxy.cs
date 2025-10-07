using System.Collections.Generic;
using UnityEngine;
using VN.SaveSystem;

/// <summary>
/// NPC 간 관계도(그래프)를 사이드카로 직렬화/역직렬화하기 위한 어댑터.
/// - 유향/무향은 프로젝트 규칙에 맞춰 쓰세요(무향이면 A-B 한 쌍만 유지).
/// - 값(value)은 친밀/적대 등 정수로 관리(원하면 float로 바꿔도 OK).
/// </summary>
public class RelationshipProxy : MonoBehaviour
{
    [System.Serializable]
    public struct Edge
    {
        public string idA;
        public string idB;
        public int value;
    }

    [Header("Edges (A ↔ B)")]
    public List<Edge> edges = new();

    public void PullFromGame()
    {
        // TODO: 실제 관계도 시스템에서 읽어 edges 채우기
        // ex) edges = graph.GetAllEdges();
    }

    public void PushToGame()
    {
        // TODO: 실제 관계도 시스템에 edges 반영
        // ex) graph.Clear(); foreach(e in edges) graph.Set(e.idA, e.idB, e.value);
    }

    public void ExportTo(List<RelationshipEntry> outList)
    {
        outList.Clear();
        foreach (var e in edges)
        {
            if (!string.IsNullOrEmpty(e.idA) && !string.IsNullOrEmpty(e.idB))
                outList.Add(new RelationshipEntry { idA = e.idA, idB = e.idB, value = e.value });
        }
    }

    public void ImportFrom(List<RelationshipEntry> inList)
    {
        edges.Clear();
        if (inList == null) return;
        foreach (var r in inList)
        {
            if (!string.IsNullOrEmpty(r.idA) && !string.IsNullOrEmpty(r.idB))
                edges.Add(new Edge { idA = r.idA, idB = r.idB, value = r.value });
        }
    }
}
