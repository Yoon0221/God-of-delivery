using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupPoint : MonoBehaviour
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
    private void OnTriggerEnter2D(Collider2D col) {
        if (col.CompareTag("PlayerCollider") && orderManager.orders.Find((x) => x.index == orderIndex[0]).state == 0) {
            enterTime = orderManager.timer;
            belowPlayer = true;
            progressBar.ShowBar();
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
            for (int i=0; i < orderIndex.Count; i++) {
                int j = orderManager.orders.FindIndex((x) => x.index == orderIndex[i]);
                if (orderManager.orders[j].state == 0)
                    orderManager.Pickup(j);
            }
            progressBar.FullBar();
            orderIndex.Clear();
            gameObject.SetActive(false);
        }
        progressBar.SetValue(time / fullTime);
    }
}
