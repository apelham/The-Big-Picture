using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitC : MonoBehaviour
{
    public void doquit()
    {
        Debug.Log("game has been quit");
        Application.Quit();
    }
}
