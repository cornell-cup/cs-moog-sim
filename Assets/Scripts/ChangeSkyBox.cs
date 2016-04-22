using UnityEngine;
using System.Collections;

public class ChangeSkyBox : MonoBehaviour {

    public Material[] skyboxes= new Material[6];
    private int sb = 2;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.JoystickButton0) || Input.GetKeyUp(KeyCode.Minus))
            change(false);
        else if (Input.GetKeyUp(KeyCode.JoystickButton1) || Input.GetKeyUp(KeyCode.Equals))
            change(true);
    }

	void change(bool increase)
    {
        sb = (sb + skyboxes.Length + (increase ? 1 : -1)) % skyboxes.Length;
        Debug.Log(sb);
        RenderSettings.skybox = skyboxes[sb];
    }
}