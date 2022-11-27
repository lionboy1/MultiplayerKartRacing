using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerKart
{    
    public class Drive : MonoBehaviour
    {
        public AudioSource skidSound;
    public AudioSource acceleration;

        public WheelCollider[] wheelColliders;
        public GameObject[] wheels;
        public float torque = 200f;
        public float maxSteeringAngle = 30f;
        public float maxBrakeTorque = 500f;

        public Transform skidTrailPrefab;
        Transform[] skidTrails = new Transform[4];
        public ParticleSystem smokePrefab;
        ParticleSystem[] skidSmoke = new ParticleSystem[4];

        public GameObject brakeLight;

        public Rigidbody rb;
        public float gearLength = 3;
        float currentSpeed{get { return rb.velocity.magnitude * gearLength; } }
        public float lowPitch  =1f;
        public float highPitch = 6f;
        public int numGears = 5;
        float rpm;
        int currentGear = 1;
        float currentGearPercentage; //for normalizing gear ratio
        public float maxSpeed = 200f;
        public float gearDuration = 5f;
        public float rpmDrop = 0.25f;//Simulates realistic rpm drop when shifting higher.

        #region
    // public void StartSkidTrail(int i)
    // {
    //     if(skidTrails[i] == null)
    //     {
    //         skidTrails[i] = Instantiate(skidTrailPrefab);

    //         skidTrails[i].parent = wheelColliders[i].transform;
    //         skidTrails[i].localRotation = Quaternion.Euler(90, 0, 0);
    //         skidTrails[i].localPosition = -Vector3.up * wheelColliders[i].radius;
    //     }
    // }

    // public void EndSkidTrail(int i)
    // {
    //     if(skidTrails[i] == null)
    //     {
    //         return;
    //     }
    //     Transform holder = skidTrails[i];
    //     skidTrails[i] = null;
    //     holder.parent = null;
    //     holder.rotation = Quaternion.Euler(90, 0, 0);
    //     Destroy(holder.gameObject, 30f);
    // }
    #endregion

        // Start is called before the first frame update
        void Start()
        {
            for(int i = 0; i < 4; i++)
            {
                skidSmoke[i] = Instantiate(smokePrefab);
                skidSmoke[i].Stop();
            }
            brakeLight.SetActive(false);
        }

        public void CalculateEngineSound()
        {
            float gearPercentage = (1/(float) numGears);
            float targetGearFactor = Mathf.InverseLerp(gearPercentage * currentGear, gearPercentage * (currentGear +1),
            Mathf.Abs(currentSpeed / maxSpeed));

            //transition from current gear to next gear over a certain time
            currentGearPercentage = Mathf.Lerp(currentGearPercentage, targetGearFactor, Time.deltaTime * gearDuration);
            //Calculate RPM
            var gearNumFactor = currentGear  / (float) numGears;
            rpm = Mathf.Lerp(gearNumFactor, 1, currentGearPercentage);//Rpm between 0(low) and 1(high)

            //When to shift into next gear?
            float speedPercentage = Mathf.Abs(currentSpeed/maxSpeed);
            float upperGearMax = (1/(float) numGears) * (currentGear + 1);
            float downGearMax = (1 / (float) numGears) * currentGear;

            if(currentGear > 0 && speedPercentage < downGearMax)
            {
                currentGear--; //Gear down
            }
            if(speedPercentage > upperGearMax && currentGear < (numGears - 1))
            {
                currentGear++;
            }
            float pitch = Mathf.Lerp(lowPitch, highPitch, rpm);
            acceleration.pitch = Mathf.Min(highPitch, pitch) * rpmDrop;;//rom drop in between shifting gears
        }

        public void Go(float accelerate, float steer, float brake)
        {
            accelerate = Mathf.Clamp(accelerate, -1, 1);
            steer = Mathf.Clamp(steer, -1, 1) * maxSteeringAngle;

            //negative numbers will cause reverse and not a complete stop.
            brake = Mathf.Clamp(brake, 0, 1) * maxBrakeTorque;
            if(brake != 0)
            {
                brakeLight.SetActive(true);
            }
            else
            {
                brakeLight.SetActive(false);
            }

            float thrustTorque = 0f;
            if(currentSpeed < maxSpeed)
            {
                thrustTorque = accelerate * torque;
            } 
            for(int i = 0; i < 4; i++)
            {
                wheelColliders[i].motorTorque = thrustTorque;//motorTorque is a property of <WheelCollider>. 

                //Only steer with the front wheels
                //they will be the first 2 in the array
                if(i < 2)
                {
                    //Brake on the first 2 wheels in the array (i.e front wheels)
                    wheelColliders[i].steerAngle = steer;
                }
                else
                {
                    //Brake only on the rear brakes then
                    wheelColliders[i].brakeTorque = brake;
                }
                //Get the position and rotation of the wheel colliders
                //and match them to the wheel game object;
                Quaternion quat;
                Vector3 position;

                wheelColliders[i].GetWorldPose( out position, out quat );
                wheels[i].transform.position = position;
                wheels[i].transform.rotation = quat;
            }
        }

        public void CheckForSkid()
        {
            int numSkidding = 0;
            for(int i = 0; i < 4; i++)
            {
                //WheelHit is Unity class
                WheelHit wheelHit;
                wheelColliders[i].GetGroundHit( out wheelHit);

                if(Mathf.Abs(wheelHit.forwardSlip) >= 0.4f || Mathf.Abs(wheelHit.sidewaysSlip) > 0.4f)
                {
                    numSkidding++;
                    if(!skidSound.isPlaying)
                    {
                        skidSound.Play();
                    }
                    //Postion the emitter at the wheel collider locations
                    skidSmoke[i].transform.position = wheelColliders[i].transform.position - wheelColliders[i].transform.up * wheelColliders[i].radius;
                    skidSmoke[i].Emit(1);//Emit 1 burst of the particle system per wheel

                    // StartSkidTrail(i); Using improved shader so no nd for this function
                }
                else
                {
                    //EndSkidTrail(i); Using improved shader so no nd for this function
                }
            }
            if(numSkidding == 0 && skidSound.isPlaying)
            {
                skidSound.Stop();
            }
        }
        // Update is called once per frame
       
    }
}
