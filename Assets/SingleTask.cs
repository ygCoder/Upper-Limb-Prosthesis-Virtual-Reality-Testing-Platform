using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.Animations;

public class SingleTask : MonoBehaviour
{
    Animator animator;
    AnimatorController ac;
    public Dropdown actionMenu;
    List<Dropdown.OptionData> taskList;
    TaskState cur_task;
    // Start is called before the first frame update
    void Start()
    {
        actionMenu = GameObject.Find("CanvasScreen/DropdownTask").GetComponent<Dropdown>();
        actionMenu.onValueChanged.AddListener(delegate {
            myDropdownValueChangedHandler(actionMenu);
        });
        taskList = new List<Dropdown.OptionData>();


        ac = GetComponent<Animator>().runtimeAnimatorController as AnimatorController;
        foreach (ChildAnimatorState state in ac.layers[0].stateMachine.states)
        {
            taskList.Add(new Dropdown.OptionData(state.state.name));            
        }
        actionMenu.ClearOptions();
        actionMenu.AddOptions(taskList);

        animator = GetComponent<Animator>();
    }
    
    void StartTask(TaskState task,int task_n)
    {
        /*
       // TaskState task=new TaskState();
        switch (task_n)
        {
            case 0:
                print("CASE reach");
                task.target = "ObjectMug";
                task.effector = "Wrist2";
                task.ang_tols = new Vector3(10, 0, 20);
                task.pos_tols = new Vector3(0.02f, 0.03f, 0.02f);
                task.desired_r = new Vector3(90, 0, 0);
                task.desired_p = new Vector3(-0.05f, -0.1f, 0);                
                break;
            case 1:
                print("CASE drink");
                task.target = "[CameraRig]/Camera";
                task.effector = "ObjectMug";
                task.ang_tols = new Vector3(10, 10, 0);
                task.pos_tols = new Vector3(0.02f, 0.02f, 0.02f);
                task.desired_r = new Vector3(120, 0, 0);
                task.desired_p = new Vector3(0.0f, 0.15f, 0.1f);
                break;            
        }

    */
        return;
    }
    // Update is called once per frame
    void Update()
    {
    }
    private void myDropdownValueChangedHandler(Dropdown target)
    {
        Debug.Log("selected: " + target.value);
        //ac.layers[0].stateMachine.states[0].state.behaviours[0] = StartTask(target.value);
        //StartTask(ref cur_task,target.value);
        //StartTask((TaskState) ac.layers[0].stateMachine.states[1].state.behaviours[0], target.value);
        //animator.StopPlayback();
        animator.SetInteger("cur_task", target.value);
        animator.Play("Start",0);

    }

    void Destroy()
    {
        actionMenu.onValueChanged.RemoveAllListeners();
    }

}
