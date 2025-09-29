using System;

namespace Game.OverlayUI
{
 
    // 간단한 UI 입력 차단기.
    // 팝업(ConfirmDialog 등)이 열릴 때 Push(), 닫힐 때 Pop() 호출.
    // depth > 0 이면 뒤쪽 UI 입력은 무시된다.
   
    public static class UIBlocker
    {
        private static int depth = 0;



        public static bool IsBlocked => depth > 0;


        

        public static void Push()
        {
            depth++;
            UnityEngine.Debug.Log($"[UIBlocker] Push -> depth={depth}");        // 차단 시작 (중첩 가능)
        }

 

        public static void Pop()
        {
            depth = Math.Max(0, depth - 1);
            UnityEngine.Debug.Log($"[UIBlocker] Pop -> depth={depth}");       // 차단 해제
        }

  

        public static void Clear()
        {
            depth = 0;
            UnityEngine.Debug.Log("[UIBlocker] Clear");                       // 강제로 차단 초기화
        }
    }
}
