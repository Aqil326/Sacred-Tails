using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class HideWithParticle : MonoBehaviour
{
    public ParticleSystem system;
    public MeshRenderer mesh;
    bool changeColor;
    Vector3[] vertices;
    Color[] colors;

    void OnEnable() {
       
    }
    // Update is called once per frame
    void Update()
    {
        if(system != null && mesh != null){
        if(system.isPlaying){
            if(changeColor == false){
                StartCoroutine(show());
                changeColor = true;
            }
            }
            else{
                if(changeColor == true){
                    StartCoroutine(hide());
                    changeColor = false;
                }
          
        }
        }
        
    }

IEnumerator show(){
        mesh.enabled = true;
        yield break;
}

IEnumerator hide(){
         mesh.enabled = false;
        yield break;
}

}
