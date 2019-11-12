using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndLevel : MonoBehaviour
{

    public PlayerController player;
    public string levelName;
    private bool textEnabled; 
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D target)
    {
        if (target.gameObject.layer == 9)
        {
            textEnabled = true; 
            StartCoroutine(ResetLevel());
        }
    }

void OnGUI()
{
    if (textEnabled == true)
    {
        GUI.Label(new Rect(40, 40, 800, 40), "You won. Congrats. Now shoo.");
    }
}

    IEnumerator ResetLevel()
    {
        player.enabled = false;
        //play death animation
        yield return new WaitForSeconds(4f);
        player.enabled = true;
        textEnabled = false;
        SceneManager.LoadScene(levelName);
    }
}