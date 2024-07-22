using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForwardMovement : MonoBehaviour
{
    private void Update()
    {
        //transform.position += transform.forward;
        transform.Translate(transform.forward,Space.World);
    }
}
