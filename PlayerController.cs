using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerKart
{    
    public class PlayerController : MonoBehaviour
    {
        Drive driveScript;
        // Start is called before the first frame update
        void Start()
        {
            driveScript = this.GetComponent<Drive>();
        }

        // Update is called once per frame
        void Update()
        {
            float movement = Input.GetAxis("Vertical");
            float steering = Input.GetAxis("Horizontal");
            //Space bar is used for braking
            float braking = Input.GetAxis("Jump");
            driveScript.Go(movement, steering, braking);
            driveScript.CheckForSkid();
            driveScript.CalculateEngineSound();
        }
    }
}
