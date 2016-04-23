using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour {
    
    public float TURN_DELTA = 0.1f; //torque applied from controls in rad/sec^2
    public float SPEED_DELTA = 10; //force applied from controls in 1 m/sec^2
    public float BRAKE_DELTA = 0.01f; //brake decrement

    //maximum along local axes
    private float MAX_ANG_ACC = Mathf.PI * 400 / 180; //400 deg/sec^2
    private float MAX_LIN_ACC = 49; //0.5 gravity
    public static float MAX_ANG_VEL = Mathf.PI / 6;
    public static float MAX_LIN_VEL = 10;

    //current velocities & accelerations of ship
    private Vector3 linVel, angVel, linAcc, angAcc;

    private Rigidbody rb; //applies forces, returns velocities
    private CamDisplay cam;

    //the two types of motion
    private enum Motion { Angular, Linear };

    //the six dof controller inputs
    private string[][] controls = {
        new string[] { "Pitch", "Yaw", "Roll" },
        new string[] { "Sway", "Heave", "Surge" } };

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = FindObjectOfType<CamDisplay>();

        //apply acceleration limits from MOOG
        TURN_DELTA = Mathf.Min(Mathf.Abs(TURN_DELTA), MAX_ANG_ACC);
        SPEED_DELTA = Mathf.Min(Mathf.Abs(SPEED_DELTA), MAX_LIN_ACC);
    }

    // Update is called once per frame
    void Update()
    {
        applyForces();
        updateVelAcc();
    }

    // logs and applies forces from controls to ship
    public void applyForces()
    {
        if (Input.GetAxis("Brake") == 0)
        {
            Vector3 x = transform.right.normalized;
            Vector3 y = transform.up.normalized;
            Vector3 z = transform.forward.normalized;
            Vector3[] axes = { x, y, z };

            rb.AddForce(SPEED_DELTA * globalize(axes, Motion.Linear));
            rb.AddTorque(TURN_DELTA * globalize(axes, Motion.Angular));
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, MAX_LIN_VEL);
            rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, Mathf.PI / 6);
        }
        else
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, BRAKE_DELTA);
            rb.angularVelocity = Vector3.Slerp(rb.angularVelocity, Vector3.zero, BRAKE_DELTA);
        }
    }

    // updates the velocity and acceleration variables
    private void updateVelAcc()
    {
        Vector3 v = localize(rb.velocity, transform);
        Vector3 a = localize(rb.angularVelocity, transform);
        cam.updateLin(v);
        cam.updateAng(a);

        // convert from Unity to MOOG orientations
        v = new Vector3(v.x, -v.y, v.z);
        a = new Vector3(-a.x, a.y, -a.z);

        linAcc = v - linVel;
        angAcc = a - angVel;
        linVel = v;
        angVel = a;
    }

    // translates force vectors (from controls) along local axes to global force vectors
    Vector3 globalize(Vector3[] axes, Motion m)
    {
        Vector3 force = new Vector3();
        for (int i = 0; i < 3; i++)
        {
            force += axes[i] * Input.GetAxis(controls[(int)m][i]);
        }
        return force;
    }

    // converts vector from global to local axes
    private Vector3 localize(Vector3 v, Transform transform)
    {
        return new Vector3(
            getScalar(v, transform.right),
            getScalar(v, transform.up),
            getScalar(v, transform.forward));
    }

    // returns scalar s where projection of v onto n = s*n
    private float getScalar(Vector3 v, Vector3 n)
    {
        return Vector3.Dot(Vector3.Project(v, n), n) / Vector3.Dot(n, n);
    }

    public Vector3 getLinVel()
    {
        return linVel;
    }

    public Vector3 getAngVel()
    {
        return angVel;
    }

    public Vector3 getLinAcc()
    {
        return linAcc;
    }

    public Vector3 getAngAcc()
    {
        return angAcc;
    }
}
