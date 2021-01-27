using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class monitorCode : MonoBehaviour
{
    private readonly string[] slider_name = new string[] {"slider","slider (1)", "slider (2)" , "slider (3)" , "slider (4)", "slider (5)", "slider (6)", "slider (7)" };
    //private string[] m = new string[] { "monitor_seq" ,"monitor_sim3", "monitor_sim4", "monitor_sim7","monitor_tra"};

    private readonly string[] screen_modes = new string[] {"sim3","sim4","sim7", "seq", "tra","nat"};
    private readonly string[] modes_convert = new string[] {"nat","seq","sim","tra"};
    private Vector3 p;
    protected int cur_mode = 0;
    protected int cur_sub_mode = 0;
    protected int cur_seq_dof = 0;
    private int num_sliders;
    private GameObject[] screens;
    private GameObject screen;
    private Material myMaterial;
    private Text t; //sequential mode text object
    private VideoPlayer vp;
    private readonly float[] knob_scale = new float[] {0f, 42.4f, 27.3f, 41.7f};
    public string screen_name;
    private GameObject screen_image;
    protected TaskMain task_object;
    protected ControlSequential seq_control;
    protected ControlTrajectory tra_control;
    protected InputManager inputs;

    private GameObject[] knob;
    private Vector3[] knob_max;
    private Vector3[] knob_min;
    private GameObject[] cover;
    private Vector3[] cover_max;
    private Vector3[] cover_min;

    // Start is called before the first frame update
    void Start()
    {
        screens = new GameObject[screen_modes.Length];
        for (int i = 0; i < screen_modes.Length; i++)
        {
            screens[i] = GameObject.Find("monitor_main/screen_hinge/screen_" + screen_modes[i]);
        }
        inputs = GameObject.Find("Inputs").GetComponent<InputManager>();
        task_object = GameObject.Find("Task").GetComponent<TaskMain>();
    }

    public int numSliders(int mode, int sub_mode)
    {
        if (mode == 1 || mode == 3)
        {
            return 1;
        } else if (mode == 2)
        {
            return sub_mode+1;
        } else
        {
            return 0;
        }
    }

    public void changeScreen(int mode, int sub_mode, int seq_dof)
    {
        // [string] mode={nat=0,seq=1,sim=2,tra=3}
        // [int] sub_mode={3/4/7 for sim+seq, 0-11 for tra, 0 for nat}
        // [int] seq_dof=0-7 for seq (current mode)
        // float[] dof={array of dof values: 4/5/8 for sim, 1 for seq+tra, 0 for nat}

        for (int i=0; i < screen_modes.Length; i++)
        {
            screens[i].SetActive(false);
        }
        if (mode == 1)
        {
            //screen = GameObject.Find("monitor_main/screen_hinge/screen_seq");
            screen = screens[3];
            screen.SetActive(true);

            //screen = GameObject.Find("monitor_main/screen_hinge/screen_seq/pic_dof");
            myMaterial = Resources.Load("Images/dof_pics/seq_dof" + seq_dof.ToString(), typeof(Material)) as Material;
            //screen.GetComponent<Renderer>().material = myMaterial;
            screen_image = GameObject.Find("monitor_main/screen_hinge/screen_seq/pic_dof");
            screen_image.GetComponent<Renderer>().material = myMaterial;
            t = GameObject.Find("monitor_main/screen_hinge/screen_seq/pic_dof/Canvas/dof_text").GetComponent<Text>();
            t.text = sub_mode.ToString() + " DOF         " + seq_dof.ToString() + "/" + sub_mode.ToString();
        }
        else if (mode == 2)
        {
            //screen = GameObject.Find("monitor_main/screen_hinge/screen_sim" + sub_mode.ToString());
            if (sub_mode == 3)
            {
                screen = screens[0];
            } else if (sub_mode == 4)
            {
                screen = screens[1];
            } else
            {
                screen = screens[2];
            }
          screen.SetActive(true);
        }
        else if (mode == 3)
        {
            //screen = GameObject.Find("monitor_main/screen_hinge/screen_tra");
            screen = screens[4];
            screen.SetActive(true);

            vp = GameObject.Find("monitor_main/screen_hinge/screen_tra/pic_dof").GetComponent<VideoPlayer>();
            vp.clip = Resources.Load("Images/dof_pics/" + sub_mode.ToString() + "DOF_traj" + seq_dof.ToString(), typeof(VideoClip)) as VideoClip;
        }
        else
        {
            //screen = GameObject.Find("monitor_main/screen_hinge/screen_nat");
            screen = screens[5];
            screen.SetActive(true);
        }
    }

    public void changeKnob(int mode, int sub_mode, float[] dof)
    {
        // [string] mode={nat=0,seq=1,sim=2,tra=3}
        // [int] sub_mode={3/4/7 for sim, 0-7 for seq, 0-11 for tra, 0 for nat}
        // [int] seq_dof=0-7 for seq (current mode)
        // float[] dof={array of dof values: 8 for sim, 1 for tra/nat, 0 for nat}
        
        num_sliders = numSliders(mode,sub_mode);
        
        knob = new GameObject[num_sliders];
        knob_max = new Vector3[num_sliders];
        knob_min = new Vector3[num_sliders];
        cover = new GameObject[num_sliders];
        cover_max = new Vector3[num_sliders];
        cover_min = new Vector3[num_sliders];

        if (mode == 2)
        {
            screen_name = modes_convert[mode] + sub_mode.ToString();
        } else
        {
            screen_name = modes_convert[mode];
        }

        for (int a = 0; a < num_sliders; a++)
        {
            knob[a] = GameObject.Find("monitor_main/screen_hinge/screen_" + screen_name + "/" + slider_name[a] + "/knob");
            knob_max[a] = GameObject.Find("monitor_main/screen_hinge/screen_" + screen_name + "/" + slider_name[a] + "/knob_max").transform.localPosition;
            knob_min[a] = GameObject.Find("monitor_main/screen_hinge/screen_" + screen_name + "/" + slider_name[a] + "/knob_min").transform.localPosition;
            cover[a] = GameObject.Find("monitor_main/screen_hinge/screen_" + screen_name + "/" + slider_name[a] + "/cover");
            cover_max[a] = GameObject.Find("monitor_main/screen_hinge/screen_" + screen_name + "/" + slider_name[a] + "/knob_min_cover").transform.localPosition;
            cover_min[a] = GameObject.Find("monitor_main/screen_hinge/screen_" + screen_name + "/" + slider_name[a] + "/knob_max_cover").transform.localPosition;
        }
    }

    public void changeMonitor(int mode,int sub_mode, int seq_dof, float[] dof)
    {
        // [string] mode={nat=0,seq=1,sim=2,tra=3}
        // [int] sub_mode={3/4/7 for sim, 0-7 for seq, 0-11 for tra, 0 for nat}
        // [int] seq_dof=0-7 for seq (current mode)
        // float[] dof={array of dof values: 8 for sim, 1 for tra/nat, 0 for nat}
        
        //print("cur_mode: " + cur_mode.ToString());
        //print("cur_sub_mode: " + cur_sub_mode.ToString());
        if (mode != cur_mode || sub_mode != cur_sub_mode || seq_dof != cur_seq_dof)
        {
            changeScreen(mode, sub_mode, seq_dof);
            changeKnob(mode, sub_mode, dof);
            cur_sub_mode = sub_mode;
            cur_mode = mode;
            cur_seq_dof = seq_dof;
        }
        for (int a = 0; a < numSliders(mode, sub_mode); a++)
        {
            p = (dof[a+1]+1) / 2  * (knob_max[a] - knob_min[a]) + knob_min[a];
            knob[a].transform.localPosition = new Vector3(p.x, p.y, p.z);
            p = ((1 - (dof[a+1]+1) / 2) * (cover_max[a] - cover_min[a])) / 2 + cover_min[a];
            cover[a].transform.localPosition = new Vector3(p.x, p.y, p.z);
            cover[a].transform.localScale = new Vector3(cover[a].transform.localScale[0], cover[a].transform.localScale[1], (1 - (dof[a+1]+1) / 2) * knob_scale[mode]);
        }
    }


    // Update is called once per frame
    void Update()
    {
        int current_mode = task_object.getMode();
        int current_joint = 0;
        if (current_mode == 1)
        { 
            if(seq_control == null)
            {
                seq_control = GameObject.Find("Inputs").GetComponent<ControlSequential>();
            }
            current_joint = seq_control.GetCurrentJoint();
        }
        if(current_mode == 3)
        {
            if(tra_control == null)
            {
                tra_control = GameObject.Find("Inputs").GetComponent<ControlTrajectory>();
            }
            current_joint = tra_control.GetCurrentMode();
        }


        changeMonitor(current_mode, task_object.getDOF() - 1, current_joint, inputs.getInput());

    }
}