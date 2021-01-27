using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class InputEMG : MonoBehaviour
{
    [SerializeField] private static string comport = "COM4";
    [SerializeField] private int inp = 11;

    SerialPort sp = new SerialPort(comport, 9600);
    float[] command = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    // Start is called before the first frame update
    void Start()
    {
        sp.ReadTimeout = 10;
      //  sp.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        sp.Open();
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public float[] getInput()
    {
        if (sp.IsOpen)
        {
            try
            {
                inp = sp.ReadByte();
            }
            catch
            {
            }           
            command[0] = (inp == 22) ? 1 : 0;
            if(inp == 12)
            {
                command[1] = 1;
            }
            else if (inp == 21)
            {
                command[1] = -1;
            }            
        }
        return (command);
    }
    }
