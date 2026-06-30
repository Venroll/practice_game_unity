using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI maxLvl;
    [SerializeField] TextMeshProUGUI maxCoins;
    private void Start()
    {
        maxLvl.text = $"MaxLvl: {PlayerPrefs.GetInt("MaxLvl", 0)}";
        maxCoins.text = $"MaxCoins: {PlayerPrefs.GetInt("MaxCoins", 0)}";
    }
    public void BackToMenu()
    {
        PlayerPrefs.SetInt("LocalMaxCoins", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene(0);
    }

    public void StartGame()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1f;
        SceneManager.LoadScene(1);
    }
    public void ExitGame()
    {
        Application.Quit();
    }
    public void ResetStats()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}