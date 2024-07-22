using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]

public class FamilyActivator : MonoBehaviour
{
    public GameObject[] AElements;
    public GameObject[] BElements;
    public GameObject[] CElements;
    public GameObject[] DElements;
    public GameObject[] EElements;
    public int groupToActive;
    public bool active;
    bool sec;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Activar(){
        StartCoroutine(secFam());
    }

    // Update is called once per frame
    void Update()
    {
        /*if(Input.GetKeyDown(KeyCode.Alpha1)){
            if(active == false){
                Activar();
                active = true;
            }
        }

        
        if(Input.GetKeyUp(KeyCode.Alpha1)){
            active = false;
        }*/

        if(active == true){
            if(sec==false){
                Activar();
                sec = true;
            }
        }

        if(active == false){
            sec = false;
        }

    }

    IEnumerator secFam(){

        for(int i = 0; i < AElements.Length; i++){
            if(i==groupToActive){
                AElements[i].SetActive(true);
                BElements[i].SetActive(true);
                CElements[i].SetActive(true);
                DElements[i].SetActive(true);
                EElements[i].SetActive(true);
            }else{
                AElements[i].SetActive(false);
                BElements[i].SetActive(false);
                CElements[i].SetActive(false);
                DElements[i].SetActive(false);
                EElements[i].SetActive(false);
            }
        }

        yield break;
    }
}
