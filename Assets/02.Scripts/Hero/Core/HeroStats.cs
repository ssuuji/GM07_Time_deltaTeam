using UnityEngine;

public class HeroStats : MonoBehaviour
{
    // 타겟 생존 판별을 위해 선언만 해둠
    public int currentHP = 100;

    public void Init(HeroInstance heroInstance) { }
    public void TakeDamage(int damage) { }
    public void Heal(int amount) { }
    public void AddShield(int amount) { }
}