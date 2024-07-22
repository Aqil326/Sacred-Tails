using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableDrink : MonoBehaviour
{
    [SerializeField] GameObject DrinkObject;

    public void Drink()
    {
        DrinkObject.SetActive(true);
    }

    public void DrinkOff()
    {
        DrinkObject.SetActive(false);
    }
}
