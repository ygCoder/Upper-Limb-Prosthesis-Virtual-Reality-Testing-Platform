using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InputManager : MonoBehaviour
{    
    TaskMain task_object;
    InputSliders sliders;
    InputEMG EMG;
    InputController controller;
    InputKeyboard keyboard;
    monitorCode monitor;
    public float[] command = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0};

    enum InputType
    {
        Controller,
        EMG,
        Sliders,
        Keyboard
    };
    [SerializeField] InputType input_type=InputType.Controller;

    // Start is called before the first frame update
    void Start()
    {
        task_object = GameObject.Find("Task").GetComponent<TaskMain>();
        sliders= GetComponent<InputSliders>();
        EMG = GetComponent<InputEMG>();
        controller = GetComponent<InputController>();
        keyboard = GetComponent<InputKeyboard>();

        monitor = GameObject.Find("monitor_main").GetComponent<monitorCode>();

    }

    // Update is called once per frame
    void Update()
    {
        switch (input_type)
        {
            case InputType.Controller:

                break;
            case InputType.EMG:

                break;
            case InputType.Sliders:
                command = sliders.getInput();
                //float[] command_raw = sliders.getInput();
                //for (int i = 0; i < command_raw.Length; i++) command[i] = (command_raw[i] - 64) / 64;
                break;
            case InputType.Keyboard:
                command = keyboard.getInput();
                break;

        }


    }
    public float[] getInput()
    {
        // [string] mode={nat=0,seq=1,sim=2,tra=3}
        // [int] sub_mode={3/4/7 for sim+seq, 0-11 for tra, 0 for nat}
        // [int] seq_dof=0-7 for seq (current mode)
        // float[] dof={array of dof values: 4/5/8 for sim, 1 for seq+tra, 0 for nat}
      

        return (command);
    }
    public float getInput(int i)
    {
        return (command[i]);
    }
}
