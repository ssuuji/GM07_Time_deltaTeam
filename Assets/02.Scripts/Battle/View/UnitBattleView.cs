using System.Collections;
using AFKHero.Sound;
using UnityEngine;

namespace AFKHero.Battle
{
    public sealed class UnitBattleView : MonoBehaviour
    {
        [Header("애니메이터")]
        [SerializeField] private Animator animator;
        // 공통 적용 Animator
        [SerializeField] private RuntimeAnimatorController battleAnimatorController;

        [Header("애니메이션 Trigger")]
        [SerializeField] private string attackTrigger = "Attack";
        [SerializeField] private string hitTrigger = "Hit";

        [Header("이동 애니메이션")]
        [SerializeField] private string moveBool = "1_Move";

        // 위치가 미세하게 흔들리는 것만으로 이동 애니메이션이 실행되지 않도록 함
        [SerializeField, Min(0f)] private float movementAnimationThreshold = 0.0001f;

        [SerializeField] private string deathTrigger = "4_Death";
        [SerializeField] private string victoryTrigger = "6_Other";

        // 사망 상태
        [SerializeField] private string deathStateBool = "isDeath";

        [Header("사망 연출")]
        [SerializeField, Min(0f)] private float deathFadeDelay = 1f;
        [SerializeField, Min(0.01f)] private float deathFadeDuration = 0.5f;

        [Header("데미지 텍스트")]
        [SerializeField] private DamageTextView damageTextPrefab;

        [SerializeField] private Transform damageTextAnchor;

        [Header("기본 공격 효과음")]
        [SerializeField]
        private SoundKey[] basicAttackSoundKeys =
        {
            SoundKey.SFX_Attack_1,
            SoundKey.SFX_Attack_2,
            SoundKey.SFX_Attack_3
        };

        private int lastBasicAttackSoundIndex = -1;

        private BattleUnit owner;
        private UnitHealth unitHealth;
        private UnitAttackController attackController;

        private BattleManager battleManager;

        private float defaultAnimatorSpeed = 1f;

        private Vector3 previousPosition;
        private bool isMoveAnimationPlaying;

        private SpriteRenderer[] unitSpriteRenderers;
        private Coroutine deathRoutine;

        private void Start()
        {
            owner = GetComponent<BattleUnit>();

            if (owner == null || !owner.IsInitialized)
            {
                Debug.LogError("초기화 된 BattleUnit을 찾을 수 없습니다.", this);

                enabled = false;
                return;
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            if (animator != null)
            {
                if (battleAnimatorController != null)
                {
                    animator.runtimeAnimatorController = battleAnimatorController;
                }

                defaultAnimatorSpeed = animator.speed;
            }

            previousPosition = transform.position;

            unitSpriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

            battleManager = FindFirstObjectByType<BattleManager>();

            if (battleManager != null)
            {
                battleManager.UltimateStarted += HandleUltimateStarted;
                battleManager.UltimateFinished += HandleUltimateFinished;
                battleManager.StateChanged += HandleBattleStateChanged;
                battleManager.UnitDied += HandleUnitDied;
            }
            else
            {
                Debug.LogWarning($"[{name}] 애니메이션 정지에 사용할 BattleManager를 찾지 못했습니다.", this);
            }

            unitHealth = owner.Health;
            attackController = owner.AttackController;

            if (damageTextAnchor == null)
            {
                damageTextAnchor = transform;
            }

            if (unitHealth != null)
            {
                unitHealth.DamageApplied += HandleDamageApplied;
                unitHealth.HealthRestored += HandleHealthRestored;
                // 추가: 회피 신호를 들을 준비
                unitHealth.OnEvaded += HandleEvaded;
            }

            if (attackController != null)
            {
                attackController.BasicAttackStarted += HandleBasicAttackStarted;
            }
        }

        private void LateUpdate()
        {
            UpdateMoveAnimation();

            // 다음 프레임의 이동 거리를 계산할 수 있도록 현재 위치를 저장합니다.
            previousPosition = transform.position;
        }

        private void UpdateMoveAnimation()
        {
            // 전투 중이며 살아 있고 직접 이동할 수 있을 때만 이동 애니메이션을 허용합니다.
            // 기절이나 넉백 중에는 위치가 바뀌더라도 걷기 애니메이션을 재생하지 않습니다.
            bool canPlayMoveAnimation =
                animator != null &&
                owner != null &&
                owner.Stats != null &&
                owner.Stats.IsAlive &&
                battleManager != null &&
                battleManager.CurrentState == BattleState.Fighting &&
                (owner.StatusEffects == null || owner.StatusEffects.CanMove);

            Vector3 movement = transform.position - previousPosition;
            float thresholdSqr = movementAnimationThreshold * movementAnimationThreshold;

            bool isMoving =
                canPlayMoveAnimation &&
                movement.sqrMagnitude > thresholdSqr;

            SetMoveAnimation(isMoving);
        }

        private void SetMoveAnimation(bool isMoving)
        {
            if (animator == null ||
                string.IsNullOrWhiteSpace(moveBool) ||
                isMoveAnimationPlaying == isMoving)
            {
                return;
            }

            isMoveAnimationPlaying = isMoving;
            animator.SetBool(moveBool, isMoving);
        }

        // 기본 공격 시 실행
        private void HandleBasicAttackStarted(BattleUnit target)
        {
            PlayRandomBasicAttackSFX();

            if (animator == null || string.IsNullOrWhiteSpace(attackTrigger))
            {
                return;
            }

            animator.SetTrigger(attackTrigger);
        }

        private void PlayRandomBasicAttackSFX()
        {
            if (SoundManager.Instance == null ||
                basicAttackSoundKeys == null ||
                basicAttackSoundKeys.Length == 0)
            {
                return;
            }

            int soundIndex = Random.Range(0, basicAttackSoundKeys.Length);

            // 소리가 두 개 이상이면 직전에 재생한 소리는 다시 선택하지 않습니다.
            if (basicAttackSoundKeys.Length > 1 && soundIndex == lastBasicAttackSoundIndex)
            {
                int randomOffset = Random.Range(1, basicAttackSoundKeys.Length);
                soundIndex = (soundIndex + randomOffset) % basicAttackSoundKeys.Length;
            }

            lastBasicAttackSoundIndex = soundIndex;
            SoundManager.Instance.PlaySFX(basicAttackSoundKeys[soundIndex]);
        }

        // 실제 피해량으로 피격 연출과 텍스트 표시
        private void HandleDamageApplied(BattleUnit target, int appliedDamage)
        {
            if (target != owner || appliedDamage <= 0)
            {
                return;
            }

            if (animator != null && !string.IsNullOrWhiteSpace(hitTrigger))
            {
                animator.SetTrigger(hitTrigger);
            }

            DamageTextView damageText = SpawnDamageText();

            if (damageText != null)
            {
                damageText.Play(appliedDamage, damageTextAnchor.position);
            }
        }

        // 실제 회복된 수치를 숫자로 표시
        private void HandleHealthRestored(BattleUnit target, int restoredHealth)
        {
            if (target != owner || restoredHealth <= 0)
            {
                return;
            }

            DamageTextView damageText = SpawnDamageText();

            if (damageText != null)
            {
                damageText.PlayText($"+{restoredHealth}", damageTextAnchor.position, Color.green);
            }
        }

        private DamageTextView SpawnDamageText()
        {
            if (damageTextPrefab == null)
            {
                return null;
            }

            Vector3 spawnPosition = damageTextAnchor != null ? damageTextAnchor.position : transform.position;

            // PoolManager가 없으면 기존 생성 방식을 사용
            if (PoolManager.Instance == null)
            {
                return Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity);
            }

            GameObject pooledObject = PoolManager.Instance.SpawnFromPool(
                damageTextPrefab.gameObject,
                spawnPosition,
                Quaternion.identity);

            DamageTextView damageText = pooledObject.GetComponent<DamageTextView>();

            if (damageText != null)
            {
                return damageText;
            }

            Debug.LogError("[UnitBattleView] 풀에서 생성한 오브젝트에 DamageTextView가 없습니다.", pooledObject);

            Poolable poolable = pooledObject.GetComponent<Poolable>();

            if (poolable != null)
            {
                poolable.Release();
            }

            return null;
        }

        private void HandleUltimateStarted(BattleUnit ultimateUser)
        {
            if (animator == null)
            {
                return;
            }

            bool isUltimateUser = ultimateUser == owner;

            animator.speed = isUltimateUser ? defaultAnimatorSpeed : 0f;
        }

        private void HandleUltimateFinished(BattleUnit _)
        {
            RestoreAnimatorSpeed();
        }

        private void HandleBattleStateChanged(BattleState state)
        {
            if (state != BattleState.UltimateSequence)
            {
                RestoreAnimatorSpeed();
            }

            if (state != BattleState.Victory ||
       owner == null ||
       owner.Team != TeamType.Ally ||
       owner.Stats == null ||
       !owner.Stats.IsAlive)
            {
                return;
            }

            if (animator != null && !string.IsNullOrWhiteSpace(victoryTrigger))
            {
                animator.SetTrigger(victoryTrigger);
            }
        }

        private void RestoreAnimatorSpeed()
        {
            if (animator != null)
            {
                animator.speed = defaultAnimatorSpeed;
            }
        }

        private void HandleUnitDied(BattleUnit deadUnit)
        {
            if (deadUnit != owner || deathRoutine != null)
            {
                return;
            }

            deathRoutine = StartCoroutine(PlayDeathRoutine());
        }

        private IEnumerator PlayDeathRoutine()
        {
            // 궁극기 연출 때문에 정지된 Animator도 사망 애니메이션은 재생
            RestoreAnimatorSpeed();

            if (animator != null)
            {
                // SPUM은 isDeath가 false일 때 Death에서 Idle로 돌아감
                // 먼저 true로 설정하여 사망 애니메이션의 마지막 자세를 유지
                if (!string.IsNullOrWhiteSpace(deathStateBool))
                {
                    animator.SetBool(deathStateBool, true);
                }

                if (!string.IsNullOrWhiteSpace(deathTrigger))
                {
                    animator.SetTrigger(deathTrigger);
                }
            }

            // 사망 애니메이션을 먼저 보여준 뒤 페이드를 시작
            if (deathFadeDelay > 0f)
            {
                yield return new WaitForSeconds(deathFadeDelay);
            }

            float safeFadeDuration = Mathf.Max(0.01f, deathFadeDuration);
            float[] startAlphas = new float[unitSpriteRenderers.Length];

            for (int i = 0; i < unitSpriteRenderers.Length; i++)
            {
                if (unitSpriteRenderers[i] != null)
                {
                    startAlphas[i] = unitSpriteRenderers[i].color.a;
                }
            }

            float elapsedTime = 0f;

            while (elapsedTime < safeFadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alphaRate = 1f - Mathf.Clamp01(elapsedTime / safeFadeDuration);

                for (int i = 0; i < unitSpriteRenderers.Length; i++)
                {
                    SpriteRenderer spriteRenderer = unitSpriteRenderers[i];

                    if (spriteRenderer == null)
                    {
                        continue;
                    }

                    Color color = spriteRenderer.color;
                    color.a = startAlphas[i] * alphaRate;
                    spriteRenderer.color = color;
                }

                yield return null;
            }

            // 완전히 시체가 사라진 뒤 초기화
            if (animator != null && !string.IsNullOrWhiteSpace(deathStateBool))
            {
                animator.SetBool(deathStateBool, false);
            }

            deathRoutine = null;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (unitHealth != null)
            {
                unitHealth.DamageApplied -= HandleDamageApplied;
                unitHealth.HealthRestored -= HandleHealthRestored;

                // 추가: 스크립트 꺼질 때 듣기 종료
                unitHealth.OnEvaded -= HandleEvaded;
            }

            if (attackController != null)
            {
                attackController.BasicAttackStarted -= HandleBasicAttackStarted;
            }

            if (battleManager != null)
            {
                battleManager.UltimateStarted -= HandleUltimateStarted;
                battleManager.UltimateFinished -= HandleUltimateFinished;
                battleManager.StateChanged -= HandleBattleStateChanged;
                battleManager.UnitDied -= HandleUnitDied;
            }
        }

        // : 추가회피 신호를 들었을 때 텍스트를 띄워주는 함수
        private void HandleEvaded(BattleUnit target)
        {
            DamageTextView damageText = SpawnDamageText();

            if (damageText != null)
            {
                damageText.PlayText("회피!", damageTextAnchor.position, Color.blue);
            }
        }

    }
}