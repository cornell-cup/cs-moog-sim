using UnityEngine;
using System.Collections;
using System;

public class Logger : MonoBehaviour
{

    private string log_filename = "log_" + DateTime.Now.ToFileTime();

    // Use this for initialization
    void Start()
    {
        System.IO.StreamWriter clearLog = new System.IO.StreamWriter(log_filename + ".txt", false);
        clearLog.Dispose();
    }

    public void logPacket(string message)
    {
        try
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(log_filename + ".txt", true))
            {
                sw.WriteLine(message);
            }
        }
        catch (Exception e)
        {
            print(e.Message);
        }
    }
}
