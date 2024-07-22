using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class PartSlot : MonoBehaviour
{
    public static new string FOLDER_NAME = "PartSlots";


    public PartType bodyPart;

    [SerializeField] public PartEntity m_selectedChild;

    [SerializeField] List<PartEntity> _listOfPartEntities = new List<PartEntity>();

    public string SelectedChildName => m_selectedChild != null ? m_selectedChild.name : "";

    public int ChildrenLength => _listOfPartEntities.Count;

    public PartEntity[] Childrens => _listOfPartEntities.ToArray();

    public void Initialize()
    {
        foreach (Transform child in transform)
        {
            if (!_listOfPartEntities.Contains(child.GetComponent<PartEntity>()))
            {
                _listOfPartEntities.Add(child.GetComponent<PartEntity>());
            }
        }
    }


    public void ActiveBodyPart(int _index)
    {
        if (_listOfPartEntities.Count <= 0)
        {
            Debug.LogWarning($"No exist partEntity of {bodyPart} type");
            return;
        }
        m_selectedChild = _listOfPartEntities[_index];
        m_selectedChild.gameObject.SetActive(true);
        if (_listOfPartEntities.Count > 0)
        {
            foreach (var item in _listOfPartEntities)
            {
                if (item.BodyPartID == m_selectedChild.BodyPartID || item.transform.parent == m_selectedChild) { continue; }

                item?.gameObject.SetActive(false);
            }
        }
    }

    public void ActiveBodyPartByName(string name)
    {
        foreach(var part in _listOfPartEntities){
            if (part.name.Contains(name)){
                m_selectedChild = part;
            }
            part.gameObject.SetActive(false);
        }
        if (m_selectedChild != null){
            m_selectedChild.gameObject.SetActive(true);
        }
    }



    public int GetIndex()
    {
        return m_selectedChild != null ? _listOfPartEntities.IndexOf(m_selectedChild) : 0;
    }
}