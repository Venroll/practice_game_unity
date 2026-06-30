using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameSystem : MonoBehaviour
{
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject lastWinPanel;
    [SerializeField] GameObject losePanel;
    [SerializeField] TextMeshProUGUI CoinCountText;
    [SerializeField] TextMeshProUGUI PauseCoinCountText;
    private int CoinCount = 0;
    private int totalCoins;
    private int maxLvl;
    private int maxCoins;
    private int localMaxCoins;
    private bool isPaused = false;

    private void Start()
    {
        totalCoins = GameObject.FindGameObjectsWithTag("Coins").Length;
        CoinCountText.text = $"COINS\n{CoinCount}/{totalCoins}";
        maxLvl = PlayerPrefs.GetInt("MaxLvl", 0);
        maxCoins = PlayerPrefs.GetInt("MaxCoins", 0);
        localMaxCoins = PlayerPrefs.GetInt("LocalMaxCoins", 0);
        PauseCoinCountText.text = $"TOTAL COINS\n{localMaxCoins}";
    }

    void Update()
    {
        if (winPanel.activeSelf || lastWinPanel.activeSelf || losePanel.activeSelf)
            return;
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!isPaused)
            {
                pausePanel.SetActive(true);
                ActivatePause();
            }
            else
            {
                pausePanel.SetActive(false);
                DeactivatePause();
            }
        }
    }
    void ActivatePause()
    {
        Time.timeScale = 0f;
        isPaused = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    void DeactivatePause()
    {
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void ActivateWinMenu()
    {
        localMaxCoins += CoinCount;
        if (SceneManager.GetActiveScene().buildIndex > maxLvl)
        {
            PlayerPrefs.SetInt("MaxLvl", SceneManager.GetActiveScene().buildIndex);
        }
        if (localMaxCoins > maxCoins)
        {
            PlayerPrefs.SetInt("MaxCoins", localMaxCoins);
        }
        PlayerPrefs.SetInt("LocalMaxCoins", localMaxCoins);
        PlayerPrefs.Save();
        winPanel.SetActive(true);
        ActivatePause();
    }
    public void ActivateLastWinMenu()
    {
        localMaxCoins += CoinCount;
        if (SceneManager.GetActiveScene().buildIndex > maxLvl)
        {
            PlayerPrefs.SetInt("MaxLvl", SceneManager.GetActiveScene().buildIndex);
        }
        if (localMaxCoins > maxCoins)
        {
            PlayerPrefs.SetInt("MaxCoins", localMaxCoins);
        }
        PlayerPrefs.SetInt("LocalMaxCoins", 0);
        PlayerPrefs.Save();
        lastWinPanel.SetActive(true);
        ActivatePause();
    }
    public void ActivateLoseMenu()
    {
        PlayerPrefs.SetInt("LocalMaxCoins", 0);
        losePanel.SetActive(true);
        ActivatePause();
    }

    public void AddCoin()
    {
        CoinCount++;
        CoinCountText.text = $"COINS\n{CoinCount}/{totalCoins}";
    }
    public void NextLevel()
    {
        DeactivatePause();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void RestartRun()
    {
        DeactivatePause();
        SceneManager.LoadScene(1);
    }
}