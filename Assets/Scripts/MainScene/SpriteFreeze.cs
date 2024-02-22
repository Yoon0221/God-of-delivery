using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteFreeze : MonoBehaviour
{
    Animator anim;
    public bool freeze = true;
    public bool rigth = true;
    private void Start() {
        anim = GetComponent<Animator>();
    }
    private void Update() {
        if (!freeze)
            return;
        Quaternion rotation = Quaternion.LookRotation(Vector3.up , Vector3.forward);
        transform.eulerAngles = new Vector3(0, 0, rotation.eulerAngles.z);
        if (rigth)
            transform.localScale = new Vector3(1,1,1);
        else
            transform.localScale = new Vector3(-1,1,1);
    }
    // public void Walk() {
    //     anim.SetTrigger("Walk");
    // }
    public void Change(int n) {
        if (n == 1)
            anim.SetTrigger("Walk");
        if (n == 2)
            anim.SetTrigger("Dino");
    }
}
