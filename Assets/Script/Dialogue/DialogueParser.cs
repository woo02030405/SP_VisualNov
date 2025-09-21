// DialogueParser.cs
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VN.Dialogue;

namespace VN.IO
{
    public static class DialogueParser
    {
        public static Dictionary<string, DialogueNode> LoadFromStreamingAssets(string lang = "ko")
        {
            string storyPath = Path.Combine(Application.streamingAssetsPath, "Story.csv");
            string dialoguePath = Path.Combine(Application.streamingAssetsPath, $"Dialogue_{lang}.csv");
            return LoadCsv(storyPath, dialoguePath);
        }

        public static Dictionary<string, DialogueNode> LoadCsv(string storyCsvPath, string dialogueCsvPath)
        {
            var map = new Dictionary<string, DialogueNode>();

            // ---- Story.csv (NodeId, SpeakerId, Text) ----
            var storyLines = File.ReadAllLines(storyCsvPath);
            var storyHeader = CsvSplit(storyLines[0]);
            int idxNodeId = Array.IndexOf(storyHeader, "NodeId");
            int idxSpeakerId = Array.IndexOf(storyHeader, "SpeakerId");
            int idxText = Array.IndexOf(storyHeader, "Text");

            for (int i = 1; i < storyLines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(storyLines[i])) continue;
                var cols = CsvSplit(storyLines[i]);

                var node = new DialogueNode
                {
                    NodeId = Safe(cols, idxNodeId),
                    SpeakerId = Safe(cols, idxSpeakerId),
                    Text = Safe(cols, idxText) // ✅ 실제 대사
                };

                if (!string.IsNullOrEmpty(node.NodeId))
                    map[node.NodeId] = node;
            }

            // ---- Dialogue_xx.csv (NodeId, Text=ID, Choices, NextNodeId, 연출 관련) ----
            var dlgLines = File.ReadAllLines(dialogueCsvPath);
            var dlgHeader = CsvSplit(dlgLines[0]);

            int dNodeId = Array.IndexOf(dlgHeader, "NodeId");
            int dChoices = Array.IndexOf(dlgHeader, "Choices");
            int dBg = Array.IndexOf(dlgHeader, "Background");
            int dBgm = Array.IndexOf(dlgHeader, "BGM");
            int dCharType = Array.IndexOf(dlgHeader, "CharacterType");
            int dExpr = Array.IndexOf(dlgHeader, "Expression");
            int dPose = Array.IndexOf(dlgHeader, "Pose");
            int dPos = Array.IndexOf(dlgHeader, "CharacterPosition");
            int dEff = Array.IndexOf(dlgHeader, "CharacterEffect");
            int dVoice = Array.IndexOf(dlgHeader, "VoiceId");
            int dSfx = Array.IndexOf(dlgHeader, "SFX");
            int dTrans = Array.IndexOf(dlgHeader, "Transition");
            int dAnim = Array.IndexOf(dlgHeader, "AnimType");
            int dTextEff = Array.IndexOf(dlgHeader, "TextEffect");
            int dSkip = Array.IndexOf(dlgHeader, "SkipFlag");
            int dNext = Array.IndexOf(dlgHeader, "NextNodeId");

            for (int i = 1; i < dlgLines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(dlgLines[i])) continue;
                var cols = CsvSplit(dlgLines[i]);
                string nodeId = Safe(cols, dNodeId);
                if (string.IsNullOrEmpty(nodeId)) continue;

                if (!map.TryGetValue(nodeId, out var node))
                {
                    node = new DialogueNode { NodeId = nodeId };
                    map[nodeId] = node;
                }

                // 🎯 Dialogue.csv의 Text는 ID라서 무시
                // node.Text = Story.csv 의 실제 대사 유지

                // 연출/효과 채움
                node.Background = Safe(cols, dBg);
                node.BGM = Safe(cols, dBgm);
                node.CharacterType = Safe(cols, dCharType);
                node.Expression = Safe(cols, dExpr);
                node.Pose = Safe(cols, dPose);
                node.CharacterPosition = Safe(cols, dPos);
                node.CharacterEffect = Safe(cols, dEff);
                node.VoiceId = Safe(cols, dVoice);
                node.SFX = Safe(cols, dSfx);
                node.Transition = Safe(cols, dTrans);
                node.ChoiceAnimType = Safe(cols, dAnim);
                node.TextEffect = Safe(cols, dTextEff);
                node.NextNodeId = Safe(cols, dNext);

                string skipRaw = Safe(cols, dSkip);
                node.SkipFlag = string.Equals(skipRaw, "true", StringComparison.OrdinalIgnoreCase);

                // 선택지
                node.Choices.Clear();
                string rawChoices = Safe(cols, dChoices);
                if (!string.IsNullOrWhiteSpace(rawChoices))
                {
                    foreach (var p in rawChoices.Split(';'))
                    {
                        var kv = p.Split(':');
                        if (kv.Length >= 2)
                            node.Choices.Add(new ChoiceOption(kv[0].Trim(), kv[1].Trim()));
                    }
                }
            }

            return map;
        }

        // CSV utils
        static string[] CsvSplit(string line)
        {
            var list = new List<string>();
            bool inQuotes = false;
            var cur = new System.Text.StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '\"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                    {
                        cur.Append('\"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    list.Add(cur.ToString());
                    cur.Length = 0;
                }
                else
                {
                    cur.Append(c);
                }
            }

            list.Add(cur.ToString());
            return list.ToArray();
        }

        static string Safe(string[] cols, int idx)
            => (idx >= 0 && idx < cols.Length) ? cols[idx]?.Trim().Trim('\r', '\n') : string.Empty;
    }
}
