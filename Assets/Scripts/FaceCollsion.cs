using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class FaceCollision : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private ARFaceManager _arFaceManager;
    
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _arFaceManager = GameObject.Find("XR Origin").GetComponent<ARFaceManager>();
    }

    // 공에 맞아야되니까 트래킹 끄기
    public void OnCollisionEnter(Collision other)
    {
        _arFaceManager.enabled = false;
        
        Vector3 lastVelocity = other.gameObject.GetComponent<Rigidbody>().velocity;
        Vector3 contactPoint = Vector3.zero;
        
        // 충돌 방향 계산 (첫 번째 접촉 지점 기준)
        if (other.contacts.Length > 0)
        {
            contactPoint = other.contacts[0].point;
        }
        
        Vector3 lastImpactDirection = (contactPoint - transform.position).normalized;
        Debug.Log(lastVelocity+ ", " + lastImpactDirection);
        
        StartCoroutine(TurnOnTracking());        
    }
    
    // 콜리전 일어났을 경우 다시 켜주고 속도 0으로
    private IEnumerator TurnOnTracking()
    {
        yield return new WaitForSeconds(0.2f);
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _arFaceManager.enabled = true;
    }
}
