using System;
using System.Collections.Generic;
using UnityEngine;

public static class ConditionParser
{
    public static bool Evaluate(string condition, SaveData saveData)        // 조건 평가 (true/false 반환)
    {
        if (string.IsNullOrEmpty(condition) || condition == "-")
            return true; // 조건 없음 → 항상 true

        try
        {
            var tokens = Tokenize(condition);
            var rpn = ToRPN(tokens);
            return EvalRPN(rpn, saveData);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ConditionParser] Error evaluating condition '{condition}': {ex}");
            return false;
        }
    }

    // 1) Tokenize
    private static List<string> Tokenize(string expr)               // 토큰 분리
    {
        List<string> tokens = new List<string>();
        string buffer = "";

        for (int i = 0; i < expr.Length; i++)
        {
            char c = expr[i];

            if ("()".IndexOf(c) >= 0)
            {
                if (buffer.Length > 0) { tokens.Add(buffer); buffer = ""; }
                tokens.Add(c.ToString());
            }
            else if (c == '&' && i + 1 < expr.Length && expr[i + 1] == '&')
            {
                if (buffer.Length > 0) { tokens.Add(buffer); buffer = ""; }
                tokens.Add("&&"); i++;
            }
            else if (c == '|' && i + 1 < expr.Length && expr[i + 1] == '|')
            {
                if (buffer.Length > 0) { tokens.Add(buffer); buffer = ""; }
                tokens.Add("||"); i++;
            }
            else if ("=!<>".IndexOf(c) >= 0)
            {
                if (buffer.Length > 0) { tokens.Add(buffer); buffer = ""; }
                string op = c.ToString();
                if (i + 1 < expr.Length && "=<>".IndexOf(expr[i + 1]) >= 0)
                {
                    op += expr[i + 1]; i++;
                }
                tokens.Add(op);
            }
            else if (char.IsWhiteSpace(c))
            {
                if (buffer.Length > 0) { tokens.Add(buffer); buffer = ""; }
            }
            else
            {
                buffer += c;
            }
        }
        if (buffer.Length > 0) tokens.Add(buffer);
        return tokens;
    }

    // 2) Shunting-yard → RPN
    private static int Precedence(string op) => op switch                       // 연산자 우선순위
    {
        "!" => 3,
        "==" or "!=" or ">=" or "<=" or ">" or "<" => 2,
        "&&" => 1,
        "||" => 0,
        _ => -1
    };

    private static bool IsOperator(string t) =>
        t is "!" or "==" or "!=" or ">=" or "<=" or ">" or "<" or "&&" or "||";

    private static List<string> ToRPN(List<string> tokens)
    {
        List<string> output = new();
        Stack<string> stack = new();

        foreach (var token in tokens)
        {
            if (IsOperator(token))
            {
                while (stack.Count > 0 && IsOperator(stack.Peek()) &&
                       Precedence(stack.Peek()) >= Precedence(token))
                    output.Add(stack.Pop());

                stack.Push(token);
            }
            else if (token == "(") stack.Push(token);
            else if (token == ")")
            {
                while (stack.Count > 0 && stack.Peek() != "(")
                    output.Add(stack.Pop());
                if (stack.Count > 0) stack.Pop();
            }
            else output.Add(token);
        }

        while (stack.Count > 0) output.Add(stack.Pop());
        return output;
    }

    // 3) RPN 평가
    private static bool EvalRPN(List<string> rpn, SaveData saveData)                // RPN 평가
    {
        Stack<object> stack = new();

        foreach (var token in rpn)
        {
            if (IsOperator(token))
            {
                if (token == "!")
                {
                    bool a = ToBool(stack.Pop());
                    stack.Push(!a);
                }
                else
                {
                    object b = stack.Pop();
                    object a = stack.Pop();
                    stack.Push(EvalOp(a, b, token));
                }
            }
            else
            {
                stack.Push(EvalValue(token, saveData));
            }
        }

        return stack.Count > 0 && ToBool(stack.Pop());
    }

    private static bool ToBool(object o)                        // 객체를 bool로 변환
    {
        if (o is bool bo) return bo;
        if (o is int ii) return ii != 0;
        if (o is string s && bool.TryParse(s, out bool bb)) return bb;
        if (o is string si && int.TryParse(si, out int bi)) return bi != 0;
        return false;
    }

    private static object EvalOp(object a, object b, string op)     // 이항 연산 평가
    {
        // 산술/비교는 숫자로, 논리연산은 bool로
        bool ab = ToBool(a);
        bool bb = ToBool(b);

        switch (op)
        {
            case "&&": return ab && bb;
            case "||": return ab || bb;
        }

        // 비교 연산은 int 기준
        int ai = (a is int ai0) ? ai0 : (ToBool(a) ? 1 : 0);
        int bi = (b is int bi0) ? bi0 : (ToBool(b) ? 1 : 0);

        return op switch
        {
            "==" => ai == bi,
            "!=" => ai != bi,
            ">=" => ai >= bi,
            "<=" => ai <= bi,
            ">" => ai > bi,
            "<" => ai < bi,
            _ => false
        };
    }

    // 4) 값 해석
    private static object EvalValue(string token, SaveData saveData)            // 토큰을 값으로 해석
    {
        if (int.TryParse(token, out int num)) return num;

        // 불리언 리터럴 지원
        if (bool.TryParse(token, out bool bl)) return bl;

        if (token.StartsWith("affinity:"))
            return saveData.GetAffinity(token.Substring("affinity:".Length));

        if (token.StartsWith("relation:"))
            return saveData.GetRelation(token.Substring("relation:".Length));

        if (token.StartsWith("stat:"))
            return saveData.GetStat(token.Substring("stat:".Length));

        if (token.StartsWith("item:"))
            return saveData.GetItem(token.Substring("item:".Length));

        if (token.StartsWith("gold:"))
            return saveData.GetGold();

        if (token.StartsWith("flag:"))
            return saveData.GetFlag(token.Substring("flag:".Length)) ? 1 : 0;

        if (token.StartsWith("rand("))
        {
            // rand(70) → 70% 확률 true
            int open = token.IndexOf('(');
            int close = token.IndexOf(')');
            if (open >= 0 && close > open)
            {
                string numStr = token.Substring(open + 1, close - open - 1);
                if (int.TryParse(numStr, out int chance))
                    return UnityEngine.Random.Range(0, 100) < chance ? 1 : 0;
            }
            return 0;
        }

        // 알 수 없는 토큰 → false
        return 0;
    }
}
