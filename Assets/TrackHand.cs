using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackHand : MonoBehaviour
{
    ViconDataStreamClient vicon;
    Transform elbow,shoulder,wrist;
    Dictionary<string, Matrix4x4> T_seg2mark = new Dictionary<string, Matrix4x4>();
    Matrix4x4 TU2V;
    public Vector3 marker_sync = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        shoulder = GameObject.Find("Shoulder").transform;
        elbow = GameObject.Find("Shoulder/Elbow").transform;
        wrist = GameObject.Find("Shoulder/Elbow/elbow_flexion/Wrist/forearm_pronation/wrist_extension/wrist_deviation/Wrist2").transform;
        
        vicon = GetComponent<ViconDataStreamClient>();
        LoadCalib();

    }

    // Update is called once per frame
    void Update()
    {
        //Matrix4x4 Tu = TU2V * MarkerCalcs.GetSegmentPose("FreeHum", "HumR") * T_seg2mark["HumR"];
        Matrix4x4 Tu = TU2V * MarkerCalcs.GetSegmentPose("FreeBraceH2", "HumR") * T_seg2mark["HumBrace"];
        shoulder.position = new Vector3(Tu.m03, Tu.m13, Tu.m23);
        shoulder.rotation = Tu.rotation;


        /*Dictionary<string, Vector3> markers_raw2 = MarkerCalcs.GetMarkersPosition("FreeFore");
        marker_sync = Vector3.zero;
        if (markers_raw2.ContainsKey("FreeForeR1"))
        {
            marker_sync = markers_raw2["FreeForeR1"];
        }*/
        Dictionary<string, Vector3> markers_raw = MarkerCalcs.GetMarkersPosition("FreeBraceH2");
        marker_sync = Vector3.zero;
        if (markers_raw.ContainsKey("HumR1"))
        {
            marker_sync = markers_raw["HumR1"];
        }


        //Matrix4x4 Tf = TU2V * MarkerCalcs.GetSegmentPose("FreeFore","FreeForeR") * T_seg2mark["ForeR"];
        Matrix4x4 Tf = TU2V * MarkerCalcs.GetSegmentPose("FreeBraceF", "ForeR") * T_seg2mark["ForeBrace"];
        elbow.position = new Vector3(Tf.m03, Tf.m13, Tf.m23);
        elbow.rotation = Tf.rotation;
        
        Matrix4x4 Th = TU2V * MarkerCalcs.GetSegmentPose("FreeHand", "HandR") * T_seg2mark["HandR"];
        wrist.position = new Vector3(Th.m03, Th.m13, Th.m23);
        wrist.rotation = Th.rotation;

    }
    public void LoadCalib()
    {
        T_seg2mark.Clear();
        T_seg2mark.Add("HumR", Utils.LoadFromXML<Matrix4x4>("TCal_HumR"));
        T_seg2mark.Add("ForeR", Utils.LoadFromXML<Matrix4x4>("TCal_ForeR2"));
        T_seg2mark.Add("HandR", Utils.LoadFromXML<Matrix4x4>("TCal_HandR2"));

        T_seg2mark.Add("HumBrace", Utils.LoadFromXML<Matrix4x4>("TCal_BraceShoulderR"));
        T_seg2mark.Add("ForeBrace", Utils.LoadFromXML<Matrix4x4>("TCal_BraceElbowR"));

        TU2V = Matrix4x4.identity;
    }
}
