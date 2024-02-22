using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float acceleration;
    public float maxSpeed;
    public float reverseSpeed;
    public float driftFactor;
    public float driveTurnSmoothTime;
    public float roadDrag;
    public float dragTime;
    public float breakForce;
    public float velMag;

    float accelerationInput;
    float deccelerationInput;
    float steeringInput;
    float turnSmoothVelocity;
    Vector2 mousePos;
    Vector2 lookDir;
    bool stopControl = false;
    int bumpCount;

    BoxCollider2D col;
    Rigidbody2D rb;
    displayObject disp;
    MapManager mapManager;
    UIController uIController;
    OrderManager orderManager;
    SpriteRenderer sr,sr2;
    public Camera cam;
    public TrailRenderer[] tireMarks;
    public GameObject hitEffect;
    public float[] fullTimes;
    public int currentUpgrade;
    public Sprite Shoes, Motorcycle, Smallcar, Truck, Policecar, Sportscar, Tank, Airplane, Dinosaur;
    public GameObject tankChild, spriteChild;
    public int[] speedValues;
    public int[] accelerationValues;
    public int[] reverseValues;
    public float[] driftValues;
    public float[] dragValues;

    void Awake() {
        col = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        disp = GetComponent<displayObject>();
        mapManager = FindObjectOfType<MapManager>();
        uIController = FindObjectOfType<UIController>();
        orderManager = FindObjectOfType<OrderManager>();
    }
    private void Start() {
        transform.position = new Vector3 (0.5f, 4f, 0);
        transform.rotation = Quaternion.Euler(0,0,-90f);
        bumpCount = 0;
        sr = spriteChild.GetComponent<SpriteRenderer>();
        sr2 = GetComponent<SpriteRenderer>();
        if (GameManager.Instance.currentUpgrade == 0)
            Change(1);
        else
            Change(GameManager.Instance.currentUpgrade);
    }

    void Update() {
        if (GameManager.Instance.State == GameState.Play) {
            //입력 제어
            mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            accelerationInput = Input.GetAxisRaw("accelerate");
            deccelerationInput = Input.GetAxisRaw("deccelerate");
            //회전 애니메이션 설정
            disp.rotation = new Vector3(0, 0, (transform.localEulerAngles.z + 90) % 360);
            //바퀴자국
            if (currentUpgrade == 1 || currentUpgrade == 2 || currentUpgrade == 3 || currentUpgrade == 5)
                CheckTrail();
        } else {
            accelerationInput = 0;
            deccelerationInput = 0;
        }
        // 테스트용
        // if (Input.GetKeyDown(KeyCode.R))
        //     GameManager.Instance.unlock = 8;
    }

    void FixedUpdate() {
        if (!stopControl) {
            ApplySteering();
            KillOrthogonalVelocity();
            ApplyForce();
        }
    }
    //플레이어 회전
    void ApplySteering() {
        //움직이는 속도에 따라 마우스 따라가는 속도 조정
        float turnSmoothTime;
        velMag = rb.velocity.magnitude;
        if (velMag < 0.2f)
            return;
        else if (velMag < maxSpeed/3)
            turnSmoothTime = driveTurnSmoothTime * 6;
        else if (velMag < maxSpeed/2)
            turnSmoothTime = driveTurnSmoothTime * 3;
        else
            turnSmoothTime = driveTurnSmoothTime;
        //마우스위치 방향벡터로 각 구함
        float targetAngle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        
        //자연스럽게 회전하도록 중간값 설정
        float angle = Mathf.SmoothDampAngle(transform.localEulerAngles.z, targetAngle, ref turnSmoothVelocity, Time.smoothDeltaTime * turnSmoothTime);
        rb.rotation = angle;
        lookDir = mousePos - rb.position;

        //애니메이터 방향 설정
        if (lookDir.x > 0)
            spriteChild.GetComponent<SpriteFreeze>().rigth = true;
        else
            spriteChild.GetComponent<SpriteFreeze>().rigth = false;
    }
    //가속
    void ApplyForce() {
        //환경 저항
        float envDrag = mapManager.GetTileDrag(transform.position);
        //감속
        if(deccelerationInput == 1) {
            rb.drag = envDrag + Mathf.Lerp(rb.drag, breakForce, Time.smoothDeltaTime * dragTime);
        } else if(accelerationInput == 0) {
            rb.drag = envDrag + Mathf.Lerp(rb.drag, roadDrag, Time.smoothDeltaTime * dragTime);
        } else {
            if (currentUpgrade != 7)
                rb.drag = envDrag;
            else 
                rb.drag = 0;
        }
        Vector2 forceVector = Vector2.zero;
        if (deccelerationInput == 1) {
            //후진
            forceVector = -transform.up * deccelerationInput * reverseSpeed;
        } else {
            //전진
            forceVector = transform.up * accelerationInput * acceleration;
        }
        if (maxSpeed - rb.velocity.magnitude < 0.1f) //quick fix
            rb.AddForce(forceVector * (maxSpeed - rb.velocity.magnitude) * 10f, ForceMode2D.Force);
        else
            rb.AddForce(forceVector, ForceMode2D.Force);

        //최대속도 넘지 않도록
        // quick fix.........
        // if(rb.velocity.magnitude > maxSpeed) {
        //     rb.velocity = rb.velocity.normalized * maxSpeed;
        // }
    }
    //옆으로 가는 힘 줄임
    void KillOrthogonalVelocity() {
        Vector2 forwardVelocity = transform.up * Vector2.Dot(rb.velocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(rb.velocity, transform.right);
        rb.velocity = forwardVelocity + rightVelocity * driftFactor;
    }
    //충격 vfx 생성
    bool collideable = true;
    //충돌
    private void OnCollisionEnter2D(Collision2D col) {
        ContactPoint2D c = col.contacts[0];
        if (/*c.normalImpulse > 1.5f && */collideable) {
            GameObject effect = Instantiate(hitEffect, c.point, Quaternion.identity);
            Destroy(effect, 0.3f);
            int damage = 0;
            if (col.gameObject.CompareTag("NPC")) {
                if (c.normalImpulse > 10)
                    damage = 5;
                else
                    damage = 3;
                bumpCount += 1;
                if (GameManager.Instance.mission == 1 && bumpCount > 10)
                    GameManager.Instance.missionSuccess = false;
                else if (GameManager.Instance.mission == 4 && bumpCount > 5)
                    GameManager.Instance.missionSuccess = false;
            } else {
                if (c.normalImpulse > 10)
                    damage = 3;
                else if (c.normalImpulse > 5)
                    damage = 2;
                else
                    damage = 1;
            }
            orderManager.AddDamage(damage);
            collideable = false;
            StartCoroutine(ColWait());
        }
    }
    IEnumerator ColWait() {
        yield return new WaitForSeconds(0.5f);
        collideable = true;
    }
    //바퀴자국 생성
    void CheckTrail() {
        if (getMovementDiff() > 0.2f && velMag > maxSpeed * 0.6f) {
            foreach(TrailRenderer tr in tireMarks) {
                tr.emitting = true;
            }
        } else {
            foreach(TrailRenderer tr in tireMarks) {
                tr.emitting = false;
            }
        }
    }
    //마우스 방향과 이동방향의 차이를 리턴한다.
    float getMovementDiff() {
        float degree = (transform.eulerAngles.z + 90) % 360;
        Vector2 playerDir = new Vector2(Mathf.Cos(degree * Mathf.Deg2Rad), Mathf.Sin(degree * Mathf.Deg2Rad));
        return Mathf.Abs((lookDir.normalized - playerDir).magnitude);
    }
    public void Change(int n) {
        currentUpgrade = n;
        GameManager.Instance.currentUpgrade = currentUpgrade;
        maxSpeed = speedValues[n];
        acceleration = accelerationValues[n];
        reverseSpeed = reverseValues[n];
        driftFactor = driftValues[n];
        roadDrag = dragValues[n];
        GameManager.Instance.currentFullTime = fullTimes[n];
        if (n == 0) {
            spriteChild.GetComponent<Animator>().enabled = true;
            sr.sprite = Shoes;
            sr2.sprite = null;
            disp.Disable();
            spriteChild.GetComponent<SpriteFreeze>().freeze = true;
            spriteChild.GetComponent<SpriteFreeze>().Change(1);
        }
        if (n == 1) {
            spriteChild.GetComponent<Animator>().enabled = false;
            sr.sprite = null;
            sr2.sprite = null;
            disp.Enable(); 
            disp.ChangeMotorcycle();
        }
        if (n == 2)
        {
            spriteChild.GetComponent<Animator>().enabled = false;
            sr.sprite = null;
            sr2.sprite = null;
            disp.Enable();
            disp.ChangeBlueCar();
        }
        if (n == 3)
        {
            spriteChild.GetComponent<Animator>().enabled = false;
            sr.sprite = null;
            sr2.sprite = null;
            disp.Enable();
            disp.ChangeTruck();
        }
        if (n == 4)
        {
            spriteChild.GetComponent<Animator>().enabled = false;
            sr.sprite = null;
            sr2.sprite = Policecar;
            disp.Disable();
            spriteChild.GetComponent<SpriteFreeze>().freeze = false;
        }
        if (n == 5)
        {
            spriteChild.GetComponent<Animator>().enabled = false;
            sr.sprite = null;
            sr2.sprite = null;
            disp.Enable(); 
            disp.ChangeSportsCar();
        }
        if (n == 6)
        {
            spriteChild.GetComponent<Animator>().enabled = false;
            sr.sprite = null;
            sr2.sprite = Tank;
            disp.Disable();
            spriteChild.GetComponent<SpriteFreeze>().freeze = false;
        }
        if (n == 7)
        {
            spriteChild.GetComponent<Animator>().enabled = false;
            sr.sprite = null;
            sr2.sprite = Airplane;
            disp.Disable();
            spriteChild.GetComponent<SpriteFreeze>().freeze = false;
        }
        if (n == 8)
        {
            spriteChild.GetComponent<Animator>().enabled = true;
            sr.sprite = Dinosaur;
            sr.sortingLayerName = "Character";
            sr2.sprite = null;
            disp.Disable();
            spriteChild.GetComponent<SpriteFreeze>().freeze = true;
            spriteChild.GetComponent<SpriteFreeze>().Change(2);
        }

        if (n == 7) {
            col.enabled = false;
            sr2.sortingLayerName = "Plane";
        } else {
            col.enabled = true;
            sr2.sortingLayerName = "Character";
        }

        if (n == 6) {
            tankChild.SetActive(true);
        } else {
            tankChild.SetActive(false);
        }

        if (n != 1 && n != 2 && n != 3 && n != 5) {
            foreach(TrailRenderer tr in tireMarks) {
                tr.emitting = false;
            }
        }
    }
}
