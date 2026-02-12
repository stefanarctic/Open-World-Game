using UnityEngine;

public class Car : MonoBehaviour
{
    public string carName;
    public float maxTorque;

    float[] gearRatios = {
    -3.2f,   // reverse
     0f,     // neutral
     3.5f,   // 1
     2.1f,   // 2
     1.4f,   // 3
     1.0f,   // 4
     0.8f    // 5
};
}
