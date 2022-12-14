using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerKart
{    
    public class Circuit : MonoBehaviour
    {
        public GameObject[] waypoints;

        void OnDrawGizmos() 
        {
            DrawGizmos(false);
        }
        void OnDrawGizmosSelected()
        {
            DrawGizmos(true);
        }

        void DrawGizmos(bool selected)        
        {
            if(selected == false)
            {
                return;
            }
            if(waypoints.Length > 1)
            {
                Vector3 previous = waypoints[0].transform.position;//Set this to the first point
                //Start at wp 1
                for(int i = 1; i < waypoints.Length; i++)
                {
                    Vector3 next = waypoints[i].transform.position;
                    Gizmos.DrawLine(previous, next);
                    previous = next;
                }
                Gizmos.DrawLine(previous, waypoints[0].transform.position);
            }
        }
    }
}
