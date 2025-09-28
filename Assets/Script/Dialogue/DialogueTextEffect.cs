using System;
using System.Text.RegularExpressions;

public static class DialogueTextEffect
{
    // 예: "color=#66CCFF;shake;highlight:유나;speed=1.5;wiggle"
    public static string Apply(string text, string effectList)
    {
        if (string.IsNullOrEmpty(effectList)) return text;

        string result = text;
        var effects = effectList.Split(';');

        foreach (var raw in effects)
        {
            var e = raw.Trim();
            if (string.IsNullOrEmpty(e)) continue;

            if (e.StartsWith("color=", StringComparison.OrdinalIgnoreCase))
            {
                var color = e.Substring("color=".Length);
                result = $"<color={color}>{result}</color>";
                continue;
            }

            if (e.StartsWith("highlight:", StringComparison.OrdinalIgnoreCase))
            {
                var target = e.Substring("highlight:".Length);
                if (!string.IsNullOrEmpty(target))
                {
                    result = Regex.Replace(result,
                        Regex.Escape(target),
                        m => $"<color=#FF0000>{m.Value}</color>");
                }
                continue;
            }

            if (e.StartsWith("speed=", StringComparison.OrdinalIgnoreCase))
            {
                var speed = e.Substring("speed=".Length);
                result = $"<speed={speed}>{result}</speed>";
                continue;
            }

            if (e.Equals("shake", StringComparison.OrdinalIgnoreCase))
            {
                result = $"<shake>{result}</shake>";
                continue;
            }

            // 두투윈 Pro 같은 커스텀 태그 통과 적용
            if (Regex.IsMatch(e, @"^[a-zA-Z][a-zA-Z0-9_-]*$"))
            {
                result = $"<{e}>{result}</{e}>";
            }
        }
        return result;
    }
}
