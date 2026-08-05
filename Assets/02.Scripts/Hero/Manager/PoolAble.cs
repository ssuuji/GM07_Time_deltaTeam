using UnityEngine;

public class Poolable : MonoBehaviour
{
    // 내가 돌아갈 창고의 이름
    public string poolKey;

    // 즉시 창고로 돌아가기
    public void Release()
    {
        PoolManager.Instance.ReturnToPool(gameObject, poolKey);
    }

    // 일정 시간 뒤에 창고로 돌아가기
    public void ReleaseAfter(float delay)
    {
        Invoke(nameof(Release), delay);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }
}
