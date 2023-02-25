using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraFollow : MonoBehaviour
{
    private GameObject player;
    public float Ahead;//当角色向右移动时，摄像机比任务位置领先，当角色向左移动时，摄像机比角色落后
    public Vector3 Targetpos;//摄像机的最终目标
    public float smooth;//摄像机平滑移动的值

    // Update is called once per frame
    void Update()
    {
        if(player == null)
        {
            player = GameObject.FindWithTag("Player");
        }
        else{
            transform.position = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z-1);
            Targetpos = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);
            if (player.transform.localScale.x > 0)
            {
                Targetpos = new Vector3(player.transform.position.x + Ahead, transform.position.y, transform.position.z);
            }
            if (player.transform.localScale.x < 0)
            {
                Targetpos = new Vector3(player.transform.position.x - Ahead, transform.position.y, transform.position.z);
            }
            //让摄像机进行平滑的移动
            transform.position = Vector3.Lerp(transform.position, Targetpos, smooth);
        }
    }
}
