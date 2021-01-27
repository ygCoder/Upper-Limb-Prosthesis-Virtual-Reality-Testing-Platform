using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LMDotNet;
using System;
public class Calibrate : MonoBehaviour
{
    static ViconDataStreamClient vicon;
    int n_meas = 0;
    bool first_display = true;
    bool calibratingVR = false;
    
    public bool displaying_joy_markers = true;
    public bool displaying_arm_markers = true;

    public bool calib_VR = false;
    public bool calib_seg = false;
    public bool calib_hand = false;

    float t_start = 0;
    static List<Matrix4x4> meas_vive, meas_vicon, meas_vive_in_cam;
    static Matrix4x4 Vicon_to_Vive, Cam_To_Vicon;
    static GameObject CamRig;
    float[,] InterMarkerDistance;


    GameObject VicMark, UniMark;


    Dictionary<string, GameObject> vicon_markers = new Dictionary<string, GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        vicon = GetComponent<ViconDataStreamClient>();
        CamRig = GameObject.Find("[CameraRig]");
        Matrix4x4 TV2U = Utils.LoadFromXML<Matrix4x4>("TCal_UnityVicon");
        Utils.MatrixToTransform(TV2U, CamRig.transform);

        VicMark = new GameObject("vic_mark");
        UniMark = new GameObject("uni_mark");

    }


    public void DisplayMarkers(string skeleton_name)
    {
        if (GameObject.Find("ViconMarkers") ==null)
        {
            SegDestroy(vicon_markers);
            vicon_markers.Add("ViconMarkers", new GameObject("ViconMarkers"));
        }
        Dictionary<string, Vector3> markers = MarkerCalcs.GetMarkersPositionU(skeleton_name);
        foreach (KeyValuePair<string, Vector3> marker in markers)
        {
          /*  if (marker.Key == "joystick_head1")
            {
                GameObject a = GameObject.Find("JoystickMarkers/joystick_head1");
                using (System.IO.StreamWriter sw = System.IO.File.AppendText("distances_sanity.txt"))
                {
                    sw.WriteLine(marker.Value.x + "," + marker.Value.y + "," + marker.Value.z + ","+ a.transform.position.x + "," + a.transform.position.y + "," + a.transform.position.z);
                }
                    print("M: " + marker.Value.x + "," + marker.Value.y + "," + marker.Value.z + " V: " + a.transform.position.x + "," + a.transform.position.y + "," + a.transform.position.z);                    
            }
            */
            GameObject sphere;
            if (!vicon_markers.ContainsKey(marker.Key))
            {
                sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                MeshRenderer mr = sphere.GetComponent<MeshRenderer>();
                mr.material.SetColor("_Color", new Color(0.7f, 0.1f, 0.1f, 0.8f));
                sphere.transform.SetParent(vicon_markers["ViconMarkers"].transform);
                sphere.transform.localScale = 0.01f * Vector3.one;
                sphere.transform.rotation = Quaternion.identity;
                sphere.name = marker.Key;
                vicon_markers.Add(marker.Key, sphere);
            }
            else
            {
                sphere = vicon_markers[marker.Key];
            }
            sphere.transform.position = marker.Value;
            
            //if (first_display) 
        }
        //if (first_display && vicon_markers.Count == 5) first_display = false;
    }


    Action<double[], double[]> CostFunctionRotPos = (x, r) =>
    {
        r[0] = 0;
        r[1] = 0;

        Matrix4x4 T=Matrix4x4.TRS(new Vector3((float)x[3], (float)x[4], (float)x[5]),
    Quaternion.Euler(new Vector3((float)x[0], (float)x[1], (float)x[2])),
    Vector3.one);

        for (int i = 0; i < meas_vicon.Count; i++)
        {
            //float ang_err = Quaternion.Angle((Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(new Vector3((float)x[0], (float)x[1], (float)x[2])), Vector3.one) * Vicon_to_Vive
            //                                * Cam_To_Vicon * meas_vive[i]).rotation, meas_vicon[i].rotation);



            Matrix4x4 viv = T * meas_vive_in_cam[i];
            float ang_err = Quaternion.Angle(viv.GetRotation(), meas_vicon[i].GetRotation());
            float pos_err = Vector3.Distance(viv.GetPosition(), meas_vicon[i].GetPosition());

            //float ang_err = Quaternion.Angle((Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(new Vector3((float)x[0], (float)x[1], (float)x[2])), Vector3.one) * meas_vive_in_cam[i]).rotation, meas_vicon[i].rotation);
            //float pos_err = Vector3.Distance((Matrix4x4.TRS(new Vector3((float)x[0], (float)x[1], (float)x[2]), Quaternion.identity, Vector3.one) * meas_vive_in_cam[i]).GetPosition(), meas_vicon[i].GetPosition());

            r[0] += (ang_err * ang_err)*1;
            r[1] += (pos_err * pos_err)*1;
        }
        r[0] = (float)Math.Sqrt(r[0]) / meas_vicon.Count;
        r[1] = (float)Math.Sqrt(r[1]) / meas_vicon.Count;
    };



    Action<double[], double[]> CostFunctionRot = (x, r) =>
    {
        r[0] = 0;
        for (int i = 0; i < meas_vicon.Count; i++)
        {
            //float ang_err = Quaternion.Angle((Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(new Vector3((float)x[0], (float)x[1], (float)x[2])), Vector3.one) * Vicon_to_Vive
            //                                * Cam_To_Vicon * meas_vive[i]).rotation, meas_vicon[i].rotation);
            float ang_err = Quaternion.Angle((Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(new Vector3((float)x[0], (float)x[1], (float)x[2])), Vector3.one) * meas_vive_in_cam[i]).rotation, meas_vicon[i].rotation);
            r[0] += ang_err * ang_err;
        }
        r[0] = (float)Math.Sqrt(r[0]) / meas_vicon.Count;
    };

    Action<double[], double[]> CostFunctionPos = (x, r) =>
    {
        r[0] = 0;
        r[1] = 0;
        r[2] = 0;
        for (int i = 0; i < meas_vicon.Count; i++)
        {
            // float pos_err = Vector3.Distance((Matrix4x4.TRS(new Vector3((float)x[0], (float)x[1], (float)x[2]), Quaternion.identity, Vector3.one) * Vicon_to_Vive
            //                             * Cam_To_Vicon * meas_vive[i]).GetPosition(), meas_vicon[i].GetPosition());
            float pos_err = Vector3.Distance((Matrix4x4.TRS(new Vector3((float)x[0], (float)x[1], (float)x[2]), Quaternion.identity, Vector3.one) * meas_vive_in_cam[i]).GetPosition(), meas_vicon[i].GetPosition());
            r[0] += pos_err * pos_err;

        }
        r[0] = (float)Math.Sqrt(r[0]) / meas_vicon.Count;
    };

    Action<double[], double[]> CostFunctionRot_old = (x, r) =>
    {
        r[0] = 0;
        for (int i = 0; i < meas_vicon.Count; i++)
        {
            float ang_err = Quaternion.Angle((Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(new Vector3((float)x[0], (float)x[1], (float)x[2])), Vector3.one) * Vicon_to_Vive
                                            * Cam_To_Vicon * meas_vive[i]).rotation, meas_vicon[i].rotation);
            r[0] += ang_err * ang_err;
        }
        r[0] = (float)Math.Sqrt(r[0]) / meas_vicon.Count;
    };

    Action<double[], double[]> CostFunctionPos_old = (x, r) =>
    {
        r[0] = 0;
        r[1] = 0;
        r[2] = 0;
        for (int i = 0; i < meas_vicon.Count; i++)
        {
            float pos_err = Vector3.Distance((Matrix4x4.TRS(new Vector3((float)x[0], (float)x[1], (float)x[2]), Quaternion.identity, Vector3.one) * Vicon_to_Vive
                                        * Cam_To_Vicon * meas_vive[i]).GetPosition(), meas_vicon[i].GetPosition());
            r[0] += pos_err * pos_err;

        }
        r[0] = (float)Math.Sqrt(r[0]) / meas_vicon.Count;
    };

    bool SetMarkerValidity(Dictionary<string, Vector3> markers)
    {
        if (markers.Count != Constants.n_joy_markers)
        {
            Debug.LogError("Not all markers are visible (Should be " + Constants.n_joy_markers + ")");
            return false;
        }
        else
        {
            InterMarkerDistance = new float[markers.Count, markers.Count];
            for (int i = 0; i < markers.Count; i++)
            {
                for (int j = 1; j < markers.Count; j++)
                {
                    InterMarkerDistance[i, j] = Vector3.Distance(markers[Constants.joy_vicon_names[i]], markers[Constants.joy_vicon_names[j]]);
                    InterMarkerDistance[j, i] = Vector3.Distance(markers[Constants.joy_vicon_names[i]], markers[Constants.joy_vicon_names[j]]);
                }
            }
            return true;
        }
    }

    bool CheckMarkersValid(Dictionary<string, Vector3> markers)
    {
        if (markers.Count != Constants.n_joy_markers) return false;
        else
        {
            for (int i = 0; i < markers.Count; i++)
            {
                for (int j = i + 1; j < markers.Count; j++)
                {
                    if (Mathf.Abs(InterMarkerDistance[i, j] - Vector3.Distance(markers[Constants.joy_vicon_names[i]], markers[Constants.joy_vicon_names[j]])) > Constants.marker_tolerance)
                    {
                        Debug.LogWarning("Bad measurement");
                        return false;
                    }
                }
            }
            return true;
        }
    }


    void AddMeasurementFrame(int n_meas)
    {
        System.Random rand = new System.Random();
        Color c = new Color((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 0.7f);

        foreach (KeyValuePair<string, GameObject> g in vicon_markers)
        {
            if (g.Value.TryGetComponent<MeshRenderer>(out MeshRenderer mr))
            {
                mr.material.SetColor("_Color", c);
            }
        }
        Dictionary<string, Vector3> markers = MarkerCalcs.GetMarkersPositionU(Constants.joy_skeleton_name);
        if (n_meas == 0 || InterMarkerDistance == null)
        {
            SetMarkerValidity(markers);
        }

        if (CheckMarkersValid(markers))
        {
            CamRig = GameObject.Find(Constants.camera_frame);

            Matrix4x4 m1 = MarkerCalcs.CreateFrame(GameObject.Find(Constants.VIVE_contr + "/" + Constants.joy_unity_names[0]).transform.position,
                            GameObject.Find(Constants.VIVE_contr + "/" + Constants.joy_unity_names[1]).transform.position,
                            GameObject.Find(Constants.VIVE_contr + "/" + Constants.joy_unity_names[2]).transform.position,
                            GameObject.Find(Constants.VIVE_contr + "/" + Constants.joy_unity_names[3]).transform.position);
            meas_vive.Add(m1);
            meas_vive_in_cam.Add(Matrix4x4.TRS(CamRig.transform.position, CamRig.transform.rotation, Vector3.one).inverse * m1);


            Dictionary<string, Vector3> markers2 = MarkerCalcs.GetMarkersPositionU(Constants.joy_skeleton_name2);


            Matrix4x4 m12 = MarkerCalcs.CreateFrame(GameObject.Find(Constants.VIVE_contr2 + Constants.joy_unity_names2[0]).transform.position,
                GameObject.Find(Constants.VIVE_contr2 + "/" + Constants.joy_unity_names2[1]).transform.position,
                GameObject.Find(Constants.VIVE_contr2 + "/" + Constants.joy_unity_names2[2]).transform.position,
                GameObject.Find(Constants.VIVE_contr2 + "/" + Constants.joy_unity_names2[3]).transform.position);
            meas_vive.Add(m12);            
            meas_vive_in_cam.Add(Matrix4x4.TRS(CamRig.transform.position, CamRig.transform.rotation, Vector3.one).inverse * m12);



            Matrix4x4 m2 = MarkerCalcs.CreateFrame(markers[Constants.joy_vicon_names[0]], markers[Constants.joy_vicon_names[1]], markers[Constants.joy_vicon_names[2]], markers[Constants.joy_vicon_names[3]]);
            meas_vicon.Add(m2);

            Matrix4x4 m22 = MarkerCalcs.CreateFrame(markers2[Constants.joy_vicon_names2[0]], markers2[Constants.joy_vicon_names2[1]], markers2[Constants.joy_vicon_names2[2]], markers2[Constants.joy_vicon_names2[3]]);
            meas_vicon.Add(m22);

            //DEBUG FRAMES
            VicMark.transform.position = m2.GetPosition();
            VicMark.transform.rotation = m2.GetRotation();
            //UniMark.transform.position = m1.GetPosition();
            //UniMark.transform.rotation = m1.GetRotation();
            UniMark.transform.SetParent(CamRig.transform);
            UniMark.transform.localPosition = meas_vive_in_cam[meas_vive_in_cam.Count - 1].GetPosition();
            UniMark.transform.localRotation = meas_vive_in_cam[meas_vive_in_cam.Count - 1].GetRotation();
            
            /*
            using (System.IO.StreamWriter sw = System.IO.File.AppendText("calibration_raw.txt"))
            {
                sw.WriteLine(meas_vicon[meas_vicon.Count - 1].GetPosition().x + "," +
                    meas_vicon[meas_vicon.Count - 1].GetPosition().y + "," +
                    meas_vicon[meas_vicon.Count - 1].GetPosition().z + "," +
                    meas_vicon[meas_vicon.Count - 1].GetRotation().w + "," +
                    meas_vicon[meas_vicon.Count - 1].GetRotation().x + "," +
                    meas_vicon[meas_vicon.Count - 1].GetRotation().y + "," +
                    meas_vicon[meas_vicon.Count - 1].GetRotation().z + "," +

                    meas_vive_in_cam[meas_vive_in_cam.Count - 1].GetPosition().x + "," +
                    meas_vive_in_cam[meas_vive_in_cam.Count - 1].GetPosition().y + "," +
                    meas_vive_in_cam[meas_vive_in_cam.Count - 1].GetPosition().z + "," +
                    meas_vive_in_cam[meas_vive_in_cam.Count - 1].GetRotation().w + "," +
                    meas_vive_in_cam[meas_vive_in_cam.Count - 1].GetRotation().x + "," +
                    meas_vive_in_cam[meas_vive_in_cam.Count - 1].GetRotation().y + "," +
                    meas_vive_in_cam[meas_vive_in_cam.Count - 1].GetRotation().z + ","
                    );
            }
            */
        }
    }


    void DestroyMarkers(List<string> markers)
    {
        foreach (string marker_name in markers)
        {
            vicon_markers.Remove(marker_name);
            GameObject obj = GameObject.Find(marker_name);
            if(obj!=null)  GameObject.Destroy(obj);
        }
    }

    Matrix4x4 OptimizeTransform()
    {
        Matrix4x4 ResultT = Matrix4x4.identity;
        var lma_solver = new LMSolver(epsilon: 0.0001, xtol: 0.0001, ftol: 0.0001, verbose: true);
        Vector3 angles = Vicon_to_Vive.GetRotation().eulerAngles;
        Vector3 pos = Vicon_to_Vive.GetPosition();
/*
        double[] x0f = { angles.x, angles.y, angles.z, pos.x, pos.y, pos.z };
        var result = lma_solver.Solve(CostFunctionRotPos, x0f);
        print("SOLVER: " + result.Message + "@" + result.Iterations + ": " + result.ErrorNorm + 
            " [" + result.OptimizedParameters[3] + "," +(float)result.OptimizedParameters[4] + "," + (float)result.OptimizedParameters[5] + "]"+
            " [" + result.OptimizedParameters[0] + "," + (float)result.OptimizedParameters[1] + "," + (float)result.OptimizedParameters[2] + "]"
            );

        ResultT.SetTRS(new Vector3((float)result.OptimizedParameters[3], (float)result.OptimizedParameters[4], (float)result.OptimizedParameters[5]),
                            Quaternion.Euler((float)result.OptimizedParameters[0], (float)result.OptimizedParameters[1], (float)result.OptimizedParameters[2]),
                            Vector3.one);
                            */



                double[] x0fR = { angles.x, angles.y, angles.z};
                //double[] x0fR = { 0, 0, 0 };
                var resultR = lma_solver.Solve(CostFunctionRot, x0fR);
                Vicon_to_Vive.SetTRS(Vector3.zero, Quaternion.Euler((float)resultR.OptimizedParameters[0], (float)resultR.OptimizedParameters[1], (float)resultR.OptimizedParameters[2]), Vector3.one);

                double[] x0fP = { pos.x, pos.y, pos.z};
                //double[] x0fP = { 0, 0, 0 };
                var resultP = lma_solver.Solve(CostFunctionPos, x0fP);

                ResultT.SetTRS(new Vector3((float)resultP.OptimizedParameters[0], (float)resultP.OptimizedParameters[1], (float)resultP.OptimizedParameters[2]),
                Quaternion.Euler((float)resultR.OptimizedParameters[0], (float)resultR.OptimizedParameters[1], (float)resultR.OptimizedParameters[2]), Vector3.one);
        
        
                print("SOLVERR: " + resultR.Message + "@" + resultR.Iterations + ": " + resultR.ErrorNorm + 
            " [" + resultR.OptimizedParameters[0] + "," +(float)resultR.OptimizedParameters[1] + "," + (float)resultR.OptimizedParameters[2] + "]");
        
        print("SOLVERP: " + resultR.Message + "@" + resultP.Iterations + ": " + resultP.ErrorNorm +
            " [" + resultP.OptimizedParameters[0] + "," + (float)resultP.OptimizedParameters[1] + "," + (float)resultP.OptimizedParameters[2] + "]");

        return (ResultT);
    }

    // Update is called once per frame
    void Update()
    {
        if (displaying_joy_markers)
        {
            DisplayMarkers(Constants.joy_skeleton_name);
            DisplayMarkers(Constants.joy_skeleton_name2);
        }
        else DestroyMarkers(Constants.joy_vicon_names);

        //if (displaying_arm_markers) DisplayMarkers(Constants.arm_skeleton_name);
        //if (displaying_arm_markers) DisplayMarkers(Constants.healthy_skeleton_name);
        if (displaying_arm_markers)
        {
            DisplayMarkers("FreeBraceH");
            DisplayMarkers("FreeBraceF");
            DisplayMarkers("FreeFore");
            DisplayMarkers("FreeHand");


        }
        else
        {
            foreach (string seg_name in Constants.healthy_segment_names) DestroyMarkers(Constants.healthy_marker_names[seg_name]);
        }

        if (calibratingVR)
        {
            if (Time.time - t_start < Constants.t_calib)
            {
                if (Time.time - t_start > Constants.delta_t * (n_meas))
                {
                    print("Time: Meas" + n_meas);
                    AddMeasurementFrame(n_meas);
                    n_meas++;
                    Vicon_to_Vive = OptimizeTransform();
                    CamRig.transform.position = Vicon_to_Vive.GetPosition();
                    CamRig.transform.rotation = Vicon_to_Vive.GetRotation();

                }
            }
            else
            {
                //Vicon_to_Vive = Matrix4x4.identity;
                Vicon_to_Vive = OptimizeTransform();
                CamRig.transform.position = Vicon_to_Vive.GetPosition();
                CamRig.transform.rotation = Vicon_to_Vive.GetRotation();
                calibratingVR = false;
                Utils.SaveToXML<Matrix4x4>("TCal_UnityVicon", Vicon_to_Vive);
            }
        }
    }

    protected void SegDestroy(Dictionary<string, GameObject> segs)
    {
        foreach (KeyValuePair<string,GameObject> seg in segs)
        {
            GameObject.Destroy(seg.Value);
        }
        segs.Clear();
    }

    protected void SegDestroy(List<GameObject> segs)
    {
        foreach (GameObject seg in segs)
        {
            GameObject.Destroy(seg);
        }
        segs.Clear();
    }

    public void DoCalibration()
    {
        if(calib_VR) DoCalibrationVR();
        if (calib_seg)
        {
            //DoCalibrationSeg(Constants.healthy_skeleton_name, Constants.healthy_segment_names, Constants.healthy_marker_names);
            //DoCalibrationBraceH();
            //DoCalibrationBraceF();
            DoCalibrationFore();
        }
        if (calib_hand) DoCalibrationHand();
    }
    protected void StartTimer()
    {
        t_start = Time.time;
        n_meas = 0;
    }


    public void DoCalibrationVR3()
    {
        Dictionary<string, Vector3> markers = MarkerCalcs.GetMarkersPositionU(Constants.joy_skeleton_name);
        Matrix4x4 ViconTmarkers = MarkerCalcs.CreateFrame(markers["joystick_head1"], markers["joystick_head2"], markers["joystick_head3"], markers["joystick_head4"]);

        Matrix4x4 UnityTmarkers = MarkerCalcs.CreateFrame(GameObject.Find("JoystickMarkers/joystick_head1").transform.position,
                                                          GameObject.Find("JoystickMarkers/joystick_head2").transform.position,
                                                          GameObject.Find("JoystickMarkers/joystick_head3").transform.position,
                                                          GameObject.Find("JoystickMarkers/joystick_head4").transform.position);


        Dictionary<string, Vector3> markers2 = MarkerCalcs.GetMarkersPositionU(Constants.joy_skeleton_name);
        Matrix4x4 ViconTmarkers2 = MarkerCalcs.CreateFrame(markers["joystick_head_v21"], markers["joystick_head_v22"], markers["joystick_head_v23"], markers["joystick_head_v24"]);

        Matrix4x4 UnityTmarkers2 = MarkerCalcs.CreateFrame(GameObject.Find("JoystickMarkers2/joystickv2_head1").transform.position,
                                                          GameObject.Find("JoystickMarkers2/joystickv2_head2").transform.position,
                                                          GameObject.Find("JoystickMarkers2/joystickv2_head3").transform.position,
                                                          GameObject.Find("JoystickMarkers2/joystickv2_head4").transform.position);



    }


    public void DoCalibrationVR2()
    {
        Dictionary<string, Vector3> markers = MarkerCalcs.GetMarkersPositionU("joy_skel");

        Matrix4x4 ViconTmarkers = MarkerCalcs.CreateFrame(markers["joystick_head1"], markers["joystick_head2"], markers["joystick_head3"], markers["joystick_head4"]);
        Matrix4x4 UnityTmarkers = MarkerCalcs.CreateFrame(GameObject.Find("JoystickMarkers/joystick_head1").transform.position,
                                                          GameObject.Find("JoystickMarkers/joystick_head2").transform.position,
                                                          GameObject.Find("JoystickMarkers/joystick_head3").transform.position,
                                                          GameObject.Find("JoystickMarkers/joystick_head4").transform.position);


     


        Matrix4x4 T = ViconTmarkers * UnityTmarkers.inverse * Utils.TransformToMatrix(GameObject.Find("[CameraRig]").transform);


        //Matrix4x4 ViconTseg = MarkerCalcs.GetSegmentPose("one_arm", "HandR");
        //Matrix4x4 MarkersTwrist = UnityTmarkers.inverse * Utils.TransformToMatrix(GameObject.Find("Wrist2").transform);

        //Matrix4x4 T = ViconTmarkers* UnityTmarkers.inverse;

        Utils.SaveToXML<Matrix4x4>("TCal_UnityVicon2", T);

    }
    public void DoCalibrationHand()
    {
        Dictionary<string, Vector3> markers = MarkerCalcs.GetMarkersPositionU("FreeHand");

        Matrix4x4 ViconTmarkers =  MarkerCalcs.CreateFrame(markers["HandR1"], markers["HandR2"], markers["HandR3"], markers["HandR4"]);
        Matrix4x4 UnityTmarkers = MarkerCalcs.CreateFrame(GameObject.Find("HandMarkers/HandR1").transform.position,
                                                          GameObject.Find("HandMarkers/HandR2").transform.position,
                                                          GameObject.Find("HandMarkers/HandR3").transform.position,
                                                          GameObject.Find("HandMarkers/HandR4").transform.position);

        Matrix4x4 ViconTseg = MarkerCalcs.GetSegmentPose("FreeHand", "HandR");
        Matrix4x4 MarkersTwrist = UnityTmarkers.inverse * Utils.TransformToMatrix(GameObject.Find("Wrist2").transform);

        Matrix4x4 T = ViconTseg.inverse * ViconTmarkers * MarkersTwrist;

        Utils.SaveToXML<Matrix4x4>("TCal_HandR2", T);

    }

    public void DoCalibrationFore()
    {
        Dictionary<string, Vector3> markers = MarkerCalcs.GetMarkersPositionU("FreeFore");

        Matrix4x4 ViconTmarkers = MarkerCalcs.CreateFrame(markers["FreeForeR1"], markers["FreeForeR2"], markers["FreeForeR6"], markers["FreeForeR5"]);
        Matrix4x4 UnityTmarkers = MarkerCalcs.CreateFrame(GameObject.Find("ForeMarkers/FreeForeR1").transform.position,
                                                          GameObject.Find("ForeMarkers/FreeForeR2").transform.position,
                                                          GameObject.Find("ForeMarkers/FreeForeR6").transform.position,
                                                          GameObject.Find("ForeMarkers/FreeForeR5").transform.position);

        Matrix4x4 ViconTseg = MarkerCalcs.GetSegmentPose("FreeFore", "FreeForeR");
        Matrix4x4 MarkersTwrist = UnityTmarkers.inverse * Utils.TransformToMatrix(GameObject.Find("Elbow").transform);

        Matrix4x4 T = ViconTseg.inverse * ViconTmarkers * MarkersTwrist;

        Utils.SaveToXML<Matrix4x4>("TCal_ForeR2", T);

    }

    public void DoCalibrationBraceH()
    {
        Dictionary<string, Vector3> markers = MarkerCalcs.GetMarkersPositionU("FreeBraceH");

        Matrix4x4 ViconTmarkers = MarkerCalcs.CreateFrame(markers["HumR1"], markers["HumR2"], markers["HumR4"], markers["HumR5"]);
        Matrix4x4 UnityTmarkers = MarkerCalcs.CreateFrame(GameObject.Find("BraceMarkersH/HumR1").transform.position,
                                                          GameObject.Find("BraceMarkersH/HumR2").transform.position,
                                                          GameObject.Find("BraceMarkersH/HumR4").transform.position,
                                                          GameObject.Find("BraceMarkersH/HumR5").transform.position);

        Matrix4x4 ViconTseg = MarkerCalcs.GetSegmentPose("FreeBraceH", "HumR");
        Matrix4x4 MarkersTwrist = UnityTmarkers.inverse * Utils.TransformToMatrix(GameObject.Find("Shoulder").transform);

        Matrix4x4 T = ViconTseg.inverse * ViconTmarkers * MarkersTwrist;

        Utils.SaveToXML<Matrix4x4>("TCal_BraceShoulderR", T);

    }

    public void DoCalibrationBraceF()
    {
        Dictionary<string, Vector3> markers = MarkerCalcs.GetMarkersPositionU("FreeBraceF");

        Matrix4x4 ViconTmarkers = MarkerCalcs.CreateFrame(markers["ForeR5"], markers["ForeR1"], markers["ForeR2"], markers["ForeR3"]);
        Matrix4x4 UnityTmarkers = MarkerCalcs.CreateFrame(GameObject.Find("BraceMarkersF/ForeR5").transform.position,
                                                          GameObject.Find("BraceMarkersF/ForeR1").transform.position,
                                                          GameObject.Find("BraceMarkersF/ForeR2").transform.position,
                                                          GameObject.Find("BraceMarkersF/ForeR3").transform.position);

        Matrix4x4 ViconTseg = MarkerCalcs.GetSegmentPose("FreeBraceF", "ForeR");
        Matrix4x4 MarkersTwrist = UnityTmarkers.inverse * Utils.TransformToMatrix(GameObject.Find("Elbow").transform);

        Matrix4x4 T = ViconTseg.inverse * ViconTmarkers * MarkersTwrist;

        Utils.SaveToXML<Matrix4x4>("TCal_BraceElbowR", T);

    }


    public void DoCalibrationSeg(string skeleton_name, List<string> segment_names, Dictionary<string,List<string> > marker_names)
    {
        Dictionary<string, Matrix4x4> segments = new Dictionary<string, Matrix4x4>();
        foreach (string seg_name in segment_names)
        {
            Matrix4x4 seg = MarkerCalcs.GetSegmentPose(skeleton_name, seg_name);
            segments.Add(seg_name, seg);
        }

        Dictionary<string, Matrix4x4> marker_frames = new Dictionary<string, Matrix4x4>();
        Dictionary<string, Vector3> markers = MarkerCalcs.GetMarkersPositionU(skeleton_name);
        
        foreach(KeyValuePair<string,List<string>> segment in marker_names)
        {
            marker_frames.Add(segment.Key,
                MarkerCalcs.CreateFrame(
                    markers[segment.Value[0]],
                    markers[segment.Value[1]],
                    markers[segment.Value[2]],
                    markers[segment.Value[3]]));
        }        
        Dictionary<string, Matrix4x4> sTm = new Dictionary<string, Matrix4x4>();
        foreach (KeyValuePair<string, Matrix4x4> seg in segments)
        {
            Matrix4x4 T = seg.Value.inverse * marker_frames[seg.Key];
            sTm.Add(seg.Key, T);
            Utils.SaveToXML<Matrix4x4>("TCal_" + seg.Key, T);
        }
    }

    public void DoCalibrationVR()
    {
        meas_vicon = new List<Matrix4x4>();
        meas_vive = new List<Matrix4x4>();
        meas_vive_in_cam = new List<Matrix4x4>();
        CamRig = GameObject.Find(Constants.camera_frame);
        Cam_To_Vicon = Matrix4x4.TRS(CamRig.transform.position, CamRig.transform.rotation, Vector3.one).inverse;
        Vicon_to_Vive = Matrix4x4.identity;
        calibratingVR = true;
        StartTimer();

        return;
    }
}
