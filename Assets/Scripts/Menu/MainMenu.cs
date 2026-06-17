using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    public void Play()
    {
        LevelManager.Instance.LoadScene("SampleScene", "CircleWipe");
    }

    public void Quit() 
    {
        Application.Quit();
    }
}
