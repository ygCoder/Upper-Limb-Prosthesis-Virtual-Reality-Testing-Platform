using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using UnityEngine;

public class ControlTrajectory : MonoBehaviour
{
    [SerializeField] private TextAsset[] csvFiles_3DOF = new TextAsset[5];
    [SerializeField] private TextAsset[] csvFiles_4DOF = new TextAsset[11];
    [SerializeField] private TextAsset[] csvFiles_7DOF = new TextAsset[11];
       
    [SerializeField] private float speed = 0.000f;

    TaskMain taskmain;
    InputManager input;
    public int current_mode = 0;
    public int current_frame = 0;
    public float current_frame_float = 0;
    bool just_switched = false;
    public bool controlling_hand = false;
    public int switching = 0;
    string[] current_file;
    GameObject pronation, extension, deviation, elbow, shoulder_poe, shoulder_add, shoulder_int,hand;
    MoveGhost ghost_hand;
    List<GameObject> joints = new List<GameObject>();
    float[] joint_values = new float[8];
    // Start is called before the first frame update
    void Start()
    {
        input = GetComponent<InputManager>();
        ghost_hand = GameObject.Find("Controller").GetComponent<MoveGhost>();
        taskmain = GameObject.Find("Task").GetComponent<TaskMain>();
        pronation = GameObject.Find("forearm_pronation");
        extension = GameObject.Find("wrist_extension");
        deviation = GameObject.Find("wrist_deviation");
        elbow = GameObject.Find("elbow_flexion");
        shoulder_poe = GameObject.Find("Shoulder_poe");
        shoulder_add = GameObject.Find("Shoulder_add");
        shoulder_int = GameObject.Find("Shoulder_int");
        hand = GameObject.Find("hand_angle");

        joints.Add(hand);
        joints.Add(deviation);
        joints.Add(extension);
        joints.Add(pronation);
        joints.Add(elbow);
        joints.Add(shoulder_int);
        joints.Add(shoulder_add);
        joints.Add(shoulder_poe);
        current_file =LoadFile(getDOF(), current_mode);
    }

    string[] LoadFile(int currentDOF, int currentMode)
    {   
        switch (currentDOF)
        {
            case 3:
                return(csvFiles_3DOF[currentMode].text.Split('\n'));                
            case 4:
                return (csvFiles_4DOF[currentMode].text.Split('\n'));
            case 7:
                return (csvFiles_7DOF[currentMode].text.Split('\n'));
            default:
                return(new string[1]{ "0,0,0,0,0,0,0"});
        }
    }

    int getDOF()
    {
        switch (taskmain.current_DOF)
        {
            case "3 DOF":
                return (3);
            case "4 DOF":
                return (4);
            case "7 DOF":
                return (7);
            default:
                return (1);
        }
    }
    int getNextMode(int cur)
    {
        switch (taskmain.current_DOF)
        {
            case "3 DOF":
                return (cur % 5);
            case "4 DOF":
                return (cur % 11);
            case "7 DOF":
                return (cur % 11);
            default:
                return (1);
        }
    }    

    public int GetCurrentMode()
    {
        if (controlling_hand) return (0);
        else return (current_mode+1);
    }
    int CheckIfSwitching(string[] file, float[] ang_cur, float command)
    {
        int ret;
        float[] ang_file = new float[7];
        float ang_diff = 0;
        for (int i = 0; i < getDOF(); i++)
        {
            ang_file[i] = float.Parse(file[i]);
            float diff = ang_file[i] - ang_cur[i+1];
            ang_diff += diff * diff;
        }
        ang_diff = Mathf.Sqrt(ang_diff / file.Length);
        if(ang_diff<0.1) return (0); //joints are inside trajectory
        else
        {
            string[] goal = new string[file.Length];

            if(command > 0)
            {
                current_frame = 0;
             ret = 1;
            }
            else
            {                
                current_frame = current_file.Length - 2;
                ret = 2;
            }
            goal = current_file[current_frame].Split(',');
            current_frame_float = (float)current_frame;

            float max_ang_diff = 0;
            for (int i = 1; i < getDOF() + 1; i++)
            {
                float diff = Mathf.Abs(float.Parse(goal[i - 1]) - ang_cur[i]);
                if (diff > max_ang_diff) max_ang_diff = diff;  
                
            }
                

            
            for (int i = 1; i < getDOF()+1; i++)
            {
                joint_values[i] += 0.004f*speed * (float.Parse(goal[i-1]) - ang_cur[i])/max_ang_diff;
                joints[i].transform.localRotation = Quaternion.Euler(Constants.getJointAxis(i)* Mathf.Rad2Deg * joint_values[i]);
            }
        //   ghost_hand.MoveJointsAbs(ang_file);

            return (ret);
        }
    }
    // Update is called once per frame
    void Update()
    {
        float[] command = input.getInput();

        if (command[0] == 3)
        {
            if (!just_switched)
            {
                joint_values[0] = Mathf.Abs(joint_values[0] - 50);
                joints[0].transform.localRotation = Quaternion.Euler(Constants.getJointAxis(0) * joint_values[0]);
                just_switched = true;
            }
        }        

        /*
        if (command[0]==2 || (command[0] == 1 && controlling_hand))
        {
            if (controlling_hand) current_mode--;
                controlling_hand = !controlling_hand;            
        }
        if (controlling_hand)
        {
            //hand.transform.Rotate(new Vector3(command[1]*speed,0,0));
            joint_values[0] += 0.2f * speed * command[1];
            if (joint_values[0] > Constants.joint_limits[0, 1]) joint_values[0] = Constants.joint_limits[0, 1];
            if (joint_values[0] < Constants.joint_limits[0, 0]) joint_values[0] = Constants.joint_limits[0, 0];
            joints[0].transform.localRotation = Quaternion.Euler(Constants.getJointAxis(0) * joint_values[0]);


            return;
        }
        */
        else if (command[0] == 1)
        {
            if (!just_switched)
            {
                current_mode = getNextMode(current_mode+1);
                current_file = LoadFile(getDOF(), current_mode);
                just_switched = true;
                switching = -1;
                current_frame = 0;
            }
        }
        else if (command[0] == 2)
        {
            if (!just_switched)
            {
                current_mode = getNextMode(current_mode -1);
                current_file = LoadFile(getDOF(), current_mode);
                just_switched = true;
                switching = -1;
                current_frame = 0;
            }
        }

        else
        {
            just_switched = false;
        }
        if (command[1] < -0.5 || command[1] > 0.5)
        {
            if (switching != 0)
            {
                switching = CheckIfSwitching(current_file[current_frame].Split(','), joint_values, command[1]);
            }
            else {
                //current_frame = current_frame + Mathf.RoundToInt(speed*(command[1]));
                current_frame_float = current_frame_float + (speed * (command[1]));
                current_frame_float = Math.Min(Math.Max(0, current_frame_float), current_file.Length-1);
                current_frame = Mathf.RoundToInt(current_frame_float);

                string[] joint_values_str=new string[taskmain.getDOF()];
                try
                {
                    joint_values_str = current_file[current_frame].Split(',');
                }
                catch(Exception e)
                {
                    print("A");
                }
                

            for (int i =1; i < taskmain.getDOF(); i++)
            {
                joint_values[i] = float.Parse(joint_values_str[i-1]);
                joints[i].transform.localRotation = Quaternion.Euler(Constants.getJointAxis(i)*Mathf.Rad2Deg*joint_values[i]);
            }
            }
        }
    }
    public bool ResetHand()
    {
        current_frame_float = 1;
        current_frame = Mathf.RoundToInt(current_frame_float);

        string[] joint_values_str = current_file[current_frame].Split(',');

        joint_values[0] = 0;
        joints[0].transform.localRotation = Quaternion.identity;
        for (int i = 0; i < getDOF(); i++)
        {
            joint_values[i+1] = float.Parse(joint_values_str[i]);
            joints[i+1].transform.localRotation = Quaternion.Euler(Constants.getJointAxis(i+1)* Mathf.Rad2Deg * joint_values[i+1]);
        }

        GameObject.Find("Shoulder_joint/Shoulder_poe/Shoulder_add/Shoulder_int/Shoulder").transform.localRotation = Constants.initial_transforms["Shoulder"].GetRotation();
        GameObject.Find("Shoulder_joint/Shoulder_poe/Shoulder_add/Shoulder_int/Shoulder").transform.localPosition = Constants.initial_transforms["Shoulder"].GetPosition();
        GameObject.Find("Shoulder_joint/Shoulder_poe/Shoulder_add/Shoulder_int/Shoulder/Elbow").transform.localRotation = Constants.initial_transforms["Elbow"].GetRotation();
        GameObject.Find("Shoulder_joint/Shoulder_poe/Shoulder_add/Shoulder_int/Shoulder/Elbow").transform.localPosition = Constants.initial_transforms["Elbow"].GetPosition();        

        return true;
    }
}
