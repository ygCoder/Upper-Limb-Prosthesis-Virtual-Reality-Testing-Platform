using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

public class ControlSequential : MonoBehaviour
{
    GameObject pronation, extension, deviation, elbow, shoulder_poe, shoulder_add, shoulder_int,hand;
    TaskMain taskmain;
    InputManager input;
    [SerializeField] float speed = 1;
    public float[] joint_angles = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    public int current_joint = 1;
    bool just_switched = false;
    List<GameObject> joints = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        input = GetComponent<InputManager>();
        taskmain = GameObject.Find("Task").GetComponent<TaskMain>();
        hand = GameObject.Find("hand_angle");
        pronation = GameObject.Find("forearm_pronation");
        extension = GameObject.Find("wrist_extension");
        deviation = GameObject.Find("wrist_deviation");
        elbow = GameObject.Find("elbow_flexion");
        shoulder_int = GameObject.Find("Shoulder_int");
        shoulder_add = GameObject.Find("Shoulder_add");
        shoulder_poe = GameObject.Find("Shoulder_poe");
        joints.Add(hand);
        joints.Add(deviation);
        joints.Add(extension);
        joints.Add(pronation);
        joints.Add(elbow);        
        joints.Add(shoulder_int);
        joints.Add(shoulder_add);
        joints.Add(shoulder_poe);

        current_joint = 1;
    }

    int getDOF()
    {
        switch (taskmain.current_DOF)
        {
            case "3 DOF":
                return (4);
            case "4 DOF":
                return (5);                
            case "7 DOF":
                return (8);
            default:
                return (1);
        }
    }
    /*Vector3 getJointAxis(int i,float ang)
    {
        switch (i)
        {            
            case 1:
            case 7:
                return (new Vector3(0, ang, 0));
            case 3:
            case 4:
                return (new Vector3(0, 0, ang));
            case 5:
                return (new Vector3(0, 0, -ang));
            case 2:
                return (new Vector3(ang, 0, 0));
            case 0:
                return (new Vector3(ang, 0, 0));            
            case 6:
                return (new Vector3(ang, 0, 0));
            default:
                return (new Vector3(0, 0, 0));
        }
    }*/

    int getNextJoint(float type, int cur)
    {
        int dof = taskmain.getDOF();
        if (type ==1 )
        {
            if (cur == 1) return (dof - 1);
            else return (cur - 1);
        }
        if (type == 2)
        {
            if (cur == dof - 1) return (1);
            else return (cur + 1);
        }
        else return (0);
    }


    int getNextJoint2(float type, int cur)
    {
        int dof = taskmain.getDOF();
        if (cur == 0) return (dof-1); // if in hand mode return last joint        
        else
        {
            if (type == 2) //switch arm to wrist
            {
                switch (dof)
                {
                    case 4:
                        return (0);
                    case 5:
                        return (0);
                    case 8:
                        if (cur == 0) return (7);
                        if (cur >= 4) return (3);
                        else return (0);
                    default:
                        return (0);
                }
            }
            else // normal 1-click case
            {
                switch (dof)
                {
                    case 4:
                        return (cur - 1);
                    case 5:
                        return (cur - 1);
                        //return (((cur-1) % taskmain.getDOF() - 1) + 1);
                    case 8:
                        if (cur == 4) return (7);
                        if (cur == 1) return (3);
                        else return (cur - 1);
                        //else return (((cur - 1) % (taskmain.getDOF() - 4)) + 1); //3-2-1-3-2-1
                    default:
                        return (0);
                }
                //return ((cur++ % taskmain.getDOF() - 1) + 1);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        float[] command = input.getInput();
        if (command[0] == 3)
        {
            if (!just_switched) {
                joint_angles[0] = Mathf.Abs(joint_angles[0] - 50);
                joints[0].transform.localRotation = Quaternion.Euler(Constants.getJointAxis(0) * joint_angles[0]);
                just_switched = true;
            }
        }
        if (command[0] == 1 || command[0]==2)
        {
            if (!just_switched)
            {
                current_joint = getNextJoint(command[0],current_joint);
                just_switched = true;
            }           
        }
        else if(command[0]==0)
        {
            just_switched = false;
        }
        if (command[1] < -0.5 || command[1] > 0.5)
        {
            joint_angles[current_joint] += speed * (command[1] - Mathf.Sign(command[1]) * 0.5f);

            if (joint_angles[current_joint] > Constants.joint_limits[current_joint, 1]) joint_angles[current_joint] = Constants.joint_limits[current_joint, 1];
            if (joint_angles[current_joint] < Constants.joint_limits[current_joint, 0]) joint_angles[current_joint] = Constants.joint_limits[current_joint, 0];


        }
        joints[current_joint].transform.localRotation = Quaternion.Euler(Constants.getJointAxis(current_joint) * joint_angles[current_joint]);

    }

    public int GetCurrentJoint()
    {
        return (current_joint);
    }
    public bool ResetHand()
    {
        Array.Clear(joint_angles, 0, joint_angles.Length);
        for (int i = 0; i < taskmain.getDOF(); i++)
        {
            joints[i].transform.localRotation = Quaternion.Euler(Constants.getJointAxis(i) * joint_angles[i]);
        }
        GameObject.Find("Shoulder_joint/Shoulder_poe/Shoulder_add/Shoulder_int/Shoulder").transform.localRotation = Constants.initial_transforms["Shoulder"].GetRotation();
        GameObject.Find("Shoulder_joint/Shoulder_poe/Shoulder_add/Shoulder_int/Shoulder").transform.localPosition = Constants.initial_transforms["Shoulder"].GetPosition();
        GameObject.Find("Shoulder_joint/Shoulder_poe/Shoulder_add/Shoulder_int/Shoulder/Elbow").transform.localRotation = Constants.initial_transforms["Elbow"].GetRotation();
        GameObject.Find("Shoulder_joint/Shoulder_poe/Shoulder_add/Shoulder_int/Shoulder/Elbow").transform.localPosition = Constants.initial_transforms["Elbow"].GetPosition();

        return true;
    }
}
