using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.Serialization;

public class InputKeyboard : MonoBehaviour
{
    public float[] command = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0};
    public float clicked = 0;
    float clicktime = 0;
    float clickdelay = 0.5f;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
      //  string[] data = udp_receiver.getLatestUDPPacket().Split();
        //print("C: " + float.Parse(data[0]) + " D: " + float.Parse(data[1]) + " V: " + float.Parse(data[2]));
                        command[1] = 1;

        if(Time.time - clicktime > clickdelay)
        {
            command[0] = 0;
        }
       
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.LeftArrow)) //a slider moved
        {
            command[0] = 0;
            if (Input.GetKey(KeyCode.RightArrow)) // get just sliders (ignore knobs)
            {
                command[1] = 1;
            }
            if (Input.GetKey(KeyCode.LeftArrow)) // get just sliders (ignore knobs)
            {
                command[1] = -1;
            }
        }
        if (Input.GetKey(KeyCode.Space)) // button event
        {
            if ( clicked==0) //note_down first
            {
                clicked = 1;
                clicktime = Time.time; 
            }
            if ( clicked == 1) //note_up first
            {
                clicked = 2;
                clicktime = Time.time;
            }
            if ( clicked == 2 && (Time.time - clicktime) < clickdelay) //double click
            {
                clicked = 3;
            }
            if((clicked==2) && (Time.time - clicktime) > clickdelay)
            {
                command[0] = 1;
                clicked = 0;
            }
            if ((clicked == 3) && (Time.time - clicktime) > clickdelay)
            {
                command[0] = 2;
                clicked = 0;    
            }
        }

        /*
        else if (float.Parse(data[0]) == 2) //button event
        {
            if (float.Parse(data[2]) == 1) //note_down
            {
                if(clicked==2 && Time.time - clicktime < clickdelay)
                {
                    command[0] = 2; //double clicked
                    clicked = 3;
                }                 
                else //first click
                {
                    command[0] = 1;
                    clicktime = Time.time; //first click
                }
            }
            if (float.Parse(data[2]) == 0) 
            {
                if (clicked == 0)
                {
                    clicked = 1;
                    clicktime = Time.time; //first click
                }
                else clicked = 2;
            }
          
               if (float.Parse(data[2]) == 1) //note_down
               {
                   clicked++;
                   if (clicked == 1)
                   {
                       command[0] = 1;
                       clicktime = Time.time; //first click
                   }
                   if (clicked > 1 && Time.time - clicktime < clickdelay)
                   {
                       command[0] = 2; //double clicked
                   }
                   else
                   {
                       command[0] = 0; 
                   }
               }    
        }*/
    }
    public float[] getInput()
    {
        /*  print(command[0].ToString() + "," + command[1].ToString() + "," + command[2].ToString() + "," + command[3].ToString() + "," + command[4].ToString());
          float[] out_com = new float[command.Length];
          Array.Copy(command, out_com,command.Length);
          Array.Clear(command, 0, command.Length);        
          return (out_com);
          */
       // print(command[0].ToString() + "," + command[1].ToString() + "," + command[2].ToString() + "," + command[3].ToString() + "," + command[4].ToString());
        return (command);
    }
}


