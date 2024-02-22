using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryPoint : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public OrderManager orderManager;
    public ProgressBar progressBar;
    public List<int> orderIndex;
    bool belowPlayer = false;
    float enterTime;
    float fullTime = 1f;
    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void pointOn() {
        fullTime = GameManager.Instance.currentFullTime;
        gameObject.SetActive(true);
    }
    //플레이어 들어왔을 때
    private void OnTriggerEnter2D(Collider2D col) {
        if (col.CompareTag("PlayerCollider")) {
            bool pass = true;
            orderManager.orders.FindAll((x) => x.index == orderIndex[0]).ForEach((x) => {if(x.state == 1) pass = false;});
            if (!pass) {
                enterTime = orderManager.timer;
                belowPlayer = true;
                progressBar.ShowBar();
            }
        }
    }
    private void OnTriggerExit2D(Collider2D col) {
        if (col.CompareTag("PlayerCollider") && belowPlayer) {
            belowPlayer = false;
            progressBar.HideBar();
        }
    }
    private void Update() {
        if (!belowPlayer)
            return;
        float time = orderManager.timer - enterTime;
        if(time >= fullTime) {
            belowPlayer = false;
            List<int> save = new List<int>(orderIndex);
            for (int i=0; i < save.Count; i++) {
                int j = orderManager.orders.FindIndex((x) => x.index == save[i]);
                if (orderManager.orders[j].state == 1) {
                    orderManager.FinishOrder(j);
                    orderIndex.Remove(save[i]);
                }
            }
            progressBar.FullBar();
            if (orderIndex.Count == 0)
                gameObject.SetActive(false);
        }
        progressBar.SetValue(time / fullTime);
    }
}
