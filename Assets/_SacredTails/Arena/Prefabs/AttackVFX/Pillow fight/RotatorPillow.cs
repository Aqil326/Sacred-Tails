using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatorPillow : MonoBehaviour
{
    int randomNumber;
    [SerializeField] float velocity = 90;
    // Start is called before the first frame update
    void Start()
    {
        randomNumber = Random.Range(-1, 2);
        randomNumber = randomNumber == 0 ? 1 : randomNumber;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0,randomNumber,0), velocity*Time.deltaTime);
    }
}
