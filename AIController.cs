using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Attach script to the car's parent.

namespace MultiplayerKart
{
    public class AIController : MonoBehaviour
    {
        public Circuit circuit;
        Drive drive;
        public float steeringSensitivity = 0.01f;
        Vector3 target;
        int currentWP = 0;
        void Start()
        {
            drive = this.GetComponent<Drive>();
            target = circuit.waypoints[currentWP].transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 localTarget = drive.rb.gameObject.transform.InverseTransformPoint(target);
            float distanceToTarget = Vector3.Distance(target, drive.rb.gameObject.transform.position);

            float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
            
            float steer = Mathf.Clamp(targetAngle * steeringSensitivity, -1, 1) * Mathf.Sin(drive.currentSpeed);
            float acceleration = 0.5f;
            float brake = 0;

            //To slow down when approaching waypoints
            if(distanceToTarget < 5)//Adjust as needed
            {
                //Adjust as needed
                brake = 0.8f;
                acceleration = 0.1f;
            }

            drive.Go(acceleration, steer, brake);

            if(distanceToTarget < 2)//Threshold, . Make larger is car is circling waypoint
            {
                currentWP++;
                if(currentWP >= circuit.waypoints.Length)
                {
                    currentWP = 0;
                    target = circuit.waypoints[currentWP].transform.position;
                }
                drive.CheckForSkid();
                drive.CalculateEngineSound();
            }
        }
    }
}
