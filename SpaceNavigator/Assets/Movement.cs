using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour
{
    public float TURN_DELTA = 0.2f;
    public float SPEED_DELTA = 0.2f;

    private Vector3 MAX_ANG_VEL = new Vector3(Mathf.PI / 6, 2 * Mathf.PI / 9, Mathf.PI / 6);
    private Vector3 MAX_LIN_VEL = new Vector3(5, 3, 5);

    private const float MIN_ANGLE = 200;
    private const float MAX_ANGLE = 160;

    private enum Motion { Angular, Linear };
    private string[][] controls = {
        new string[] { "Pitch", "Yaw", "Roll" },
        new string[] { "Sway", "Heave", "Surge" } };

    private string label;
    private Rigidbody rb;
    private UDPSend udp;

    private Vector3 linVel = new Vector3();
    private Vector3 angVel = new Vector3();
    private Vector3 linAcc, angAcc;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        udp = FindObjectOfType<UDPSend>();
        //max accelerations
        TURN_DELTA = Mathf.Min(TURN_DELTA, Mathf.PI * 400 / 180); //400 deg/sec^2
        SPEED_DELTA = Mathf.Min(SPEED_DELTA, 49); //0.5 G
    }

    // Update is called once per frame
    void Update()
    {
        applyForces();
        limitRotation();
        limitVelocity(Motion.Angular, MAX_ANG_VEL);
        limitVelocity(Motion.Linear, MAX_LIN_VEL);
        updateVelAcc();
        sendData();
    }

    private void applyForces()
    {
        Vector3 x = transform.right.normalized;
        Vector3 y = transform.up.normalized;
        Vector3 z = transform.forward.normalized;
        Vector3[] axes = { x, y, z };

        rb.AddTorque(move(axes, Motion.Angular) * TURN_DELTA);
        rb.AddForce(move(axes, Motion.Linear) * SPEED_DELTA);
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

    private void limitRotation()
    {
        Vector3 r = transform.rotation.eulerAngles;
        Vector3 rv = rb.angularVelocity;

        rv.x = lerpAngVel(r.x, rv.x);
        rv.y = lerpAngVel(r.y, rv.y);
        rv.z = lerpAngVel(r.z, rv.z);

        if (!rv.Equals(rb.angularVelocity))
            rb.angularVelocity = rv;
    }

    // if ang is outside bounds, lerp vel towards zero
    // else return vel
    private float lerpAngVel(float ang, float vel)
    {
        if ((ang > MAX_ANGLE && ang < 180 && vel > 0.01) ||
            (ang < MIN_ANGLE && ang > 180 && vel < -0.01))
            vel = Mathf.Lerp(vel, 0, 0.1f);
        return vel;
    }

    private void limitVelocity(Motion type, Vector3 limit)
    {
        Vector3 v = (type == Motion.Angular) ? rb.angularVelocity : rb.velocity;
        Vector3 limV = v;
        limV.x = Mathf.Clamp(v.x, -limit.x, limit.x);
        limV.y = Mathf.Clamp(v.y, -limit.y, limit.y);
        limV.z = Mathf.Clamp(v.z, -limit.z, limit.z);

        if (!v.Equals(limV))
        {
            if (type == Motion.Angular)
                rb.angularVelocity = limV;
            else
                rb.velocity = limV;
        }
    }

    // data format: [time] [vel 6dof] [acc 6dof] 
    // 6dof in the following order:
    // pitch, yaw, roll, sway, heave, surge
    private void updateVelAcc()
    {
        Vector3 v = localize(rb.velocity);
        v *= 0.1f; //scale sim to MOOG velocity (0.1x)
        v = new Vector3(v.x, -v.y, v.z);
        Vector3 a = localize(rb.angularVelocity);
        a *= 180 / Mathf.PI; //convert rad/sec to deg/sec
        a = new Vector3(-a.x, a.y, -a.z);

        linAcc = v - linVel;
        angAcc = a - angVel;

        linVel = v;
        angVel = a;
    }

    private Vector3 localize(Vector3 v)
    {
        return new Vector3(
            getScalar(v, transform.right),
            getScalar(v, transform.up),
            getScalar(v, transform.forward));
    }

    private float getScalar(Vector3 v, Vector3 n)
    {
        return Vector3.Dot(Vector3.Project(v, n), n) / Vector3.Dot(n, n);
    }

    private void sendData()
    {
        string msg = Time.time.ToString();

        foreach (Vector3 v in new Vector3[] { angVel, linVel, angAcc, linAcc })
        {
            msg += " " + v.x;
            msg += " " + v.y;
            msg += " " + v.z;
        }

        Debug.Log(msg);
        udp.sendString(msg);

        //TODO more elegant UI updating
        label = "position:\t" + transform.position.ToString();
        label += "\nrotation:\t" + transform.rotation.eulerAngles.ToString();
        label += "\nvelocity:\t" + linVel.ToString();
        label += "\nangular:\t" + angVel.ToString();
    }

    void OnGUI()
    {
        GUI.Label(new Rect(30, 0, Screen.width, 100), label);
        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
    }
}
