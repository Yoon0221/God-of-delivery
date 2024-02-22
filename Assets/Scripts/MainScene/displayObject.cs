using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class displayObject : MonoBehaviour
{
    public stackobject blueCar, motorcycle,truck,sportscar;
    public stackobject stackObject;
    public Vector3 rotation;
    public Vector3 offset = new Vector3(0, 0.05f, 0);
    public int orderInLayer = 0;

    public List<GameObject> partList;
    GameObject parts;
    void GenerateStack()
    {
        parts = new GameObject("Parts");
        parts.transform.parent = transform;
        parts.transform.localPosition = Vector3.zero;
        for (int i = 0; i < stackObject.stack.Count; i++)
        {
            GameObject stackPart = new GameObject("part"+i);
            SpriteRenderer sp = stackPart.AddComponent<SpriteRenderer>();
            sp.sprite = stackObject.stack[i];
            sp.sortingLayerName = "Character";
            stackPart.transform.parent = parts.transform;
            stackPart.transform.position = Vector3.zero;
           
            partList.Add(stackPart);
        }
    }
    
    void Start()
    {
        GenerateStack();
        parts.SetActive(true);
    }
    void draw_stack()
    {
        int s = orderInLayer;
        Vector3 v = Vector3.zero;
        parts.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -rotation.z + 90));
        foreach (GameObject part in partList)
        {
            part.transform.localPosition = v;
            v += offset;
            
            part.transform.localRotation = Quaternion.Euler(rotation);
            
            part.GetComponent<SpriteRenderer>().sortingOrder = s;
            s += 1;
        }
    }
    void Update()
    {
        draw_stack();
    }
    public void Enable() {
        parts.SetActive(true);
    }
    public void Disable() {
        parts.SetActive(false);
    }
    public void ChangeBlueCar() {
        stackObject = blueCar;
        Destroy(parts);
        partList.Clear();
        GenerateStack();
    }
    public void ChangeMotorcycle() {
        stackObject = motorcycle;
        Destroy(parts);
        partList.Clear();
        GenerateStack();
    }
    public void ChangeTruck() {
        stackObject = truck;
        Destroy(parts);
        partList.Clear();
        GenerateStack();
    }
    public void ChangeSportsCar() {
        stackObject = sportscar;
        Destroy(parts);
        partList.Clear();
        GenerateStack();
    }
}
