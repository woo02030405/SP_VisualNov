using System.Collections.Generic;
using UnityEngine;

namespace VN.Data
{
    [CreateAssetMenu(menuName = "VN/Catalog/CGCatalog")]
    public class CGCatalog : ScriptableObject
    {
        public List<CGRecord> items = new List<CGRecord>();
    }
}
