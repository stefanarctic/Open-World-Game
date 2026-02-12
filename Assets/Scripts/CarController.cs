using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    public enum DriveMode { Automatic, Manual }
    public DriveMode driveMode = DriveMode.Automatic;

    [Header("Wheels")]
    public WheelCollider frontLeft;
    public WheelCollider frontRight;
    public WheelCollider rearLeft;
    public WheelCollider rearRight;

    [Header("Engine")]
    public float idleRPM = 900f;
    public float redlineRPM = 8700f;
    public float engineInertia = 0.3f;
    public AnimationCurve torqueCurve;

    [Header("Steering")]
    public float maxSteerAngle = 30f;

    [Header("Transmission")]
    public float finalDrive = 3.73f;
    public float[] gearRatios =
    {
        -2.9f,   // 0 Reverse
         0f,     // 1 Neutral
         3.8f,   // 2 First
         2.3f,
         1.7f,
         1.3f,
         1.05f,
         0.88f,
         0.75f
    };

    public int currentGear = 2;
    public float shiftTime = 0.2f;

    [Header("Automatic Settings")]
    public float shiftUpRPM = 8200f;
    public float shiftDownRPM = 2500f;

    [Header("Drivetrain")]
    [Range(0f, 1f)]
    public float frontTorqueSplit = 0.4f;
    public float drivetrainEfficiency = 0.85f;

    [Header("Brakes")]
    public float brakePower = 4000f;

    private Rigidbody rb;
    public float engineRPM;
    private float throttle;
    private float brake;
    private float clutch = 1f;
    private bool shifting = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        engineRPM = idleRPM;
        rb.centerOfMass = new Vector3(0, -0.5f, 0);

        torqueCurve = new AnimationCurve(
            new Keyframe(900, 200),
            new Keyframe(3000, 500),
            new Keyframe(5000, 650),
            new Keyframe(6750, 720),
            new Keyframe(8500, 600)
        );
    }

    void Update()
    {
        // --- Debugging
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");
        Debug.Log("V: " + v + " H: " + h);
        // ---

        //throttle = Mathf.Clamp01(Input.GetAxis("Vertical"));
        //brake = Mathf.Clamp01(-Input.GetAxis("Vertical"));

        float vertical = Input.GetAxis("Vertical");

        if (vertical > 0f)
        {
            throttle = vertical;
            brake = 0f;

            if (currentGear == 0) // if in reverse
                currentGear = 2;  // switch to first
        }
        else if (vertical < 0f)
        {
            if (GetAverageWheelRPM() < 5f)
            {
                currentGear = 0; // reverse gear
                throttle = -vertical;
                brake = 0f;
            }
            else
            {
                throttle = 0f;
                brake = -vertical;
            }
        }
        else
        {
            throttle = 0f;
            brake = 0f;
        }

        float steerInput = Input.GetAxis("Horizontal");

        frontLeft.steerAngle = steerInput * maxSteerAngle;
        frontRight.steerAngle = steerInput * maxSteerAngle;

        if (driveMode == DriveMode.Manual)
        {
            if (Input.GetKeyDown(KeyCode.E))
                TryShiftUp();

            if (Input.GetKeyDown(KeyCode.Q))
                TryShiftDown();
        }
        else
        {
            if (!shifting)
                AutoShiftLogic();
        }
    }

    void FixedUpdate()
    {
        UpdateEngineRPM();
        ApplyTorque();
        ApplyBrakes();
    }

    void UpdateEngineRPM()
    {
        float wheelRPM = GetAverageWheelRPM();
        float gearRatio = gearRatios[currentGear];

        if (gearRatio != 0 && clutch > 0.05f)
        {
            float targetRPM = Mathf.Abs(wheelRPM * gearRatio * finalDrive);
            engineRPM = Mathf.Lerp(engineRPM, targetRPM, clutch * 8f * Time.fixedDeltaTime);
        }
        else
        {
            float engineTorque = torqueCurve.Evaluate(engineRPM);
            float rpmChange = (engineTorque / engineInertia) * throttle * Time.fixedDeltaTime;
            engineRPM += rpmChange;
        }

        engineRPM = Mathf.Clamp(engineRPM, idleRPM, redlineRPM);
    }

    void ApplyTorque()
    {
        float gearRatio = gearRatios[currentGear];
        if (gearRatio == 0) return;

        float engineTorque = torqueCurve.Evaluate(engineRPM) * throttle;
        float driveTorque = engineTorque * gearRatio * finalDrive * drivetrainEfficiency * clutch;

        float frontTorque = driveTorque * frontTorqueSplit;
        float rearTorque = driveTorque * (1f - frontTorqueSplit);

        frontLeft.motorTorque = frontTorque / 2f;
        frontRight.motorTorque = frontTorque / 2f;
        rearLeft.motorTorque = rearTorque / 2f;
        rearRight.motorTorque = rearTorque / 2f;
    }

    void ApplyBrakes()
    {
        float brakeTorque = brake * brakePower;

        frontLeft.brakeTorque = brakeTorque;
        frontRight.brakeTorque = brakeTorque;
        rearLeft.brakeTorque = brakeTorque;
        rearRight.brakeTorque = brakeTorque;
    }

    float GetAverageWheelRPM()
    {
        return (frontLeft.rpm + frontRight.rpm + rearLeft.rpm + rearRight.rpm) / 4f;
    }

    void TryShiftUp()
    {
        if (shifting) return;
        if (currentGear >= gearRatios.Length - 1) return;

        StartCoroutine(ShiftGear(currentGear + 1));
    }

    void TryShiftDown()
    {
        if (shifting) return;
        if (currentGear <= 0) return;

        StartCoroutine(ShiftGear(currentGear - 1));
    }

    void AutoShiftLogic()
    {
        if (engineRPM > shiftUpRPM && currentGear < gearRatios.Length - 1)
            StartCoroutine(ShiftGear(currentGear + 1));

        if (engineRPM < shiftDownRPM && currentGear > 2)
            StartCoroutine(ShiftGear(currentGear - 1));
    }

    IEnumerator ShiftGear(int newGear)
    {
        shifting = true;

        // disengage clutch smoothly
        float t = 0f;
        while (t < shiftTime)
        {
            clutch = Mathf.Lerp(1f, 0f, t / shiftTime);
            t += Time.deltaTime;
            yield return null;
        }

        currentGear = newGear;

        t = 0f;
        while (t < shiftTime)
        {
            clutch = Mathf.Lerp(0f, 1f, t / shiftTime);
            t += Time.deltaTime;
            yield return null;
        }

        clutch = 1f;
        shifting = false;
    }
}