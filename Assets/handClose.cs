using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class handClose : MonoBehaviour
{
    [SerializeField] public float hand_closure;
    [SerializeField] private bool close = false;
    [SerializeField] private bool open = false;
    private List<Transform> hand_joints = new List<Transform>();
    Transform angle_input;
    [SerializeField] private int angle_limit;

    // Start is called before the first frame update
    void Start()
    {
        //input = GameObject.Find("Inputs").GetComponent<InputManager>();

        //angle_input = GameObject.Find("hand_angle").transform;

        angle_input = transform;


        /*for (int i = 0; i < this.transform.childCount; i++)
        {
            if (this.transform.GetChild(i).name == "hand_angle") angle_input = this.transform.GetChild(i);
        }
        */
        //angle_input = this.gameObject.transform.Find("hand_angle");

        hand_closure = 0;
        angle_limit = 100;


        /*        hand_joints.Add(GameObject.Find("hand_prefab:Right_Index_Finger_Joint_01a"));
                hand_joints.Add(GameObject.Find("hand_prefab:Right_Index_Finger_Joint_01b"));
                hand_joints.Add(GameObject.Find("hand_prefab:Right_Index_Finger_Joint_01c"));
                hand_joints.Add(GameObject.Find("hand_prefab:Right_Middle_Finger_Joint_01a"));
                hand_joints.Add(GameObject.Find("hand_prefab:Right_Middle_Finger_Joint_01b"));
                hand_joints.Add(GameObject.Find("hand_prefab:Right_Middle_Finger_Joint_01c"));
                hand_joints.Add(GameObject.Find("hand_prefab:Right_Ring_Finger_Joint_01a"));
                hand_joints.Add(GameObject.Find("hand_prefab:Right_Ring_Finger_Joint_01b"));
                hand_joints.Add(GameObject.Find("hand_prefab:Right_Ring_Finger_Joint_01c"));
                hand_joints.Add(GameObject.Find("hand_prefab:Right_Pinky_Finger_Joint_01a"));
                hand_joints.Add(GameObject.Find("hand_prefab:Right_Pinky_Finger_Joint_01b"));
                hand_joints.Add(GameObject.Find("hand_prefab:Right_Pinky_Finger_Joint_01c"));
                hand_joints.Add(GameObject.Find("hand_prefab:Right_Thumb_Base_Joint_01"));
                //hand_joints.Add(GameObject.Find("hand_prefab:Right_Thumb_Joint_01a"));
                hand_joints.Add(GameObject.Find("hand_prefab:Right_Thumb_Joint_01b"));
          */
        hand_joints.Add(transform.parent.Find("hand_prefab:Right_Wrist_Joint_01/hand_prefab:Right_Index_Finger_Joint_01a"));
        hand_joints.Add(transform.parent.Find("hand_prefab:Right_Wrist_Joint_01/hand_prefab:Right_Index_Finger_Joint_01a/hand_prefab:Right_Index_Finger_Joint_01b"));
        hand_joints.Add(transform.parent.Find("hand_prefab:Right_Wrist_Joint_01/hand_prefab:Right_Index_Finger_Joint_01a/hand_prefab:Right_Index_Finger_Joint_01b/hand_prefab:Right_Index_Finger_Joint_01c"));
        hand_joints.Add(transform.parent.Find("hand_prefab:Right_Wrist_Joint_01/hand_prefab:Right_Middle_Finger_Joint_01a"));
        hand_joints.Add(transform.parent.Find("hand_prefab:Right_Wrist_Joint_01/hand_prefab:Right_Middle_Finger_Joint_01a/hand_prefab:Right_Middle_Finger_Joint_01b"));
        hand_joints.Add(transform.parent.Find("hand_prefab:Right_Wrist_Joint_01/hand_prefab:Right_Middle_Finger_Joint_01a/hand_prefab:Right_Middle_Finger_Joint_01b/hand_prefab:Right_Middle_Finger_Joint_01c"));
        hand_joints.Add(transform.parent.Find("hand_prefab:Right_Wrist_Joint_01/hand_prefab:Right_Ring_Finger_Joint_01a"));
        hand_joints.Add(transform.parent.Find("hand_prefab:Right_Wrist_Joint_01/hand_prefab:Right_Ring_Finger_Joint_01a/hand_prefab:Right_Ring_Finger_Joint_01b"));
        hand_joints.Add(transform.parent.Find("hand_prefab:Right_Wrist_Joint_01/hand_prefab:Right_Ring_Finger_Joint_01a/hand_prefab:Right_Ring_Finger_Joint_01b/hand_prefab:Right_Ring_Finger_Joint_01c"));
        hand_joints.Add(transform.parent.Find("hand_prefab:Right_Wrist_Joint_01/hand_prefab:Right_Pinky_Finger_Joint_01a"));
        hand_joints.Add(transform.parent.Find("hand_prefab:Right_Wrist_Joint_01/hand_prefab:Right_Pinky_Finger_Joint_01a/hand_prefab:Right_Pinky_Finger_Joint_01b"));
        hand_joints.Add(transform.parent.Find("hand_prefab:Right_Wrist_Joint_01/hand_prefab:Right_Pinky_Finger_Joint_01a/hand_prefab:Right_Pinky_Finger_Joint_01b/hand_prefab:Right_Pinky_Finger_Joint_01c"));
        hand_joints.Add(transform.parent.Find("hand_prefab:Right_Wrist_Joint_01/hand_prefab:Right_Thumb_Base_Joint_00/hand_prefab:Right_Thumb_Base_Joint_01/hand_prefab:Right_Thumb_Joint_01a"));
        //hand_joints.Add(GameObject.Find("hand_prefab:Right_Thumb_Base_Joint_00/hand_prefab:Right_Thumb_Base_Joint_01hand_prefab:Right_Thumb_Joint_01a"));
        hand_joints.Add(transform.parent.Find("hand_prefab:Right_Wrist_Joint_01/hand_prefab:Right_Thumb_Base_Joint_00/hand_prefab:Right_Thumb_Base_Joint_01/hand_prefab:Right_Thumb_Joint_01a/hand_prefab:Right_Thumb_Joint_01b"));
        


    }

    public void SetHandClosure(float inp)
    {
        hand_closure = inp;
        if (hand_closure > angle_limit) hand_closure = angle_limit;
        if (hand_closure < 0) hand_closure = 0;

        angle_input.localRotation = Quaternion.Euler(hand_closure, 0, 0);
        /*    foreach (GameObject j in hand_joints)
            {
                j.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, hand_closure);
            }
            */
    }

    void Update()
    {
        if (angle_input.localRotation.eulerAngles.x > angle_limit) angle_input.localRotation = angle_input.localRotation;
        if (angle_input.localRotation.eulerAngles.x < 0) angle_input.localRotation = Quaternion.Euler(0, 0, 0);
        foreach (Transform j in hand_joints)
        {
            j.localRotation = Quaternion.Euler(0.0f, 0.0f, angle_input.localRotation.eulerAngles.x);
        }


        /*   hand_closure += input.getInput(8);
           if (hand_closure > angle_limit) hand_closure = angle_limit;
           if (hand_closure < 0) hand_closure = 0;
           foreach (GameObject j in hand_joints)
           {
               j.transform.localRotation=Quaternion.Euler(0.0f, 0.0f, hand_closure);
           }
           */
    }

    // Update is called once per frame
    void Update2()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            close = true;
            angle_limit += 1;
        }
        else
        {
            close = false;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            open = true;
            angle_limit -= 1;
        }
        else
        {
            open = false;
        }

        if (angle_limit < 0)
        {
            angle_limit = 0;
        }
        if (angle_limit > 50)
        {
            angle_limit = 50;
        }

        if (close && angle_limit < 50)
        {
            print("hello");
            foreach (Transform j in hand_joints)
            {
                j.Rotate(0.0f, 0.0f, (float)hand_closure, Space.Self);
            }
            //GameObject.Find("hand_prefab:Right_Thumb_Joint_01a").transform.Rotate((float)hand_closure, 0, 0, Space.Self);

        }
        //if (Input.GetKeyDown(KeyCode.LeftArrow))
        if (open && angle_limit > 0)
        {
            print("bye");
            foreach (Transform j in hand_joints)
            {
                j.Rotate(0.0f, 0.0f, -(float)hand_closure, Space.Self);
            }
            //GameObject.Find("hand_prefab:Right_Thumb_Joint_01a").transform.Rotate(-(float)hand_closure, 0, 0, Space.Self);
        }
        /*
        foreach (GameObject j in hand_joints)
            j.transform.Rotate(0.0f, 0.0f, 1.0f, Space.Self);
        
        GameObject.Find("hand_prefab:Right_Index_Finger_Joint_01a").transform.localRotation = Quaternion.Euler(0, 0, (float)hand_closure);
        GameObject.Find("hand_prefab:Right_Index_Finger_Joint_01b").transform.localRotation = Quaternion.Euler(0, 0, (float)hand_closure);
        GameObject.Find("hand_prefab:Right_Index_Finger_Joint_01c").transform.localRotation = Quaternion.Euler(0, 0, (float)hand_closure);
        GameObject.Find("hand_prefab:Right_Middle_Finger_Joint_01a").transform.localRotation = Quaternion.Euler(0, 0, (float)hand_closure);
        GameObject.Find("hand_prefab:Right_Middle_Finger_Joint_01b").transform.localRotation = Quaternion.Euler(0, 0, (float)hand_closure);
        GameObject.Find("hand_prefab:Right_Middle_Finger_Joint_01c").transform.localRotation = Quaternion.Euler(0, 0, (float)hand_closure);
        GameObject.Find("hand_prefab:Right_Ring_Finger_Joint_01a").transform.localRotation = Quaternion.Euler(0, 0, (float)hand_closure);
        GameObject.Find("hand_prefab:Right_Ring_Finger_Joint_01b").transform.localRotation = Quaternion.Euler(0, 0, (float)hand_closure);
        GameObject.Find("hand_prefab:Right_Ring_Finger_Joint_01c").transform.localRotation = Quaternion.Euler(0, 0, (float)hand_closure);
        GameObject.Find("hand_prefab:Right_Pinky_Finger_Joint_01a").transform.localRotation = Quaternion.Euler(0, 0, (float)hand_closure);
        GameObject.Find("hand_prefab:Right_Pinky_Finger_Joint_01b").transform.localRotation = Quaternion.Euler(0, 0, (float)hand_closure);
        GameObject.Find("hand_prefab:Right_Pinky_Finger_Joint_01c").transform.localRotation = Quaternion.Euler(0, 0, (float)hand_closure);
        GameObject.Find("hand_prefab:Right_Thumb_Joint_01a").transform.localRotation = Quaternion.Euler(0, 0, (float)hand_closure);
        GameObject.Find("hand_prefab:Right_Thumb_Joint_01b").transform.localRotation = Quaternion.Euler(0, 0, (float)hand_closure);
        */
    }
}