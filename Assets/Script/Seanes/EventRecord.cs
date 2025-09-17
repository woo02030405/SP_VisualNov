using UnityEngine;

namespace VN.Data
{
    [System.Serializable]
    public class EventRecord
    {
        public string id;               // ���� ID (��: EV_CH1_01)
        public string title;            // ǥ�� ����
        [TextArea] public string desc;  // ����
        public Sprite thumbnail;        // �����
        public bool unlocked;           // ���/����
        public string replayScene;      // ����� �� �̸�(�������� �����)
    }
}
