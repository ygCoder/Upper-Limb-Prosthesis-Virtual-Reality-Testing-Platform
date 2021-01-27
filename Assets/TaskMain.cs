using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;
using Valve.VR.InteractionSystem;

public class TaskMain : MonoBehaviour
{
    // Start is called before the first frame update
    public TaskState current_state;
    public string current_mode;
    public string current_DOF = "7 DOF";
    Animator animator;
    public bool fill_in_states = true;
    public bool logging_data = false;
    List<GameObject> task_scenes = new List<GameObject>();
    string tasks_object_name = "Room 1/Tasks";
    GameObject current_scene;
    Vector3 angs_t1 = new Vector3(19, -2, -11);
    Vector3 angs_t2 = new Vector3(25, 14, -20);
    Vector3 angs_t3 = new Vector3(50, 24, -2);
    Vector3 angs_t4 = new Vector3(46, -7, -5);
    Vector3 angs_t5 = new Vector3(53, -4, -6);
    DirectParse wrist_parser;
    ControlTrajectory traj_control;
    ControlSequential seq_control;
    Vector3 initial_wrist;
    void Start()
    {
        if (fill_in_states) FillInStates();

        task_scenes = GetAllChildren(GameObject.Find(tasks_object_name));
        animator = GetComponent<Animator>();
        wrist_parser = GameObject.Find("Inputs").GetComponent<DirectParse>();
        traj_control = GameObject.Find("Inputs").GetComponent<ControlTrajectory>();
        seq_control = GameObject.Find("Inputs").GetComponent<ControlSequential>();
    }
    public static List<GameObject> GetAllChildren(GameObject Go)
    {
        List<GameObject> list = new List<GameObject>();
        for (int i = 0; i < Go.transform.childCount; i++)
        {
            list.Add(Go.transform.GetChild(i).gameObject);
        }
        return list;
    }

    public void SetRecording(Toggle tog)
    {
        logging_data = tog.isOn;
    }
    public void FillInStates()
    {
        //animator = GetComponent<Animator>();
        AnimatorController ac = GetComponent<Animator>().runtimeAnimatorController as AnimatorController;

        for (int i = 0; i < ac.layers.Length; i++)
        {
            foreach (ChildAnimatorStateMachine sm in ac.layers[i].stateMachine.stateMachines)
                foreach (ChildAnimatorState state in sm.stateMachine.states)
                {
                    switch (state.state.name)
                    {
                        case "CupReach":
                            FillInState((TaskState)state.state.behaviours[0], "CupReach", "Mug", "Wrist2",
                                new Vector3(0, 0, 0), new Vector3(0.02f, 0.08f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                //new Vector3(-87, -62.5f, -118), new Vector3(-0.08f, -0.09f, -0.05f), false, Vector3.zero, Vector3.zero, true);
                                new Vector3(-77, 171.5f, 13), new Vector3(-0.04f, -0.12f, -0.035f), false, Vector3.zero, Vector3.zero, true);
                            ((TaskState)state.state.behaviours[0]).des_grasping_state = 0;
                            break;

                        case "CupGrab":
                            FillInState((TaskState)state.state.behaviours[0], "CupGrab", "Mug", "Wrist2",
                                                        /*new Vector3(10,0, 20), new Vector3(0.02f, 0.03f, 0.02f),
                                                        Vector3.zero, Vector3.zero,
                                                        new Vector3(90, 0, 0), new Vector3(-0.05f, -0.1f, 0),false,Vector3.zero,Vector3.zero,true);*/
                                                        new Vector3(0, 0, 0), new Vector3(0.02f, 0.08f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(-77, 171.5f, 13), new Vector3(-0.04f, -0.12f, -0.035f), false, Vector3.zero, Vector3.zero, true);
                            ((TaskState)state.state.behaviours[0]).des_grasping_state = 25;
                            break;
                        case "CupDrink":
                            FillInState((TaskState)state.state.behaviours[0], "CupDrink", "Mouth", "MugMouth",
                                new Vector3(20, 0, 20), new Vector3(0.02f, 0.02f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(0, 0, 0), new Vector3(-0.0f, 0.05f, 0.00f), true,
                            //new Vector3(-19, -83, -97), new Vector3(-0.06f, -0.11f, -0.03f), false); //wrt Mug
                            new Vector3(0, 0, 0), new Vector3(0f, 0.0f, 0.0f), false); //wrt MugMouth
                            break;
                        case "CupReplace":
                            FillInState((TaskState)state.state.behaviours[0], "CupReplace", "C1", "MugMouth",
                                new Vector3(20, 0, 20), new Vector3(0.0f, 0.05f, 0.0f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(0, 0, 0), new Vector3(-0.0f, -0.1f, 0.00f), true,
                            //new Vector3(-19, -83, -97), new Vector3(-0.06f, -0.11f, -0.03f), false); //wrt Mug
                            new Vector3(0, 0, 0), new Vector3(0f, 0.0f, 0.0f), false); //wrt MugMouth
                            ((TaskState)state.state.behaviours[0]).des_grasping_state = -1;
                            break;
                        case "OverheadReturn":
                            FillInState((TaskState)state.state.behaviours[0], "OverheadReturn", "Torso_origin", "Mug",
                                new Vector3(30, 0, 30), new Vector3(0.15f, 0.15f, 0.15f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(0, -180, 0), new Vector3(-0.0f, 0.30f, 0.30f),
                                //true, new Vector3(-58, 5f, 176), new Vector3(-0.072f, -0.09f, -0.074f), 
                                true, Vector3.zero, Vector3.zero,
                                true);
                            break;

                        case "OverheadReplace":
                            FillInState((TaskState)state.state.behaviours[0], "OverheadReplace", "OverheadPlace", "Mug",
                                new Vector3(10, 0, 10), new Vector3(0.04f, 0.05f, 0.04f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(0, 0, 0), new Vector3(-0.0f, -0.0f, -0.0f),
                                //true, new Vector3(-58, 5f, 176), new Vector3(-0.072f, -0.09f, -0.074f), 
                                true, Vector3.zero, Vector3.zero,
                                true);
                            break;

                        case "OverheadRelease":
                            FillInState((TaskState)state.state.behaviours[0], "OverheadRelease", "OverheadPlace", "Mug",
                                new Vector3(10, 0, 10), new Vector3(0.04f, 0.05f, 0.04f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(0, 0, 0), new Vector3(-0.0f, -0.0f, -0.0f),
                                //true, new Vector3(-58, 5f, 176), new Vector3(-0.072f, -0.09f, -0.074f), 
                                true, Vector3.zero, Vector3.zero,
                                true);
                            ((TaskState)state.state.behaviours[0]).des_grasping_state = 0;
                            break;

                        case "OverheadReach":
                            FillInState((TaskState)state.state.behaviours[0], "OverheadReach", "MugMiddle", "Wrist2",
                                new Vector3(0, 20, 20), new Vector3(0.02f, 0.03f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                 // new Vector3(-58, 5f, 176), new Vector3(-0.072f, -0.11f, -0.037f), false, Vector3.zero, Vector3.zero, true);
                                 new Vector3(-67, -15f, -163), new Vector3(-0.05f, -0.088f, -0.00951f), false, Vector3.zero, Vector3.zero, true);
                            ((TaskState)state.state.behaviours[0]).des_grasping_state = 0;
                            break;
                        case "OverheadGrab":
                            FillInState((TaskState)state.state.behaviours[0], "OverheadGrab", "MugMiddle", "Wrist2",
                                new Vector3(0, 20, 20), new Vector3(0.02f, 0.03f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                    // new Vector3(-58, 5f, 176), new Vector3(-0.072f, -0.11f, -0.037f), false, Vector3.zero, Vector3.zero, true);
                                    new Vector3(-67, -15f, -163), new Vector3(-0.05f, -0.088f, -0.00951f), false, Vector3.zero, Vector3.zero, true);
                            ((TaskState)state.state.behaviours[0]).des_grasping_state = 25;
                            break;

                        case "ForkReach":
                            FillInState((TaskState)state.state.behaviours[0], "ForkReach", "ForkParent", "Wrist2",
                                new Vector3(20, 20, 20), new Vector3(0.02f, 0.02f, 0.03f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(-22, 104, -90), new Vector3(-0.06f, -0.124f, 0.039f), false, Vector3.zero, Vector3.zero, false);
                            break;

                        case "ForkGrab":
                            FillInState((TaskState)state.state.behaviours[0], "ForkGrab", "ForkParent", "Wrist2",
                                new Vector3(20, 20, 20), new Vector3(0.02f, 0.02f, 0.03f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(-22, 104, -90), new Vector3(-0.06f, -0.124f, 0.039f), false, Vector3.zero, Vector3.zero, false);
                            ((TaskState)state.state.behaviours[0]).des_grasping_state = 25;
                            break;
                        case "ForkStab":
                            FillInState((TaskState)state.state.behaviours[0], "ForkStab", "MeatParent", "ForkParent",
                                new Vector3(10, 0, 10), new Vector3(0.01f, 0.01f, 0.01f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(-50, -17, 60), new Vector3(0.096f, 0f, 0.01f), true, new Vector3(-40, -80, -60), new Vector3(-0.07f, -0.123f, 0.016f), false);
                            break;

                        case "ForkEat":
                            FillInState((TaskState)state.state.behaviours[0], "ForkEat", "Mouth", "ForkMouth",
                                new Vector3(0, 0, 0), new Vector3(0.02f, 0.02f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(115, -60, -141), new Vector3(-0.018f, -0.015f, 0.03f),
                                //true, new Vector3(-40, -80, -60), new Vector3(-0.07f, -0.123f, 0.016f), false); //wrt ForkParent
                                true, new Vector3(-31, -107, -12), new Vector3(-0.1f, -0.1f, 0.118f), false); //wrt ForkMouth
                            break;

                        case "SuitcaseReach":
                            FillInState((TaskState)state.state.behaviours[0], "SuitcaseReach", "suitcase_handle_root", "Wrist2",
                                new Vector3(120, 20, 20), new Vector3(0.05f, 0.05f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                 new Vector3(-45, -89, -14), new Vector3(-0.065f, -0.147f, 0.0355f), false, Vector3.zero, Vector3.zero, false);
                            break;
                        case "SuitcaseGrab":
                            FillInState((TaskState)state.state.behaviours[0], "SuitcaseGrab", "suitcase_handle_root", "Wrist2",
                                new Vector3(120, 20, 20), new Vector3(0.05f, 0.05f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(-45, -89, -14), new Vector3(-0.065f, -0.147f, 0.0355f), false, Vector3.zero, Vector3.zero, false);
                            ((TaskState)state.state.behaviours[0]).des_grasping_state = 40;

                            break;
                        case "SuitcaseTransfer":
                            FillInState((TaskState)state.state.behaviours[0], "SuitcaseTransfer", "placemat_above", "suitcase_handle_root",
                                new Vector3(20, 20, 0), new Vector3(0.02f, 0.02f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(0, 90, 0), new Vector3(0.0f, -0.0f, 0.0f),
                                //true, new Vector3(-45, -89, -14), new Vector3(-0.065f, -0.147f, 0.0355f), 
                                true, Vector3.zero, Vector3.zero,
                                true);
                            break;
                        case "CupPourReach":
                            FillInState((TaskState)state.state.behaviours[0], "CupPourReach", "Mug (1)", "Wrist2",
                                                        /*new Vector3(10,0, 20), new Vector3(0.02f, 0.03f, 0.02f),
                                                        Vector3.zero, Vector3.zero,
                                                        new Vector3(90, 0, 0), new Vector3(-0.05f, -0.1f, 0),false,Vector3.zero,Vector3.zero,true);*/
                                                        new Vector3(10, 0, 20), new Vector3(0.02f, 0.03f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                //new Vector3(-87, -62.5f, -118), new Vector3(-0.08f, -0.09f, -0.05f), false, Vector3.zero, Vector3.zero, true);
                                new Vector3(-88, -87, -87), new Vector3(-0.04f, -0.11f, -0.056f), false, Vector3.zero, Vector3.zero, true);
                            ((TaskState)state.state.behaviours[0]).des_grasping_state = 0;
                            break;

                        case "CupPourGrab":
                            FillInState((TaskState)state.state.behaviours[0], "CupPourGrab", "Mug (1)", "Wrist2",
                                                        /*new Vector3(10,0, 20), new Vector3(0.02f, 0.03f, 0.02f),
                                                        Vector3.zero, Vector3.zero,
                                                        new Vector3(90, 0, 0), new Vector3(-0.05f, -0.1f, 0),false,Vector3.zero,Vector3.zero,true);*/
                                                        new Vector3(10, 0, 20), new Vector3(0.02f, 0.03f, 0.02f),
                                new Vector3(10, 0, 20), new Vector3(0.02f, 0.03f, 0.02f),
                                new Vector3(-88, -87, -87), new Vector3(-0.04f, -0.11f, -0.056f), false, Vector3.zero, Vector3.zero, true);
                            ((TaskState)state.state.behaviours[0]).des_grasping_state = 25;
                            break;

                        case "CupPourPour":
                            FillInState((TaskState)state.state.behaviours[0], "CupPourPour", "Mug (2)/PourTarget", "Mug (1)",
                                new Vector3(30, 0, 20), new Vector3(0.02f, 0.02f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                //new Vector3(55, -40, 120), new Vector3(0.068f, 0.146f, 0.123f),true,
                                new Vector3(16, 47, -6), new Vector3(-0.0145f, 0.129f, 0.0067f), true,
                                 Vector3.zero, Vector3.zero, false);
                            break;

                        case "CupPourReturn":
                            FillInState((TaskState)state.state.behaviours[0], "CupPourReturn", "C_sit", "Mug (1)",
                                new Vector3(20, 0, 20), new Vector3(0.0f, 0.05f, 0.0f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(0, 0, 0), new Vector3(-0.0f, -0.01f, 0.00f), true,
                            //new Vector3(-19, -83, -97), new Vector3(-0.06f, -0.11f, -0.03f), false); //wrt Mug
                            new Vector3(0, 0, 0), new Vector3(0f, 0.0f, 0.0f), false); //wrt MugMouth
                            break;

                        case "SpoonReach":
                            FillInState((TaskState)state.state.behaviours[0], "SpoonReach", "utensil_spoon", "Wrist2",
                                new Vector3(20, 20, 20), new Vector3(0.02f, 0.02f, 0.04f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(-22, 104, -90), new Vector3(-0.06f, -0.124f, 0.039f), false, Vector3.zero, Vector3.zero, false);
                            break;

                        case "SpoonGrab":
                            FillInState((TaskState)state.state.behaviours[0], "SpoonGrab", "utensil_spoon", "Wrist2",
                                new Vector3(20, 20, 20), new Vector3(0.02f, 0.02f, 0.04f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(-22, 104, -90), new Vector3(-0.06f, -0.124f, 0.039f), false, Vector3.zero, Vector3.zero, false);
                            ((TaskState)state.state.behaviours[0]).des_grasping_state = 25;
                            break;

                        case "SpoonStab":
                            FillInState((TaskState)state.state.behaviours[0], "SpoonStab", "BLANDA_Serving_Bowl04", "utensil_spoon",
                                new Vector3(20, 0, 20), new Vector3(0.02f, 0.02f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(56, -28, 0), new Vector3(0.1f, -0.10f, -0.008f), true, new Vector3(-177, -115, 50), new Vector3(-0.025f, -0.083f, 0.024f), false);
                            break;
                        case "SpoonScoop":
                            FillInState((TaskState)state.state.behaviours[0], "SpoonScoop", "BLANDA_Serving_Bowl04", "utensil_spoon",
                                new Vector3(20, 0, 20), new Vector3(0.02f, 0.02f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(94, 19, 39), new Vector3(0.1f, 0.03f, -0.05f), true, Vector3.zero, Vector3.zero, false);
                            break;
                        case "SpoonEat":
                            FillInState((TaskState)state.state.behaviours[0], "SpoonEat", "Mouth", "utensil_spoon",
                                new Vector3(0, 0, 0), new Vector3(0.02f, 0.02f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(320, 56, -187), new Vector3(0.09f, 0.006f, -0.037f),
                                //true, new Vector3(-40, -80, -60), new Vector3(-0.07f, -0.123f, 0.016f), false); //wrt ForkParent
                                //true, new Vector3(-31, -107, -12), new Vector3(-0.1f, -0.1f, 0.118f), false); //wrt ForkMouth
                                true, Vector3.zero, Vector3.zero, false);
                            break;

                        case "ScrewReach":
                            FillInState((TaskState)state.state.behaviours[0], "ScrewReach", "screwdriver", "Wrist2",
                                new Vector3(20, 0, 20), new Vector3(0.02f, 0.03f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                 new Vector3(31, -215f, 54), new Vector3(-0.004f, -0.08f, 0.04f), false, Vector3.zero, Vector3.zero, true);
                            break;

                        case "ScrewGrab":
                            FillInState((TaskState)state.state.behaviours[0], "ScrewGrab", "screwdriver", "Wrist2",
                                new Vector3(20, 0, 20), new Vector3(0.02f, 0.03f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                 new Vector3(31, -215f, 54), new Vector3(-0.004f, -0.08f, 0.04f), false, Vector3.zero, Vector3.zero, true);
                            ((TaskState)state.state.behaviours[0]).des_grasping_state = 25;
                            break;
                        case "ScrewInsert":
                            FillInState((TaskState)state.state.behaviours[0], "ScrewInsert", "screw", "screwdriver",
                                new Vector3(10, 0, 10), new Vector3(0.02f, 0.03f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                 new Vector3(0, 0, -90), new Vector3(-0.1f, -0.0f, -0.0f), true, Vector3.zero, Vector3.zero, true);
                            break;
                        case "ScrewTurn":
                            FillInState((TaskState)state.state.behaviours[0], "ScrewTurn", "screw", "arrow",
                                new Vector3(10, 10, 10), new Vector3(0.02f, 0.03f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                 new Vector3(0, -90, 0), new Vector3(-0.0f, -0.0f, -0.0f), true, Vector3.zero, Vector3.zero, false);
                            break;
                        case "KettleReach":
                            FillInState((TaskState)state.state.behaviours[0], "KettleReach", "kettle_handle", "Wrist2",
                                new Vector3(20, 0, 20), new Vector3(0.02f, 0.03f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                 new Vector3(-9, 78, -175), new Vector3(-0.01f, -0.086f, 0.034f), false, Vector3.zero, Vector3.zero, true);
                            break;

                        case "KettleGrab":
                            FillInState((TaskState)state.state.behaviours[0], "KettleGrab", "kettle_handle", "Wrist2",
                                new Vector3(20, 0, 20), new Vector3(0.02f, 0.03f, 0.02f),
                                Vector3.zero, Vector3.zero,
                             new Vector3(-9, 78, -175), new Vector3(-0.01f, -0.086f, 0.034f), false, Vector3.zero, Vector3.zero, true);
                            ((TaskState)state.state.behaviours[0]).des_grasping_state = 35;
                            break;

                        case "KettlePour":
                            FillInState((TaskState)state.state.behaviours[0], "KettlePour", "Mug (3)/PourTarget", "kettle_handle",
                                new Vector3(20, 0, 20), new Vector3(0.02f, 0.03f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                 new Vector3(24, 125, 10), new Vector3(0.136f, 0.005f, -0.03f), true,
                                 Vector3.zero, Vector3.zero, false);
                            ((TaskState)state.state.behaviours[0]).des_grasping_state = 25;
                            break;

                        case "KettleReturn":
                            FillInState((TaskState)state.state.behaviours[0], "KettleReturn", "Table", "kettle_handle",
                                new Vector3(20, 0, 20), new Vector3(0.1f, 0.04f, 0.1f),
                                Vector3.zero, Vector3.zero,
                                 new Vector3(90, 0f, 00), new Vector3(0.2f, 0.92f, -0.75f), true,
                                 Vector3.zero, Vector3.zero, false);
                            break;

                        case "CellPhoneCall":
                            FillInState((TaskState)state.state.behaviours[0], "CellPhoneCall", "Mouth", "phone",
                                new Vector3(30, 30, 30), new Vector3(0.1f, 0.1f, 0.1f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(300, 90, -44), new Vector3(-0.02f, 0.09f, 0.055f),
                                //true, new Vector3(-40, -80, -60), new Vector3(-0.07f, -0.123f, 0.016f), false); //wrt ForkParent
                                true, new Vector3(-41, -172, -105), new Vector3(0.005f, -0.113f, 0.027f), false); //wrt ForkMouth
                            break;
                        case "CookReachPan":
                            FillInState((TaskState)state.state.behaviours[0], "CookReachPan", "frying_pan_grabhandle", "Wrist2",
                                new Vector3(40, 20, 0), new Vector3(0.02f, 0.02f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(35, 9, -24), new Vector3(-0.007f, -0.086f, 0.007f),
                                //true, new Vector3(-40, -80, -60), new Vector3(-0.07f, -0.123f, 0.016f), false); //wrt ForkParent
                                false, Vector3.zero, Vector3.zero, true); //wrt ForkMouth
                            break;
                        case "CookGrabPan":
                            FillInState((TaskState)state.state.behaviours[0], "CookGrabPan", "frying_pan_grabhandle", "Wrist2",
                                new Vector3(40, 20, 0), new Vector3(0.02f, 0.02f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(35, 9, -24), new Vector3(-0.007f, -0.086f, 0.007f),
                                //true, new Vector3(-40, -80, -60), new Vector3(-0.07f, -0.123f, 0.016f), false); //wrt ForkParent
                                false, Vector3.zero, Vector3.zero, true); //wrt ForkMouth
                            ((TaskState)state.state.behaviours[0]).des_grasping_state = 35;
                            break;
                        case "CookPlacePan":
                            FillInState((TaskState)state.state.behaviours[0], "CookPlacePan", "cook_base", "frying_pan_grabhandle",
                                new Vector3(20, 0, 20), new Vector3(0.02f, 0.02f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(0, -90, 0), new Vector3(0.0f, -0.01f, 0.23f),
                                //true, new Vector3(-40, -80, -60), new Vector3(-0.07f, -0.123f, 0.016f), false); //wrt ForkParent
                                true, Vector3.zero, Vector3.zero, true);
                            break;

                        case "CookReachKnob":
                            FillInState((TaskState)state.state.behaviours[0], "CookReachKnob", "cook_knob", "Wrist2",
                                new Vector3(15, 15, 15), new Vector3(0.03f, 0.03f, 0.03f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(27, -103, -33), new Vector3(0.014f, -0.124f, 0.058f),
                                //true, new Vector3(-40, -80, -60), new Vector3(-0.07f, -0.123f, 0.016f), false); //wrt ForkParent
                                false, Vector3.zero, Vector3.zero, false);
                            break;

                        case "CookTurnKnob":
                            FillInState((TaskState)state.state.behaviours[0], "CookTurnKnob", "cook_knob", "Wrist2",
                                new Vector3(15, 15, 15), new Vector3(0.03f, 0.03f, 0.03f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(-22, -42, -45), new Vector3(-0.01f, -0.125f, 0.055f),
                                //true, new Vector3(-40, -80, -60), new Vector3(-0.07f, -0.123f, 0.016f), false); //wrt ForkParent
                                false, Vector3.zero, Vector3.zero, false);
                            break;

                        case "AxillaReach":
                            FillInState((TaskState)state.state.behaviours[0], "AxillaReach", "Torso_origin", "Wrist2",
                                new Vector3(30, 30, 30), new Vector3(0.1f, 0.1f, 0.1f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(-44, 46, 116), new Vector3(-0.19f, -0.08f, 0.056f),
                                //true, new Vector3(-40, -80, -60), new Vector3(-0.07f, -0.123f, 0.016f), false); 
                                false, Vector3.zero, Vector3.zero, false);
                            ((TaskState)state.state.behaviours[0]).des_grasping_state = -1;

                            break;
                        case "KnobReach":
                            FillInState((TaskState)state.state.behaviours[0], "KnobReach", "Door_Knob", "Wrist2",
                                new Vector3(15, 15, 15), new Vector3(0.02f, 0.02f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(-53, -143, 27), new Vector3(0.003f, -0.146f, 0.072f),
                                //true, new Vector3(-40, -80, -60), new Vector3(-0.07f, -0.123f, 0.016f), false); //wrt ForkParent
                                false, Vector3.zero, Vector3.zero, false); //wrt ForkMouth
                            break;
                        case "KnobGrab":
                            FillInState((TaskState)state.state.behaviours[0], "KnobReach", "Door_Knob", "Wrist2",
                                new Vector3(15, 15, 15), new Vector3(0.02f, 0.02f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(-53, -143, 27), new Vector3(0.003f, -0.146f, 0.072f),
                                //true, new Vector3(-40, -80, -60), new Vector3(-0.07f, -0.123f, 0.016f), false); //wrt ForkParent
                                false, Vector3.zero, Vector3.zero, false); //wrt ForkMouth
                            ((TaskState)state.state.behaviours[0]).des_grasping_state = 25;
                            break;

                        case "KnobTurn":
                            FillInState((TaskState)state.state.behaviours[0], "KnobTurn", "Door_Knob", "Wrist2",
                                new Vector3(15, 15, 15), new Vector3(0.02f, 0.02f, 0.02f),
                                Vector3.zero, Vector3.zero,
                                new Vector3(-28, -172, -42), new Vector3(-0.009f, -0.134f, 0.061f),
                                //true, new Vector3(-40, -80, -60), new Vector3(-0.07f, -0.123f, 0.016f), false); //wrt ForkParent
                                false, Vector3.zero, Vector3.zero, false); //wrt ForkMouth
                            break;
                    }
                }
        }
    }
    public void PresetHand()
    {
        PresetHand(initial_wrist);
        if (current_mode == "Natural") PresetHand(Vector3.zero);
        if (current_mode == "NoWrist") PresetHand(Vector3.zero);

    }
    void PresetHand(Vector3 angs)
    {
        /*
        GameObject.Find("forearm_pronation").transform.localRotation = Quaternion.Euler(0, -angs.x, 0);
        GameObject.Find("wrist_deviation").transform.localRotation = Quaternion.Euler(-angs.y, 0, 0);
        GameObject.Find("wrist_extension").transform.localRotation = Quaternion.Euler(0, 0, -angs.z);       
        wrist_parser.pro_sup = (int)angs.x;
        wrist_parser.ext_flex = (int)angs.y;
        wrist_parser.rad_uln = (int)-angs.z;
         */
        /*
       GameObject.Find("hand_angle").transform.localRotation = Quaternion.identity;
       GameObject.Find("forearm_pronation").transform.localRotation = Quaternion.identity;
       GameObject.Find("wrist_extension").transform.localRotation = Quaternion.identity;
       GameObject.Find("wrist_deviation").transform.localRotation = Quaternion.identity;
       GameObject.Find("elbow_flexion").transform.localRotation = Quaternion.identity;
       GameObject.Find("Shoulder_int").transform.localRotation = Quaternion.identity;
       GameObject.Find("Shoulder_add").transform.localRotation = Quaternion.identity;
       GameObject.Find("Shoulder_poe").transform.localRotation=Quaternion.identity;
       */

        switch (current_mode)
        {
            case "Simultaneous":
                GameObject.Find("Inputs").GetComponent<ControlSimultaneous>().ResetHand();
                break;
            case "Sequential":
                GameObject.Find("Inputs").GetComponent<ControlSequential>().ResetHand();
                break;
            case "Trajectory":
                GameObject.Find("Inputs").GetComponent<ControlTrajectory>().ResetHand();
                break;
        }

    }
    void SetScene(string scene)
    {
        GameObject old_scene = current_scene;
        if (current_scene != null)
        {
            current_scene.SetActive(false);
        }
        //GameObject.Destroy(current_scene);
        //current_scene = GameObject.Find("WorldFrame");
        print("Setting " + scene);
        foreach (GameObject o in task_scenes)
        {
            if (o.name == scene)
            {
                current_scene = GameObject.Instantiate(o);
                current_scene.SetActive(true);
                current_scene.name = scene + "_active";
                current_scene.transform.position = o.transform.position;
                current_scene.transform.rotation = o.transform.rotation;
                //current_scene.transform.SetParent(GameObject.Find("Task").transform);                
            }
            else
            {
                //   o.SetActive(false);
            }
        }
        GameObject.Destroy(old_scene);
    }

    void ResetObjects(string scene)
    {
        //  animator.Play("Start", 0);     
    }

    public void SetupState(string control_mode_in)
    {
        SetupState(current_state.state_name, control_mode_in);
    }

    public void SetupState(string state_name, string control_mode)
    {
        print("STATE " + state_name);

        switch (state_name)
        {
            case "CupReach":
                //PresetHand(angs_t4);
                SetScene("task07_cup_prox");
                ResetObjects("task07_cup_prox");
                //    traj_control.current_mode = 4;
                //    traj_control.trajFrame = 1;
                break;


            case "SpoonReach":
                SetScene("task01_spoon_scoop");
                ResetObjects("task01_spoon_scoop");
                //case "CupDrink":
                //PresetHand(angs_t2);
                //    SetScene("task07_cup_prox");
                //    ResetObjects("task07_cup_prox");
                //  traj_control.current_mode = 2;
                //  traj_control.trajFrame = 388;
                break;
            case "CupPourReach":
                //PresetHand(angs_t3);
                SetScene("task04_pour");
                ResetObjects("task04_pour");
                // traj_control.current_mode = 3;
                // traj_control.trajFrame = 265;
                break;

            case "ForkReach":
                //PresetHand(angs_t5);
                SetScene("task02_fork_stab");
                ResetObjects("task02_fork_stab");
                // traj_control.current_mode = 5;
                // traj_control.trajFrame = 184;
                break;

            //case "ForkStab":
            //PresetHand(angs_t4);
            //  SetScene("task02_fork_stab");
            //  ResetObjects("task02_fork_stab");
            // traj_control.current_mode = 4;
            // traj_control.trajFrame = 298;
            // break;

            case "OverheadReach":
                //PresetHand(angs_t4);
                SetScene("task20_overhead");
                ResetObjects("task20_overhead");
                // traj_control.current_mode = 4;
                // traj_control.trajFrame = 1;                
                break;

            //case "OverheadReturn":
            //PresetHand(angs_t2);
            //    SetScene("task20_overhead");
            //    ResetObjects("task20_overhead");
            // traj_control.current_mode = 2;
            // traj_control.trajFrame = 389;
            //     break;

            // case "ForkEat":
            //PresetHand(angs_t1);
            //     SetScene("task02_fork_stab");
            //     ResetObjects("task02_fork_stab");
            // traj_control.current_mode = 1;
            // traj_control.trajFrame = 1;
            //    break;

            case "SuitcaseReach":
                //PresetHand(angs_t5);
                SetScene("task13_suitcase");
                ResetObjects("task13_suitcase");
                // traj_control.current_mode = 5;
                // traj_control.trajFrame = 1;
                break;

            case "SuitcaseTransfer":
                //PresetHand(angs_t4);
                SetScene("task13_suitcase");
                ResetObjects("task13_suitcase");
                // traj_control.current_mode = 4;
                // traj_control.trajFrame = 298;
                break;
            case "ScrewReach":
                //PresetHand(angs_t4);
                SetScene("task21_screw");
                ResetObjects("task21_screw");
                // traj_control.current_mode = 4;
                // traj_control.trajFrame = 298;
                break;

            //case "ScrewInsert":
            //    SetScene("task21_screw");
            //    ResetObjects("task21_screw");
            //    break;
            //case "ScrewTurn":
            //    SetScene("task21_screw");
            //    ResetObjects("task21_screw");
            //    break;
            case "KettleReach":
                SetScene("task22_kettle");
                ResetObjects("task22_kettle");
                break;
            //case "KettlePour":
            //    SetScene("task22_kettle");
            //    ResetObjects("task22_kettle");
            //    break;
            case "CellPhoneCall":
                SetScene("task23_cellphone");
                ResetObjects("task23_cellphone");
                break;
            case "CookReachPan":
                SetScene("task24_cook");
                ResetObjects("task24_cook");
                break;
            case "AxillaReach":
                SetScene("task25_empty");
                ResetObjects("task25_empty");
                break;

            case "KnobReach":
                SetScene("task15_door_knob");
                ResetObjects("task15_door_knob");
                break;


                //case "CookPlacePan":
                //    SetScene("task24_cook");
                //    ResetObjects("task24_cook");
                //    break;
                //case "CookTurnKnob":
                //    SetScene("task24_cook");
                //    ResetObjects("task24_cook");
                //    break;

        }
        //     float[] initial = traj_control.GetFrame(traj_control.current_mode, traj_control.trajFrame);
        //     initial_wrist = new Vector3(Mathf.Rad2Deg*initial[0], Mathf.Rad2Deg*initial[1], Mathf.Rad2Deg*initial[2]);
        //     PresetHand();
        //     if (control_mode == "Natural")            PresetHand(Vector3.zero);
        //     if (control_mode == "NoWrist")           PresetHand(Vector3.zero);


    }
    public void FillInState(TaskState state, string name, string target, string effector, Vector3 tol_r, Vector3 tol_p, Vector3 thres_r, Vector3 thres_p, Vector3 des_r, Vector3 des_p, bool grasped, Vector3 grasped_r, Vector3 grasped_p, bool display)
    {
        state.state_name = name;
        state.target = target;
        state.effector = effector;
        state.desired_p = des_p;
        state.desired_r = des_r;
        state.ang_tols = tol_r;
        state.pos_tols = tol_p;
        state.pos_thres = thres_p;
        state.ang_thres = thres_r;
        state.object_grasped = grasped;
        state.grasped_r = grasped_r;
        state.grasped_p = grasped_p;
        state.display_cones = display;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public int getCurrentDOF()
    {
        int mode = getMode();
        if (mode == 1) return (seq_control.current_joint);
        else if (mode == 3) return (traj_control.current_mode);
        else return (0);
    }
    public int getMode()
    {
        // [string] mode={nat=0,seq=1,sim=2,tra=3}

        switch (current_mode)
        {
            case "Natural":
                return (0);
            case "Sequential":
                return (1);
            case "Simultaneous":
                return (2);
            case "Trajectory":
                return (3);
            default:
                return (1);
        }
    }
    public int getDOF()
    {
        switch (current_DOF)
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
}
