// Assets/Script/Dialogue/OverlayUI/UIBlocker.cs
using System;

namespace Game.OverlayUI
{
    public static class UIBlocker
    {
        private static int depth = 0;
        public static bool IsBlocked => depth > 0;

        public static void Push() { depth++; }
        public static void Pop() { depth = Math.Max(0, depth - 1); }
        public static void Clear() { depth = 0; }
    }
}
