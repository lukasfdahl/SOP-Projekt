using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Demo : MonoBehaviour
{
    public PlayerController player;

    public List<GameObject> points;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            player.Respawn(points[0].transform.position);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            player.Respawn(points[1].transform.position);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            player.Respawn(points[2].transform.position);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            player.Respawn(points[3].transform.position);
        }

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            SceneManager.LoadScene(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            SceneManager.LoadScene(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SceneManager.LoadScene(2);
        }
    }
}
