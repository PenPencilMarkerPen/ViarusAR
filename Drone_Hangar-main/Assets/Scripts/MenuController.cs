using Vrs.Internal;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] private DroneCamera dCam;
    [SerializeField] private Transform FPVParent;
    private Vrs.Internal.VrsHead head;
    private GameObject MainCam, VrsSDK, Menu, JoyInfo;//, Props;
    private CalibratedJoyInput joy;
    private bool isMenuOpened = false;
    private Vector3 prevCamPos;
    private Quaternion preCamOrient;
    public float debug = 0;

    void Start()
    {
        joy = GameObject.Find("Drone_White").GetComponent<CalibratedJoyInput>();
        MainCam = GameObject.Find("MainCamera");
        VrsSDK = GameObject.Find("ViarusSDK");
        Menu = GameObject.Find("Menu");
        JoyInfo = GameObject.Find("JoyInfo");
        Menu.SetActive(false);
        JoyInfo.SetActive(false);
        JoyInfo.transform.parent = MainCam.transform;
        JoyInfo.transform.localPosition = new Vector3(0, 0, 1.5f);
        VrsViewer.Instance.GazeApi(GazeTag.Hide);
        dCam = MainCam.GetComponent<DroneCamera>();
        head = MainCam.GetComponent<Vrs.Internal.VrsHead>();
        prevCamPos = MainCam.transform.position;
        preCamOrient = MainCam.transform.rotation;
        switchFPV();
    }

    void Update()
    {
        if (ViarusInput.GetKeyDown(ViarusTask.CKeyEvent.KEYCODE_DPAD_CENTER) && !isMenuOpened)
        {
            openMenu();
        }
    }

    public void switchFPV()
    {
        VrsViewer.Instance.UpdateCameraFov(70);
        MainCam.transform.SetParent(null); // Скрипт камеры не успевает на это среагирровать
        head.SetTrackRotation(false);
        dCam.positionBehindDrone = new Vector3(0,0,0);
        dCam.tpsFieldOfView = 110;
        dCam.enabled = true;
        dCam.inputEditorFPS = 1; // Вообще, это не должно требоваться, но ради отладки в редакторе необходимо
        dCam.FPS = true;
        debug = 1;
        closeMenu();
    }
    public void switchTPV()
    {
        VrsViewer.Instance.ResetCameraFov();
        MainCam.transform.SetParent(null);
        head.SetTrackRotation(true);
        dCam.positionBehindDrone = new Vector3(0, 2, -4);
        dCam.tpsFieldOfView = 80;
        dCam.enabled = true;
        dCam.inputEditorFPS = 0; // Вообще, это не должно требоваться, но ради отладки в редакторе необходимо
        dCam.FPS = false;
        debug = 2;
        closeMenu();
    }

    public void switchGround()
    {
        VrsViewer.Instance.ResetCameraFov();
        head.SetTrackRotation(true);
        dCam.enabled = false;
        MainCam.transform.parent = VrsSDK.transform;
        MainCam.transform.position = prevCamPos;
        MainCam.transform.rotation = preCamOrient;
        debug = 3;
        closeMenu();
    }

    public void openMenu()
    {
        isMenuOpened = true;
        dCam.enabled = false;
        VrsViewer.Instance.GazeApi(GazeTag.Show);
        head.SetTrackRotation(true);
        joy.suppress = true;
        Menu.SetActive(true);
        MainCam.GetComponent<Camera>().cullingMask = -1;
        Vector3 newPos = MainCam.transform.forward * 3.5f;
        Menu.transform.position = MainCam.transform.position+newPos;
        Menu.transform.rotation = MainCam.transform.rotation;
        Menu.transform.localEulerAngles = new Vector3(0, Menu.transform.localEulerAngles.y, 0);
    }
    public void closeMenu()
    {
        VrsViewer.Instance.ShowFPS = false;
        isMenuOpened = false;
        VrsViewer.Instance.GazeApi(GazeTag.Hide);
        joy.suppress = false;
        Menu.SetActive(false);
    }
}
