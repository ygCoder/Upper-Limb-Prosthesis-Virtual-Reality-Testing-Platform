using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class InputController : MonoBehaviour
{
    // Start is called before the first frame update
    public SteamVR_ActionSet actionSetEnable;
    public SteamVR_Action_Boolean Extension, Flexion, DPadClick;
    //SteamVR_Action_Boolean Radial, Ulnar, Pronate, Supinate;
    public SteamVR_Action_Single Hand;
    public SteamVR_Action_Vector2 DPadPos;

    float[] command = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    void Start()
    {
        actionSetEnable.Activate();

    }

    // Update is called once per frame
    void Update()
    {
     
    }
    public float[] getInput()
    {
           if (DPadClick.GetState(SteamVR_Input_Sources.Any))
        {
            print(DPadPos.GetAxis(SteamVR_Input_Sources.Any));
            Vector2 vp = DPadPos.GetAxis(SteamVR_Input_Sources.Any);
            command[0] = (vp.y < -0.5) ? 1 : 0;
            command[1] = (vp.y > 0.5) ? 1 : 0;
            command[2] = (vp.x < -0.5) ? 1 : 0;
            command[3] = (vp.x > 0.5) ? 1 : 0;

        }
        command[4] = Extension.GetState(SteamVR_Input_Sources.Any) ? 0 : 1;
        command[5] = Flexion.GetState(SteamVR_Input_Sources.Any) ? 0 : 1;

        command[6] = Hand.GetAxis(SteamVR_Input_Sources.Any) > 0.9 ? 1 : 0;
        command[7] = Hand.GetAxis(SteamVR_Input_Sources.Any) < 0.1 ? 1 : 0;
        return (command);
    }
}
