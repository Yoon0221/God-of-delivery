using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public UIController uIController;
    public PickupPoint[] pickupPoints;
    public DeliveryPoint[] deliveryPoints;
    public List<Order> orders = new List<Order>();
    public List<GameObject> arrows = new List<GameObject>();
    public List<AvailableOrder> orderPool = new List<AvailableOrder>();
    
    public int[][] bonus = {
        new int[] {0,0,0,0,0,0},
        new int[] {0,0,0,1000,2000,3000},
        new int[] {0,0,3000,6000,8000,10000},
        new int[] {0,0,10000,20000,30000,50000},
        new int[] {0,0,50000,80000,100000,150000},
        new int[] {0,0,100000,200000,300000,500000},
        new int[] {0,0,200000,400000,600000,1000000}
    };
    public int[] pickupPerLvl;
    public int[] deliveryPerLvl;

    public float timer;
    public GameObject arrowPrefab;
    public Transform canvas;
    int orderPoolIdx = 0;
    //오더 클래스
    //index: 리스트에서의 인덱스, state: 상태(0: 픽업 전, 1: 픽업후 배달전), 픽업.배달 포인트, 목표 시간, 최대 보상, 시작시간
    public class Order {
        public int index;
        public int state;
        public PickupPoint pickupPoint;
        public DeliveryPoint deliveryPoint;
        public float targetTime;
        public int maxReward;
        public float startTime;
        public int damage;
        //생성자
        public Order(int i, PickupPoint pp, DeliveryPoint dp, float tT, int mR, float sT) {
            index = i;
            state = 0;
            pickupPoint = pp;
            deliveryPoint = dp;
            targetTime = tT;
            maxReward = mR;
            startTime = sT;

            pickupPoint.orderIndex.Add(i);
            deliveryPoint.orderIndex.Add(i);
            pickupPoint.pointOn();
            deliveryPoint.pointOn();
        }
    }
    public class AvailableOrder {
        public int index;
        //0 받기 전, 1 받은 상태
        public int state;
        public PickupPoint pickupPoint;
        public DeliveryPoint deliveryPoint;
        public float targetTime;
        public int maxReward;
        public AvailableOrder(int i, PickupPoint pp, DeliveryPoint dp, float tT, int mR) {
            index = i;
            state = 0;
            pickupPoint = pp;
            deliveryPoint = dp;
            targetTime = tT;
            maxReward = mR;
        }
        public Order AddToOrderList(int index, float startTime) {
            return new Order(index, pickupPoint, deliveryPoint, targetTime, maxReward, startTime);
        }
    }
    private void Awake() {
        StartCoroutine("AddEvent");
    }
    IEnumerator AddEvent() {
        yield return new WaitForSeconds(0.2f);
        GameManager.OnGameStateChanged += GameStateChange;
    }
    public void GameStateChange(GameState gameState) {
        if (gameState == GameState.Play) {
            ResetTimer();
            orders.RemoveAll(x => true);
            orderPool.RemoveAll(x => true);
        }
    }
    private void Start() {
        ResetTimer();
        if (GameManager.Instance.Day == 0) {
            orderPool.Add(new AvailableOrder(0, pickupPoints[0], deliveryPoints[0], 120, 1000));
        } else {
            for (int i = 0; i < GameManager.Instance.maxOrderPool; i++) {
                AddOrderPool();
            }
        }
    }
    private void Update() {
        if (GameManager.Instance.State == GameState.Play)
            timer += Time.deltaTime;
    }
    void ResetTimer() {
        timer = 0;
    }
    public void AddOrderPool() {
        int d = GameManager.Instance.Lvl;
        int poolSize = orderPool.Count;
        int actualLvl = (GameManager.Instance.buildingNum >= 1)? 6 : GameManager.Instance.Lvl;
        PickupPoint pPoint = pickupPoints[Random.Range(0, pickupPerLvl[actualLvl]+1)];
        DeliveryPoint dPoint = deliveryPoints[Random.Range(0, deliveryPerLvl[actualLvl]+1)];
        float dist = Vector2.Distance(pPoint.transform.position, dPoint.transform.position);
        float t = Mathf.RoundToInt(dist/8) + 17f;
        //mul: 나눠떨어지는 자리 baseC: 거리와 상관없이 기본보상 dM: 미터당 곱해지는 보상
        int mul = 0, baseC = 0, dM = 0;
        switch (d) {
            case 1:
                mul = 100;
                baseC = 4000;
                dM = 90;
                break;
            case 2:
                mul = 100;
                baseC = 10000;
                dM = 330;
                break;
            case 3:
                mul = 1000;
                baseC = 47000;
                dM = 1200;
                break;
            case 4:
                mul = 10000;
                baseC = 130000;
                dM = 5000;
                break;
            case 5:
                mul = 10000;
                baseC = 380000;
                dM = 20000;
                break;
            case 6:
                mul = 100000;
                baseC = 800000;
                dM = 30000;
                break;
            default:
                break;
        }
        int r = Mathf.FloorToInt(Mathf.Round(dist*dM)/mul)*mul + baseC;
        float luck = Random.Range(-1f, 1f);
        if (luck > 0.9f) {
            t *= 2;
            r *= 2;
        } else if (luck > 0.7f) {
            t = Mathf.RoundToInt(t * 1.5f);
            r = Mathf.FloorToInt(Mathf.Round(r * 1.5f)/mul)*mul;
        } else if (luck < -0.9f) {
            t = Mathf.RoundToInt(t * 0.8f);
            r = Mathf.FloorToInt(Mathf.Round(r * 0.3f)/mul)*mul;
        } else if (luck < -0.7f) {
            t = Mathf.RoundToInt(t * 0.9f);
            r = Mathf.FloorToInt(Mathf.Round(r * 0.7f)/mul)*mul;
        }
        orderPool.Add(new AvailableOrder(orderPoolIdx++, pPoint, dPoint, t, r));
    }
    public bool CheckIfNotFull() {
        return (orders.Count < GameManager.Instance.maxOrder);
    }
    public void MakeOrder(int index) {
        Order order = orderPool[index].AddToOrderList(index, timer);
        orders.Add(order);
        uIController.AddOrderToUI(new string[] {order.pickupPoint.transform.name,order.deliveryPoint.transform.name,order.targetTime.ToString(),order.maxReward.ToString()});

        GameObject arrow = Instantiate(arrowPrefab, new Vector3(-100f,-100f,0), Quaternion.identity);
        arrow.GetComponent<Arrow>().followPoint = orderPool[index].pickupPoint.transform;
        arrow.transform.SetParent(canvas);
        arrows.Add(arrow);

        orderPool[index].state = 1;
    }
    public void Pickup(int index) {
        orders[index].state = 1;
        //orders[index].pickupPoint.pointOff();

        arrows[index].GetComponent<Arrow>().followPoint = orders[index].deliveryPoint.transform;
        arrows[index].GetComponent<Arrow>().ChangeColor();
    }
    public void FinishOrder(int index) {
        Order order = orders[index];
        GameManager.Instance.DeliveryCount += 1;

        int clearTime = Mathf.FloorToInt(timer - order.startTime);
        int safteyStar = ReviewSaftey(order.damage);
        int speedStar = ReviewSpeed(clearTime, order.targetTime);
        int reward = orders[index].maxReward;
        int b = bonus[GameManager.Instance.Lvl][Mathf.RoundToInt((speedStar + safteyStar) / 2)];

        GameManager.Instance.Cash += reward + b;

        if (GameManager.Instance.mission == 2 && clearTime > 30f)
            GameManager.Instance.missionSuccess = false;
        else if (GameManager.Instance.mission == 5 && clearTime > 25f)
            GameManager.Instance.missionSuccess = false;

        uIController.CashAnim();
        uIController.ReviewMessage(speedStar,safteyStar,clearTime,reward,b);
        orders.RemoveAt(index);
        Destroy(arrows[index]);
        arrows.RemoveAt(index);
    }
    //안정성 평가 (0~5의 정수 리턴)
    int[,] safteyPerUpgrade = new int[,] {
        {0,1,3,5,6},
        {0,2,4,6,7},
        {0,2,4,6,8},
        {1,3,5,7,9},
        {0,2,4,6,8},
        {2,4,6,8,10},
        {3,5,7,9,12},
        {9,9,9,9,9},
        {99,99,99,99,99}
    };
    int ReviewSaftey(int damage) {
        int point = 0;
        int cu = GameManager.Instance.currentUpgrade;
        if (damage <= safteyPerUpgrade[cu,0]) {
            point = 5;
        } else if (damage <= safteyPerUpgrade[cu,1]) {
            point = 4;
        } else if (damage <= safteyPerUpgrade[cu,2]) {
            point = 3;
        } else if (damage <= safteyPerUpgrade[cu,3]) {
            point = 2;
        } else if (damage <= safteyPerUpgrade[cu,4]) {
            point = 1;
        } else {
            point = 0;
        }
        return point;
    }
    //속도 평가 (0~5의 정수 리턴)
    int ReviewSpeed(int clearTime, float targetTime) {
        int point = 0;
        if (clearTime >= targetTime) {
            point = 0;
        } else if (clearTime <= targetTime * 0.5f) {
            point = 5;
        } else if (clearTime <= targetTime * 0.6f) {
            point = 4;
        } else if (clearTime <= targetTime * 0.8f) {
            point = 3;
        } else if (clearTime <= targetTime * 0.9f) {
            point = 2;
        } else {
            point = 1;
        }
        return point;
    }
    public void AddDamage(int d) {
        foreach (Order order in orders) {
            order.damage += d;
        }
    }
    public List<string[]> PassOrders() {
        if (orderPool.Count == 0)
            return null;
        //string배열의 리스트를 리턴, string 배열은 {출발지,도착지,남은시간,보상}
        List<string[]> res = new List<string[]>();
        foreach(Order order in orders) {
            int timeLeft = Mathf.FloorToInt(order.targetTime - timer + order.startTime);
            res.Add(new string[] {order.pickupPoint.transform.name,order.deliveryPoint.transform.name,timeLeft.ToString(),order.maxReward.ToString()});
        }
        return res;
    }
    public List<string[]> PassOrders_A() {
        if (orderPool.Count == 0)
            return null;
        //string배열의 리스트를 리턴, string 배열은 {출발지,도착지,총 시간,보상,인덱스}
        List<string[]> res = new List<string[]>();
        foreach(AvailableOrder order in orderPool) {
            if(order.state == 1)
                continue;
            res.Add(new string[] {order.pickupPoint.transform.name,order.deliveryPoint.transform.name,order.targetTime.ToString(),order.maxReward.ToString(),order.index.ToString()});
        }
        return res;
    }
}
