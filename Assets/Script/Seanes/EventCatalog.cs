using System.Collections.Generic;
using UnityEngine;

namespace VN.Data
{
    [CreateAssetMenu(menuName = "VN/Catalog/EventCatalog")]
    public class EventCatalog : ScriptableObject
    {
        public List<EventRecord> events = new List<EventRecord>();
    }
}
