using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField]
    GameObject bg, panel, phone, deliveringOrderParent, deliveringOrderPrefab, availableOrderParent, availableOrderPrefab, reviewMessage, resultScreen, resultText, resultBox, clearText, contBtn, reBtn, basicMessage, missionText;
    [SerializeField]
    GameObject[] apps, appIcons, resultRow, informations;
    [SerializeField]
    TextMeshProUGUI timeText, cashText, multiDeliv;
    
    [SerializeField]
    RectTransform panelRT;
    [SerializeField]
    Sprite[] stars;
    [SerializeField]
    PlayerController player;
    [SerializeField]
    OrderManager orderManager;
    [SerializeField]
    CameraController mainCam;
    [SerializeField]
    Transform upgradeItems;
    public AnimationCurve vibrateCurve;
    public ProgressBar progressBar;
    public bool phoneOpen = false;
    bool onTransition = false;
    int currentApp = 0, pastApp = 0, moneyBefore = 0;
    public string[][] reviewComment = {
        //속도도 빠르고 안정성도 높은 경우
        new string[] {"완벽하네요!","최고에요:]","정말 배달의 신이네요","정말 만족합니다","빠르고 상태도 좋아요"},
        //속도가 빠른 경우
        new string[] {"굉장히 빠르네요!","배달 속도가 빨라요"},
        //안정성이 높은 경우
        new string[] {"음식 상태가 좋아요","기사님이 운전 정말 잘하시네요"},
        //보통인 경우
        new string[] {"뭐 나쁘지 않네요","무난하네요"},
        //속도가 느린 경우
        new string[] {"배달중에 사고라도 났나요. 왜이렇게 오래걸리죠!","너무 오래걸리네요"},
        //안정성이 낮은 경우
        new string[] {"포장이 다 뜯겼네요! 오다 태풍이라도 만났나봐요","배달을 어떻게 했길래 상태가 이따구에요!","배달을 정말 과격하게 하시네요"},
        //둘다 안좋은 경우
        new string[] {"정말 속도와 상태가 최악입니다! 다시는 안시킬게요","돈 아까울 정도로 별로에요","배달 기사가 최악입니다"}
    };
    private void Awake() {
        StartCoroutine("AddEvent");
    }
    private void Start() {
        if (GameManager.Instance.unlockMessage) {
            UnlockMessage(GameManager.Instance.unlock);
        }
        if (GameManager.Instance.orderCountMessage) {
            OrderCountMessage(GameManager.Instance.maxOrder);
        }
        if (GameManager.Instance.missionMessage) {
            MissionMessage(GameManager.Instance.mission);
        }
    }
    IEnumerator AddEvent() {
        yield return new WaitForSeconds(0.2f);
        GameManager.OnGameStateChanged += GameStateChange;
    }
    private void GameStateChange(GameState gameState) {
        if (gameState == GameState.Clear) {
            DayClear(true);
        }
        if (gameState == GameState.GameOver) {
            DayClear(false);
        }
    }
    private void Update() {
        // 테스트용
        // if(Input.GetKeyDown(KeyCode.Q)) {
        //     GameManager.Instance.Cash += 100000;
        // }
        // if(Input.GetKeyDown(KeyCode.W)) {
        //     GameManager.Instance.Cash += 10000000;
        // }
        // if(Input.GetKeyDown(KeyCode.E)) {
        //     orderManager.timer += 170;
        // }
        if(Input.GetButtonDown("Menu") && !onTransition && GameManager.Instance.State == GameState.Play) {
            onTransition = true;
            if(phoneOpen) {
                Resume();
                CloseUI();
                CloseApp(currentApp);
                progressBar.Show();
            }
            else {
                Pause();
                OpenUI();
                progressBar.Hide();
            }
            phoneOpen = !phoneOpen;
        }
        UpdateMoneyDisplay();
        if (GameManager.Instance.Day == 0)
            return;
        UpdateTimerDisplay();
    }
    public void OpenUI() {
        LeanTween.alpha(panelRT, 0.6f, 0.3f).setIgnoreTimeScale(true);
        LeanTween.moveLocal(phone, new Vector3(-490f, -54.275f, 0), 0.4f).setEase(LeanTweenType.easeOutBack).setOnComplete(transitionFinish).setIgnoreTimeScale(true);
        LeanTween.rotateLocal(phone, new Vector3(0, 0, 3f), 0.4f).setEase(LeanTweenType.easeOutBack).setOnComplete(OpenFirstApp).setIgnoreTimeScale(true);
    }
    //핸드폰 닫기
    public void CloseUI() {
        LeanTween.alpha(panelRT, 0, 0.3f);
        LeanTween.moveLocal(phone, new Vector3(-544f, -952.7f, 0), 0.3f).setEase(LeanTweenType.easeOutQuint).setOnComplete(transitionFinish);
        LeanTween.rotateLocal(phone, Vector3.zero, 0.3f).setEase(LeanTweenType.easeOutElastic);
    }
    //앱 클릭
    public void AppIconClicked(int id) {
        if (id != currentApp && !onTransition) {
            CloseApp(currentApp);
            currentApp = id;
            OpenApp(currentApp);
        }
    }
    //핸드폰 처음 열었을 때 앱 열기
    public void OpenFirstApp() {
        currentApp = 0;
        pastApp = 0;
        OpenApp(0);
    }
    //앱 열기
    public void OpenApp(int id) {
        onTransition = true;
        GameObject app = apps[id];
        app.SetActive(true);
        if (id == 0)
            DeliveryApp();
        if (id == 1)
            Upgrade_app();
        app.transform.position = appIcons[id].transform.position;
        app.transform.localScale = new Vector3(0.05f, 0.05f, 1f);
        LeanTween.moveLocal(app, new Vector3(400f, 10f, 0f), 0.3f).setEase(LeanTweenType.easeOutQuad).setIgnoreTimeScale(true);
        LeanTween.scale(app, new Vector3(1f, 1f, 1f), 0.3f).setEase(LeanTweenType.easeOutQuad).setOnComplete(transitionFinish).setIgnoreTimeScale(true);
    }
    //앱 닫기
    public void CloseApp(int id) {
        onTransition = true;
        GameObject app = apps[id];
        LeanTween.moveLocal(app, new Vector3(400f, -1000f, 0f), 0.3f).setEase(LeanTweenType.easeOutQuad).setOnComplete(deactivateApp).setIgnoreTimeScale(true);
        LeanTween.scaleY(app, 0.05f, 0.3f).setEase(LeanTweenType.easeOutQuad).setOnComplete(transitionFinish).setIgnoreTimeScale(true);
    }
    void transitionFinish() {
        onTransition = false;
    }
    void deactivateApp() {
        apps[pastApp].SetActive(false);
        apps[pastApp].transform.localScale = new Vector3(1f,1f,1f);
        pastApp = currentApp;
    }
    //게임 중지
    void Pause() {
        Time.timeScale = 0f;
    }
    //게임 재개
    void Resume() {
        Time.timeScale = 1f;
    }
    //리뷰 메세지 띄우기
    public void ReviewMessage(int speedStar, int safteyStar, int time, int reward, int bonus) {
        GameObject message = Instantiate(reviewMessage, new Vector3(900f, -150f, 0f), Quaternion.identity);
        message.transform.localScale = new Vector3(0.05f, 0.05f, 1f);
        message.transform.SetParent(bg.transform);

        string reviewText = "";
        if (speedStar <= 1 && safteyStar <= 1)
            reviewText = reviewComment[6][Random.Range(0,reviewComment[6].Length)];
        else if (safteyStar <= 1)
            reviewText = reviewComment[5][Random.Range(0,reviewComment[5].Length)];
        else if (speedStar <= 1)
            reviewText = reviewComment[4][Random.Range(0,reviewComment[4].Length)];
        else if (speedStar == 5 && safteyStar == 5)
            reviewText = reviewComment[0][Random.Range(0,reviewComment[0].Length)];
        else if (speedStar == 5)
            reviewText = reviewComment[1][Random.Range(0,reviewComment[1].Length)];
        else if (safteyStar == 5)
            reviewText = reviewComment[2][Random.Range(0,reviewComment[2].Length)];
        else
            reviewText = reviewComment[3][Random.Range(0,reviewComment[3].Length)];
        
        string r = CashToString(reward);
        string t;
        if (bonus == 0) {
            t = $"배달 완료!\n시간: {time}초\n보상: {r}원\n리뷰:\n{reviewText}";
        } else {
            string b = CashToString(bonus);
            t = $"배달 완료!\n시간: {time}초\n보상: {r}원  팁: {b}원\n리뷰:\n{reviewText}";
        }
        message.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = t;

        int starCount = Mathf.RoundToInt((speedStar + safteyStar) / 2);
        message.transform.GetChild(1).GetComponent<Image>().sprite = stars[starCount];

        ViberatePhone();
        LeanTween.moveLocal(message, new Vector3(0f, -370f, 0f), 0.3f).setEase(LeanTweenType.easeOutBack).setIgnoreTimeScale(true);
        LeanTween.scale(message, new Vector3(1f, 1f, 1f), 0.3f).setEase(LeanTweenType.easeOutQuad).setIgnoreTimeScale(true);
        LeanTween.moveLocal(message, new Vector3(0f, -100f, 0f), 0.5f).setDelay(4f).setEase(LeanTweenType.easeOutCubic).setIgnoreTimeScale(true);
        LeanTween.alpha(message.GetComponent<RectTransform>(), 0f, 0.5f).setDelay(4f).setIgnoreTimeScale(true);
        LeanTween.alphaCanvas(message.transform.GetChild(0).GetComponent<CanvasGroup>(), 0f, 0.5f).setDelay(4f).setIgnoreTimeScale(true);
        Destroy(message, 4.5f);
    }
    void ViberatePhone() {
        LeanTween.moveX(phone, 403f, 0.3f).setEase(vibrateCurve);
        LeanTween.moveX(phone, 400f, 0.1f).setDelay(0.3f);
    }
    //배달앱 열었을때 텍스트 처리
    void DeliveryApp() {
        foreach (Transform child in deliveringOrderParent.transform) {
            GameObject.Destroy(child.gameObject);
        }
        foreach (Transform child in availableOrderParent.transform) {
            GameObject.Destroy(child.gameObject);
        }
        multiDeliv.text = "동시 배달 가능: " + GameManager.Instance.maxOrder.ToString();
        List<string[]> orders = orderManager.PassOrders();
        foreach (string[] order in orders) {
            GameObject item = Instantiate(deliveringOrderPrefab, Vector3.zero, Quaternion.identity);
            item.transform.SetParent(deliveringOrderParent.transform);
            if (int.Parse(order[2]) > 0)
                order[2] += "초 남음";
            else
                order[2] = "시간 초과";
            order[3] = CashToString(int.Parse(order[3])) + "원";
            for (int i=0; i<4; i++) {
                TextMeshProUGUI text = item.transform.GetChild(i).GetComponent<TextMeshProUGUI>();
                text.text = order[i];
            }
        }
        List<string[]> aOrders = orderManager.PassOrders_A();
        foreach (string[] order in aOrders) {
            GameObject item = Instantiate(availableOrderPrefab, Vector3.zero, Quaternion.identity);
            item.transform.SetParent(availableOrderParent.transform);
            order[2] += "초";
            order[3] = CashToString(int.Parse(order[3])) + "원";
            for (int i=0; i<4; i++) {
                TextMeshProUGUI text = item.transform.GetChild(i).GetComponent<TextMeshProUGUI>();
                text.text = order[i];
            }
            item.transform.GetChild(4).name = order[4];
        }
    }
    GameObject tmpObj;
    //오더 확인했을 때 풀에서 제거
    public void AcceptOrder(GameObject obj) {
        if (orderManager.CheckIfNotFull()) {
            int index = int.Parse(obj.transform.GetChild(4).name);
            orderManager.MakeOrder(index);
            LeanTween.scale(obj,new Vector2(1f,0.05f),0.3f).setIgnoreTimeScale(true);
            tmpObj = obj;
            LeanTween.alphaCanvas(obj.GetComponent<CanvasGroup>(),0,0.3f).setIgnoreTimeScale(true).setOnComplete(DestroyObj);
        } else {
            LeanTween.moveLocalX(obj, obj.GetComponent<RectTransform>().localPosition.x + 0.3f, 0.2f).setEase(vibrateCurve).setIgnoreTimeScale(true);
            LeanTween.moveLocalX(obj, obj.GetComponent<RectTransform>().localPosition.x , 0.1f).setDelay(0.2f).setIgnoreTimeScale(true);
        }
    }
    void DestroyObj() {
        Destroy(tmpObj);
        tmpObj = null;
    }
    public void AddOrderToUI(string[] order) {
        GameObject item = Instantiate(deliveringOrderPrefab, Vector3.zero, Quaternion.identity);
        item.transform.SetParent(deliveringOrderParent.transform);
        order[2] += "초 남음";
        order[3] = CashToString(int.Parse(order[3])) + "원";
        for (int i=0; i<4; i++) {
            TextMeshProUGUI text = item.transform.GetChild(i).GetComponent<TextMeshProUGUI>();
            text.text = order[i];
        }
        item.transform.localScale = new Vector2(1f,0);
        item.GetComponent<CanvasGroup>().alpha = 0;
        LeanTween.scale(item,new Vector2(1f,1f),0.3f).setIgnoreTimeScale(true);
        LeanTween.alphaCanvas(item.GetComponent<CanvasGroup>(),1f,0.3f).setIgnoreTimeScale(true);
    }
    //시간 업데이트
    void UpdateTimerDisplay() {
        float timer = (180 - orderManager.timer >= 0)? 180 - orderManager.timer : 0;
        float minutes = Mathf.FloorToInt(timer / 60);
        float seconds = Mathf.FloorToInt(timer % 60);
        string currentTime = string.Format("{00:00}:{1:00}",minutes,seconds);
        timeText.text = currentTime;
        if (timer <= 10)
            timeText.color = Color.red;
    }
    //돈 업데이트
    void UpdateMoneyDisplay() {
        int cash = GameManager.Instance.Cash;
        cashText.text = CashToString(cash) + "원";
    }
    public void CashAnim() {
        cashText.color = Color.green;
        LeanTween.scale(cashText.rectTransform,new Vector2(2f,2f), 1f).setEase(LeanTweenType.easeOutCirc).setIgnoreTimeScale(true);
        LeanTween.scale(cashText.rectTransform,new Vector2(1f,1f), 1f).setEase(LeanTweenType.easeInBack).setDelay(1f).setOnComplete(FinishCashAnim).setIgnoreTimeScale(true);
    }
    void FinishCashAnim() {
        cashText.color = Color.white;
    }
    //한국식 표현으로 전환
    string CashToString(int cash) {
        string st = "0";
        if(cash >= 100000000) {
            st = (cash/100000000).ToString() + "억";
            if((cash%100000000)/10000 > 0)
                st += ((cash%100000000)/10000).ToString() + "만";
        }
        else if(cash >= 10000) {
            st = (cash/10000).ToString() + "만";
            if((cash%10000) > 0)
                st +=(cash%10000).ToString();
        }
        else
            st = cash.ToString();
        return st;
    }
    //결과 창
    public void DayClear(bool pass) {
        if (GameManager.Instance.mission == 3 && GameManager.Instance.DeliveryCount < 12)
            GameManager.Instance.missionSuccess = false;
        else if (GameManager.Instance.mission == 6 && GameManager.Instance.DeliveryCount < 15)
            GameManager.Instance.missionSuccess = false;
        if (pass) {
            contBtn.SetActive(true);
            reBtn.SetActive(false);
            contBtn.GetComponent<CanvasGroup>().alpha = 0;
        } else {
            contBtn.SetActive(false);
            reBtn.SetActive(true);
            reBtn.GetComponent<CanvasGroup>().alpha = 0;
        }
        CloseApp(currentApp);
        resultScreen.SetActive(true);
        //패널
        LeanTween.alpha(panelRT, 0.6f, 3f).setIgnoreTimeScale(true).setDelay(3f).setOnComplete(Pause);
        //카메라
        mainCam.ResultScreen();
        //폰 숨기기
        LeanTween.moveY(phone, -1200f, 1f).setEase(LeanTweenType.easeInQuad).setDelay(3f).setIgnoreTimeScale(true);
        //헤더
        resultText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Day " + GameManager.Instance.Day.ToString();
        resultText.GetComponent<CanvasGroup>().alpha = 0;
        LeanTween.alphaCanvas(resultText.GetComponent<CanvasGroup>(), 1f, 0.3f).setDelay(6f).setIgnoreTimeScale(true);
        //시간, 돈
        foreach(GameObject row in resultRow) {
            row.GetComponent<CanvasGroup>().alpha = 0;
        }
        //첫번째 줄
        resultRow[0].GetComponent<RectTransform>().localPosition = new Vector2(0, 125f);
        LeanTween.moveLocalY(resultRow[0],220f, 0.3f).setDelay(7f).setIgnoreTimeScale(true);
        LeanTween.alphaCanvas(resultRow[0].GetComponent<CanvasGroup>(), 1f, 0.3f).setDelay(7f).setIgnoreTimeScale(true);
        int min = Mathf.RoundToInt(orderManager.timer) / 60;
        int sec = Mathf.RoundToInt(orderManager.timer) % 60;
        TextMeshProUGUI timeText = resultRow[0].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        LeanTween.value(0, min, 0.5f).setDelay(7f).setIgnoreTimeScale(true).setOnUpdate((float minVal) => {
            timeText.text = Mathf.RoundToInt(minVal).ToString() + "분 0초";
        });
        LeanTween.value(0, sec, 0.5f).setDelay(7.5f).setIgnoreTimeScale(true).setOnUpdate((float secVal) => {
            timeText.text = min.ToString() + "분 " + Mathf.RoundToInt(secVal).ToString() + "초";
        });
        //두번째 줄
        resultRow[1].GetComponent<RectTransform>().localPosition = new Vector2(0, 30f);
        LeanTween.moveLocalY(resultRow[1],125f, 0.3f).setDelay(8f).setIgnoreTimeScale(true);
        LeanTween.alphaCanvas(resultRow[1].GetComponent<CanvasGroup>(), 1f, 0.3f).setDelay(8f).setIgnoreTimeScale(true);
        TextMeshProUGUI moneyText = resultRow[1].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        LeanTween.value(0, GameManager.Instance.Cash, 1f).setDelay(8f).setEase(LeanTweenType.easeOutQuart).setIgnoreTimeScale(true).setOnUpdate((float cash) => {
            moneyText.text = CashToString(Mathf.RoundToInt(cash)) + "원";
        });
        //세번째 줄
        if (GameManager.Instance.mission == 0) {
            missionText.SetActive(false);
        } else {
            if (GameManager.Instance.missionSuccess) {
                missionText.GetComponent<TextMeshProUGUI>().text = "미션 성공!";
                missionText.GetComponent<TextMeshProUGUI>().color = Color.green;
            } else {
                missionText.GetComponent<TextMeshProUGUI>().text = "미션 실패";
                missionText.GetComponent<TextMeshProUGUI>().color = new Color32(214, 48, 49, 255);
            }
            LeanTween.alphaCanvas(missionText.GetComponent<CanvasGroup>(), 1f, 0.3f).setDelay(9f).setIgnoreTimeScale(true);
        }
        resultRow[2].GetComponent<RectTransform>().localPosition = new Vector2(0, -65f);
        LeanTween.moveLocalY(resultRow[2], 30f, 0.3f).setDelay(9f).setIgnoreTimeScale(true);
        LeanTween.alphaCanvas(resultRow[2].GetComponent<CanvasGroup>(), 1f, 0.3f).setDelay(9f).setIgnoreTimeScale(true);
        TextMeshProUGUI dCountText = resultRow[2].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        LeanTween.value(0, GameManager.Instance.DeliveryCount, 1f).setDelay(9f).setEase(LeanTweenType.easeOutCubic).setIgnoreTimeScale(true).setOnUpdate((float d) => {
            dCountText.text = Mathf.RoundToInt(d) + "건";
        });
        //결과
        resultBox.GetComponent<CanvasGroup>().alpha = 0;
        resultBox.GetComponent<RectTransform>().localScale = new Vector3(0.01f, 1f, 1f);
        LeanTween.alphaCanvas(resultBox.GetComponent<CanvasGroup>(), 1f, 0.3f).setDelay(10f).setIgnoreTimeScale(true);
        LeanTween.scaleX(resultBox, 1f, 0.3f).setEase(LeanTweenType.easeOutSine).setDelay(10f).setIgnoreTimeScale(true);
        TextMeshProUGUI totalMoneyText = resultBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        LeanTween.value(moneyBefore, GameManager.Instance.TotalCash, 1f).setDelay(10f).setEase(LeanTweenType.easeOutQuart).setIgnoreTimeScale(true).setOnUpdate((float cash) => {
            totalMoneyText.text = CashToString(Mathf.RoundToInt(cash)) + "원";
        });
        TextMeshProUGUI cT = clearText.GetComponent<TextMeshProUGUI>();
        if (pass) {
            cT.text = "클리어";
            cT.color = Color.white;
        } else {
            cT.text = "게임 오버";
            cT.color = new Color32(214, 48, 49, 255);
        }
        clearText.GetComponent<CanvasGroup>().alpha = 0;
        clearText.GetComponent<RectTransform>().localScale = new Vector3(3f, 3f, 1f);
        LeanTween.rotateLocal(clearText, new Vector3(0, 0, 60f), 0).setIgnoreTimeScale(true);
        LeanTween.rotateLocal(clearText, new Vector3(0, 0, 0), 0.3f).setEase(LeanTweenType.easeOutBack).setDelay(12f).setIgnoreTimeScale(true);
        LeanTween.scale(clearText, new Vector3(1f,1f,1f), 0.3f).setEase(LeanTweenType.easeOutCubic).setDelay(12f).setIgnoreTimeScale(true);
        if (pass) {
            LeanTween.alphaCanvas(clearText.GetComponent<CanvasGroup>(), 1f, 0.1f).setDelay(12f).setOnComplete(ShowContinueButton).setIgnoreTimeScale(true);
        } else {
            LeanTween.alphaCanvas(clearText.GetComponent<CanvasGroup>(), 1f, 0.1f).setDelay(12f).setOnComplete(ShowRestartButton).setIgnoreTimeScale(true);
        }
    }
    void ShowContinueButton() {
        moneyBefore = GameManager.Instance.TotalCash;
        LeanTween.alphaCanvas(contBtn.GetComponent<CanvasGroup>(), 1f, 0.3f).setIgnoreTimeScale(true);
    }
    void ShowRestartButton() {
        LeanTween.alphaCanvas(reBtn.GetComponent<CanvasGroup>(), 1f, 0.3f).setIgnoreTimeScale(true);
    }
    public void PressContinue() {
        GameManager.Instance.NewDay();
        Time.timeScale = 1f;
    }
    public void PressRestart() {
        GameManager.Instance.ResetDay();
        Time.timeScale = 1f;
    }
    GameObject unlockM;
    void UnlockMessage(int unlock) {
        string unlockText;
        switch (unlock) {
            case 1:
                unlockText = "오토바이를";
                break;
            case 2:
                unlockText = "소형차를";
                break;
            case 3:
                unlockText = "트럭을";
                break;
            case 4:
                unlockText = "경찰차를";
                break;
            case 5:
                unlockText = "버스를";
                break;
            case 6:
                unlockText = "탱크를";
                break;
            case 7:
                unlockText = "비행기를";
                break;
            case 8:
                unlockText = "공룡을";
                break;
            default:
                unlockText = "error";
                break;
        }
        unlockText = $"이제부터 {unlockText} 이용하실 수 있습니다.\n핸드폰의 업그레이드 앱에서 탈것을 바꾸세요.";
        InfoMessage(unlockText, new Vector2(695f, 170f));
    }
    void OrderCountMessage(int count) {
        string t = $"이제부터 {count.ToString()}개의 배달을 동시에 처리할 수 있습니다.";
        InfoMessage(t, new Vector2(770f, 80f));
    }
    void MissionMessage(int mission) {
        string t = "";
        if (mission == 1) {
            t = "[오늘의 미션!]\n지나가는 차에 10번 이하로 부딪히기\n달성시 보상 1.5배";
        }
        else if (mission == 2) {
            t = "[오늘의 미션!]\n모든 배달을 30초 안에 하기\n달성시 보상 1.5배";
        }
        else if (mission == 3) {
            t = "[오늘의 미션!]\n배달 12건 이상 하기\n달성시 보상 1.5배";
        }
        else if (mission == 4) {
            t = "[오늘의 미션!]\n지나가는 차에 5번 이하로 부딪히기\n달성시 보상 2배";
        }
        else if (mission == 5) {
            t = "[오늘의 미션!]\n모든 배달을 25초 안에 하기\n달성시 보상 2배";
        }
        else if (mission == 6) {
            t = "[오늘의 미션!]\n배달 15건 이상 하기\n달성시 보상 2배";
        }
        InfoMessage(t, new Vector2(605f, 180f));
    }
    void InfoMessage(string st, Vector2 size) {
        unlockM = Instantiate(basicMessage, new Vector3(600f, -350f, 0f), Quaternion.identity);
        unlockM.transform.SetParent(bg.transform);
        unlockM.GetComponent<RectTransform>().sizeDelta = size;
        unlockM.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = st;
        LeanTween.move(unlockM, new Vector3(700f, 60f, 0f), 0.3f).setEase(LeanTweenType.easeOutBack).setIgnoreTimeScale(true);
        //complete
        LeanTween.move(unlockM, new Vector3(700f, 300f, 0f), 0.5f).setDelay(5f).setEase(LeanTweenType.easeOutCubic).setIgnoreTimeScale(true);
        LeanTween.alpha(unlockM.GetComponent<RectTransform>(), 0f, 0.5f).setDelay(5f).setIgnoreTimeScale(true);
        LeanTween.alphaCanvas(unlockM.GetComponent<CanvasGroup>(), 0f, 0.5f).setDelay(5f).setIgnoreTimeScale(true);
        Destroy(unlockM, 5.55f);
    }
    void Upgrade_app() {
        TurnUpgradeOff();
        for (int i = 0; i < upgradeItems.childCount; i++) {
            if (i > GameManager.Instance.unlock) {
                upgradeItems.GetChild(i).GetComponent<Image>().color = Color.gray;
            }
        }
    }
    void TurnUpgradeOff() {
        foreach (GameObject item in informations) {
            item.SetActive(false);
        }
    }
    public void UpgradeBtn(int n) {
        if (GameManager.Instance.unlock >= n) {
            TurnUpgradeOff();
            informations[n].SetActive(true);
            player.Change(n);
        }
    }
}
