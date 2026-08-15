using System.Collections.Generic;
using UnityEngine;
namespace AFKHero.Battle
{
    public sealed class TestBattleSystem : MonoBehaviour
    {
        [Header("전투 관리자")]

        [Tooltip("현재 테스트 씬의 BattleManager를 연결합니다.")]
        [SerializeField]
        private BattleManager battleManager;

        [Header("테스트 전투 생성")]

        [Tooltip("실제 유닛 생성과 배치를 담당하는 BattleSpawner입니다.")]
        [SerializeField]
        private BattleSpawner battleSpawner;

        [Tooltip("테스트 전투에 출전시킬 아군 HeroData를 순서대로 넣습니다.")]
        [SerializeField]
        private List<HeroData> testAllyHeroes = new();

        [Tooltip("테스트 전투에 출전시킬 적군 HeroData를 순서대로 넣습니다.")]
        [SerializeField]
        private List<HeroData> testEnemyHeroes = new();

        [Tooltip("테스트 아군에게 적용할 레벨입니다.")]
        [SerializeField, Min(1)]
        private int testAllyLevel = 1;

        [Tooltip("테스트 적군에게 적용할 레벨입니다.")]
        [SerializeField, Min(1)]
        private int testEnemyLevel = 1;

        [Tooltip("테스트 전투의 제한시간입니다.")]
        [SerializeField, Min(1f)]
        private float testBattleTimeLimit = 90f;

        [Header("테스트 유닛")]

        [Tooltip("기절·침묵을 적용하거나 궁극기를 선택할 유닛입니다.")]
        [SerializeField]
        private BattleUnit testUnit;

        [Header("군중제어 지속시간")]

        [Tooltip("테스트할 기절 지속시간입니다.")]
        [SerializeField, Min(0.1f)]
        private float stunDuration = 3f;

        [Tooltip("테스트할 침묵 지속시간입니다.")]
        [SerializeField, Min(0.1f)]
        private float silenceDuration = 3f;

        /// <summary>
        /// 궁극기를 자동 모드로 변경합니다.
        /// </summary>
        [ContextMenu("궁극기/자동 모드로 변경")]
        private void SetAutomaticUltimateMode()
        {
            if (!ValidateBattleManager())
            {
                return;
            }

            battleManager.SetAutomaticUltimateUse(true);

            Debug.Log(
                "[테스트] 궁극기를 자동 모드로 변경했습니다.",
                battleManager);
        }

        /// <summary>
        /// 궁극기를 수동 모드로 변경합니다.
        /// </summary>
        [ContextMenu("궁극기/수동 모드로 변경")]
        private void SetManualUltimateMode()
        {
            if (!ValidateBattleManager())
            {
                return;
            }

            battleManager.SetAutomaticUltimateUse(false);

            Debug.Log(
                "[테스트] 궁극기를 수동 모드로 변경했습니다.",
                battleManager);
        }

        /// <summary>
        /// testUnit을 수동 궁극기 실행 대상으로 선택합니다.
        /// 아군이며 궁극기 에너지가 가득 차고 대기열에 있어야 합니다.
        /// </summary>
        [ContextMenu("궁극기/테스트 유닛 수동 선택")]
        private void SelectTestUnitUltimate()
        {
            if (!ValidateBattleManager() ||
                !ValidateTestUnit())
            {
                return;
            }

            bool wasSelected =
                battleManager.TrySelectQueueUltimate(testUnit);

            if (wasSelected)
            {
                Debug.Log(
                    $"[테스트] {testUnit.name}의 궁극기를 선택했습니다.",
                    testUnit);
            }
            else
            {
                Debug.LogWarning(
                    $"[테스트] {testUnit.name}의 궁극기를 선택하지 못했습니다. " +
                    "아군 여부, 에너지, 대기열, 기절·침묵 상태를 확인하세요.",
                    testUnit);
            }
        }

        /// <summary>
        /// testUnit에게 기절을 적용합니다.
        /// 기절 중에는 이동, 기본 공격, 궁극기를 사용할 수 없습니다.
        /// </summary>
        [ContextMenu("군중제어/테스트 유닛 기절")]
        private void ApplyStun()
        {
            if (!ValidateTestUnit())
            {
                return;
            }

            if (testUnit.StatusEffects == null)
            {
                Debug.LogError(
                    $"{testUnit.name}에 " +
                    "UnitStatusEffectController가 없습니다.",
                    testUnit);

                return;
            }

            testUnit.StatusEffects.ApplyStatusEffect(
                StatusEffectType.Stun,
                stunDuration);
        }

        /// <summary>
        /// testUnit에게 침묵을 적용합니다.
        /// 침묵 중에는 이동과 기본 공격이 가능하지만 궁극기는 사용할 수 없습니다.
        /// </summary>
        [ContextMenu("군중제어/테스트 유닛 침묵")]
        private void ApplySilence()
        {
            if (!ValidateTestUnit())
            {
                return;
            }

            if (testUnit.StatusEffects == null)
            {
                Debug.LogError(
                    $"{testUnit.name}에 " +
                    "UnitStatusEffectController가 없습니다.",
                    testUnit);

                return;
            }

            testUnit.StatusEffects.ApplyStatusEffect(
                StatusEffectType.Silence,
                silenceDuration);
        }

        /// <summary>
        /// testUnit에게 적용된 모든 군중제어를 즉시 해제합니다.
        /// </summary>
        [ContextMenu("군중제어/모든 상태 즉시 해제")]
        private void ClearAllStatusEffects()
        {
            if (!ValidateTestUnit())
            {
                return;
            }

            testUnit.StatusEffects?.ClearAllStatusEffects();

            Debug.Log(
                $"[테스트] {testUnit.name}의 모든 상태이상을 해제했습니다.",
                testUnit);
        }

        [ContextMenu("전투 테스트/선택한 영웅으로 전투 시작")]
        public void StartSelectedHeroBattle()
        {
            // 편집 모드에서 실행하면 씬에 테스트 유닛이 저장될 수 있으므로
            // 반드시 Play Mode에서만 실행하도록 제한합니다.
            if (!Application.isPlaying)
            {
                Debug.LogWarning(
                    "[전투 테스트] Play Mode에서 실행해 주세요.",
                    this);

                return;
            }

            if (!ValidateBattleSpawner())
            {
                return;
            }

            List<HeroInstance> allyParty = CreateTestAllyParty();

            if (allyParty.Count == 0)
            {
                Debug.LogError(
                    "[전투 테스트] 사용할 수 있는 아군 HeroData가 없습니다.",
                    this);

                return;
            }

            if (!ContainsValidHeroData(testEnemyHeroes))
            {
                Debug.LogError(
                    "[전투 테스트] 사용할 수 있는 적군 HeroData가 없습니다.",
                    this);

                return;
            }

            bool battleStarted = battleSpawner.SpawnBattle(
                allyParty,
                testEnemyHeroes,
                Mathf.Max(1, testEnemyLevel),
                Mathf.Max(1f, testBattleTimeLimit));

            if (battleStarted)
            {
                Debug.Log(
                    $"[전투 테스트] 아군 {allyParty.Count}명과 " +
                    $"적군 {testEnemyHeroes.Count}명의 전투를 시작했습니다.",
                    this);
            }
            else
            {
                Debug.LogError(
                    "[전투 테스트] 테스트 전투 생성에 실패했습니다.",
                    this);
            }
        }

        /// <summary>
        /// HeroData 목록을 BattleSpawner가 요구하는 HeroInstance 목록으로 변환합니다.
        /// 테스트 도중 원본 영웅 저장 데이터가 변경되지 않도록 새 인스턴스를 생성합니다.
        /// </summary>
        private List<HeroInstance> CreateTestAllyParty()
        {
            List<HeroInstance> party = new();

            if (testAllyHeroes == null)
            {
                return party;
            }

            for (int i = 0; i < testAllyHeroes.Count; i++)
            {
                HeroData heroData = testAllyHeroes[i];

                if (heroData == null)
                {
                    continue;
                }

                HeroInstance heroInstance = new HeroInstance(
                    heroData,
                    defaultUnlocked: true);

                heroInstance.level = Mathf.Max(1, testAllyLevel);
                party.Add(heroInstance);
            }

            return party;
        }

        /// <summary>
        /// 목록에 실제로 사용할 수 있는 HeroData가 하나 이상 있는지 검사합니다.
        /// </summary>
        private static bool ContainsValidHeroData(
            IReadOnlyList<HeroData> heroes)
        {
            if (heroes == null)
            {
                return false;
            }

            for (int i = 0; i < heroes.Count; i++)
            {
                if (heroes[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private bool ValidateBattleSpawner()
        {
            // Game 씬에서는 BattleSpawner가 같은 Battle 오브젝트에 있으므로
            // Inspector 연결이 빠졌을 때 같은 오브젝트에서 자동으로 찾습니다.
            if (battleSpawner == null)
            {
                battleSpawner = GetComponent<BattleSpawner>();
            }

            if (battleSpawner != null)
            {
                return true;
            }

            Debug.LogError(
                "[전투 테스트] BattleSpawner가 연결되지 않았습니다.",
                this);

            return false;
        }
        private bool ValidateBattleManager()
        {
            if (battleManager != null)
            {
                return true;
            }

            Debug.LogError(
                "[테스트] BattleManager가 연결되지 않았습니다.",
                this);

            return false;
        }

        private bool ValidateTestUnit()
        {
            if (testUnit != null)
            {
                return true;
            }

            Debug.LogError(
                "[테스트] 테스트할 BattleUnit이 연결되지 않았습니다.",
                this);

            return false;
        }
    }

    // ===== [변경 끝: 궁극기 모드 및 군중제어 테스트 기능 추가] =====
}

