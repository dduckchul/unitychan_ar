using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class BodyTracker3D : MonoBehaviour
{
    private ARHumanBodyManager _arHumanBodyManager;
    private Dictionary<int, GameObject> _jointObjects;
    
    [SerializeField] private GameObject jointPrefab;
    
    private void Awake()
    {
        _arHumanBodyManager = GetComponent<ARHumanBodyManager>();
        _jointObjects = new Dictionary<int, GameObject>();
    }

    private void OnEnable()
    {
        _arHumanBodyManager.humanBodiesChanged += OnHumanBodyChanged;
    }

    private void OnDisable()
    {
        _arHumanBodyManager.humanBodiesChanged -= OnHumanBodyChanged;
        _jointObjects.Clear();
    }

    void OnHumanBodyChanged(ARHumanBodiesChangedEventArgs eventArgs)
    {
        foreach (ARHumanBody arHumanBody in eventArgs.updated)
        {
            NativeArray<XRHumanBodyJoint> jointArrays = arHumanBody.joints;

            foreach (XRHumanBodyJoint joint in jointArrays)
            {
                GameObject obj;
                if (!_jointObjects.TryGetValue(joint.index, out obj))
                {
                    obj = Instantiate(jointPrefab);
                    _jointObjects.Add(joint.index, obj);
                }

                // update joint transform
                if (joint.tracked)
                {
                    obj.transform.parent = arHumanBody.transform;
                    obj.transform.localPosition = joint.anchorPose.position;
                    obj.transform.localRotation = joint.anchorPose.rotation;
                    obj.SetActive(true);
                }
                else
                {
                    obj.SetActive(false);
                }
            }
        }
    }
}
