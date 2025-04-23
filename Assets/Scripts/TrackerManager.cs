using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class TrackerManager : MonoBehaviour
{
    private ARFaceManager _arFaceManager;
    private ARHumanBodyManager _arHumanBodyManager;
    private BodyTracker3D _bodyTracker3D;
    
    [SerializeField]
    private bool isFirstFaceTracking = true;
    public Button button;
    private TextMeshProUGUI _btnText;
    private string _currentMode = "Body"; 
    
    private void Awake()
    {
        _arFaceManager = GetComponent<ARFaceManager>();
        _arHumanBodyManager = GetComponent<ARHumanBodyManager>();
        _bodyTracker3D = GetComponent<BodyTracker3D>();
        _btnText = button.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (isFirstFaceTracking)
        {
            TurnOnFaceTracker();
            _currentMode = "Face";
        }
        else
        {
            TurnOnBodyTracker();
            _currentMode = "Body";
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
        FindAndDestroyTrackables();
    }

    public void TurnOnBodyTracker()
    {
        _arHumanBodyManager.enabled = true;
        _bodyTracker3D.enabled = true;
        _arFaceManager.enabled = false;
        _btnText.text = "Face";
        FindAndDestroyTrackables();
    }
}
