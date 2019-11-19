using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Lvl1C : MonoBehaviour
{
    public void SceneSwitcher()
    {
        //Application.LoadLevel(2);
        SceneManager.LoadScene(2);
    }
}
