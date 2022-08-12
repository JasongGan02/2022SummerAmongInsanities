using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public int range = 1;
    private Animator animator;
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        BreakTile();
    }

    private void BreakTile()
    {
        animator.SetBool("MeleeTool", false);
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetBool("MeleeTool", true);
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            if (Vector2.Distance(mousePos2D, transform.position) > range) return;

            // Check if the clicked object is a terrain tile
            RaycastHit2D clickHit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (clickHit.transform != null)
            {
                if (clickHit.transform.gameObject.layer == Constants.Layer.GROUND)
                {
                    // if yes, then break the closest terrain tile to the player
                    Vector2 direction = mousePos2D - new Vector2(transform.position.x, transform.position.y);

                    // TODO: check why using ground layerMask doesn't work here
                    RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, range);
                    Debug.Log(hits.Length);

                    GameObject closestObject = null;
                    float closestDistance = float.MaxValue;
                    foreach (RaycastHit2D hit in hits)
                {
                        if (hit.transform == null) continue;

                        GameObject potentialTarget = hit.transform.gameObject;
                        if (potentialTarget.layer != Constants.Layer.GROUND) continue;

                        float distance = Vector2.Distance(hit.transform.position, transform.position);
                        if (closestDistance > distance)
                    {
                            closestObject = potentialTarget;
                            closestDistance = distance;
                        }
                    }
                    ClickOnGameObject(closestObject);
                    }
                    else
                    {
                    // if it's not a terrain tile, then break the clicked object
                    ClickOnGameObject(clickHit.transform.gameObject);
                }
                    }
                }
            }

    private void ClickOnGameObject(GameObject target)
    {
        BreakableObject breakableTile = target.GetComponent<BreakableObject>();
        if (breakableTile != null)
        {
            breakableTile.OnClicked();
        }
        else
        {
            Destroy(target);
        }

    }
}
