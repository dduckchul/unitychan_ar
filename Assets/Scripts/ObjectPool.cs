using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class ObjectPool : MonoBehaviour
{
    [Header("Prefab to Pool")] 

    [Range(5,50)]
    public int poolSize;
    public GameObject[] ballsPrefab;
    
    private Dictionary<GameObject, ObjectPool<GameObject>> _poolsDict;
    
    void Start()
    {
        // 프리팹으로 풀 초기화
        _poolsDict = new Dictionary<GameObject, ObjectPool<GameObject>>();
        foreach (GameObject prefab in ballsPrefab)
        {
            GameObject poolParent = new GameObject(prefab.name);
            poolParent.transform.parent = gameObject.transform;
            _poolsDict.Add(prefab, MakeObjectPool(prefab, poolParent.transform));
        }
    }

    private ObjectPool<GameObject> MakeObjectPool(GameObject go, Transform parent)
    {
        return new ObjectPool<GameObject>(
            () =>  Instantiate(go, parent),
            obj =>
            {
                obj.transform.position = Vector3.zero;
                obj.SetActive(true);
            },
            obj => obj.SetActive(false),
            obj => Destroy(go),
            true,
            defaultCapacity : poolSize,
            maxSize : poolSize
        );
    }

    #nullable enable
    public GameObject? SpawnBallFromPool(GameObject prefab)
    {
        if (_poolsDict == null || !_poolsDict.ContainsKey(prefab))
        {
            Debug.LogError("풀이 없는 오브젝트 생성 요청임");
            return null;
        }

        return _poolsDict[prefab].Get();
    }

    public void OnTriggerEnter(Collider other)
    {
        GameObject otherCollides = other.gameObject;
        GameObject? collidesPrefab = FindPrefabByName(otherCollides);
        
        if (collidesPrefab == null || !_poolsDict.ContainsKey(collidesPrefab))
        {
            return;
        }
        
        _poolsDict[collidesPrefab].Release(otherCollides);
    }
    
    #nullable enable
    private GameObject? FindPrefabByName(GameObject instantiated)
    {
        foreach (GameObject ballPref in ballsPrefab)
        {
            if (instantiated.name.Contains(ballPref.name))
            {
                return ballPref;
            }
        }

        Debug.Log(instantiated.name);
        return null;
    }

    // 파라미터 없으면 랜덤
    public void GetBallFromPool()
    {
        int rand = Random.Range(0, 4);
        GameObject? ball = SpawnBallFromPool(ballsPrefab[rand]);

        if (ball == null)
        {
            return;
        }
    }
    
    // 프리팹 번호로 Instnatiated
    public void GetBallFromPool(int specificBall)
    {
        int rand = Random.Range(0, 4);
        GameObject? ball = SpawnBallFromPool(ballsPrefab[rand]);

        if (ball == null)
        {
            return;
        }
    }    
}
