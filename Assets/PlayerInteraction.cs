using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public int range = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        BreakTile();
    }

    private void BreakTile()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            if (Vector2.Distance(mousePos2D, transform.position) < range)
            {
                RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

                if (hit.transform != null)
                {
                    // Temporary
                    // Will remove when we use prefabs for all tile classes
                    BreakableTile target = hit.transform.gameObject.GetComponent<BreakableTile>();
                    if (target != null)
                    {
                        target.OnClicked();
                    }
                    else
                    {
                        Destroy(hit.transform.gameObject);
                    }
                }
            }
        }
    }
}