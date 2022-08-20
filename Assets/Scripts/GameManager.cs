using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState {
    Title,
    Story,
    Menu,
    Tutorial,
    Play,
    Clear,
    //나중에 지울 것
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public OrderManager orderManager;
    public GameState State;
    //그날 모은 돈
    public int Cash;
    //총 돈
    public int TotalCash;
    //총 날짜
    public int Day;
    //현재 건물 짓기 시작한시점부터 날짜
    public int buildDay;
    //난이도및 보상
    public int Lvl;
    //그날 배달 횟수
    public int DeliveryCount;
    //현재 건물의 업그레이드 상태
    public int buildState;
    //짓고있는 건물
    public int buildingNum;
    public int[] buildPrice;
    public int[] buildDayLimits;
    public static event Action<GameState> OnGameStateChanged;
    public int maxOrderPool = 1;
    public int unlock = 1;
    public bool unlockMessage = false;
    float timer;
    int nextOrderTime;
    int orderAddingInterval;
    bool loading;
    public bool playDataExist;
    public float currentFullTime = 1f;
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
        playDataExist = false;
    }
    private void Start() {
        TotalCash = 0;
        Day = 0;
        UpdateGameState(GameState.Title);
    }
    public void Title_GameStart() {
        UpdateGameState(GameState.Story);
    }
    public void EndStory() {
        UpdateGameState(GameState.Menu);
    }
    public void ToTitle() {
        UpdateGameState(GameState.Title);
    }
    public void StartGamePlay() {
        if (!playDataExist) {
            Day = 0;
            buildDay = 1;
            buildState = 0;
            buildingNum = 0;
            Lvl = 1;
            buildingNum = 0;
            unlock = 1;
            playDataExist = true;
            UpdateGameState(GameState.Play);
        }
        else {
            UpdateGameState(GameState.Play);
        }
    }
    public void UpdateGameState(GameState newState) {
        State = newState;
        switch (newState) {
            case GameState.Title:
                SceneManager.LoadScene("Start");
                break;
            case GameState.Story:
                SceneManager.LoadScene("Story");
                break;
            case GameState.Menu:
                SceneManager.LoadScene("PreStart");
                break;
            case GameState.Play:
                Cash = 0;
                DeliveryCount = 0;
                Lvl = buildState + 1;
                orderAddingInterval = 10;
                if (Lvl > 6)
                    Lvl = 6;
                if (Day == 0) {
                    maxOrderPool = 1;
                    orderAddingInterval = 100000; 
                } else if (Lvl == 1) {
                    maxOrderPool = 3;
                } else if (Lvl == 2) {
                    maxOrderPool = 4;
                } else if (Lvl == 3) {
                    maxOrderPool = 4;
                } else if (Lvl == 4) {
                    maxOrderPool = 5;
                } else if (Lvl == 5) {
                    maxOrderPool = 5;
                } else if (Lvl == 6) {
                    maxOrderPool = 6;
                }
                unlockMessage = true;
                if (buildingNum == 0 && buildState == 1 && unlock != 2) {
                    unlock = 2;
                } else if (buildingNum == 0 && buildState == 3 && unlock != 3) {
                    unlock = 3;
                } else if (buildingNum == 1 && buildState == 0 && unlock != 4) {
                    unlock = 4;
                }else if (buildingNum == 1 && buildState == 3 && unlock != 5) {
                    unlock = 5;
                } else if (buildingNum == 2 && buildState == 0 && unlock != 6) {
                    unlock = 6;
                }else if (buildingNum == 2 && buildState == 3 && unlock != 7) {
                    unlock = 7;
                } else if (buildingNum == 3 && buildState == 0 && unlock != 8) {
                    unlock = 8;
                } else if (buildingNum == 4 && buildState == 0 && unlock != 9) {
                    unlock = 9;
                } else {
                    unlockMessage = false;
                }
                nextOrderTime = orderAddingInterval;
                OnGameStateChanged = null;
                StartCoroutine(Loading());
                break;
            case GameState.Clear:
                TotalCash += Cash;
                break;
            case GameState.GameOver:
                break;
            default:
                break;
        }
        OnGameStateChanged?.Invoke(newState);
    }
    IEnumerator Loading() {
        loading = true;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainScene");
        while(!asyncLoad.isDone) {
            yield return null;
        }
        orderManager = FindObjectOfType<OrderManager>();
        loading = false;
    }
    private void Update() {
        if (State == GameState.Play && !loading) {
            timer = orderManager.timer;
            if (timer > nextOrderTime) {
                if (orderManager.orderPool.FindAll(x => x.state == 0).Count < maxOrderPool)
                    orderManager.AddOrderPool();
                nextOrderTime += orderAddingInterval;
            }
            if (Day == 0)
                return;
            if (orderManager.timer >= 3 * 60) {
                ClearDay();
            }
        }
    }
    void ClearDay() {
        UpdateGameState(GameState.Clear);
    }
    void GameOver() {
        UpdateGameState(GameState.GameOver);
    }
    public void NewDay() {
        Day += 1;
        buildDay += 1;
        Cash = 0;
        UpdateGameState(GameState.Menu);
    }
    public void ResetDay() {
        //저장해둔거 로딩
        UpdateGameState(GameState.Menu);
    }
    public void EndTutorial() {
        Day = 1;
        buildDay = 1;
        TotalCash = 1000;
        UpdateGameState(GameState.Menu);
    }
}

