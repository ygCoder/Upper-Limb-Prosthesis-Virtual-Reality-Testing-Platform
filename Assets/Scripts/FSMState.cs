using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;


using Transf = System.Tuple<UnityEngine.Quaternion, UnityEngine.Vector3>;

public class FSMState : StateMachineBehaviour
{
    protected GameObject parent_obj;
    protected GameObject child_obj;
    public string parent_frame;
    public string child_frame;
    public Matrix4x4 desired_transform;
    //public float ang_tol, pos_tol; //Deprecated
    public Vector3 ang_tols, pos_tols, ang_thres, pos_thres; 
    public GameObject target_object;
    protected List<GameObject> target_cones = new List<GameObject>(3);
    protected List<GameObject> target_axes = new List<GameObject>(3);
    protected GameObject target_sphere;
    private GUIMove gui_script;
    public float wait;
    protected float time_s;




    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        UnityEngine.MonoBehaviour.print("Entered " + this.GetType().ToString());
        wait = 5.0f;
        //GameObject.FindObjectOfType<TaskMain>().current_state = this;
       // Initialize(parent_frame, child_frame, desired_transform);
        gui_script = GameObject.FindObjectOfType<GUIMove>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //System.Tuple<float, float> errs = getError();
        Transf a = getErrors();
        Color c = ErrorToColor();
        
        gui_script.SetTextBox("E: " + Utils.WrapAngle(a.Item1.eulerAngles) + " P: " + a.Item2*1000 + "a: " + c.a);

    }
    public void Initialize(string parent, string child, Matrix4x4 desiredT)
    {
        parent_frame = parent;
        child_frame = child;
        desired_transform = desiredT;
        parent_obj = GameObject.Find(parent_frame);
        child_obj = GameObject.Find(child_frame);
        //ang_tol = 20;
        //pos_tol = 0.06f;
        target_object = Instantiate<GameObject>(GameObject.Find(child_frame));
        target_object.name = "Target";
        MeshRenderer[] rends = target_object.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer r in rends)
        {
            r.material = Resources.Load<Material>("TargetMaterial");
        }
    }

    public void Initialize(string parent, string child, Vector3 des_p, Vector3 des_ang, Vector3 tol_p, Vector3 tol_r, Vector3 thres_p, Vector3 thres_r)
    {
        Matrix4x4 desiredT = Matrix4x4.TRS(des_p, Quaternion.Euler(des_ang), Vector3.one);
        Initialize(parent, child, desiredT);
        ang_tols = tol_r;
        pos_tols = tol_p;
        pos_thres = thres_p;
        ang_thres = thres_r;
        target_cones = CreateCones(desiredT, tol_r, tol_p);
    }
    protected GameObject CreateAxis(Matrix4x4 T, Vector3 dir, string name, GameObject parent)
    {
        GameObject cyl = Instantiate(Resources.Load("arrow")) as GameObject;
        cyl.transform.SetParent(parent.transform, false);
        cyl.name = name;
        cyl.transform.localPosition = new Vector3(T.m03, T.m13, T.m23);
        cyl.transform.localRotation = (T.rotation * Quaternion.AngleAxis(-90, dir));
        cyl.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        


        return (cyl);
    }

    protected GameObject CreateCone(Matrix4x4 T,Vector3 dir, Vector3 dims, string name, GameObject parent)
    {
        GameObject cone = Instantiate(Resources.Load("cone")) as GameObject;
        cone.transform.SetParent(parent.transform, false);
        cone.name = name;// "Cone_" + (i + 1);
        cone.transform.localRotation = (T.rotation * Quaternion.AngleAxis(-90, dir));
        cone.transform.localPosition = new Vector3(T.m03, T.m13, T.m23);
        cone.transform.localScale = Quaternion.AngleAxis(-90, dir)*dims;
        cone.GetComponentInChildren<MeshRenderer>().material = Resources.Load<Material>("TargetMaterial");


        return (cone);
    }
    protected List<GameObject> CreateCones(Matrix4x4 desiredT, Vector3 tol_r_ang, Vector3 tol_p)
    {
        Vector3[] axes = { Vector3.up, Vector3.forward, Vector3.right };
        Matrix4x4 T = desiredT;
        List<GameObject> cones = new List<GameObject>();
        float cone_height = 0.1f;

        GameObject cyl = Instantiate(Resources.Load("sphere")) as GameObject;
        cyl.transform.SetParent(target_object.transform, false);
        cyl.name = "Sphere";
        cyl.transform.localRotation = T.rotation;
        cyl.transform.localPosition = new Vector3(T.m03, T.m13, T.m23);
        cyl.transform.localScale = tol_p;
        cyl.GetComponentInChildren<MeshRenderer>().material = Resources.Load<Material>("TargetMaterial");

        cones.Add(cyl);

        Vector3 tol_r = new Vector3(Mathf.Tan(Mathf.Deg2Rad*tol_r_ang.x) *cone_height, Mathf.Tan(Mathf.Deg2Rad * tol_r_ang.y) * cone_height, Mathf.Tan(Mathf.Deg2Rad * tol_r_ang.z) * cone_height);



        if (tol_r.x == 0)
        {
            if (tol_r.y == 0)
            {
                if (tol_r.z == 0)
                {
                    Debug.LogError("(Cones) Don't know what to do (xyz)");
                }
                else
                {
                    Debug.LogError("(Cones) Don't know what to do (xy)");
                }               
            }
            else
            {
                if (tol_r.z == 0)
                {
                    Debug.LogError("(Cones) Don't know what to do (xz)");

                }
                else
                {
                    //cone around x
                    cones.Add(CreateCone(T,Vector3.forward,new Vector3(cone_height,tol_r.z,tol_r.y),"ConeX", target_object));
                    Matrix4x4 T2 = Matrix4x4.identity;
                    T2.SetColumn(3, T.GetColumn(3));
                    cones.Add(CreateAxis(desiredT, Vector3.back, "AxisX", child_obj));

                }
            }
        }
        else
        {
            if (tol_r.y == 0)
            {
                if (tol_r.z == 0)
                {
                    Debug.LogError("(Cones) Don't know what to do (yz)");
                }
                else
                {
                    //cone around y
                    cones.Add(CreateCone(T, Vector3.up, new Vector3(tol_r.z, cone_height, tol_r.x), "ConeY", target_object));
                    Matrix4x4 T2 = Matrix4x4.identity;
                    T2.SetColumn(3, T.GetColumn(3));
                    cones.Add(CreateAxis(desiredT, Vector3.up, "AxisY", child_obj));
                }
            }
            else
            {
                if (tol_r.z == 0)
                {
                    //cone around z
                    cones.Add(CreateCone(T, Vector3.right, new Vector3(tol_r.y, tol_r.x, cone_height), "ConeZ", target_object));
                    Matrix4x4 T2 = Matrix4x4.identity;
                    T2.SetColumn(3, T.GetColumn(3));                    
                    cones.Add(CreateAxis(desiredT, Vector3.right, "AxisZ", child_obj));
                }
                else
                {
                    //two cones
                    cones.Add(CreateCone(T, Vector3.forward, new Vector3(cone_height, tol_r.y, tol_r.z), "ConeX2", target_object));
                    cones.Add(CreateCone(T, Vector3.up, new Vector3(tol_r.x, cone_height, tol_r.z), "ConeY2", target_object));
                    
                    Matrix4x4 T2 = Matrix4x4.identity;
                    T2.SetColumn(3, T.GetColumn(3));
                    cones.Add(CreateAxis(desiredT, Vector3.back, "AxisX2", child_obj));
                    cones.Add(CreateAxis(desiredT, Vector3.up, "AxisY2", child_obj));


                }
            }
        }

        //  target_cones.Add(CreateCone());


        return (cones);
    }

    protected bool CheckAngles(Vector3 tol)
    {
        bool ret = true;
        Transf a = getErrors();
        Vector3 ang_err=Utils.WrapAngle(a.Item1.eulerAngles);
        if (Mathf.Abs(ang_err.x) > tol.x && tol.x != 0) ret = false;
        if (Mathf.Abs(ang_err.y) > tol.y && tol.y != 0) ret = false;
        if (Mathf.Abs(ang_err.z) > tol.z && tol.z != 0) ret = false;
        return (ret);
    }

    protected bool CheckPosition(Vector3 tol)
    {
        bool ret = true;
        Transf a = getErrors();
        Vector3 pos_err = a.Item2;
        if (Mathf.Abs(pos_err.x) > tol.x && tol.x != 0) ret = false;
        if (Mathf.Abs(pos_err.y) > tol.y && tol.y != 0) ret = false;
        if (Mathf.Abs(pos_err.z) > tol.z && tol.z != 0) ret = false;
        return (ret);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject.FindObjectOfType<TaskMain>().current_state = null;
        GameObject.Destroy(target_object);
        UnityEngine.MonoBehaviour.print("Exiting " + this.GetType().ToString());
    }

    public Transf getTarget()
    {
        Matrix4x4 Tp = Matrix4x4.TRS(parent_obj.transform.position, parent_obj.transform.rotation, new Vector3(1, 1, 1));
        Matrix4x4 wTd = Tp * desired_transform.inverse;
        return new Transf(wTd.rotation, new Vector3(wTd.GetColumn(3).x, wTd.GetColumn(3).y, wTd.GetColumn(3).z));
    }
    public bool ReachedT()
    {
        bool ret = false;
        //Tuple<float, float> errors = getError();
        if (CheckAngles(ang_tols) && CheckPosition(pos_tols)) ret = true;
        //ret = ret && grasp;

        return (ret);
    }
    public Transf getTransform()
    {

        Matrix4x4 Tp = Matrix4x4.TRS(parent_obj.transform.position, parent_obj.transform.rotation, new Vector3(1, 1, 1));
        Matrix4x4 Tc = Matrix4x4.TRS(child_obj.transform.position, child_obj.transform.rotation, new Vector3(1, 1, 1));

        Matrix4x4 pTc = (Tc.inverse * Tp);

        return new Transf(pTc.rotation, new Vector3(pTc.GetColumn(3).x, pTc.GetColumn(3).y, pTc.GetColumn(3).z));
    }

    public Tuple<Quaternion, Vector3> getErrors()
    {
        Transf curr_T = getTransform();
        return (new Tuple<Quaternion, Vector3>(curr_T.Item1 * Quaternion.Inverse(desired_transform.rotation), curr_T.Item2- new Vector3(desired_transform.GetColumn(3).x, desired_transform.GetColumn(3).y, desired_transform.GetColumn(3).z)));
    }
    public Tuple<float, float> getError()
    {
        Transf curr_T = getTransform();
        float ang_err = Math.Abs(Quaternion.Angle(curr_T.Item1, desired_transform.rotation));
        float pos_err = Vector3.Distance(curr_T.Item2, new Vector3(desired_transform.GetColumn(3).x, desired_transform.GetColumn(3).y, desired_transform.GetColumn(3).z));
        return (new Tuple<float, float>(ang_err, pos_err));
    }
    public void DrawTarget(GameObject target_object,Material target_material)
    {
        Transf target_pose = getTarget();
        target_object.transform.rotation = target_pose.Item1;
        target_object.transform.position = target_pose.Item2;
        
        MeshRenderer[] rends = target_object.GetComponentsInChildren<MeshRenderer>();
        //Tuple<float, float> errors = getError();
        foreach (MeshRenderer r in rends)
        {
            //r.material.color = new Color(1 - pos_tol / errors.Item2, pos_tol / errors.Item2, 0, 1 - errors.Item1 / 90);
            //float ang_err = Math.Abs(Utils.WrapAngle(errors.Item1)) / 180;

            
             //r.material = target_material; // This causes the memory leak
             r.material.color = ErrorToColor();
        }
    }

    public Color ErrorToColor()
    {
        Color color;
        Transf Terrors = getErrors();
        Vector3 eul_error = Terrors.Item1.eulerAngles;

        float ang_err;
        Vector3 discard = new Vector3();
        Terrors.Item1.ToAngleAxis(out ang_err, out discard);
        //Debug.Log("A: " + CheckAngles(ang_tols) + "[" + ang_tols + "]" + "P: " + CheckPosition(pos_tols));
        if (CheckAngles(ang_tols) && CheckPosition(pos_tols))
        {
            color = new Color(0, 1, 0, Mathf.Exp(-Mathf.Abs(ang_err/ang_tols.magnitude + Terrors.Item2.magnitude)));
        }
        else
        {
            color = new Color(1, 0, 0, Mathf.Abs(ang_err) / 360 + Terrors.Item2.magnitude);
            
        }
        
        

        //float ang_err = Math.Abs(Utils.WrapAngle(errors.Item1)) / 180;
        //float pos_err = Math.Abs(errors.Item2);


        return color;
    }

}
