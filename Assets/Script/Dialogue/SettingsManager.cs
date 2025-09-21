public static class SettingsManager
{
    public static float TextSpeed { get; set; } = 0.05f; // 글자 간격
    public static bool AutoModeEnabled { get; set; } = false;
    public static float AutoDelay { get; set; } = 2.5f;  // Auto 진행 간격
    public static bool SkipMode { get; set; } = false;   // 스킵 모드
}
