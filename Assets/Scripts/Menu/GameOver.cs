using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public TextMeshProUGUI first, second, third;

    // Start is called before the first frame update
    void Start()
    {
        first.text = "1st: " + SaveSettings.winners[0];
        second.text = "2nd: " + SaveSettings.winners[1];
        third.text = "3rd: " + SaveSettings.winners[2];
    }

    /// <summary>
    /// Go back to Main Menu
    /// </summary>
    /// <param name="sceneName"></param>
    public void BackButton(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
