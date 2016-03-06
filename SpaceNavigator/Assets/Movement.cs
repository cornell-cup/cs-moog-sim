using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour
{

    public float TURN_DELTA = 0.2f;
    public float SPEED_DELTA = 0.2f;
    public float MAX_TURN = 1.0f;
    private float[] lastVelocity = { 0, 0, 0, 0, 0, 0 };
    private Rigidbody rb;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = MAX_TURN;
    }

    // Update is called once per frame
    void Update()
    {        
        Vector3 x = transform.right.normalized;
        Vector3 y = transform.up.normalized;
        Vector3 z = transform.forward.normalized;

        Vector3 rot = new Vector3();
        rot += x * Input.GetAxis("Pitch");
        rot += y * Input.GetAxis("Yaw");
        rot += z * Input.GetAxis("Roll");
        rb.AddTorque(rot * TURN_DELTA);

        Vector3 dir = new Vector3();
        dir += x * Input.GetAxis("Sway");
        dir += y * Input.GetAxis("Heave");
        dir += z * Input.GetAxis("Surge");
        rb.AddForce(dir * SPEED_DELTA);

        Vector3 ang = rb.angularVelocity;
        Vector3 vel = rb.velocity;
        float[] currVelocity = { ang.x, ang.y, ang.z, vel.x, vel.y, vel.z };
        float[] currAccel = new float[6];
        for (int i = 0; i < 6; i++) currAccel[i] = currVelocity[i] - lastVelocity[i];

        //TODO send curr Velocity and Accel to MOOG
        Debug.Log(currVelocity);
        Debug.Log(currAccel);

        lastVelocity = currVelocity;
    }
}
