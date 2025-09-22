using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private string _gameSceneName;
    public void Play()
    {
        SceneManager.LoadScene(_gameSceneName);
    }
    public void Exit()
    {
        Application.Quit();
    }
}
