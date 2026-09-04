using System;
using UnityEngine;

namespace AFKHero.Battle
{
    public sealed class UnitStatusEffectView : MonoBehaviour
    {
        [Serializable]
        private struct StatusIconBinding
        {
            [SerializeField] private StatusEffectType type;
            [SerializeField] private GameObject iconObject;

            public StatusEffectType Type => type;
            public GameObject IconObject => iconObject;
        }

        [Header("군중제어 UI")]
        [Tooltip("군중제어 아이콘들을 포함하는 최상위 오브젝트입니다.")]
        [SerializeField] private GameObject statusRoot;

        [Tooltip("상태 종류와 해당 상태를 표시할 아이콘을 연결합니다.")]
        [SerializeField] private StatusIconBinding[] statusIcons;

        private BattleUnit owner;
        private UnitStatusEffectController statusEffects;

        private void Start()
        {
            owner = GetComponent<BattleUnit>();

            if (owner == null || !owner.IsInitialized)
            {
                Debug.LogError("[UnitStatusEffectView] 초기화된 BattleUnit을 찾을 수 없습니다.", this);
                enabled = false;
                return;
            }

            statusEffects = owner.StatusEffects;

            if (statusEffects == null)
            {
                Debug.LogError($"[{owner.name}] UnitStatusEffectController를 찾을 수 없습니다.", this);
                enabled = false;
                return;
            }

            statusEffects.StatusEffectApplied += HandleStatusEffectApplied;
            statusEffects.StatusEffectRemoved += HandleStatusEffectRemoved;

            RefreshAllIcons();
        }

        private void HandleStatusEffectApplied(BattleUnit target, StatusEffectType type, float remainingDuration)
        {
            if (target != owner)
            {
                return;
            }

            SetIconActive(type, true);
        }

        private void HandleStatusEffectRemoved(BattleUnit target, StatusEffectType type)
        {
            if (target != owner)
            {
                return;
            }

            SetIconActive(type, false);
        }

        private void RefreshAllIcons()
        {
            if (statusIcons == null)
            {
                RefreshRootVisibility();
                return;
            }

            for (int i = 0; i < statusIcons.Length; i++)
            {
                GameObject iconObject = statusIcons[i].IconObject;

                if (iconObject == null)
                {
                    continue;
                }

                bool isActive = statusEffects.HasStatusEffect(statusIcons[i].Type);
                iconObject.SetActive(isActive);
            }

            RefreshRootVisibility();
        }

        private void SetIconActive(StatusEffectType type, bool isActive)
        {
            if (statusIcons == null)
            {
                return;
            }

            for (int i = 0; i < statusIcons.Length; i++)
            {
                if (statusIcons[i].Type != type || statusIcons[i].IconObject == null)
                {
                    continue;
                }

                statusIcons[i].IconObject.SetActive(isActive);
                break;
            }

            RefreshRootVisibility();
        }

        private void RefreshRootVisibility()
        {
            if (statusRoot == null)
            {
                return;
            }

            bool hasVisibleIcon = false;

            if (statusIcons != null)
            {
                for (int i = 0; i < statusIcons.Length; i++)
                {
                    GameObject iconObject = statusIcons[i].IconObject;

                    if (iconObject != null && iconObject.activeSelf)
                    {
                        hasVisibleIcon = true;
                        break;
                    }
                }
            }

            statusRoot.SetActive(hasVisibleIcon);
        }

        private void OnDestroy()
        {
            if (statusEffects == null)
            {
                return;
            }

            statusEffects.StatusEffectApplied -= HandleStatusEffectApplied;
            statusEffects.StatusEffectRemoved -= HandleStatusEffectRemoved;
        }
    }
}