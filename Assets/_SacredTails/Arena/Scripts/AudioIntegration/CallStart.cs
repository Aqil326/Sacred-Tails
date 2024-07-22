using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Timba.SacredTails.AudioIntegration
{
    public class CallStart : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            FindObjectOfType<AudioSettings>(true).CustomStart();
        }
    }
}