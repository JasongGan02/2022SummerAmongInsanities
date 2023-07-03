using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class medicine : MonoBehaviour
{

    GameObject player;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");

        if (Vector2.Distance(player.transform.position, this.transform.position) < 1f)
        {
            player.GetComponent<PlayerController>().Heal(10 * Time.deltaTime);
        }
    }
}
