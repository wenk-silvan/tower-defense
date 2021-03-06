using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private LevelManager levelManager;
    private UIInputController uiInputController;

    private float timeRemaining;
    private bool timerIsRunning = false;
    private bool gameIsOver = false;

    [SerializeField] private Text healthText;
    [SerializeField] private Text moneyText;
    [SerializeField] private Text waveText;
    [SerializeField] private Text timerText;
    [SerializeField] private Button startWaveButton;
    [SerializeField] private Button menueButton;
    [SerializeField] private RectTransform nextWaveNotificationPanel;
    [SerializeField] private RectTransform countdownPanel;
    [SerializeField] private Text countdownText;

    private Boolean notified = false;
    private Boolean countDownStarted = false;
    public event Action HandleWaveStart = delegate { };
    public event Action ToggleMenue = delegate { };
    public event Action HandleManagerButtonClickSound = delegate { };
    public event Action HandleCountdownSound = delegate { };
    public event Action HandleNextWaveSound = delegate { };
    private void Awake()
    {
        levelManager = FindObjectOfType<LevelManager>();
        uiInputController = FindObjectOfType<UIInputController>();
    }

    private void OnEnable()
    {
        levelManager.HandleBaseHealthChange += DisplayHealth;
        levelManager.HandleMoneyChange += DisplayMoney;
        levelManager.HandleWaveChange += DisplayWave;
        levelManager.HandleGameOver += SetGameIsOver;
        uiInputController.StartWave += ResetTimerDisplay;
        startWaveButton.onClick.AddListener(ResetTimerDisplay);
        startWaveButton.onClick.AddListener(PlayClickSound);
        menueButton.onClick.AddListener(HandleMenueButton);
        menueButton.onClick.AddListener(PlayClickSound);
    }

    private void OnDisable()
    {
        levelManager.HandleBaseHealthChange -= DisplayHealth;
        levelManager.HandleMoneyChange -= DisplayMoney;
        levelManager.HandleWaveChange -= DisplayWave;
        levelManager.HandleGameOver -= SetGameIsOver;
        uiInputController.StartWave -= ResetTimerDisplay;
        startWaveButton.onClick.RemoveListener(ResetTimerDisplay);
        startWaveButton.onClick.RemoveListener(PlayClickSound);
        menueButton.onClick.RemoveListener(HandleMenueButton);
        menueButton.onClick.RemoveListener(PlayClickSound);
    }

    private void DisplayHealth(int newHealth)
    {
        healthText.text = "Health: " + newHealth;
    }
    private void DisplayMoney(int newMoney)
    {
        moneyText.text = "Money: " + newMoney + "$";
    }

    private void DisplayWave(int cur, int tot)
    {
        if(gameIsOver) {
            return;
        }

        waveText.text = "Waves: " + cur + "/" + tot;
        if (cur > 1)
        {
            timerIsRunning = true;
            timeRemaining = 30;
            startWaveButton.interactable = true;
        }
    }

    private void DisplayTimer(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timerText.text = "Next wave: " + string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void DisplayCountdown(float timeToDisplay)
    {
        if(!countDownStarted) {
            countDownStarted = true;
            StartCoroutine(PlayCountdownSound());            
        }

        countdownPanel.gameObject.SetActive(true);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        if (seconds > 0)
        {
            countdownText.text = seconds + "s";
        }
        else
        {
            countdownText.text = "GO!";
            if(!notified) {
                notified = true;
                StartCoroutine(NextWaveNotification());
            }
        }
    }

    private IEnumerator PlayCountdownSound() {
        int count = 5;
        while(count>=1) {
            if(!timerIsRunning)
            {
                yield break;
            }
            count--;
            HandleCountdownSound.Invoke();
            yield return new WaitForSeconds(1.0f);
        }
    }

    private IEnumerator NextWaveNotification() {
        HandleNextWaveSound.Invoke();
        int count = 4;
        while(count>=1) {
            count--;
            NextWaveNotificationToggle();
            yield return new WaitForSeconds(0.4f);
        }
    }

    private void NextWaveNotificationToggle()
    {
        if (nextWaveNotificationPanel.gameObject.activeSelf)
        {
            nextWaveNotificationPanel.gameObject.SetActive(false);
        }
        else
        {
            nextWaveNotificationPanel.gameObject.SetActive(true);
        }
    }

    private void ResetTimerDisplay()
    {
        if (timerIsRunning)
        {
            timeRemaining = 0;
        }
        else
        {
            HandleWaveStart();
            startWaveButton.interactable = false;
        }
    }

    private void HandleMenueButton()
    {
        ToggleMenue();
    }

    private void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                if (timeRemaining <= 6)
                {
                    DisplayCountdown(timeRemaining);
                }
            }
            else
            {
                timeRemaining = 0;
                timerIsRunning = false;
                startWaveButton.interactable = false;
                countdownPanel.gameObject.SetActive(false);
                notified = false;
                countDownStarted = false;
                HandleWaveStart();
            }
            DisplayTimer(timeRemaining);
        }
    }

    private void PlayClickSound() {
        HandleManagerButtonClickSound();
    }

    private void SetGameIsOver(bool notUsed) {
        gameIsOver = true;
    }
}
