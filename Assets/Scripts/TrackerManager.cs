using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TrackerManager : MonoBehaviour
{
    private ARFaceManager _arFaceManager;
    private ARHumanBodyManager _arHumanBodyManager;
    private BodyTracker3D _bodyTracker3D;
    
    [SerializeField]
    private bool isFirstFaceTracking = true;
    public Button camChangeButton;
    public Button debugEyeButton;
    public GameObject bodyTrackingButtons;
    public GameObject faceTrackingButtons;
    
    private TextMeshProUGUI _btnText;
    public UnityEvent debugEyeTracking;
    
    private void Awake()
    {
        _arFaceManager = GetComponent<ARFaceManager>();
        _arHumanBodyManager = GetComponent<ARHumanBodyManager>();
        _bodyTracker3D = GetComponent<BodyTracker3D>();
        _btnText = camChangeButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (isFirstFaceTracking)
        {
            TurnOnFaceTracker();
        }
        else
        {
            TurnOnBodyTracker();
        }
    }
    
    public void OnChangeTracker()
    {
        if (_btnText.text.Equals("Face"))
        {
            TurnOnFaceTracker();
        }
        else
        {
            TurnOnBodyTracker();
        }
    }

    public void FindAndDestroyTrackables()
    {
        var trackables = gameObject.transform.Find("Trackables");

        if (trackables == null || trackables.childCount == 0)
        {
            return;
        }

        for (int i = 0; i < trackables.childCount; i++)
        {
            Destroy(trackables.GetChild(i).gameObject);
        }
    }

    public void TurnOnFaceTracker()
    {
        _arFaceManager.enabled = true;
        _arHumanBodyManager.enabled = false;
        _bodyTracker3D.enabled = false;
        _btnText.text = "Body";
        bodyTrackingButtons.SetActive(false);
        faceTrackingButtons.SetActive(true);
        FindAndDestroyTrackables();
    }

    public void TurnOnBodyTracker()
    {
        _arHumanBodyManager.enabled = true;
        _bodyTracker3D.enabled = true;
        _arFaceManager.enabled = false;
        _btnText.text = "Face";
        bodyTrackingButtons.SetActive(true);
        faceTrackingButtons.SetActive(false);
        FindAndDestroyTrackables();
    }

    public void InvokeEvent()
    {
        debugEyeTracking.Invoke();
    }
}
