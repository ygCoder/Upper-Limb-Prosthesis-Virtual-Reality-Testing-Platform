using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Toggle;

public class GUIMove : MonoBehaviour
{
    // Start is called before the first frame update

    public float s1_val,s2_val,s3_val;
    public bool vicon_toggle;
    Slider s1_sld, s2_sld, s3_sld;    
    Text textbox;

    public void ToggleVicon(Toggle tog_in)
    {
       vicon_toggle=tog_in.isOn;
    }
    public void SetTextBox(string text)
    {
        textbox.text = text;
    }
    void Start()
    {
        vicon_toggle = true;
        s1_sld = GameObject.Find("J1Slider").GetComponent<Slider>();
        s2_sld = GameObject.Find("J2Slider").GetComponent<Slider>();
        s3_sld = GameObject.Find("J3Slider").GetComponent<Slider>();
        textbox = GameObject.Find("StatusText").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        s1_val = s1_sld.value;
        s2_val = s2_sld.value;
        s3_val = s3_sld.value;
        
    }
    public float getSliderValue(int i)
    {
        switch(i){
            case 1:
                return (s1_val);
            case 2:
                return (s2_val);
            case 3:
                return (s3_val);
            default:
                return (0);
        }
    }

    private void OnGUI()
    {
     

    }
}
