using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class PartEntity : MonoBehaviour
{
    public static new string FOLDER_NAME = "PartEntities";

    [SerializeField] private string bodyPartID;
    [SerializeField] private List<GameObject> _childs = new List<GameObject>();
    [SerializeField] private List<GameObject> _childFromChilds = new List<GameObject>();
    private void OnEnable()
    {
        foreach (Transform child in transform)
        {
            if (!_childs.Contains(child.gameObject))
            {
                _childs.Add(child.gameObject);
                foreach (Transform item in child.transform)
                {
                    if (!_childFromChilds.Contains(item.gameObject))
                    {
                        _childFromChilds.Add(item.gameObject);
                    }
                }
            }
        }
        //Dragons
        DragonsEyes();
    }

    public void PutEvolution(bool isOn)
    {
        GetComponent<SkinnedMeshRenderer>().material.SetFloat("_Evo", isOn ? 1 : 0);
        GetComponent<SkinnedMeshRenderer>().material.SetInt("_Evo", isOn ? 1 : 0);
    }

    private void DragonsEyes()
    {
        if (_childs.Count > 0)
        {
            if (bodyPartID.Split('_').First() == "cabeza")
            {
                foreach (var item in _childs)
                {
                    if (item.name.Contains("ojo_B"))
                    {
                        item.SetActive(false);
                    }
                }
                //_childs[1].SetActive(false);
            }
        }
    }
    private void Start()
    {
        // SacredTailsLog.LogMessage(bodyPartID.Split('_').First());
        //SacredTailsLog.LogMessage(bodyPartID.Split('_')[0] + bodyPartID.Split('_')[1]);

        if (GetComponent<SpriteRenderer>() != null)
        {
            if (GetComponent<SpriteRenderer>().sortingLayerName == "Default")
            {
                GetComponent<SpriteRenderer>().sortingLayerName = bodyPartID.Split('_')[0] + bodyPartID.Split('_')[1];
            }
        }
        if (_childs.Count > 0)
        {
            foreach (var item in _childs)
            {
                if (item.GetComponent<SpriteRenderer>() != null)
                {
                    if (item.GetComponent<SpriteRenderer>().sortingLayerName == "Default")
                    {
                        item.GetComponent<SpriteRenderer>().sortingLayerName = item.name.Split('_')[0] + item.name.Split('_')[1];
                    }
                }
            }
            if (_childFromChilds.Count > 0)
            {
                foreach (var item in _childFromChilds)
                {
                    if (item.GetComponent<SpriteRenderer>() != null)
                    {
                        if (item.GetComponent<SpriteRenderer>().sortingLayerName == "Default")
                        {
                            item.GetComponent<SpriteRenderer>().sortingLayerName = item.name.Split('_')[0] + item.name.Split('_')[1] + item.name.Split('_')[2];
                        }
                    }
                }
            }
        }
    }
    public string BodyPartID
    {
        get
        {
            if (string.IsNullOrEmpty(bodyPartID) || bodyPartID != name)
            {
                bodyPartID = name;

                //config for 2 or more childs in a object, like weapons
                ChangeChildsSortingLayer();


            }
            return bodyPartID;
        }
        set { bodyPartID = value; }
    }
    private void ChangeSortingLayer()
    {
        var result = bodyPartID.Split('_').First();
        if (GetComponent<SpriteRenderer>() != null)
        {
            GetComponent<SpriteRenderer>().sortingLayerName = result;
        }
        else
        {
            var last = _childs.Last();
        }
    }
    private void ChangeChildsSortingLayer()
    {
        if (_childs.Count > 0)
        {
            if (bodyPartID.Split('_').First() == "Accesorio" || bodyPartID.Split('_').First() == "Accessory")
            {
                _childs[0].GetComponent<SpriteRenderer>().sortingLayerName = "Accesorio";
                _childs[1].GetComponent<SpriteRenderer>().sortingLayerName = "Accesorio_Atras";
            }
            if (bodyPartID.Split('_').First() == "Arma")
            {
                _childs[0].GetComponent<SpriteRenderer>().sortingLayerName = "Arma";
                _childs[1].GetComponent<SpriteRenderer>().sortingLayerName = "Arma_Atras";
            }
        }
    }
}
/*Sorting Layer
    - CabelloAtras
    - Accesorio_Atras
    - costumes_Back
    - Arma_Atras
    - Skin
    - costumes
    - Arma
    - mouth
    - eyesbrows
    - accesories
    - hair
    - Eyes
*/