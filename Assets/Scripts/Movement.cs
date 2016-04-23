using UnityEngine;
using UnityEngine.UI;
using System;

public class Movement : MonoBehaviour
{
    public float TURN_DELTA = 0.1f; //torque applied from controls in rad/sec^2
    public float SPEED_DELTA = 10; //force applied from controls in 1 m/sec^2
    public float BRAKE_DELTA = 0.01f; //brake decrement

    //the two types of motion
    private enum Motion { Angular, Linear };

    //the six dof controller inputs
    private string[][] controls = {
        new string[] { "Pitch", "Yaw", "Roll" },
        new string[] { "Sway", "Heave", "Surge" } };

    private Rigidbody rb; //applies forces, returns velocities

    //maximum along local axes
    private float MAX_ANG_ACC = Mathf.PI * 400 / 180; //400 deg/sec^2
    private float MAX_LIN_ACC = 49; //0.5 gravity
    public float MAX_ANG_VEL = Mathf.PI / 6;
    public float MAX_LIN_VEL = 10;
    private float SCALE_VEL = 10;

    //current rotation, velocities & accelerations of ship
    private Vector3 linVel = new Vector3();
    private Vector3 angVel = new Vector3();
    private Vector3 linAcc, angAcc;
    private Vector3 rotation = new Vector3();

    private UDPSend udpSend; //sends motion data to comm
    public bool on_platform = true; //whether game is being played on MOOG
    public bool log = false; //whether to store input and output into a log file
    private string lf = ""; //next line to be added to log file
    private Logger logger; //logs to file

    public GameObject model; //orientation 3d display
    public Image avx, avy, avz, lvx, lvy, lvz; //gui velocity displays

    private Canvas canvas;
    private MeshRenderer[] axes, ori;

    // Use this for initialization
    void Start()
    {
        canvas = FindObjectOfType<Canvas>();
        axes = GameObject.Find("axes").GetComponentsInChildren<MeshRenderer>();
        ori = GameObject.Find("orientation").GetComponentsInChildren<MeshRenderer>();

        rb = GetComponent<Rigidbody>();
        udpSend = FindObjectOfType<UDPSend>();

        //apply acceleration limits from MOOG
        TURN_DELTA = Mathf.Min(Mathf.Abs(TURN_DELTA), MAX_ANG_ACC);
        SPEED_DELTA = Mathf.Min(Mathf.Abs(SPEED_DELTA), MAX_LIN_ACC);

        if (log)
        {
            logger = new Logger();
            string header = string.Format(" {0,8} {1,8}", "Time", "Brake");
            header += string.Format(" {0,8} {1,8} {2,8}", controls[0]);
            header += string.Format(" {0,8} {1,8} {2,8}", controls[1]);
            header += string.Format(" {0,8} {1,8} {2,8} {3,8}", "||v||", "v_ang_x", "v_ang_y", "v_ang_z");
            header += string.Format(" {0,8} {1,8} {2,8}", "a_x", "a_y", "a_z");
            header += string.Format(" {0,8} {1,8} {2,8}", "a_ang_x", "a_ang_y", "a_ang_z");
            header += string.Format(" {0,8} {1,8} {2,8}", "rot_x", "rot_y", "rot_z");
            logger.logPacket(header);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.JoystickButton2))
        {
            canvas.enabled = !canvas.enabled;
            foreach (MeshRenderer mr in axes)
            {
                mr.enabled = !mr.enabled;
            }
            foreach (MeshRenderer mr in ori)
            {
                mr.enabled = !mr.enabled;
            }
        }
        if (on_platform)
        {
            UDPSend.newPacket();
            addToLog(Time.time, true);
            applyForces();
            correctOrientation();
            sendData();
        } else
        {
            applyForces();
        }
        if (log)
        {
            logger.logPacket(lf);
            lf = "";
        }
        updateVelAcc();
        updateGUI();
    }

    // logs and applies forces from controls to ship
    private void applyForces()
    {
        bool canMove = addToLog(Input.GetAxis("Brake"), false) == 0;

        Vector3 x = transform.right.normalized;
        Vector3 y = transform.up.normalized;
        Vector3 z = transform.forward.normalized;
        Vector3[] axes = { x, y, z };

        Vector3 lin = globalize(axes, Motion.Linear);
        Vector3 ang = globalize(axes, Motion.Angular);

        if (canMove)
        {
            rb.AddForce(SPEED_DELTA * lin);
            rb.AddTorque(TURN_DELTA * ang);
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, MAX_LIN_VEL);
            rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, Mathf.PI / 6);
        }
        else
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, BRAKE_DELTA);
            rb.angularVelocity = Vector3.Slerp(rb.angularVelocity, Vector3.zero, BRAKE_DELTA);
        }

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
        if (packet != null)
        {
            float time = BitConverter.ToSingle(packet, 0);
            Vector3 orientation = new Vector3(
                BitConverter.ToSingle(packet, 4),
                BitConverter.ToSingle(packet, 8),
                BitConverter.ToSingle(packet, 12));
            transform.Rotate(-orientation);
        }
    }

    // updates current velocities and accelerations vars and on GUI
    private void updateVelAcc()
    {
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
        
    }
    
    // converts vector from global to local axes
    private Vector3 localize(Vector3 v)
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

    // updates gui with positions and velocities
    private void updateGUI()
    {
        model.transform.localRotation = transform.rotation;
        Vector3 av = degModAngle(angVel);
        avx.transform.localRotation = Quaternion.AngleAxis(-av.x, Vector3.forward);
        avy.transform.localRotation = Quaternion.AngleAxis(-av.y, Vector3.forward);
        avz.transform.localRotation = Quaternion.AngleAxis(-av.z, Vector3.forward);
        Debug.Log(linVel);
        lvx.transform.localScale = new Vector3(linVel.x / (2*MAX_LIN_VEL),1,1);
        lvx.transform.localPosition = (linVel.x / (4 * MAX_LIN_VEL) * 100 ) * Vector3.right;
        lvy.transform.localScale = new Vector3(-linVel.y / (2*MAX_LIN_VEL),1, 1);
        lvy.transform.localPosition = (-linVel.y / (4 * MAX_LIN_VEL) * 100 )* Vector3.right;
        lvz.transform.localScale = new Vector3(linVel.z / (2*MAX_LIN_VEL),1, 1);
        lvz.transform.localPosition = (linVel.z / (4 * MAX_LIN_VEL) * 100 )* Vector3.right;
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
        return string.Format("{0,6:F2},{1,6:F2},{2,6:F2}", v.x, v.y, v.z);
    }

    // log and send ship motion info to comm
    private void sendData()
    {
        addToLog(SCALE_VEL * linVel.magnitude, true);
        addToLog(SCALE_VEL * angVel, true);
        addToLog(SCALE_VEL * linAcc, true);
        addToLog(SCALE_VEL * angAcc, true);
        addToLog(transform.rotation.eulerAngles, true);
        UDPSend.sendPacket();
    }

    // adds vector v to log, if udpSend then also adds to udpSend packet
    private Vector3 addToLog(Vector3 v, bool udpSend)
    {
        addToLog(v.x, udpSend);
        addToLog(v.y, udpSend);
        addToLog(v.z, udpSend);
        return v;
    }

    // adds float f to log, if udpSend then also adds to udpSend packet
    private float addToLog(float f, bool udpSend)
    {
        if (log)
        {
            lf += string.Format(" {0,8:F4}", f);
            if (udpSend) UDPSend.addFloat(f);
        }
        return f;
    }
}
