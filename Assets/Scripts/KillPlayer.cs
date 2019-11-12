using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KillPlayer : MonoBehaviour
{

    public PlayerController player;
    public string levelName;
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
            Debug.Log("Dead.");
            StartCoroutine(ResetLevel());
        }
    }

    IEnumerator ResetLevel()
    {
        player.enabled = false;
        //play death animation
        yield return new WaitForSeconds(0.5f);
        player.enabled = true;
        SceneManager.LoadScene(levelName);
    }
}