using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameSettings : MonoBehaviour
{
    public Toggle redCPU, redHuman;
    public Toggle greenCPU, greenHuman;
    public Toggle yellowCPU, yellowHuman;
    public Toggle blueCPU, blueHuman;

    void ReadToggle()
    {
        //---------RED - 0-----------
        if (redCPU.isOn)
        {
            SaveSettings.players[0] = "CPU";
        }else if (redHuman.isOn)
        {
            SaveSettings.players[0] = "HUMAN";
        }
        //---------GREEN - 1---------
        if (greenCPU.isOn)
        {
            SaveSettings.players[1] = "CPU";
        }
        else if (greenHuman.isOn)
        {
            SaveSettings.players[1] = "HUMAN";
        }
        //---------YELLOW - 2--------
        if (yellowCPU.isOn)
        {
            SaveSettings.players[2] = "CPU";
        }
        else if (yellowHuman.isOn)
        {
            SaveSettings.players[2] = "HUMAN";
        }
        //---------BLUE - 3----------
        if (blueCPU.isOn)
        {
            SaveSettings.players[3] = "CPU";
        }
        else if (blueHuman.isOn)
        {
            SaveSettings.players[3] = "HUMAN";
        }
    }

    public void StartGame(string sceneName)
    {
        ReadToggle();
        SceneManager.LoadScene(sceneName);
    }

    /*//---------RED - 0-----------
    public void SetRedHumanType(bool on)
    {
        if (on)
        {
            SaveSettings.players[0] = "HUMAN";
        }
    }
    public void SetRedCPUType(bool on)
    {
        if (on)
        {
            SaveSettings.players[0] = "cpu";
        }
    }

    //---------GREEN - 1-----------
    public void SetGreenHumanType(bool on)
    {
        if (on)
        {
            SaveSettings.players[1] = "HUMAN";
        }
    }
    public void SetGreenCPUType(bool on)
    {
        if (on)
        {
            SaveSettings.players[1] = "cpu";
        }
    }

    //---------YELLOW - 2-----------
    public void SetYellowHumanType(bool on)
    {
        if (on)
        {
            SaveSettings.players[2] = "HUMAN";
        }
    }
    public void SetYellowCPUType(bool on)
    {
        if (on)
        {
            SaveSettings.players[2] = "cpu";
        }
    }

    //---------BLUE - 3-----------
    public void SetBlueHumanType(bool on)
    {
        if (on)
        {
            SaveSettings.players[3] = "HUMAN";
        }
    }
    public void SetBlueCPUType(bool on)
    {
        if (on)
        {
            SaveSettings.players[3] = "cpu";
        }
    }//*/
}

public static class SaveSettings
{
    // 0     1     2     3
    // RED GREEN YELLOW BLUE
    public static string[] players = new string[4];

    // Store winners
    public static string[] winners = new string[3]
    { string.Empty, string.Empty, string.Empty};
}

