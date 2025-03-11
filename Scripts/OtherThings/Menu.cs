using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void InitGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void Kill()
    {
        Debug.Log("Adios");
        Application.Quit();
    }
}
