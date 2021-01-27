using System.Collections.Generic;
using UnityEngine;
public class Constants
{
    // Marker variables
    public static readonly float[,] joint_limits = new float[,] { { 0, 90 }, { -20, 50 }, { -70, 90 }, { -90, 90 }, { 0, 150 }, { -150, 90 }, { 0, 180 }, { -45, 130 } };
    public static readonly string joy_skeleton_name = "joy_skel";
    public static readonly string joy_skeleton_name2 = "joy_skel_v2";
    public static readonly string torso_name = "FreeTorso2";
    public static readonly string pelvis_name = "FreePelvis";


    public static readonly string arm_skeleton_name = "FreeBrace";
    public static readonly List<string> arm_segment_names = new List<string>()
    {
        "HumR",
        "ForeR",
        //"handR"
    };


    public static readonly List<string> pelvis_segment_names = new List<string>()
    {
    "Pelvis"
    };


    public static readonly List<string> pelvis_marker_names = new List<string>()
    {
        "FreePelvis1",
        "FreePelvis2",
        "FreePelvis3",
        "FreePelvis4",
        "FreePelvis5"
    };


    public static readonly List<string> torso_segment_names = new List<string>()
    {
    "Torso"
    };

    public static readonly List<string> torso_marker_names = new List<string>()
    {
        "FreeTorso1",
        "FreeTorso2",
        "FreeTorso3",
        "FreeTorso4",
        "FreeTorso5",
        "FreeTorso6",
        "FreeTorso7",
        "FreeTorso8",
        "FreeTorso9"
    };


    public static readonly Dictionary<string, Matrix4x4> initial_transforms = new Dictionary<string, Matrix4x4>()
    {
        { "Shoulder",Matrix4x4.TRS(
            new Vector3(-0.0638f,-0.0904f,-0.0008f),
            Quaternion.Euler(0,90,0),Vector3.one) },

        { "Elbow",Matrix4x4.TRS(
            new Vector3(0.001f,-0.192f,0.027f),
            Quaternion.Euler(0,0,0),Vector3.one) }
    };


    public static readonly Dictionary<string,List<string>> arm_marker_names = new Dictionary<string, List<string>>()
    {
        { arm_segment_names[0] ,new List<string> {
            "HumR1",
            "HumR2",
            "HumR3",
            "HumR4",
            "HumR5",
            "HumR6"} },
        { arm_segment_names[1], new List<string> {            
            "ForeR1",
            "ForeR3",
            "ForeR4",
            "ForeR2",
            "ForeR5"} },
        /*{ arm_segment_names[2], new List<string> {
            "handR1",
            "handR2",
            "handR3",
            "handR4",
            "handR5" } }*/
    };
    public static Vector3 getJointAxis(int i)
    {
        switch (i)
        {
            case 0:
                return (new Vector3(1, 0, 0));
            case 1:
                return (new Vector3(1, 0, 0));
            case 2:
                return (new Vector3(0, 0, -1));            
            case 3:
                return (new Vector3(0, -1, 0));
            case 4:
                return (new Vector3(0, 0, 1));
            case 5:
                return (new Vector3(0, -1, 0));
            case 6:
                return (new Vector3(0, 0, -1));
            case 7:
                return (new Vector3(0, -1, 0));
            default:
                return (new Vector3(0, 0, 0));
        }
    }

    public static readonly string healthy_skeleton_name = "one_arm";
    public static readonly List<string> healthy_segment_names = new List<string>()
    {
      //  "Pelvis",
      //  "Torso",
        "HumR",
        "ForeR",
        "HandR"
    };
    public static readonly Dictionary<string, List<string>> healthy_marker_names = new Dictionary<string, List<string>>()
    {/*
        { healthy_segment_names[0] ,new List<string> {
            "PelvisBL",
            "PelvisFL",
            "PelvisBR",
            "PelvisFR"
        } },
        { healthy_segment_names[1] ,new List<string> {
            "C7",
            "StrTop",
            "StrBottom",
            "T8",
            "TorsoSide"
        } },*/
        { healthy_segment_names[0] ,new List<string> {
            "HumR1",
            "HumR2",
            "HumR3",
            "HumR4",
            "HumR5",
            "HumR6"
        } },
        { healthy_segment_names[1], new List<string> {
            "ForeR1",
            "ForeR2",
            "ForeR3",
            "ForeR4",
            "ForeR5",
            "ForeR6"
        } },
        { healthy_segment_names[2], new List<string> {
            "HandR1",
            "HandR2",
            "HandR3",
            "HandR4",
            "HandR5" } }
    };


    //Calibration variables
    public static readonly int n_joy_markers = 4;
    public static readonly float t_calib = 4, delta_t = 1f;
    public static readonly string VIVE_contr = "Controller (right)";
    public static readonly List<string> joy_vicon_names = new List<string>()
    {
        "joystick_head1",
        "joystick_head2",
        "joystick_head3",
        "joystick_head4" };


    public static readonly string VIVE_contr2 = "Controller (left)";
    public static readonly List<string> joy_vicon_names2 = new List<string>()
    {
        "joystick_head_v21",
        "joystick_head_v22",
        "joystick_head_v23",
        "joystick_head_v24" };


    public static readonly List<string> joy_unity_names = new List<string>()
    {
        "JoystickMarkers/joystick_head1",
        "JoystickMarkers/joystick_head2",
        "JoystickMarkers/joystick_head3",
        "JoystickMarkers/joystick_head4" };

    public static readonly List<string> joy_unity_names2 = new List<string>()
    {
        "JoystickMarkers2/joystickv2_head1",
        "JoystickMarkers2/joystickv2_head2",
        "JoystickMarkers2/joystickv2_head3",
        "JoystickMarkers2/joystickv2_head4" };


    public static readonly float marker_tolerance = 0.005f;   
    public static readonly string camera_frame = "[CameraRig]";


}