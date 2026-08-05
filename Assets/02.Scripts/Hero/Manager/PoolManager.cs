using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private Dictionary<string, Queue<GameObject>> poolDictionary = 
        new Dictionary<string, Queue<GameObject>>();

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

    // 큐에서 오브젝트를 꺼내오는 함수
    public GameObject SpawnFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        string key = prefab.name;

        // 1. 해당 프리팹의 큐가 아예 없다면 하나 만들어줍니다.
        if (!poolDictionary.ContainsKey(key))
        {
            poolDictionary.Add(key, new Queue<GameObject>());
        }

        GameObject objToSpawn;

        // 큐에 남은 재고가 있다면 꺼내옵니다.
        if (poolDictionary[key].Count > 0)
        {
            objToSpawn = poolDictionary[key].Dequeue();
        }
        // 재고가 없다면 새로 하나 생산합니다.
        else
        {
            objToSpawn = Instantiate(prefab);
            Poolable poolable = objToSpawn.GetComponent<Poolable>();
            if (poolable == null) poolable = objToSpawn.AddComponent<Poolable>();

            poolable.poolKey = key;
        }

        // 위치와 회전값을 맞추고 화면에 보이게 함
        objToSpawn.transform.position = position;
        objToSpawn.transform.rotation = rotation;
        objToSpawn.SetActive(true);

        return objToSpawn;
    }

    // 다 쓴 오브젝트를 창고로 다시 돌려보내는 함수
    public void ReturnToPool(GameObject obj, string key)
    {
        obj.SetActive(false); // 화면에서 숨김

        // 해당 창고에 다시 집어넣어 다음 사용을 대기시킴
        if (poolDictionary.ContainsKey(key))
        {
            poolDictionary[key].Enqueue(obj);
        }
        else
        {
            Destroy(obj); // 큐가 없다면 그냥 파괴
        }
    }
}