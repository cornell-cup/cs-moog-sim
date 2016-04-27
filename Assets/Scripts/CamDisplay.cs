using UnityEngine;
using UnityEngine.UI;

public class CamDisplay : MonoBehaviour
{

    private GameObject orientation; //orientation 3d display
    public Image[] ang = new Image[3];
    public Image[] lin = new Image[3];

    private Image panel;
    private Text instructions;
    private MeshRenderer[] axesMesh, oriMesh;
    private AudioSource music;
    private bool paused = true;

    // Use this for initialization
    void Start()
    {
        Time.timeScale = 0;
        panel = GameObject.Find("Panel").GetComponent<Image>();
        orientation = GameObject.Find("orientation");
        oriMesh = orientation.GetComponentsInChildren<MeshRenderer>();
        axesMesh = GameObject.Find("axes").GetComponentsInChildren<MeshRenderer>();
        instructions = GameObject.Find("Instructions").GetComponent<Text>();
        music = GameObject.Find("Music").GetComponent<AudioSource>();
        music.volume = 0.1f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.JoystickButton1) || Input.GetKeyUp(KeyCode.G))
        {
            foreach (MeshRenderer mr in axesMesh)
            {
                mr.enabled = !mr.enabled;
            }
            foreach (MeshRenderer mr in oriMesh)
            {
                mr.enabled = !mr.enabled;
            }
            panel.enabled = !panel.enabled;
            foreach (CanvasRenderer cr in panel.GetComponentsInChildren<CanvasRenderer>())
            {
                cr.SetAlpha(cr.GetAlpha() == 0 ? 1 : 0);
            }
        }
        if (Input.GetKeyUp(KeyCode.JoystickButton2) || Input.GetKeyUp(KeyCode.P))
        {
            paused = !paused;
            instructions.enabled = paused;
            if (paused)
            {
                music.volume = 0.1f;
                Time.timeScale = 0;
            } else
            {
                music.volume = 1;
                Time.timeScale = 1;
            }
        }
    }

    public void updateAng(Vector3 angVel)
    {
        orientation.transform.localRotation = transform.rotation;
        Vector3 av = degModAngle(angVel);
        av = new Vector3(av.x, -av.y, av.x);
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
