using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARFoundation;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObjectPool : MonoBehaviour
{
    [Header("Prefab to Pool")]  

    [Range(5,50)]
    public int poolSize;
    public GameObject[] ballsPrefab;
    public GameObject debugHead;
    
    private Transform _shootOrigin;
    private Vector3 _targetPosition;
    private ARFaceManager _faceManager;
    private ARFace _arFace;

    [SerializeField] private float angle = 30f;
    [SerializeField] private Vector3 shootPos;
    [SerializeField] private Quaternion shootRot;
    
    private Dictionary<GameObject, UnityEngine.Pool.ObjectPool<GameObject>> _poolsDict;
    
    void Start()
    {
        _shootOrigin = FindObjectOfType<XROrigin>().transform;
        _faceManager = _shootOrigin.GetComponent<ARFaceManager>();
        shootPos = _shootOrigin.position + new Vector3(0, 1.5f, -2f);
        
        // 프리팹으로 풀 초기화
        _poolsDict = new Dictionary<GameObject, UnityEngine.Pool.ObjectPool<GameObject>>();
        foreach (GameObject prefab in ballsPrefab)
        {
            GameObject poolParent = new GameObject(prefab.name);
            poolParent.transform.parent = gameObject.transform;
            _poolsDict.Add(prefab, MakeObjectPool(prefab, poolParent.transform));
        }
    }

    private UnityEngine.Pool.ObjectPool<GameObject> MakeObjectPool(GameObject go, Transform parent)
    {
        return new UnityEngine.Pool.ObjectPool<GameObject>(
            () =>  Instantiate(go, parent),
            obj =>
            {
                obj.transform.position = shootPos;
                obj.SetActive(true);
            },
            obj =>
            {
                obj.GetComponent<Rigidbody>().velocity = Vector3.zero;
                obj.SetActive(false);
            },
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
    public void ShootBallFromPool()
    {
        int rand = Random.Range(0, 4);
        GameObject? ball = SpawnBallFromPool(ballsPrefab[rand]);

        if (ball == null)
        {
            return;
        }
        
        AddForceToTarget(ball);
    }
    
    // 프리팹 번호로 Instnatiated
    public void ShootBallFromPool(int specificBall)
    {
        int rand = Random.Range(0, 4);
        GameObject? ball = SpawnBallFromPool(ballsPrefab[rand]);

        if (ball == null)
        {
            return;
        }

        AddForceToTarget(ball);
    }
    
    void AddForceToTarget(GameObject ball)
    {
        FindTarget();
        
        Rigidbody rigid = ball.GetComponent<Rigidbody>();

        
        // 목표 위치로의 방향 계산        
        Vector3 direction = _targetPosition - ball.transform.position;
        float h = direction.y;
        direction.y = 0;
        
        // 목표 위치까지의 거리 계산
        float distance = direction.magnitude;
        float radianAngle = angle * Mathf.Deg2Rad;

        float gravity = Physics.gravity.y;
        float velocity = Mathf.Sqrt(distance * Mathf.Abs(gravity) / (distance * Mathf.Tan(radianAngle) + h));

        Debug.Log("D : " + distance + " Rad :" + radianAngle + " G : " + gravity + " V : " + velocity);
        
        Vector3 velocityVec = direction.normalized;
        velocityVec *= velocity * Mathf.Cos(radianAngle);
        velocityVec.y = velocity * Mathf.Sin(radianAngle);

        rigid.velocity = velocityVec;
    }

    private void FindTarget()
    {
        // 디버그용
        if (debugHead != null 
            && debugHead.gameObject != null 
            && debugHead.gameObject.activeSelf)
        {
            _targetPosition = debugHead.transform.position;
            return;
        }

        // 이미 찾아놓은 얼굴이 있다면 리턴
        if (_arFace != null 
            && _arFace.gameObject != null 
            && _arFace.gameObject.activeSelf)
        {
            return;
        }
        
        foreach (ARFace face in _faceManager.trackables)
        {
            if (face != null && face.isActiveAndEnabled)
            {
                _arFace = face;
                _targetPosition = face.transform.position;
            }
        }
    }
}
