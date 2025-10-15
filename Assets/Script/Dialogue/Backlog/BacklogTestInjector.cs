using Game.Dialogue;
using UnityEngine;

public class BacklogTestInjector: MonoBehaviour
{
    void Start()
    {
        BacklogManager.Instance?.AddLog("주인공", "첫 줄", null);
        BacklogManager.Instance?.AddLog("주인공", "둘째 줄", null);
        BacklogManager.Instance?.AddLog("시스템", "알림: 테스트", null);
    }
}
