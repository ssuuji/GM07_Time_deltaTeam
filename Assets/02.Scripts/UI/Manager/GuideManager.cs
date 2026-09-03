using AFKHero.Quest;
using System;
using UnityEngine;

namespace AFKHero.UI
{
    public class GuideManager : MonoBehaviour
    {
        public static GuideManager Instance { get; private set; }

        [SerializeField] private GuideArrow guideArrow;
        private string currentGuideQuestId;

        private GuideTarget currentTarget = GuideTarget.None;
        private GuideStep currentStep = GuideStep.None;

        public GuideTarget CurrentTarget => currentTarget;
        public GuideStep CurrentStep => currentStep;

        public event Action<GuideStep> OnGuideStepChanged;

        private void Awake()
        {
            Instance = this;
        }

        //현재 메인 퀘스트가 자동 가이드 퀘스트라면 가이드 시작
        public void TryStartAutoGuide()
        {
            if (QuestManager.Instance == null) return;

            QuestData questData = QuestManager.Instance.GetCurrentMainQuest();

            if (questData == null)
            {
                EndGuide();
                return;
            }

            //현재 퀘스트의 목표를 이미 달성했다면 가이드 종료
            if (QuestManager.Instance.CanClaimReward(questData))
            {
                EndGuide();
                return;
            }

            //이전 퀘스트의 가이드가 남아있다면 종료
            if (!string.IsNullOrEmpty(currentGuideQuestId) && currentGuideQuestId != questData.QuestId)
            {
                EndGuide();
            }

            if (currentStep != GuideStep.None) return;
            if (!questData.AutoGuide) return;

            currentGuideQuestId = questData.QuestId;
            currentTarget = questData.GuideTarget;
            ChangeStep(GuideStep.ClickQuestButton);
        }

        //퀘스트를 직접 클릭했을 때 가이드 시작
        public void StartQuestGuide(QuestData questData)
        {
            if (questData == null) return;

            if (QuestManager.Instance != null && QuestManager.Instance.CanClaimReward(questData))
            {
                EndGuide();
                return;
            }

            currentGuideQuestId = questData.QuestId;
            StartGuide(questData);
        }

        //퀘스트 가이드 시작
        private void StartGuide(QuestData questData)
        {
            if (questData.GuideTarget == GuideTarget.None)
            {
                EndGuide();
                return;
            }

            currentTarget = questData.GuideTarget;

            switch (currentTarget)
            {
                case GuideTarget.Party:
                    ChangeStep(GuideStep.SelectHero);
                    break;

                case GuideTarget.HeroUpgrade:
                    ChangeStep(GuideStep.SelectHero);
                    break;

                case GuideTarget.HeroSummon:
                    ChangeStep(GuideStep.ClickSummon);
                    break;

                case GuideTarget.Battle:
                    ChangeStep(GuideStep.ClickStageStart);
                    break;
            }
        }

        //가이드 단계 변경
        public void ChangeStep(GuideStep nextStep)
        {
            currentStep = nextStep;

            guideArrow.Hide();

            OnGuideStepChanged?.Invoke(currentStep);
        }

        //현재 가이드 단계 확인
        public bool IsStep(GuideStep step)
        {
            return currentStep == step;
        }

        //현재 가이드 목적지 확인
        public bool IsTarget(GuideTarget target)
        {
            return currentTarget == target;
        }

        //가이드 화살표 표시
        public void ShowGuide(RectTransform target)
        {
            guideArrow.Show(target);
        }

        //가이드 종료
        public void EndGuide()
        {
            currentGuideQuestId = null;
            currentTarget = GuideTarget.None;
            currentStep = GuideStep.None;

            guideArrow.Hide();

            OnGuideStepChanged?.Invoke(currentStep);
        }

        //가이드 상태는 유지하고 화살표만 숨기기
        public void HideGuide()
        {
            guideArrow.Hide();
        }

        //가이드 화살표 잠시 숨기기
        public void PauseGuide()
        {
            guideArrow.Pause();
        }

        //잠시 숨긴 가이드 다시 표시
        public void ResumeGuide()
        {
            guideArrow.Resume();
        }
    }
}