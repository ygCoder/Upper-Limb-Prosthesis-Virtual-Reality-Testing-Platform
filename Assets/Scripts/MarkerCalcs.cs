using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViconDataStreamSDK.CSharp;

public class MarkerCalcs : MonoBehaviour
{
    static ViconDataStreamClient vicon;

    void Start()
    {
        vicon = GameObject.Find("Vicon").GetComponent<ViconDataStreamClient>();
        
    }

    private void Update()
    {
    }
    // Start is called before the first frame update
    public static Matrix4x4 CreateFrame(Vector3 v1, Vector3 v2, Vector3 v3) {
       Vector3 x, y, z;
        x = (v2 - v1).normalized;
        z = Vector3.Cross(x, (v3 - v1).normalized);
        y = Vector3.Cross(z, x).normalized;
        //Right-handed
        return (new Matrix4x4(new Vector4(x.x, x.y, x.z, 0),
                               new Vector4(y.x, y.y, y.z, 0),
                                    new Vector4(z.x, z.y, z.z, 0),
                                    new Vector4(v1.x, v1.y, v1.z, 1)));
    }

    public static Matrix4x4 CreateFrame(Vector3 m1, Vector3 m2, Vector3 m3, Vector3 m4)
    {
        Vector3 x, y, z;
        y = (m1 - m2).normalized;
        x = Vector3.Cross((m3 - m4).normalized, y).normalized;
        z = Vector3.Cross(x, y).normalized;

       return (new Matrix4x4(new Vector4(x.x, x.y, x.z, 0),
                               new Vector4(y.x, y.y, y.z, 0),
                                    new Vector4(z.x, z.y, z.z, 0),
                                    new Vector4(m1.x, m1.y, m1.z, 1)));
    }

    public static Matrix4x4 GetFrame(string skeleton_name, List<string> marker_names)
    {
        if (marker_names.Count != 4)
        {
            Debug.LogError("Should input 4 markers");
            return (Matrix4x4.identity);
        }

        Dictionary<string, Vector3> markers_raw = GetMarkersPosition(skeleton_name);
        Dictionary<string, Vector3> markers = new Dictionary<string, Vector3>();

        foreach (string str in marker_names)
        {
            if (!markers_raw.ContainsKey(str))
            {
                Debug.LogError("Marker " + str + "not tracked");
                return (Matrix4x4.identity);
            }

            Vector3 pos = new Vector3(markers_raw[str].x * 0.001f, markers_raw[str].z * 0.001f, markers_raw[str].y * 0.001f);
            markers.Add(str, pos);
        }
        return MarkerCalcs.CreateFrame(markers[marker_names[0]], markers[marker_names[1]], markers[marker_names[2]], markers[marker_names[3]]);
    }

    public static Matrix4x4 GetSegmentPose(string SubjectName, string SegmentName)
    {
        //Output_GetSegmentGlobalRotationQuaternion segrot = vicon.m_Client.GetSegmentGlobalRotationQuaternion(SubjectName, SegmentName);
        Output_GetSegmentGlobalRotationMatrix segrot_m = vicon.m_Client.GetSegmentGlobalRotationMatrix(SubjectName, SegmentName);
        Output_GetSegmentGlobalTranslation segtran = vicon.m_Client.GetSegmentGlobalTranslation(SubjectName, SegmentName);
        //Matrix4x4 ret = Matrix4x4.TRS(new Vector3((float)segtran.Translation[0] * 0.001f, (float)segtran.Translation[2] * 0.001f, (float)segtran.Translation[1] * 0.001f),
        //Utils.ConvertToUnity(new Quaternion((float)segrot.Rotation[0],(float)segrot.Rotation[1], (float)segrot.Rotation[2], (float)segrot.Rotation[3])), Vector3.one);

        Matrix4x4 ret = new Matrix4x4(new Vector4((float)segrot_m.Rotation[0], (float)segrot_m.Rotation[6], (float)segrot_m.Rotation[3], 0).normalized,
                                      new Vector4((float)segrot_m.Rotation[2], (float)segrot_m.Rotation[8], (float)segrot_m.Rotation[5], 0).normalized,
                                      new Vector4((float)segrot_m.Rotation[1], (float)segrot_m.Rotation[7], (float)segrot_m.Rotation[4], 0).normalized,
                                      new Vector4((float)segtran.Translation[0] * 0.001f, (float)segtran.Translation[2] * 0.001f, (float)segtran.Translation[1] * 0.001f, 1));

        return ret;
    }

    public static Dictionary<string, Vector3> GetMarkersPositionU(string SubjectName)
    {
        
        Dictionary<string, Vector3> markers_raw = GetMarkersPosition(SubjectName);
        Dictionary<string, Vector3> markers = new Dictionary<string, Vector3>();
        foreach (KeyValuePair<string, Vector3> item in markers_raw)
        {
            markers.Add(item.Key, new Vector3(item.Value.x * 0.001f, item.Value.z * 0.001f, item.Value.y * 0.001f));
        }
        return (markers);
    }

    public static Dictionary<string, Vector3> GetMarkersPosition(string SubjectName)
    {
        uint n_markers = vicon.m_Client.GetMarkerCount(SubjectName).MarkerCount;
        Dictionary<string, Vector3> ret = new Dictionary<string, Vector3>();

        if (n_markers > 3 && n_markers < 100)
        {
            for (uint i = 0; i < n_markers; i++)
            {
                double[] pos_array = new double[3];
                Vector3 pos = Vector3.zero;
                string name = vicon.m_Client.GetMarkerName(SubjectName, i).MarkerName;
                Output_GetMarkerGlobalTranslation marker = vicon.m_Client.GetMarkerGlobalTranslation(SubjectName, name);
                //print("[" + i + "]" + "m:" + m_Client.GetMarkerName(SubjectName, i).MarkerName + "r: " + marker.Result.ToString() + " Occ: " + marker.Occluded + "P: " + marker.Translation[0] + "," + marker.Translation[1] + "," + marker.Translation[2]);
                pos_array = marker.Translation;
                if (marker.Result == Result.Success)
                {
                    pos.x = (float)pos_array[0];
                    pos.y = (float)pos_array[1];
                    pos.z = (float)pos_array[2];
                }
                ret.Add(name, pos);
            }
        }

        return (ret);
    }

}
