using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.UI;

using Transf = System.Tuple<UnityEngine.Quaternion, UnityEngine.Vector3>;
using System.Linq;

public class TaskState : StateMachineBehaviour
{

    [SerializeField] public string target, effector;
    GameObject target_object, effector_object,task_parent, wrist_object,hand_angle_obj;
    List<GameObject> visualization_objects;
    Matrix4x4 desired_T;
    [SerializeField] public Vector3 ang_tols, pos_tols, ang_thres, pos_thres,desired_r,desired_p, grasped_p, grasped_r;
    Material target_material;
    public bool display_cones = false;
    public string state_name;
    public int des_grasping_state = -1;
    string current_mode;
    TaskMain task_main;

    [SerializeField] public bool object_grasped=false;
    float started;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        task_main = GameObject.FindObjectOfType<TaskMain>();
        current_mode=task_main.current_mode;
        task_main.SetupState(state_name, current_mode);
        UnityEngine.MonoBehaviour.print("Entered " + this.GetType().ToString());

        animator.SetBool("reached", false);
        task_main.current_state = this;
        target_material = Resources.Load<Material>("TargetMaterial");
        Initialize(effector, target, desired_p, desired_r, pos_tols, ang_tols, pos_thres, ang_thres);
        started = Time.time;        
    }

    GameObject CreateObject(string obj,string name)
    {
        GameObject ret_obj;
        GameObject A = GameObject.Find(obj);        
        ret_obj = Instantiate<GameObject>(A);
        ret_obj.name = name;
        ret_obj.transform.SetParent(task_parent.transform);
        return (ret_obj);
    }

    void Initialize(string effector, string target, Vector3 des_p, Vector3 des_ang, Vector3 tol_p, Vector3 tol_r, Vector3 thres_p, Vector3 thres_r)
    {
        visualization_objects = new List<GameObject>();
        desired_T = Matrix4x4.TRS(des_p, Quaternion.Euler(des_ang), Vector3.one);
        task_parent = GameObject.Find("Task");

        UnityEngine.MonoBehaviour.print("Target:" + target);

        target_object = GameObject.Find(target);
        effector_object = GameObject.Find(effector);
        task_parent.transform.position = GameObject.Find(target).transform.position;
        task_parent.transform.rotation = GameObject.Find(target).transform.rotation;


        hand_angle_obj = GameObject.Find("hand_angle");

        wrist_object = GameObject.Find("Wrist2");
        if (object_grasped && grasped_p.magnitude == 0 && grasped_r.magnitude == 0)
        {
            Matrix4x4 MM= Utils.TransformToMatrix(wrist_object.transform).inverse * Utils.TransformToMatrix(effector_object.transform);
            grasped_p = MM.GetPosition();
            grasped_r = MM.GetRotation().eulerAngles;

        }
        visualization_objects.Add(CreateObject(effector, "Effector"));
        visualization_objects.AddRange(CreateCones(Utils.TransformToMatrix(target_object.transform), ang_tols, pos_tols, task_parent));

    }

    void SetWristObject(string wrist_name)
    {
        wrist_object = GameObject.Find(wrist_name);
    }


    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       // if (Time.time - started < 1) return;


        if (object_grasped)
        {
            //Debug.Log("GR P:" + grasped_p.ToString() + " R:" + grasped_r.ToString());
            Matrix4x4 obj_T = Utils.TransformToMatrix(wrist_object.transform) * Matrix4x4.TRS(grasped_p, Quaternion.Euler(grasped_r), Vector3.one);
            Utils.MatrixToTransform(obj_T, effector_object.transform);

        }
        try
        {
            DrawTargets(visualization_objects);
            Dropdown mode = GameObject.Find("DropdownMode").GetComponent<Dropdown>();
            //GameObject.Find("[CameraRig]/Camera/Canvas/StatusText").GetComponent<Text>().text = "E: " + Utils.WrapAngle(getErrors().Item1.eulerAngles) + " P: " + getErrors().Item2 * 1000;
            GameObject.Find("[CameraRig]/Camera/Canvas/StatusText").GetComponent<Text>().text = state_name + " "   + mode.options[mode.value].text;
            
        } catch (Exception e)
        {
            Debug.LogWarning("Object not found: " + e.Message);
        }

        animator.SetBool("reached", CheckComplete() && !animator.GetBool("paused"));
        animator.SetBool("failed", CheckFail());
    }

    public bool CheckGrasp()
    {
        if (current_mode == "Natural") return true;
        if (des_grasping_state == 0)
        {
            return (hand_angle_obj.transform.localRotation.eulerAngles.x < 10);
        }
        else if (des_grasping_state > 0)
        {
            return (hand_angle_obj.transform.localRotation.eulerAngles.x > des_grasping_state);            
        }
        else return true;
    }

    public bool CheckComplete()
    {
       // Debug.Log(CheckAngles(ang_tols)+ " " + CheckPosition(pos_tols) + " " + CheckGrasp());
        return CheckAngles(ang_tols) && CheckPosition(pos_tols) && CheckGrasp();

    }
    public bool CheckFail()
    {
        return !(CheckAngles(ang_thres) && CheckPosition(pos_thres)) || (object_grasped && !CheckGrasp());
    }


    void DrawTargets(List<GameObject> objects)
    {
        foreach (GameObject g in objects)
        {
            Matrix4x4 localrot = Matrix4x4.identity;
            if (g.name.Substring(0, 4) == "Effe")
            {
                DrawTarget(g, Utils.TransformToMatrix(target_object.transform) * desired_T.inverse);
            }
            else if (!display_cones)
            {
                //Don't draw the cones
                MeshRenderer[] rends = g.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer r in rends)
                {
                    r.enabled = false;
                    r.material.color = new Color(0, 0, 0, 0);
                }

            }
            else if (g.name.Substring(0, 4) == "Cone")
            {
                switch (g.name[4])
                {
                    case 'X':
                        localrot = Matrix4x4.Rotate(Quaternion.AngleAxis(90, Vector3.back));
                        break;
                    case 'Y':
                        localrot = Matrix4x4.Rotate(Quaternion.AngleAxis(-90, Vector3.up));
                        break;
                    case 'Z':
                        localrot = Matrix4x4.Rotate(Quaternion.AngleAxis(-90, Vector3.right));
                        break;
                }


                DrawTarget(g, Utils.TransformToMatrix(target_object.transform) * localrot);
            }
            else if (g.name.Substring(0, 4) == "Axis")
            {
                switch (g.name[4])
                {
                    case 'X':
                        localrot = Matrix4x4.Rotate(Quaternion.AngleAxis(-90, Vector3.back));
                        break;
                    case 'Y':
                        localrot = Matrix4x4.Rotate(Quaternion.AngleAxis(-90, Vector3.up));
                        break;
                    case 'Z':
                        localrot = Matrix4x4.Rotate(Quaternion.AngleAxis(-90, Vector3.right));
                        break;
                }
                DrawTarget(g, Utils.TransformToMatrix(effector_object.transform)*desired_T*localrot);
            }
            else if (g.name.Substring(0, 4) == "Sphe")
            {
                DrawTarget(g, Utils.TransformToMatrix(target_object.transform));
            }
        }
    }
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        task_main.current_state = null;
        foreach (GameObject g in visualization_objects)
        {
            GameObject.Destroy(g);
        }
        
        UnityEngine.MonoBehaviour.print("Exiting " + this.GetType().ToString());
    }


    protected GameObject CreateAxis(Matrix4x4 T, Vector3 dir, string name, GameObject parent)
    {
        GameObject cyl = Instantiate(Resources.Load("arrow")) as GameObject;
        cyl.transform.SetParent(parent.transform, false);
        cyl.name = name;
       // cyl.transform.localPosition = new Vector3(T.m03, T.m13, T.m23);
       // cyl.transform.localRotation = (T.rotation * Quaternion.AngleAxis(-90, dir));
        cyl.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

        return (cyl);
    }

    public Transf getTransform()
    {        
        Matrix4x4 Tp = Matrix4x4.TRS(target_object.transform.position, target_object.transform.rotation, new Vector3(1, 1, 1));
        Matrix4x4 Tc = Matrix4x4.TRS(effector_object.transform.position, effector_object.transform.rotation, new Vector3(1, 1, 1));

        Matrix4x4 pTc = (Tc.inverse * Tp);
        
        return new Transf(pTc.rotation, new Vector3(pTc.GetColumn(3).x, pTc.GetColumn(3).y, pTc.GetColumn(3).z));
    }

    public Tuple<Quaternion, Vector3> getErrors()
    {
        Transf curr_T = getTransform();
        //return (new Tuple<Quaternion, Vector3>(curr_T.Item1 * Quaternion.Inverse(desired_T.rotation), curr_T.Item2 - new Vector3(desired_T.GetColumn(3).x, desired_T.GetColumn(3).y, desired_T.GetColumn(3).z)));
        return (new Tuple<Quaternion, Vector3>(Quaternion.Inverse(curr_T.Item1) * desired_T.rotation, curr_T.Item2 - new Vector3(desired_T.GetColumn(3).x, desired_T.GetColumn(3).y, desired_T.GetColumn(3).z)));
    }


    public void DrawTarget(GameObject target_object, Matrix4x4 T)
    {
        
        target_object.transform.rotation = T.GetRotation();
        target_object.transform.position = T.GetPosition();

        Renderer[] rends = target_object.GetComponentsInChildren<Renderer>();        

        foreach (Renderer r in rends)
        {
            Color m_e=ErrorToColor();
            foreach (Material m in r.materials)
            {
                m.color = m_e;
            }
            //r.material.color = ErrorToColor();
        }
        if (target_object.name == "Effector" && target_object.GetComponentInChildren<handClose>() != null)
        {
            //target_object.GetComponentInChildren<handClose>().gameObject.transform.localRotation= Quaternion.Euler(des_grasping_state, 0, 0) ;
            //target_object.GetComponentInChildren<handClose>().transform.localRotation = Quaternion.Euler(des_grasping_state/2, 0, 0);
            target_object.GetComponentInChildren<handClose>().SetHandClosure(des_grasping_state);
        }
    }

    public Color ErrorToColor()
    {
        Color color;
        Transf Terrors = getErrors();
        float ang_err;
        Terrors.Item1.ToAngleAxis(out ang_err, out Vector3 _);        
        if (CheckAngles(ang_tols) && CheckPosition(pos_tols)) color = new Color(0, 1, 0, Mathf.Exp(-Mathf.Abs(ang_err / ang_tols.magnitude + Terrors.Item2.magnitude)));
        else color = new Color(1, 0, 0, Mathf.Abs(ang_err) / 360 + Terrors.Item2.magnitude);
        return color;
    }

    public Transf getTarget()
    {
        Matrix4x4 Tp = Matrix4x4.TRS(target_object.transform.position, target_object.transform.rotation, new Vector3(1, 1, 1));
        Matrix4x4 wTd = Tp * desired_T.inverse;
        return new Transf(wTd.rotation, new Vector3(wTd.GetColumn(3).x, wTd.GetColumn(3).y, wTd.GetColumn(3).z));
    }
    protected bool CheckAngles(Vector3 tol)
    {
        bool ret = true;
        Transf a = getErrors();
        Vector3 ang_err = Utils.WrapAngle(a.Item1.eulerAngles);
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

    protected List<GameObject> CreateCones(Matrix4x4 desiredT, Vector3 tol_r_ang, Vector3 tol_p, GameObject child_obj)
    {
        Vector3[] axes = { Vector3.up, Vector3.forward, Vector3.right };
        Matrix4x4 T = desiredT;
        List<GameObject> cones = new List<GameObject>();
        float cone_height = 0.1f;

        GameObject cyl = Instantiate(Resources.Load("sphere")) as GameObject;
        cyl.transform.SetParent(task_parent.transform, false);
        cyl.name = "Sphere";
     //   cyl.transform.localRotation = T.rotation;
     //   cyl.transform.localPosition = new Vector3(T.m03, T.m13, T.m23);
        cyl.transform.localScale = tol_p;
        cyl.GetComponentInChildren<MeshRenderer>().material = Resources.Load<Material>("TargetMaterial");
        cones.Add(cyl);
        Vector3 tol_r = new Vector3(Mathf.Tan(Mathf.Deg2Rad * tol_r_ang.x) * cone_height, Mathf.Tan(Mathf.Deg2Rad * tol_r_ang.y) * cone_height, Mathf.Tan(Mathf.Deg2Rad * tol_r_ang.z) * cone_height);

        if (tol_r.x == 0)
        {
            if (tol_r.y == 0)
            {
                if (tol_r.z == 0)
                {
                    Debug.LogWarning("(Cones) Don't know what to do (xyz)");
                }
                else
                {
                    Debug.LogWarning("(Cones) Don't know what to do (xy)");
                }
            }
            else
            {
                if (tol_r.z == 0)
                {
                    Debug.LogWarning("(Cones) Don't know what to do (xz)");

                }
                else
                {
                    //cone around x
                    cones.Add(CreateCone(T, Vector3.forward, new Vector3(cone_height, tol_r.z, tol_r.y), "ConeX", target_object));
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
                    Debug.LogWarning("(Cones) Don't know what to do (yz)");
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
                    //THIS IS WRONG
                    cones.Add(CreateCone(T, Vector3.forward, new Vector3(cone_height, tol_r.y, tol_r.z), "ConeX2", target_object));
                    cones.Add(CreateCone(T, Vector3.up, new Vector3(tol_r.x, cone_height, tol_r.z), "ConeY2", target_object));

                    Matrix4x4 T2 = Matrix4x4.identity;
                    T2.SetColumn(3, T.GetColumn(3));
                    cones.Add(CreateAxis(desiredT, Vector3.back, "AxisX2", child_obj));
                    cones.Add(CreateAxis(desiredT, Vector3.up, "AxisY2", child_obj));


                }
            }
        }
        return (cones);
    }
    protected GameObject CreateCone(Matrix4x4 T, Vector3 dir, Vector3 dims, string name, GameObject parent)
    {
        GameObject cone = Instantiate(Resources.Load("cone")) as GameObject;
        cone.transform.SetParent(task_parent.transform, false);
        cone.name = name;// "Cone_" + (i + 1);
        cone.transform.localRotation = (T.rotation * Quaternion.AngleAxis(-90, dir));
        cone.transform.localPosition = new Vector3(T.m03, T.m13, T.m23);
        cone.transform.localScale = Quaternion.AngleAxis(-90, dir) * dims;
        cone.GetComponentInChildren<MeshRenderer>().material = Resources.Load<Material>("TargetMaterial");


        return (cone);
    }
}
