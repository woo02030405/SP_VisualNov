using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VN.Dialogue;

namespace VN.IO
{
    public static class DialogueParser
    {
        // 내부적으로 노드를 저장하는 Dictionary
        // Key = NodeId, Value = DialogueNode
        public static Dictionary<string, DialogueNode> LoadCsv(string storyCsvPath, string dialogueCsvPath)
        {
            var map = new Dictionary<string, DialogueNode>();

            // 1. Story CSV 먼저 로드
            if (File.Exists(storyCsvPath))
            {
                string[] lines = File.ReadAllLines(storyCsvPath);

                // 헤더 인덱스 확인
                var headers = CsvSplit(lines[0]);
                int sChapter = Array.IndexOf(headers, "Chapter");
                int sScene = Array.IndexOf(headers, "Scene");
                int sNodeId = Array.IndexOf(headers, "NodeId");
                int sNodeType = Array.IndexOf(headers, "NodeType");
                int sSpeaker = Array.IndexOf(headers, "SpeakerId");
                int sText = Array.IndexOf(headers, "Text");
                int sChoiceText = Array.IndexOf(headers, "ChoiceText");
                int sNextNodeId = Array.IndexOf(headers, "NextNodeId");

                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;
                    var cols = CsvSplit(lines[i]);

                    string nodeId = Safe(cols, sNodeId);
                    if (string.IsNullOrEmpty(nodeId))
                    {
                        Debug.LogWarning($"[Parser:Story] Missing NodeId at line {i + 1}: {lines[i]}");
                        continue;
                    }

                    if (!map.TryGetValue(nodeId, out var node))
                    {
                        node = new DialogueNode { NodeId = nodeId, NodeType = "Dialogue" };
                        map[nodeId] = node;
                    }

                    // NodeType (비어 있으면 Dialogue가 기본)
                    string parsedType = Safe(cols, sNodeType);
                    if (!string.IsNullOrEmpty(parsedType))
                        node.NodeType = parsedType;

                    // Choice_xxx_N → Choice 로 보정
                    if (!string.IsNullOrEmpty(node.NodeType) && node.NodeType.StartsWith("Choice_"))
                        node.NodeType = "Choice";

                    node.Chapter = Safe(cols, sChapter);
                    node.Scene = Safe(cols, sScene);
                    node.SpeakerId = Safe(cols, sSpeaker);
                    node.Text = Safe(cols, sText);
                    node.ChoiceText = Safe(cols, sChoiceText);
                    node.NextNodeId = Safe(cols, sNextNodeId);

                    Debug.Log($"[Parser:Story] Node={node.NodeId} Type={node.NodeType} Speaker={node.SpeakerId}");
                }
            }

            // 2. Dialogue CSV 로드 (조건, 효과, 스타일)
            if (File.Exists(dialogueCsvPath))
            {
                string[] dlgLines = File.ReadAllLines(dialogueCsvPath);

                // 헤더 인덱스 확인
                var headers = CsvSplit(dlgLines[0]);
                int dChapter = Array.IndexOf(headers, "Chapter");
                int dScene = Array.IndexOf(headers, "Scene");
                int dNodeId = Array.IndexOf(headers, "NodeId");
                int dNodeType = Array.IndexOf(headers, "NodeType");
                int dSkipping = Array.IndexOf(headers, "Skipping");
                int dTextEffect = Array.IndexOf(headers, "TextEffect");
                int dConditions = Array.IndexOf(headers, "Conditions");
                int dEffects = Array.IndexOf(headers, "Effects");
                int dElseIfConditions = Array.IndexOf(headers, "ElseIfConditions");
                int dElseIfEffects = Array.IndexOf(headers, "ElseIfEffects");
                int dElseEffects = Array.IndexOf(headers, "ElseEffects");
                int dSkipPenalty = Array.IndexOf(headers, "SkipPenalty");
                int dFlagTag = Array.IndexOf(headers, "FlagTag");
                int dChoiceStyle = Array.IndexOf(headers, "ChoiceStyle");

                for (int i = 1; i < dlgLines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(dlgLines[i])) continue;
                    var cols = CsvSplit(dlgLines[i]);

                    string nodeId = Safe(cols, dNodeId);
                    if (string.IsNullOrEmpty(nodeId))
                    {
                        Debug.LogWarning($"[Parser:Dialogue] Missing NodeId at line {i + 1}: {dlgLines[i]}");
                        continue;
                    }

                    if (!map.TryGetValue(nodeId, out var node))
                    {
                        node = new DialogueNode { NodeId = nodeId, NodeType = "Dialogue" };
                        map[nodeId] = node;
                    }

                    // NodeType 보정
                    string parsedType = Safe(cols, dNodeType);
                    if (!string.IsNullOrEmpty(parsedType))
                        node.NodeType = parsedType;
                    if (!string.IsNullOrEmpty(node.NodeType) && node.NodeType.StartsWith("Choice_"))
                        node.NodeType = "Choice";
                    if (!string.IsNullOrEmpty(node.NodeType) && node.NodeType.StartsWith("END_"))
                        node.NodeType = "End";

                    // 값 할당
                    node.Chapter = Safe(cols, dChapter);
                    node.Scene = Safe(cols, dScene);
                    node.Skipping = Safe(cols, dSkipping);
                    node.TextEffect = Safe(cols, dTextEffect);
                    node.Conditions = Safe(cols, dConditions);
                    node.Effects = Safe(cols, dEffects);
                    node.ElseIfConditions = Safe(cols, dElseIfConditions);
                    node.ElseIfEffects = Safe(cols, dElseIfEffects);
                    node.ElseEffects = Safe(cols, dElseEffects);
                    node.SkipPenalty = Safe(cols, dSkipPenalty);
                    node.FlagTag = Safe(cols, dFlagTag);

                    // ChoiceStyle (비어 있으면 Default)
                    string style = Safe(cols, dChoiceStyle);
                    node.ChoiceStyle = string.IsNullOrEmpty(style) ? "Default" : style;

                    Debug.Log($"[Parser:Dialogue] Node={node.NodeId} Type={node.NodeType}");
                }
            }

            return map;
        }

        // CSV 스플리터 (따옴표, 콤마 처리)
        private static string[] CsvSplit(string line)
        {
            var list = new List<string>();
            bool inQuotes = false;
            var cur = "";

            foreach (var c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    list.Add(cur);
                    cur = "";
                }
                else
                {
                    cur += c;
                }
            }
            list.Add(cur);
            return list.ToArray();
        }

        // 안전하게 배열 접근
        private static string Safe(string[] cols, int idx)
        {
            if (idx < 0 || idx >= cols.Length) return "";
            return cols[idx].Trim();
        }
    }
}
