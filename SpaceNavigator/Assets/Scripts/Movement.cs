using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class Movement : MonoBehaviour
{

    //torque applied from controls in rad/sec^2
    public float TURN_DELTA = 0.1f;
    //force applied from controls in 1 m/sec^2
    public float SPEED_DELTA = 5;

    //the two types of motion
    private enum Motion { Angular, Linear };

    private string[][] controls = {
        new string[] { "Pitch", "Yaw", "Roll" },
        new string[] { "Sway", "Heave", "Surge" } };

    //maximum velocity along local axes
    //private Vector3 MAX_ANG_VEL = new Vector3(Mathf.PI / 3, 4 * Mathf.PI / 9, Mathf.PI / 3);
    //private Vector3 MAX_LIN_VEL = new Vector3(10, 6, 10);

    //min/max angle of the ship
    private const float MIN_ANGLE = 0;
    private const float MAX_ANGLE = 360;

    private string label; //GUI label for position, rotation & ang/lin velocity
    private Rigidbody rb; //applies forces, returns velocities
    private UDPSend udp; //communication channel
    public Text hud; //head-up display of motion info

    //current rotation, velocities & accelerations of ship
    private Vector3 linVel = new Vector3();
    private Vector3 angVel = new Vector3();
    private Vector3 linAcc, angAcc;
    private Vector3 rotation = new Vector3();

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        udp = FindObjectOfType<UDPSend>();

        //acceleration limits from MOOG
        TURN_DELTA = Mathf.Min(TURN_DELTA, Mathf.PI * 400 / 180); //400 deg/sec^2
        SPEED_DELTA = Mathf.Min(SPEED_DELTA, 49); //0.5 G
    }

    // Update is called once per frame
    void Update()
    {
        applyForces();
        limitRotation();
        //limitVelocity(Motion.Angular, MAX_ANG_VEL);
        //limitVelocity(Motion.Linear, MAX_LIN_VEL);
        updateVelAcc();
        sendData();
        receiveData();
    }

    // applies forces from controls to ship
    private void applyForces()
    {
        Vector3 x = transform.right.normalized;
        Vector3 y = transform.up.normalized;
        Vector3 z = transform.forward.normalized;
        Vector3[] axes = { x, y, z };

        rb.AddTorque(move(axes, Motion.Angular) * TURN_DELTA);
        rb.AddForce(move(axes, Motion.Linear) * SPEED_DELTA);
    }

    // translates force vectors (from controls) along local axes to global force vectors
    Vector3 move(Vector3[] vectors, Motion m)
    {
        Vector3 force = new Vector3();
        for (int i = 0; i < 3; i++)
        {
            force += vectors[i] * Input.GetAxis(controls[(int)m][i]);
        }
        return force;
    }

    // brings ship to a stop if it has breached angular bounds along any axis
    private void limitRotation()
    {
        Vector3 r = transform.rotation.eulerAngles;
        Vector3 rv = rb.angularVelocity;
        Vector3 lerp = Vector3.Slerp(rv, new Vector3(), 0.05f);

        if (outOfBounds(r.x, rv.x) || outOfBounds(r.y, rv.y) || outOfBounds(r.z, rv.z))
            rb.angularVelocity = lerp;
    }

    // returns whether ang is out of bounds and vel is moving further away from bounds
    private bool outOfBounds(float ang, float vel)
    {
        return (ang > MAX_ANGLE && ang < 180 && vel > 0.01) ||
             (ang < MIN_ANGLE && ang > 180 && vel < -0.01);
    }

    // clamps velocity of type m to limit
    private void limitVelocity(Motion m, Vector3 limit)
    {
        Vector3 v = (m == Motion.Angular) ? rb.angularVelocity : rb.velocity;
        Vector3 limV = v;
        limV.x = Mathf.Clamp(v.x, -limit.x, limit.x);
        limV.y = Mathf.Clamp(v.y, -limit.y, limit.y);
        limV.z = Mathf.Clamp(v.z, -limit.z, limit.z);

        if (!v.Equals(limV))
        {
            if (m == Motion.Angular)
                rb.angularVelocity = limV;
            else
                rb.velocity = limV;
        }
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

    // log and send ship motion info to comm
    private void sendData()
    {
        string msg = "";
        UDPSend.newPacket();
        msg += string.Format("{0,8:F6}", UDPSend.addFloat(Time.time));
        msg += string.Format(" {0,8:F6}", UDPSend.addFloat(linVel.magnitude));

        foreach (Vector3 v in new Vector3[] { angVel, linAcc, angAcc, /*rotation*/new Vector3() })
        {
            msg += string.Format(" {0,8:F6}", UDPSend.addFloat(v.x));
            msg += string.Format(" {0,8:F6}", UDPSend.addFloat(v.y));
            msg += string.Format(" {0,8:F6}", UDPSend.addFloat(v.z));
        }

        udp.logPacket(msg);
        UDPSend.sendPacket();
    }

    private void receiveData()
    {
        byte[] data = UDPReceive.getMOOGData();

        float time = BitConverter.ToSingle(data, 0);
        float roll = BitConverter.ToSingle(data, 4);
        float pitch = BitConverter.ToSingle(data, 8);
        float yaw = BitConverter.ToSingle(data, 12);

        string msg = "" + time + " " + roll + " " + pitch + " " + yaw;
        if(time > 0) Debug.Log(msg);
    }
}
