using UnityEngine;
using UnityEngine.UI;

public class CamDisplay : MonoBehaviour
{

    private GameObject orientation; //orientation 3d display
    public Image[] ang = new Image[3];
    public Image[] lin = new Image[3];

    private Canvas canvas;
    private MeshRenderer[] axesMesh, oriMesh;

    // Use this for initialization
    void Start()
    {
        canvas = FindObjectOfType<Canvas>();
        orientation = GameObject.Find("orientation");
        oriMesh = orientation.GetComponentsInChildren<MeshRenderer>();
        axesMesh = GameObject.Find("axes").GetComponentsInChildren<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.JoystickButton2) || Input.GetKeyUp(KeyCode.G))
        {
            canvas.enabled = !canvas.enabled;
            foreach (MeshRenderer mr in axesMesh)
                mr.enabled = !mr.enabled;
            foreach (MeshRenderer mr in oriMesh)
                mr.enabled = !mr.enabled;
        }
    }

    public void updateAng(Vector3 angVel)
    {
        orientation.transform.localRotation = transform.rotation;
        Vector3 av = degModAngle(angVel);
        for (int i = 0; i < 3; i++)
        {
            ang[i].transform.localRotation = Quaternion.AngleAxis(av[i], Vector3.forward);
        }
    }

    public void updateLin(Vector3 linVel)
    {
        float scale = 0.5f / Movement.MAX_LIN_VEL;
        for (int i = 0; i < 3; i++)
        {
            lin[i].transform.localScale = new Vector3(linVel[i] * scale, 1, 1);
            lin[i].transform.localPosition = Vector3.right * (linVel[i] * scale * 50);
        }
    }

    // converts vector from radians to degrees and from [0,360) to [-180,180)
    private Vector3 degModAngle(Vector3 v)
    {
        v *= 180 / Mathf.PI;
        for (int i = 0; i < 3; i++)
            v[i] = v[i] < 180 ? v[i] : v[i] - 360;
        return v;
    }
}
