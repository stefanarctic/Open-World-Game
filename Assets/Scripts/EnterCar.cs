using UnityEngine;

[RequireComponent(typeof(EnterCar))]
public class EnterCar : MonoBehaviour
{
    public Transform seat;
    public Transform exitPoint;
    public float maxDistance = 100f;
    public KeyCode enterCarKey = KeyCode.E;
    public KeyCode exitCarKey = KeyCode.Escape;
    public GameObject player;
    Camera mainCamera;
    CarController carController;
    bool enteredCar = false;

    Vector3 oldPlayerPosition;
    Quaternion oldPlayerRotation;
    public Camera carCamera;

    private void Awake()
    {
        //mainCamera = Camera.main;
        //player = mainCamera.transform.parent.gameObject;
        mainCamera = player.GetComponentInChildren<Camera>();
        carController = GetComponent<CarController>();
    }

    private void Update()
    {
        if (enteredCar)
            return;
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, maxDistance))
        {
            //print("Looking at: " + hit.collider.gameObject.name);
            if(hit.collider.gameObject == gameObject)
            {
                print("Looking at " + gameObject.name);
                if (Input.GetKeyDown(enterCarKey))
                    Enter();
            }
        }

        if(enteredCar && Input.GetKeyDown(exitCarKey))
            Exit();
    }
    void Enter()
    {
        if (enteredCar)
            return;

        enteredCar = true;
        // Disable controller before teleport (important)
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        //oldPlayerTransform = player.transform;
        //oldPlayerPosition = playe
        player.transform.SetParent(seat);

        player.transform.localPosition = Vector3.zero;
        player.transform.localRotation = Quaternion.identity;
        //player.transform.position = seat.position;
        //player.transform.rotation = seat.rotation;


        // Don't enable CharacterController
        //if (cc != null) cc.enabled = true;

        mainCamera.enabled = false;
        carCamera.enabled = true;
        carController.enabled = true;

        print("Entered car");
    }

    void Exit()
    {
        enteredCar = false;
        carCamera.enabled = false;
        mainCamera.enabled = true;
        carController.enabled = false;

        player.transform.SetParent(null);

        player.transform.position = exitPoint.position;
        player.transform.rotation = exitPoint.rotation;
        //player.transform.parent = null;
        //player.transform = oldPlayerTransform;
    }
}
