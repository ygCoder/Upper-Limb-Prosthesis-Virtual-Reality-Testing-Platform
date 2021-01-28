using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using ViconDataStreamSDK.CSharp;

public class DataLogger : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] string filename="";
    [SerializeField] string subject = "";
    [SerializeField] string n_trial = "";
    StringBuilder sbOut;
    string header = "% Time,Wrist,,,,,,,Effector,,,,,,,Target,,,,,,,Desired,,,,,,,Head,,,,,,,Elbow,,,,,,,Shoulder,,,,,,,Torso,,,,,,,Pelvis,,,,,,,ArmJoints,,,,,,,HandClosure,NSwitches,Complete";
    string str_sep = ",";
    
    StreamWriter streamWriter;
    List<GameObject> objects;
    List<string> object_names;
    List<GameObject> JointAngles= new List<GameObject>();
    handClose handclose;
    GameObject controller_obj;
    TaskMain taskMain;
    string cur_target,cur_mode;
    bool got_objects = false;
    string start_time;
    public int n_switches = 0;
    void Start()
    {
        taskMain = GameObject.Find("Task").GetComponent<TaskMain>();
        sbOut = new StringBuilder();
        controller_obj = GameObject.Find("Controller");
    }

    string GetFilename()
    {
        string fname = "Results\\p" + subject + "_" + taskMain.current_state.state_name + "_" + taskMain.current_mode + "_" + (taskMain.getDOF()-1).ToString() + "_" + n_trial +"_"+ start_time +".csv";
        return (fname);
    }

    public void Start2()
    {
        got_objects = false;
    }



    // Update is called once per frame
    void Update()
    {
        if (taskMain.logging_data)
        {
            sbOut.Clear();
            if (!got_objects)
            {
                objects = new List<GameObject>();
                JointAngles = new List<GameObject>();
                JointAngles.Add(GameObject.Find("Shoulder_poe"));
                JointAngles.Add(GameObject.Find("Shoulder_add"));
                JointAngles.Add(GameObject.Find("Shoulder_int"));
                JointAngles.Add(GameObject.Find("elbow_flexion"));
                JointAngles.Add(GameObject.Find("forearm_pronation"));
                JointAngles.Add(GameObject.Find("wrist_extension"));
                JointAngles.Add(GameObject.Find("wrist_deviation"));
                JointAngles.Add(GameObject.Find("hand_angle"));
                //handclose = GameObject.Find("hand_angle").GetComponent<handClose>();
                n_switches = 0;
                if (GetObjects(object_names, objects))
                {
                    start_time = System.DateTime.UtcNow.ToFileTime().ToString();
                    filename = GetFilename();
                    got_objects = true;
                }
            }
            else
            {
                if (!File.Exists(filename)) File.WriteAllText(filename, header + "\n");
                if (taskMain.current_state.target != cur_target || taskMain.current_mode != cur_mode) Start2(); //Reset if object or mode changed
                sbOut.Append(Time.time.ToString("000.000") + str_sep);
                foreach (GameObject obj in objects)
                {
                    sbOut.Append(PoseToString(obj.transform) + str_sep);
                }
                List<float> joint_angles = GetJointAngles();
                foreach (float j in joint_angles)
                {
                    sbOut.Append(j.ToString("000.000") + str_sep);
                }

                //sbOut.Append(handclose.hand_closure.ToString()+str_sep);
                n_switches = taskMain.getCurrentDOF();
                sbOut.Append(n_switches.ToString() + str_sep);

                int result =0;
                if (taskMain.current_state.CheckComplete()) result = 1;
                else if (taskMain.current_state.CheckFail()) result = -1;


                sbOut.Append(result);


                //Marker Sync bit
                Vector3 marker_sync = Vector3.zero;
                if (controller_obj.GetComponent<TrackHand>().enabled)
                {
                    marker_sync = controller_obj.GetComponent<TrackHand>().marker_sync;
                }
                if (controller_obj.GetComponent<MoveArm>().enabled)
                {
                    marker_sync = controller_obj.GetComponent<MoveArm>().marker_sync;
                }

                sbOut.Append(str_sep + marker_sync.x.ToString("000.000") 
                           + str_sep + marker_sync.y.ToString("000.000")
                           + str_sep + marker_sync.z.ToString("000.000"));


                File.AppendAllText(filename, sbOut.ToString() + "\n");
            }
        }
        else
        {
            got_objects = false;
        }
        
    }

    List<float> GetJointAngles()
    {
        List<float> j_angs = new List<float>();
        ///TODO: Calculate Joint Angles
        j_angs.Add(Utils.WrapAngle(JointAngles[0].transform.localRotation.eulerAngles.y)); //shoulder1
        j_angs.Add(Utils.WrapAngle(JointAngles[1].transform.localRotation.eulerAngles.z)); //shoulder2
        j_angs.Add(Utils.WrapAngle(JointAngles[2].transform.localRotation.eulerAngles.y)); //shoulder3
        j_angs.Add(Utils.WrapAngle(JointAngles[3].transform.localRotation.eulerAngles.z)); //elbow

        j_angs.Add(Utils.WrapAngle(JointAngles[4].transform.localRotation.eulerAngles.y)); //wrist pronation 
        j_angs.Add(Utils.WrapAngle(JointAngles[5].transform.localRotation.eulerAngles.z)); //wrist extension
        j_angs.Add(Utils.WrapAngle(JointAngles[6].transform.localRotation.eulerAngles.x)); //wrist deviation
        j_angs.Add(Utils.WrapAngle(JointAngles[7].transform.localRotation.eulerAngles.x)); //hand_closure
        
        return j_angs;
    }

    string PoseToString(Transform transf_in)
    {
        string a = transf_in.position.x.ToString("00.000") + str_sep +
            transf_in.position.y.ToString("00.000") + str_sep +
            transf_in.position.z.ToString("00.000") + str_sep +
            transf_in.rotation.w.ToString("0.000") + str_sep +
            transf_in.rotation.x.ToString("0.000") + str_sep +
            transf_in.rotation.y.ToString("0.000") + str_sep +
            transf_in.rotation.z.ToString("0.000");
        return (a);
    }
   bool GetObjects(List<string> objects, List<GameObject> out_objs)
    {
        if (taskMain.current_state == null) return false;
        cur_target = taskMain.current_state.target;
        cur_mode = taskMain.current_mode;
        objects = new List<string>();
        objects.Add("Wrist2");
        objects.Add(taskMain.current_state.effector);
        objects.Add(cur_target);
        objects.Add("Effector");
        objects.Add("[CameraRig]/Camera");
        objects.Add("Elbow");
        objects.Add("Shoulder");
        objects.Add("Torso_origin");
        objects.Add("Pelvis");
        //List<GameObject> out_objs= new List<GameObject>();
        foreach (string str in objects)
        {  
                out_objs.Add(GameObject.Find(str));         
        }
        if (objects.Count == out_objs.Count) return (true);
        else return (false);
        //return (out_objs);
    }
}
