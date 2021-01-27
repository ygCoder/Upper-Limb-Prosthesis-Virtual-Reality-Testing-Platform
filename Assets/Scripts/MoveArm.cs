using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MoveArm : MonoBehaviour
{
    // Start is called before the first frame update
    Transform shoulder, elbow, wrist, base_pos;
    Vector3 mouse_init, init_pose;
    ViconDataStreamClient vicon;
    GUIMove gui_script;
    [SerializeField] private float speed = 100;
    float t_start;
    //string skeleton_name = "FreeBrace";
    Dictionary<string, Matrix4x4> T_seg2mark = new Dictionary<string, Matrix4x4>();
    Matrix4x4 TU2V;
    GameObject old_GameObj;
    public Vector3 marker_sync=Vector3.zero;
    //public int DOF_controlled = 3;
    TaskMain taskmain;

    void Start()
    {

        //mouse_init = Input.mousePosition;
        base_pos = GameObject.Find("Torso_origin").transform;
        init_pose = base_pos.position;
        //ZombieArm/Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Neck/Bip01 R Clavicle/
        taskmain = GameObject.Find("Task").GetComponent<TaskMain>();

        //shoulder = GameObject.Find("Shoulder").transform;
        //elbow = GameObject.Find("Elbow").transform;
        //wrist = GameObject.Find("Wrist").transform;
        try
        {
            shoulder = base_pos.Find("Shoulder_joint/Shoulder_poe/Shoulder_add/Shoulder_int/Shoulder").transform;
            elbow = base_pos.Find("Shoulder_joint/Shoulder_poe/Shoulder_add/Shoulder_int/Shoulder/Elbow").transform;
            wrist = base_pos.Find("Shoulder_joint/Shoulder_poe/Shoulder_add/Shoulder_int/Shoulder/Elbow/elbow_flexion/Wrist").transform;
            vicon = GetComponent<ViconDataStreamClient>();
            gui_script = GetComponent<GUIMove>();
            LoadCalib();
            old_GameObj = new GameObject("old_shoulder");
        }
        catch (Exception e)
        {
            Debug.LogError("Error Setting up MoveArm");
                
        }



    }
    public void LoadCalib()
    {
        T_seg2mark.Clear();
        T_seg2mark.Add("HumBrace", Utils.LoadFromXML<Matrix4x4>("TCal_BraceShoulderR"));
        T_seg2mark.Add("ForeBrace", Utils.LoadFromXML<Matrix4x4>("TCal_BraceElbowR"));
        //TU2V=Utils.LoadFromXML<Matrix4x4>("TCal_UnityVicon");
        //if (!TU2V.ValidTRS())
        //{
        //    TU2V = Matrix4x4.identity;
        //}
        //print("VALID TRS: " + TU2V.ValidTRS());        
        TU2V = Matrix4x4.identity;
    }
    public void startTiming()
    {
        t_start = Time.time;
    }
    // Update is called once per frame
    void Update()
    {
        Camera[] view = SceneView.GetAllSceneCameras();
        //print(view[0].transform.position);
        //print(view[0].transform.rotation);
        // base_pos.position = init_pose + Quaternion.Euler(0,-90,0)*(Input.mousePosition - mouse_init) * 0.001f;
        // print("L: " + wrist.localRotation.eulerAngles);
        // print("G: " + wrist.rotation.eulerAngles);

        //shoulder.Rotate(new Vector3(0, 1, 0), 0.1f);
        //elbow.Rotate(new Vector3(0, 0, 1), 0.1f);
        //wrist.Rotate(new Vector3(0, 0, 1), 0.5f);
        //wrist.localRotation = Quaternion.Euler(0, 0, 45f);
        //elbow.localRotation = Quaternion.Euler(0, 0, 0f);
        //Vector3 m_cur_pos = vicon.GetMarkerPosition("Joao_skeleton_1", "S21");

        
        Dictionary<string, Vector3> markers_raw = MarkerCalcs.GetMarkersPosition("FreeBraceH2");
        marker_sync = Vector3.zero;
        if (markers_raw.ContainsKey("HumR1")){
            marker_sync = markers_raw["HumR1"];
        }


        /*
        Dictionary<string, Vector3> markers = new Dictionary<string, Vector3>();
         foreach (KeyValuePair<string, Vector3> item in markers_raw)
         {
           // print(item.Key + " --- " + item.Value);
            markers.Add(item.Key,new Vector3(item.Value.x, item.Value.z, item.Value.y));             
         }
         */

        if (gui_script.vicon_toggle && markers_raw.Count>0)
        {
            //Matrix4x4 Tu = MarkerCalcs.CreateFrame(markers["upper1"] * 0.001f, markers["upper2"] * 0.001f, markers["upper3"] * -0.001f);
            //Matrix4x4 Tu_old = MarkerCalcs.CreateFrame(markers["humR1"] * 0.001f, markers["humR2"] * 0.001f, markers["humR4"] * 0.001f, markers["humR5"] * 0.001f);
            //old_GameObj.transform.position = Tu_old.GetPosition();
            //old_GameObj.transform.rotation = Tu_old.GetRotation();


            if (taskmain.getDOF() != 8)
            {
                Matrix4x4 Tu = TU2V * MarkerCalcs.GetSegmentPose("FreeBraceH2", "HumR") * T_seg2mark["HumBrace"];
                shoulder.position = new Vector3(Tu.m03, Tu.m13, Tu.m23);
                shoulder.rotation = Tu.rotation;
                //print(base_pos.position + "::" + base_pos.rotation + "++" + markers["upper1"]);
            }
            //Matrix4x4 Tf = MarkerCalcs.CreateFrame(markers["forearm1"] * 0.001f, markers["forearm2"] * 0.001f, markers["forearm3"] * 0.001f);
            //Matrix4x4 Tf = MarkerCalcs.CreateFrame(markers["humR6"] * 0.001f, markers["foreR1"] * 0.001f, markers["foreR3"] * 0.001f, markers["foreR4"] * 0.001f);
            if (taskmain.getDOF() == 4)
            {
                Matrix4x4 Tf = TU2V * MarkerCalcs.GetSegmentPose("FreeBraceF", "ForeR") * T_seg2mark["ForeBrace"];
                elbow.position = new Vector3(Tf.m03, Tf.m13, Tf.m23);
                elbow.rotation = Tf.rotation;
            }


        }

        //wrist.Rotate(new Vector3(1, 0, 0), gui_script.s1_val * Time.deltaTime * speed);
        //wrist.Rotate(new Vector3(0, 1, 0), gui_script.s2_val * Time.deltaTime * speed);
        //wrist.Rotate(new Vector3(0, 0, 1), gui_script.s3_val * Time.deltaTime * speed);


        //wrist.rotation = Quaternion.Slerp(wrist.rotation , wrist.rotation * new Quaternion(Math.Sign(gui_script.s1_val), 0, 0, 0), Math.Abs(gui_script.s1_val) * Time.deltaTime);
        //wrist.rotation = Quaternion.Slerp(wrist.rotation, wrist.rotation * new Quaternion(0, Math.Sign(gui_script.s2_val), 0, 0), Math.Abs(gui_script.s2_val) * Time.deltaTime);
        //wrist.rotation = Quaternion.Slerp(wrist.rotation, wrist.rotation * new Quaternion(0,0,Math.Sign(gui_script.s3_val), 0), Math.Abs(gui_script.s3_val) * Time.deltaTime);


    }

  public float getJointValue(int i)
    {
        Vector3 wrist_angs = wrist.localEulerAngles;
        Vector3 elbow_angs = elbow.localEulerAngles;
        switch (i)
        {
            case 1:
                return (wrist_angs.x);
            case 2:
                return (wrist_angs.y);
            case 3:
                return (wrist_angs.z);
            case 4:
                return (elbow_angs.z);
            default:
                return (0);
        }
    }
}
