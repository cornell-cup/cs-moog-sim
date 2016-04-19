using UnityEngine;
using UnityEngine.UI;
using System;

public class Movement : MonoBehaviour
{
    
    public float TURN_DELTA = 0.1f; //torque applied from controls in rad/sec^2
    public float SPEED_DELTA = 5; //force applied from controls in 1 m/sec^2
    private float MAX_ANG_ACC = Mathf.PI * 400 / 180; //400 deg/sec^2
    private float MAX_LIN_ACC = 49; //0.5 gravity

    //the two types of motion
    private enum Motion { Angular, Linear };

    //the six dof controller input
    private string[][] controls = {
        new string[] { "Pitch", "Yaw", "Roll" },
        new string[] { "Sway", "Heave", "Surge" } };
    
    private Rigidbody rb; //applies forces, returns velocities
    private UDPSend udpSend; //sends motion data to comm
    public Text hud; //head-up display of motion info
    private string log = ""; //next line to be added to log file

    //current rotation, velocities & accelerations of ship
    private Vector3 linVel = new Vector3();
    private Vector3 angVel = new Vector3();
    private Vector3 linAcc, angAcc;
    private Vector3 rotation = new Vector3();

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        udpSend = FindObjectOfType<UDPSend>();

        //apply acceleration limits from MOOG
        TURN_DELTA = Mathf.Min(Mathf.Abs(TURN_DELTA), MAX_ANG_ACC);
        SPEED_DELTA = Mathf.Min(Mathf.Abs(SPEED_DELTA), MAX_LIN_ACC);
    }

    // Update is called once per frame
    void Update()
    {
        UDPSend.newPacket();
        addToLog(Time.time, true);
        applyForces();
        correctOrientation();
        updateVelAcc();
        logsendOutput();
        updateLog();
    }

    // applies forces from controls to ship
    // logs controls
    private void applyForces()
    {
        bool canMove = addToLog(Input.GetAxis("Brake"), false) != 0;

        Vector3 x = transform.right.normalized;
        Vector3 y = transform.up.normalized;
        Vector3 z = transform.forward.normalized;
        Vector3[] axes = { x, y, z };

        Vector3 ang = globalize(axes, Motion.Angular);
        Vector3 lin = globalize(axes, Motion.Linear);

        rb.AddTorque(TURN_DELTA * (canMove ? ang : -rb.angularVelocity));
        rb.AddForce(SPEED_DELTA * (canMove ? lin : -rb.velocity));
    }

    // translates force vectors (from controls) along local axes to global force vectors
    Vector3 globalize(Vector3[] axes, Motion m)
    {
        Vector3 force = new Vector3();
        for (int i = 0; i < 3; i++)
        {
            force += axes[i] * addToLog(Input.GetAxis(controls[(int)m][i]), false);
        }
        return force;
    }

    // cancels out MOOG rotations from the camera
    private void correctOrientation()
    {
        byte[] packet = UDPReceive.getMOOGData();
        float time = BitConverter.ToSingle(packet, 0);
        Vector3 orientation = new Vector3(
            BitConverter.ToSingle(packet, 4),
            BitConverter.ToSingle(packet, 8),
            BitConverter.ToSingle(packet, 12));
        transform.Rotate(-orientation);
    }

    // updates current velocities and accelerations vars and on GUI
    private void updateVelAcc()
    {
        // TODO replace with rb.GetRelativePointVelocity(seat position from center)
        Vector3 v = localize(rb.velocity);
        Vector3 a = localize(rb.angularVelocity);

        // convert from Unity to MOOG orientations
        v = new Vector3(v.x, -v.y, v.z);
        a = new Vector3(-a.x, a.y, -a.z);

        rotation = transform.rotation.eulerAngles * Mathf.PI / 180;
        linAcc = v - linVel;
        angAcc = a - angVel;
        linVel = v;
        angVel = a;

        //update gui 
        hud.text = "pos: " + fixLen(transform.position);
        hud.text += "\trot: " + fixLen(degModAngle(rotation));
        hud.text += "\nvel: " + fixLen(linVel);
        hud.text += "\tvel: " + fixLen(degModAngle(angVel));
    }

    // converts vector from global to local axes
    private Vector3 localize(Vector3 v)
    {
        return new Vector3(
            getScalar(v, transform.right),
            getScalar(v, transform.up),
            getScalar(v, transform.forward));
    }

    // returns scalar s where s*n is the projection of v onto n
    private float getScalar(Vector3 v, Vector3 n)
    {
        return Vector3.Dot(Vector3.Project(v, n), n) / Vector3.Dot(n, n);
    }

    // converts vector from radians to degrees and from [0,360) to [-180,180)
    private Vector3 degModAngle(Vector3 v)
    {
        v *= 180 / Mathf.PI;
        v.x = v.x < 180 ? v.x : v.x - 360;
        v.y = v.y < 180 ? v.y : v.y - 360;
        v.z = v.z < 180 ? v.z : v.z - 360;
        return v;
    }

    // outputs a fixed length Vector3 toString
    private string fixLen(Vector3 v)
    {
        return string.Format("{0,6:F2} {1,6:F2} {2,6:F2}", v.x, v.y, v.z);
    }

    // send ship motion info to comm
    private void logsendOutput()
    {
        addToLog(linVel.magnitude, true);

        foreach (Vector3 v in new Vector3[] { angVel, linAcc, angAcc, /*rotation*/Vector3.zero })
        {
            addToLog(v, true);
        }
        UDPSend.sendPacket();
    }

    // log controller input and ship motion output
    private void updateLog()
    {
        udpSend.logPacket(log);
        log = "";
    }

    private Vector3 addToLog(Vector3 v, bool udpSend)
    {
        addToLog(v.x, udpSend);
        addToLog(v.y, udpSend);
        addToLog(v.z, udpSend);
        return v;
    }

    private float addToLog(float f, bool udpSend)
    {
        log += string.Format(" {0,8:F6}", f);
        if (udpSend) UDPSend.addFloat(f);
        return f;
    }
}
