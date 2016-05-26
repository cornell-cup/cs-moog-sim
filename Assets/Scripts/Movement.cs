using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    public static Movement instance;
    public float TURN_DELTA = 0.1f; //torque applied from controls in rad/sec^2
    public float SPEED_DELTA = 5; //force applied from controls in 1 m/sec^2
    public float BRAKE_DELTA = 0.01f; //brake decrement
    public float RETURN_DELTA = 0.5f; //return rotation to zero decrement

    //maximum along local axes
    private float MAX_ANG_ACC = Mathf.PI * 400 / 180; //400 deg/sec^2
    private float MAX_LIN_ACC = 49; //0.5 gravity
    public static float MAX_ANG_VEL = Mathf.PI / 6;
    public static float MAX_LIN_VEL = 20;

    //current velocities & accelerations of ship
    private Vector3 linVel, angVel, linAcc, angAcc;

    private Rigidbody rb; //applies forces, returns velocities
    private CamDisplay cam;
    public Text instructions;

    //the two types of motion
    private enum Motion { Angular, Linear };

    //the six dof controller inputs
    private string[][] controls = {
        new string[] { "Pitch", "Yaw", "Roll" },
        new string[] { "Sway", "Heave", "Surge" } };

    // Use this for initialization
    void Start()
    {
        instance = this;
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
        bool canMove = true;
        if (Input.GetKey(KeyCode.JoystickButton4) || Input.GetKey(KeyCode.Z))
        {
            canMove = false;
            rb.angularVelocity = Vector3.Slerp(rb.angularVelocity, Vector3.zero, BRAKE_DELTA);
        }
        if (Input.GetKey(KeyCode.JoystickButton5) || Input.GetKey(KeyCode.X))
        {
            canMove = false;
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, BRAKE_DELTA);
        }
        if (Input.GetKey(KeyCode.JoystickButton3) || Input.GetKey(KeyCode.R))
        {
            canMove = false;
            rb.angularVelocity = Vector3.Slerp(rb.angularVelocity, Vector3.zero, BRAKE_DELTA);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(Vector3.zero), RETURN_DELTA);
        }
        if(canMove)
        {
            rb.AddRelativeForce(SPEED_DELTA * getInput(Motion.Linear));
            rb.AddRelativeTorque(TURN_DELTA * getInput(Motion.Angular));
            //rb.velocity = Vector3.ClampMagnitude(rb.velocity, MAX_LIN_VEL);
            //rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, Mathf.PI / 6);
        }
    }

    // updates the velocity and acceleration variables
    private void updateVelAcc()
    {
        Vector3 v = transform.InverseTransformDirection(rb.velocity);
        Vector3 a = transform.InverseTransformDirection(rb.angularVelocity);
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
    Vector3 getInput(Motion m)
    {
        return new Vector3(
            Input.GetAxis(controls[(int)m][0]), 
            Input.GetAxis(controls[(int)m][1]), 
            Input.GetAxis(controls[(int)m][2]));
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
