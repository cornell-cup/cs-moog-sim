using UnityEngine;
using System.Collections;

public class ChangeSkybox : MonoBehaviour {

    public Material[] skyboxes= new Material[1];
    private int sb = 0;

    void Start() { }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.JoystickButton0) || Input.GetKeyUp(KeyCode.Equals))
        {
            change();
        }

    }

	void change()
    {
        sb = (sb + 1) % skyboxes.Length;
        Debug.Log(sb);
        RenderSettings.skybox = skyboxes[sb];
    }
}