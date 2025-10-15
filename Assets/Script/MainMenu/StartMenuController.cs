using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 메인/스타트 메뉴에서 사용하는 컨트롤러.
/// - 새 게임 시: 세션 상태만 초기화, 백로그만 삭제, 읽음 기록(스킵 근거)/세이브포인트 해금은 유지
/// - 계속하기: 게임 씬으로 진입 (저장 로드는 게임 로더가 맡는 구조라면 단순 진입)
/// - 하드 리셋: 진행 메모리까지 모두 삭제 (환경설정에서 버튼 연결 예정)
/// </summary>
public class StartMenuController : MonoBehaviour
{
    [Header("Target Scene")]
    [Tooltip("게임 플레이 씬 이름")]
    public string gameSceneName = "GameScene";

    [Header("Options")]
    [Tooltip("새 게임 시 퀵/오토 저장만 삭제 (수동 세이브는 유지)")]
    public bool deleteTemporarySavesOnNewGame = true;

    [Tooltip("세션 상태(아이템/변수/호감도 등) 초기화 시도")]
    public bool resetSessionOnNewGame = true;

    [Tooltip("환경설정 등에서 '데이터 모두 삭제(하드 리셋)' 버튼을 노출할지 여부")]
    public bool exposeHardResetButton = true;

    // --- PUBLIC: UI 버튼에 직접 연결 ---

    /// <summary>새 게임 시작</summary>
    public void OnClickNewGame()
    {
        try
        {
            if (resetSessionOnNewGame)
                TryCallStatic("GameState", "ResetAll"); // 세션 상태만 초기화(프로젝트 내 구현)

            // 읽음 기록은 유지! (ResetAll 같은 건 절대 호출 금지)
            // 백로그만 정리
            TryCallSingleton("BacklogManager", "Clear"); // 존재하면 호출

            if (deleteTemporarySavesOnNewGame)
                DeleteTemporarySaves();

            // 필요 시, 시작용 세이브/플래그 초기화 로직 추가 가능
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[StartMenu] NewGame init warning: {e.Message}");
        }

        // 게임 씬으로 진입
        SafeLoadGameScene();
    }

    /// <summary>계속하기</summary>
    public void OnClickContinue()
    {
        // 프로젝트마다 '자동으로 마지막 세이브를 로드' 구조가 다르므로
        // 여기서는 일단 게임 씬으로만 진입시키고,
        // 실제 로드는 게임 씬의 로더가 담당하도록 위임하는 방식이 가장 안전함.
        SafeLoadGameScene();
    }

    /// <summary>환경설정에 연결할 '데이터 모두 삭제(하드 리셋)' 버튼</summary>
    public void OnClickHardResetAllData()
    {
        if (!exposeHardResetButton)
        {
            Debug.Log("[StartMenu] Hard reset button is hidden.");
            return;
        }

        try
        {
            // 1) 읽음/스킵/백로그까지 전부 삭제 (있으면 호출)
            TryCallSingleton("ReadSkipBacklogManager", "ResetAll");
            TryCallSingleton("BacklogManager", "Clear");

            // 2) 세이브포인트 해금 레지스트리 삭제
            // SavePointRegistry.Ensure().HardReset() 호출 (반사로 안전 처리)
            TryCallStatic("SavePointRegistry", "Ensure"); // Ensure로 인스턴스 확보
            TryCallSingleton("SavePointRegistry", "HardReset");

            // 3) 파일 시스템 세이브도 통째로 삭제(옵션)
            DeleteAllSaves();

            Debug.Log("[StartMenu] All data hard-reset completed.");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[StartMenu] Hard reset warning: {e.Message}");
        }
    }

    // --- 내부 유틸 ---

    private void SafeLoadGameScene()
    {
        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            Debug.LogError("[StartMenu] gameSceneName is empty.");
            return;
        }
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// 프로젝트에서 사용하는 저장 파일 네이밍에 맞춰 임시(퀵/오토)만 삭제.
    /// 필요하면 패턴을 아래에서 조정.
    /// </summary>
    private void DeleteTemporarySaves()
    {
        try
        {
            var root = Application.persistentDataPath;
            if (!Directory.Exists(root)) return;

            // 예시 패턴: quick_xxx.*, auto_xxx.*, *_qs.*
            // 필요하면 추가 패턴을 배열에 확장
            string[] keywords = { "quick", "auto", "_qs" };

            foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.TopDirectoryOnly))
            {
                var name = Path.GetFileName(path).ToLowerInvariant();
                if (keywords.Any(k => name.Contains(k)))
                {
                    File.Delete(path);
                    // Debug.Log($"[StartMenu] Delete temp save: {name}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[StartMenu] DeleteTemporarySaves warning: {e.Message}");
        }
    }

    /// <summary>
    /// 모든 세이브 파일을 삭제합니다. (하드 리셋 전용)
    /// 프로젝트 저장 규칙에 맞춰 확장자/접두어를 조정하세요.
    /// </summary>
    private void DeleteAllSaves()
    {
        try
        {
            var root = Application.persistentDataPath;
            if (!Directory.Exists(root)) return;

            // 예: .sav / .json / .bin / .dat 등 사용 확장자에 맞춰 필터
            string[] exts = { ".sav", ".save", ".json", ".bin", ".dat" };

            foreach (var p in Directory.EnumerateFiles(root, "*", SearchOption.TopDirectoryOnly))
            {
                var ext = Path.GetExtension(p).ToLowerInvariant();
                if (exts.Contains(ext))
                {
                    File.Delete(p);
                    // Debug.Log($"[StartMenu] Delete save: {Path.GetFileName(p)}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[StartMenu] DeleteAllSaves warning: {e.Message}");
        }
    }

    // --- Reflection helpers: 외부 싱글톤/정적 API 안전 호출 ---

    /// <summary>
    /// 타입명으로 싱글톤(Instance/static Instance/Ensure() 등) 찾아서 메서드 호출.
    /// </summary>
    private void TryCallSingleton(string typeName, string methodName)
    {
        try
        {
            var t = FindType(typeName);
            if (t == null) return;

            // 1) public static Instance {get;}
            var instProp = t.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            object inst = instProp != null ? instProp.GetValue(null) : null;

            // 2) public static Ensure(): 인스턴스 반환
            if (inst == null)
            {
                var ensure = t.GetMethod("Ensure", BindingFlags.Public | BindingFlags.Static);
                if (ensure != null) inst = ensure.Invoke(null, null);
            }

            // 3) 씬 안 오브젝트 FindObjectOfType (최후 fallback)
            if (inst == null && typeof(UnityEngine.Object).IsAssignableFrom(t))
            {
                var found = GameObject.FindObjectOfType(t);
                if (found != null) inst = found;
            }

            if (inst == null) return;

            var m = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            if (m != null) m.Invoke(inst, null);
        }
        catch { /* ignore */ }
    }

    /// <summary>
    /// 정적 메서드 호출 (예: GameState.ResetAll)
    /// </summary>
    private void TryCallStatic(string typeName, string methodName)
    {
        try
        {
            var t = FindType(typeName);
            if (t == null) return;

            var m = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            if (m != null) m.Invoke(null, null);
        }
        catch { /* ignore */ }
    }

    /// <summary>
    /// 현재 AppDomain에서 타입 탐색 (네임스페이스가 다르면 전체이름으로 넣어도 됨)
    /// </summary>
    private static Type FindType(string typeName)
    {
        // 먼저 정확히
        var t = Type.GetType(typeName);
        if (t != null) return t;

        // 어셈블리 전체 탐색
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                t = asm.GetType(typeName);
                if (t != null) return t;

                // 같은 이름의 타입을 네임스페이스 포함 검색
                t = asm.GetTypes().FirstOrDefault(x => x.Name == typeName);
                if (t != null) return t;
            }
            catch { /* ignore dynamic assemblies */ }
        }
        return null;
    }
}
