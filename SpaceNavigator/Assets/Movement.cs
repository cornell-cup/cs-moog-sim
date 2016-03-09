using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour
{
    public float TURN_DELTA = 0.2f;
    public float SPEED_DELTA = 0.2f;

    //private float[] velocity = { 0, 0, 0, 0, 0, 0 };
    //private float[] acceleration = { 0, 0, 0, 0, 0, 0 };
    private const float MAX_ANG_VEL = 30;
    private const float MIN_ANGLE = 200;
    private const float MAX_ANGLE = 160;
    private Vector3 vel;
    private float smooth = 0.2f;
    private enum Motion { Angular, Linear };
    private string[][] controls = { new string[] { "Pitch", "Yaw", "Roll" }, new string[] { "Sway", "Heave", "Surge" } };
    //private float[][] velocityLimits = { new float[] { 40, 30, 30 }, new float[] { 0.5f, 0.3f, 0.5f } };

    private Rigidbody rb;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = MAX_ANG_VEL;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 currentRotation = transform.rotation.eulerAngles;
        Vector3 r = currentRotation;
        r.x = r.x <= MAX_ANGLE || r.x >= MIN_ANGLE ? r.x : r.x < 180 ? MAX_ANGLE : MIN_ANGLE;
        r.y = r.y <= MAX_ANGLE || r.y >= MIN_ANGLE ? r.y : r.y < 180 ? MAX_ANGLE : MIN_ANGLE;
        r.z = r.z <= MAX_ANGLE || r.z >= MIN_ANGLE ? r.z : r.z < 180 ? MAX_ANGLE : MIN_ANGLE;

        if (!r.Equals(currentRotation))
        {
            //TODO smoother/slowed down stop around limits
            transform.Rotate(r - currentRotation);
            rb.Sleep();
        }
        else {
            Vector3 x = transform.right.normalized;
            Vector3 y = transform.up.normalized;
            Vector3 z = transform.forward.normalized;
            Vector3[] axes = { x, y, z };

            rb.AddTorque(move(axes, Motion.Angular) * TURN_DELTA);
            rb.AddForce(move(axes, Motion.Linear) * SPEED_DELTA);

            //Vector3 v = rb.velocity;
            //Vector3 a = rb.angularVelocity;
            //float[] newVelocity = { a.x, a.y, a.z, a.x, a.y, a.z };
            //for (int i = 0; i < 6; i++) acceleration[i] = newVelocity[i] - velocity[i];
            //velocity = newVelocity;
            //TODO send velocity and acceleration to MOOG
        }
    }

    Vector3 move(Vector3[] vectors, Motion m)
    {
        Vector3 force = new Vector3();
        for (int i = 0; i < 3; i++)
        {
            force += vectors[i] * Input.GetAxis(controls[(int)m][i]);
        }
        return force;
    }
}
