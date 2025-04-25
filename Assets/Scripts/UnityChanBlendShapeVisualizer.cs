using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARKit;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARFace))]
public class UnityChanBlendShapeVisualizer : MonoBehaviour
{
    [SerializeField] float m_CoefficientScale = 100.0f;

    public float coefficientScale
    {
        get { return m_CoefficientScale; }
        set { m_CoefficientScale = value; }
    }

    [SerializeField] SkinnedMeshRenderer m_SkinnedMeshRenderer;

    public SkinnedMeshRenderer skinnedMeshRenderer
    {
        get
        {
            return m_SkinnedMeshRenderer;
        }
        set
        {
            m_SkinnedMeshRenderer = value;
            CreateFeatureBlendMapping();
        }
    }

    ARKitFaceSubsystem m_ARKitFaceSubsystem;
    Dictionary<ARKitBlendShapeLocation, int> m_FaceArkitBlendShapeIndexMap;
    ARFace m_Face;
    private GameObject m_HeadInstance;
    
    [Header("For Eye Tracking")]
    public bool isEyeTrackable;
    private bool _isEyeTrackable;
    
    public Transform m_LeftEye;
    public Transform m_RightEye;

    public Vector3 m_EyeModelDiff;
    
    private Vector3 m_LeftEyeBeforePos;
    private Vector3 m_RightEyeBeforePos;
    
    [Header("For Eye Tracking Debugging")]
    public GameObject DebugEyePrefab;
    private bool DebugEyeTracking;

    private GameObject debugLeftEye;
    private GameObject debugRightEye;
    
    void Awake()
    {
        m_Face = GetComponent<ARFace>();
        m_HeadInstance = m_Face.gameObject;
        CreateFeatureBlendMapping();
    }

    void Start()
    {
        TrackerManager man = FindObjectOfType<TrackerManager>();

        if (man != null && man.debugEyeTracking != null)
        {
            man.debugEyeTracking.AddListener(OnDebugEyeTracking);            
        }
    }

    private void OnDestroy()
    {
        TrackerManager man = FindObjectOfType<TrackerManager>();

        if (man != null && man.debugEyeTracking != null)
        {
            man.debugEyeTracking.RemoveListener(OnDebugEyeTracking);            
        }
    }

    void CreateFeatureBlendMapping()
    {
        if (skinnedMeshRenderer == null || skinnedMeshRenderer.sharedMesh == null)
        {
            return;
        }

        const string strPrefix = "face.";

        // 유니티쨩 얼굴에 맞게 컨버팅 해주기
        m_FaceArkitBlendShapeIndexMap = new Dictionary<ARKitBlendShapeLocation, int>();
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.BrowDownLeft        ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "BLW_sad");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.BrowDownRight       ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "BLW_sad");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.BrowInnerUp         ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "BLW_sup");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.BrowOuterUpLeft     ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "BLW_ang");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.BrowOuterUpRight    ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "BLW_ang");

        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeBlinkLeft        ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "EYE_blk");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeBlinkRight       ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "EYE_blk");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeSquintLeft       ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "EYE_rlx");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeSquintRight      ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "EYE_rlx");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeWideLeft         ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "EYE_sup");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeWideRight        ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "EYE_sup");

        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.JawOpen             ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "MTH_sml2");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthFunnel         ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "MTH_o");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthLowerDownLeft  ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "MTH_e");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthLowerDownRight ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "MTH_e");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthPucker         ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "MTH_u");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthSmileLeft      ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "MTH_L");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthSmileRight     ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "MTH_R");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthStretchLeft    ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "MTH_i");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthStretchRight   ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "MTH_i");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthUpperUpLeft    ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "MTH_a");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthUpperUpRight   ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "MTH_a");

        # region 미사용 코드
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.CheekPuff           ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "cheekPuff");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.CheekSquintLeft     ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "cheekSquint_L");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.CheekSquintRight    ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "cheekSquint_R");        

        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeLookDownLeft     ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "eyeLookDown_L");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeLookDownRight    ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "eyeLookDown_R");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeLookInLeft       ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "eyeLookIn_L");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeLookInRight      ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "eyeLookIn_R");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeLookOutLeft      ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "eyeLookOut_L");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeLookOutRight     ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "eyeLookOut_R");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeLookUpLeft       ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "eyeLookUp_L");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeLookUpRight      ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "eyeLookUp_R");

        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.JawForward          ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "jawForward");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.JawLeft             ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "jawLeft");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.JawRight            ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "jawRight");

        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthClose          ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthClose");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthDimpleLeft     ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthDimple_L");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthDimpleRight    ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthDimple_R");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthFrownLeft      ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthFrown_L");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthFrownRight     ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthFrown_R");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthLeft           ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthLeft");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthPressLeft      ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthPress_L");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthPressRight     ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthPress_R");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthRight          ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthRight");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthRollLower      ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthRollLower");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthRollUpper      ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthRollUpper");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthShrugLower     ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthShrugLower");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthShrugUpper     ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthShrugUpper");

        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.NoseSneerLeft       ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "noseSneer_L");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.NoseSneerRight      ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "noseSneer_R");
        // m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.TongueOut           ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "tongueOut");
        # endregion        
    }

    void SetVisible(bool visible)
    {
        if (m_HeadInstance == null) return;

        for (int i = 0; i < m_HeadInstance.transform.childCount; i++)
        {
            GameObject nthChild = m_HeadInstance.transform.GetChild(i).gameObject;

            if (nthChild.name.Equals("HeadRef") || nthChild.gameObject.name.Equals("Mesh"))
            {
                nthChild.gameObject.SetActive(visible);                
            }
        }
    }

    void UpdateVisibility()
    {
        var visible = 
            enabled 
            && (m_Face.trackingState == TrackingState.Tracking) 
            && (ARSession.state > ARSessionState.Ready)
            && !DebugEyeTracking;
        
        SetVisible(visible);
    }

    void OnEnable()
    {
        var faceManager = FindObjectOfType<ARFaceManager>();
        if (faceManager != null)
        {
            m_ARKitFaceSubsystem = (ARKitFaceSubsystem)faceManager.subsystem;
        }

        if (faceManager.subsystem != null && faceManager.descriptor.supportsEyeTracking && isEyeTrackable)
        {
            _isEyeTrackable = true;
        }
        else
        {
            _isEyeTrackable = false;
        }
        
        UpdateVisibility();
        m_Face.updated += OnUpdated;
        ARSession.stateChanged += OnSystemStateChanged;
    }

    void OnDisable()
    {
        m_Face.updated -= OnUpdated;
        ARSession.stateChanged -= OnSystemStateChanged;
    }

    void OnSystemStateChanged(ARSessionStateChangedEventArgs eventArgs)
    {
        UpdateVisibility();
    }

    // 얼굴 트래킹이 한개라면 eventArgs 쓸 필요 없음
    void OnUpdated(ARFaceUpdatedEventArgs eventArgs)
    {
        UpdateVisibility();
        UpdateFaceFeatures();
    }

    void UpdateFaceFeatures()
    {
        if (skinnedMeshRenderer == null || !skinnedMeshRenderer.enabled || skinnedMeshRenderer.sharedMesh == null)
        {
            return;
        }
        
        if (_isEyeTrackable)
        {
            UpdateEyePosition();
        }

        UpdateBlendShapes();
    }

    public void OnDebugEyeTracking()
    {
        DebugEyeTracking = !DebugEyeTracking;

        if (m_Face == null)
        {
            m_Face = GetComponent<ARFace>();
        }
        
        if (DebugEyeTracking && debugLeftEye == null)
        {
            debugLeftEye = Instantiate(DebugEyePrefab, m_Face.leftEye);
            debugRightEye = Instantiate(DebugEyePrefab, m_Face.rightEye);
        }
        
        debugLeftEye.SetActive(DebugEyeTracking);
        debugRightEye.SetActive(DebugEyeTracking);
        UpdateVisibility();
    }

    private void UpdateEyePosition()
    {
        if (m_LeftEyeBeforePos != Vector3.zero || m_RightEyeBeforePos != Vector3.zero)
        {
            if (m_LeftEyeBeforePos != m_LeftEye.position)
            {
                // 1. 그냥 포지션 적용해보기
                m_LeftEye.position = m_Face.leftEye.position + m_EyeModelDiff;
                
                // 2. 모델 상대 포지션으로 적용해보기
                // Vector3 leftEyeDiff = (m_Face.leftEye.position - m_LeftEyeBeforePos) / 5f;
                // m_LeftEye.position += leftEyeDiff;
            }

            if (m_RightEyeBeforePos != m_RightEye.position)
            {
                // 1. 그냥 포지션 적용해보기
                m_RightEye.position = m_Face.rightEye.position + m_EyeModelDiff;
                
                // 2. 모델 상대 포지션으로 적용해보기
                // Vector3 rightEyeDiff = (m_Face.rightEye.position - m_RightEyeBeforePos) / 5f;
                // m_RightEye.position += rightEyeDiff;
            }
        }
            
        m_LeftEyeBeforePos = m_Face.leftEye.position;
        m_RightEyeBeforePos = m_Face.rightEye.position;        
    }

    private void UpdateBlendShapes()
    {
        using (var blendShapes = m_ARKitFaceSubsystem.GetBlendShapeCoefficients(m_Face.trackableId, Allocator.Temp))
        {
            foreach (var featureCoefficient in blendShapes)
            {
                int mappedBlendShapeIndex;
                if (m_FaceArkitBlendShapeIndexMap.TryGetValue(featureCoefficient.blendShapeLocation, out mappedBlendShapeIndex))
                {
                    if (mappedBlendShapeIndex >= 0)
                    {
                        skinnedMeshRenderer.SetBlendShapeWeight(mappedBlendShapeIndex, featureCoefficient.coefficient * coefficientScale);
                    }
                }
            }
        }        
    }
}
