using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using VN.Dialogue;   

#if DOTWEEN
using DG.Tweening;
#endif

namespace VN.Rendering
{
    public class CharacterManager : MonoBehaviour
    {
        [SerializeField] private RectTransform characterRoot;
        [SerializeField] private CharacterView characterPrefab;

        private readonly Dictionary<string, CharacterView> activeCharacters = new();

        public void ApplyCharacters(DialogueNode node)
        {
            if (characterRoot == null || characterPrefab == null) return;
            if (string.IsNullOrEmpty(node.CharacterId)) return;

            var ids = node.CharacterId.Split('/');
            var exps = node.Expression.Split('/');
            var poses = node.Pose.Split('/');
            var pos = node.CharacterPosition.Split('/');
            var eff = node.CharacterEffect.Split('/');
            var ani = node.AnimationId.Split('/');

            // ���� ǥ���ؾ� �� ĳ���� ����
            var toShow = new HashSet<string>(ids);

            // ���� ��� ���� ó��
            var toRemove = activeCharacters.Keys.Where(k => !toShow.Contains(k)).ToList();
            foreach (var dead in toRemove)
            {
                var view = activeCharacters[dead];
                if (view != null)
                    view.PlayEffect("fadeout", () => Destroy(view.gameObject));
                activeCharacters.Remove(dead);
            }

            // ǥ���� ĳ���� ����
            for (int i = 0; i < ids.Length; i++)
            {
                string id = ids[i];
                if (string.IsNullOrEmpty(id)) continue;

                if (!activeCharacters.TryGetValue(id, out var view) || view == null)
                {
                    view = Instantiate(characterPrefab, characterRoot);
                    view.name = $"CHAR_{id}";
                    activeCharacters[id] = view;
                }

                string exp = i < exps.Length ? exps[i] : "";
                string pose = i < poses.Length ? poses[i] : "";
                string posToken = i < pos.Length ? pos[i] : "middle";
                string effect = i < eff.Length ? eff[i] : "";
                string anim = i < ani.Length ? ani[i] : "";

                // Sprite �ε� ��Ģ: Resources/Characters/{id}_{exp}
                var sprite = Resources.Load<Sprite>($"Characters/{id}_{exp}");
                if (sprite == null)
                    sprite = Resources.Load<Sprite>($"Characters/{id}");

                view.ApplySprite(sprite);
                view.ApplyPosition(posToken);
                view.PlayEffect(effect);
                view.PlayAnimation(anim);
            }

            // front �� back ������� ����
            SortCharacterOrder();
        }

        private void SortCharacterOrder()
        {
            var ordered = activeCharacters.Values.OrderBy(v => v.ZOrder).ToList();
            for (int i = 0; i < ordered.Count; i++)
                ordered[i].transform.SetSiblingIndex(i);
        }
    }
}
