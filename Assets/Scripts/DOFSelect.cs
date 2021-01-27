using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DOFSelect : MonoBehaviour
{
    // Start is called before the first frame update
    Dropdown dropdown;
    GameObject inputs_object,control_object, wrist,task_object, elbow;
    TaskMain taskmain;

    public static List<string> DofModes = new List<string>()
    {
        "3 DOF", 
        "4 DOF",
        "7 DOF"        
    };   
       
    void Start()
    {
        inputs_object = GameObject.Find("Inputs");
        task_object = GameObject.Find("Task");
        taskmain = task_object.GetComponent<TaskMain>();

        control_object = GameObject.Find("Controller");
        wrist = GameObject.Find("Wrist2");
        elbow = GameObject.Find("Elbow");
        dropdown = GetComponent<Dropdown>();
        dropdown.AddOptions(DofModes);
        dropdown.onValueChanged.AddListener(delegate {
            myDropdownValueChangedHandler(dropdown);
        });
        dropdown.value = 2;
        myDropdownValueChangedHandler(dropdown);
    }
    private void myDropdownValueChangedHandler(Dropdown target)
    {
        Debug.Log("DOF selected: " + target.value);
        task_object.GetComponent<TaskMain>().current_DOF = DofModes[target.value];
       // wrist.transform.localPosition = Vector3.zero;
       // wrist.transform.localRotation = Quaternion.identity;
       // elbow.transform.localPosition = Vector3.zero;
       // elbow.transform.localRotation = Quaternion.identity;
       /* switch (DofModes[target.value]){
            case "3 DOF":
                
                taskmain. = 3;                
                break;

            case "4 DOF":
                control_object.GetComponent<MoveArm>().DOF_controlled = 4;
                break;

            case "7 DOF":
                control_object.GetComponent<MoveArm>().DOF_controlled = 7;
                break;
        }*/
        //GameObject.Find("Task").GetComponent<TaskMain>().SetupState(controlModes[target.value]);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
