using System;
using System.Collections.Generic;

namespace VN.Dialogue
{
    public static class DialogueParser
    {
        private static string GetStr(Dictionary<string, string> row, string key)
            => (row != null && key != null && row.TryGetValue(key, out var v)) ? v : "";

        private const string TOK_COND = "<=>";   // 조건
        private const string TOK_EFF = "<|>";   // 효과
        private const string TOK_ELSE = "<->";   // else 효과
        private static readonly string[] CH_MARKERS = new[] { "<ch>" }; // 선택지 구분자

        /// <summary>
        /// Story + Dialogue + Resource CSV를 합쳐서 NodeMap 생성
        /// </summary>
        public static Dictionary<string, DialogueNode> Parse(
            List<Dictionary<string, string>> storyCsv,
            List<Dictionary<string, string>> dialogueCsv,
            List<Dictionary<string, string>> resourceCsv)
        {
            var nodeDict = new Dictionary<string, DialogueNode>();

            // --- Story CSV ---
            if (storyCsv != null)
            {
                foreach (var row in storyCsv)
                {
                    string id = GetStr(row, "NodeId");
                    if (string.IsNullOrEmpty(id)) continue;

                    // Text: "=>" 뒤는 기획자 주석 → 제거
                    string rawText = GetStr(row, "Text");
                    string cleanText = CutLabel(rawText);

                    var node = new DialogueNode
                    {
                        Id = id,
                        Chapter = GetStr(row, "Chapter"),
                        Scene = GetStr(row, "Scene"),
                        SpeakerId = GetStr(row, "Speaker"),
                        Text = cleanText,
                        NextNodeId = GetStr(row, "NextNodeId")
                    };

                    // Story에서 선택지 문구 파싱
                    string choiceField = GetStr(row, "Choices");
                    if (!string.IsNullOrEmpty(choiceField))
                    {
                        node.Choices = new List<ChoiceOption>();
                        var labels = choiceField.Split(CH_MARKERS, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var lbl in labels)
                        {
                            string label = CutLabel(lbl.Trim());
                            if (string.IsNullOrEmpty(label)) continue;

                            node.Choices.Add(new ChoiceOption
                            {
                                Text = label,       // Story에서 표시할 선택지 문구
                                NextNodeId = null,  // Dialogue에서 채움
                                Condition = "-",
                                Effect = "",
                                ElseEffect = ""
                            });
                        }
                    }

                    nodeDict[id] = node;
                }
            }

            // --- Dialogue CSV ---
            if (dialogueCsv != null)
            {
                foreach (var row in dialogueCsv)
                {
                    string nodeId = GetStr(row, "NodeId");
                    if (string.IsNullOrEmpty(nodeId) || !nodeDict.ContainsKey(nodeId)) continue;

                    var node = nodeDict[nodeId];
                    string choicesCol = GetStr(row, "Choices");

                    // Story에서 선택지 개수만큼 이미 만들어져 있음 → Dialogue에서 로직 채워넣기
                    if (!string.IsNullOrEmpty(choicesCol) && node.Choices != null && node.Choices.Count > 0)
                    {
                        var parsed = ParseChoices(choicesCol);
                        for (int i = 0; i < node.Choices.Count && i < parsed.Count; i++)
                        {
                            node.Choices[i].NextNodeId = parsed[i].NextNodeId;
                            node.Choices[i].Condition = parsed[i].Condition;
                            node.Choices[i].Effect = parsed[i].Effect;
                            node.Choices[i].ElseEffect = parsed[i].ElseEffect;
                        }
                    }
                }
            }

            // --- Resource CSV (배경/사운드/연출 등) ---
            if (resourceCsv != null)
            {
                foreach (var row in resourceCsv)
                {
                    string nodeId = GetStr(row, "NodeId");
                    if (string.IsNullOrEmpty(nodeId) || !nodeDict.ContainsKey(nodeId)) continue;

                    var node = nodeDict[nodeId];
                    node.Background = GetStr(row, "Background");
                    node.BGM = GetStr(row, "BGM");
                    node.SFX = GetStr(row, "SFX");
                    node.VoiceId = GetStr(row, "VoiceId");
                    node.CharacterId = GetStr(row, "CharacterId");
                    node.Expression = GetStr(row, "Expression");
                    node.Pose = GetStr(row, "Pose");
                    node.CharacterPosition = GetStr(row, "CharacterPosition");
                    node.CharacterEffect = GetStr(row, "CharacterEffect");
                    node.AnimationId = GetStr(row, "AnimType");
                    node.EventCG = GetStr(row, "EventCG");
                    node.Transition = GetStr(row, "Transition");
                }
            }

            return nodeDict;
        }

        /// <summary>
        /// Story 텍스트/선택지 문구에서 "=>" 뒤는 잘라냄 (기획자 주석 제거)
        /// </summary>
        private static string CutLabel(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "";
            int idx = raw.IndexOf("=>");
            return (idx >= 0) ? raw.Substring(0, idx).Trim() : raw.Trim();
        }

        /// <summary>
        /// Dialogue CSV Choices 파싱 (조건/효과/NextNodeId)
        /// </summary>
        private static List<ChoiceOption> ParseChoices(string choicesText)
        {
            var list = new List<ChoiceOption>();
            if (string.IsNullOrWhiteSpace(choicesText)) return list;

            var rawChoices = choicesText.Split(CH_MARKERS, StringSplitOptions.RemoveEmptyEntries);
            foreach (var rawStr in rawChoices)
            {
                var raw = rawStr?.Trim();
                if (string.IsNullOrEmpty(raw)) continue;

                string next = null, cond = "-", eff = "", effElse = "";
                ParseAdvanced(raw, ref next, ref cond, ref eff, ref effElse);

                list.Add(new ChoiceOption
                {
                    Text = null, // Story에서 채운다
                    NextNodeId = next,
                    Condition = cond,
                    Effect = eff,
                    ElseEffect = effElse
                });
            }

            return list;
        }

        /// <summary>
        /// "<=> 조건 <|> 효과 <-> else효과" 파싱
        /// next 부분은 조건 앞에 항상 존재
        /// </summary>
        private static void ParseAdvanced(string raw, ref string next, ref string cond, ref string eff, ref string effElse)
        {
            string cur = raw;

            // <-> Else 효과
            string[] partsElse = cur.Split(new[] { TOK_ELSE }, StringSplitOptions.None);
            cur = partsElse[0].Trim();
            if (partsElse.Length > 1) effElse = partsElse[1].Trim();

            // <|> 효과
            string[] partsEff = cur.Split(new[] { TOK_EFF }, StringSplitOptions.None);
            cur = partsEff[0].Trim();
            if (partsEff.Length > 1) eff = partsEff[1].Trim();

            // <=> 조건
            string[] partsCond = cur.Split(new[] { TOK_COND }, StringSplitOptions.None);
            next = partsCond[0].Trim();
            if (partsCond.Length > 1) cond = partsCond[1].Trim();
        }
    }
}
