using UnityEngine;
using System;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Header("보유 재화")]
    public int currentGold = 1000;  // 테스트를 위해 초기 골드 1000 지급
    public int currentDiamond = 0;
    public int drawTicket = 0;

    public Action<int> OnGoldChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 골드 획득
    public void AddGold(int amount)
    {
        currentGold += amount;
        OnGoldChanged?.Invoke(currentGold);
        Debug.Log($"[재화] 골드 획득: +{amount} / 현재 골드: {currentGold}");
    }

    // 골드 소모
    public bool UseGold(int amount)
    {
        // 보유 골드가 요구 비용보다 많거나 같은지 체크
        if (currentGold >= amount)
        {
            currentGold -= amount;
            OnGoldChanged?.Invoke(currentGold);
            return true; // 소비 성공
        }
        else
        {
            // 재화 부족 처리
            Debug.LogWarning($"[시스템] 골드가 부족합니다! (필요 골드: {amount} / 보유 골드: {currentGold})");
            return false; // 소비 실패
        }
    }
}