using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlSimultaneous : MonoBehaviour
{
    GameObject pronation, extension, deviation, elbow, shoulder_poe, shoulder_add, shoulder_int, hand;
    TaskMain taskmain;
    InputManager input;
    handClose hand_closer;
    [SerializeField] float speed = 1;
    public float[] joint_angles= new float[] { 0, 0, 0, 0, 0, 0, 0, 0};
    //public float[,] joint_limits = new float[,] { { 0, 100 }, { 0, 100 }, { 0, 100 }, { 0, 100 }, { 0, 100 }, { 0, 100 }, { 0, 100 }, { 0, 100 } };
    List<GameObject> joints = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        input= GetComponent<InputManager>();
        taskmain = GameObject.Find("Task").GetComponent<TaskMain>();
        pronation = GameObject.Find("forearm_pronation");
        extension = GameObject.Find("wrist_extension");
        deviation = GameObject.Find("wrist_deviation");
        elbow = GameObject.Find("elbow_flexion");
        shoulder_int = GameObject.Find("Shoulder_int");
        shoulder_add = GameObject.Find("Shoulder_add");
        shoulder_poe = GameObject.Find("Shoulder_poe");
        hand = GameObject.Find("hand_angle");
        hand_closer = GameObject.Find("final_hand_v2").GetComponent<handClose>();
        joints.Add(hand);
        joints.Add(deviation);
        joints.Add(extension);
        joints.Add(pronation);
        joints.Add(elbow);
        joints.Add(shoulder_int);        
        joints.Add(shoulder_add);
        joints.Add(shoulder_poe);
    }

    // Update is called once per frame
    void Update()
    {
        float[] command=input.getInput();
        //for (int i = 0; i < joint_angles.Length; i++)
        for (int i = 0; i < taskmain.getDOF(); i++)
            
        {
            if (command[i + 1] < -0.5 || command[i + 1] > 0.5)
            {
                joint_angles[i] += speed * (command[i + 1]-Mathf.Sign(command[i + 1])*0.5f);
                if (joint_angles[i] > Constants.joint_limits[i, 1]) joint_angles[i] = Constants.joint_limits[i, 1];
                if (joint_angles[i] < Constants.joint_limits[i, 0]) joint_angles[i] = Constants.joint_limits[i, 0];

            }
            joints[i].transform.localRotation = Quaternion.Euler(Constants.getJointAxis(i) * joint_angles[i]);
        }


        //print("C: " + command[0] + "," + command[1]);
        /*pronation.transform.localRotation = Quaternion.Euler(0, -joint_angles[1]*speed, 0);
        deviation.transform.localRotation = Quaternion.Euler(-joint_angles[2] * speed, 0, 0);
        extension.transform.localRotation = Quaternion.Euler(0, 0, -joint_angles[3] * speed);
        if(taskmain.current_DOF == "4 DOF") elbow.transform.localRotation= Quaternion.Euler(0, 0, joint_angles[4] * speed);
        if (taskmain.current_DOF == "7 DOF")
        {
            elbow.transform.localRotation = Quaternion.Euler(0, 0, joint_angles[4] * speed);
            shoulder_poe.transform.localRotation = Quaternion.Euler(0, -joint_angles[5] * speed,0);
            shoulder_add.transform.localRotation = Quaternion.Euler(0, 0,-joint_angles[6] * speed);
            shoulder_int.transform.localRotation = Quaternion.Euler(0,-joint_angles[7] * speed,0);
        }
        hand_closer.SetHandClosure(joint_angles[0]);*/
    }
    public bool ResetHand()
    {
        System.Array.Clear(joint_angles, 0, joint_angles.Length);
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
