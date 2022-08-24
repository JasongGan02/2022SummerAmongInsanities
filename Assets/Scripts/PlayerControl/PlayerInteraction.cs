using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactRange = 1f;
    public float pickUpRange = 1f;
    public LayerMask resourceLayer;
    public LayerMask groundLayer;
    private Animator animator;

    [Header("hold to interact setting")]
    public float waitTime = 1.0f;
    private float timeStamp = float.MaxValue;
    private GameObject targetObject;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        PickUpResource();
        BreakTile();
    }

    private void StartTimer()
    {
        timeStamp = Time.time + waitTime;
    }

    private void ResetTimer()
    {
        timeStamp = Mathf.Infinity;
    }

    private bool IsTimerCompleted()
    {
        return Time.time >= timeStamp;
    }

    private void BreakTile()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 mouseDownPosition = GetMousePosition2D();
            if (Vector2.Distance(mouseDownPosition, transform.position) > interactRange) return;

            RaycastHit2D clickHit = Physics2D.Raycast(mouseDownPosition, Vector2.zero);
            if (clickHit.transform != null)
            {
                if (clickHit.transform.gameObject.GetComponent<BreakableObject>() != null)
                {
                    animator.SetBool(Constants.Animator.MELEE_TOOL, true);
                    GameObject tempTargetObject = GetTargetObject(clickHit, mouseDownPosition);
                    if (tempTargetObject != null && tempTargetObject != targetObject)
                    {
                        targetObject = tempTargetObject;
                        StartTimer();
                    }
                }  
                
                
            } else
            {
                animator.SetBool(Constants.Animator.MELEE_TOOL, false);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            animator.SetBool(Constants.Animator.MELEE_TOOL, false);
            ResetTimer();
        }

        if (IsTimerCompleted())
        {
            ClickOnGameObject(targetObject);
        }
    }

    private void PickUpResource()
    {
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, pickUpRange, Vector2.zero, 0, resourceLayer);
        if (hit.transform != null)
        {
            ResourceObject resoureObject = hit.transform.gameObject.GetComponent<ResourceObject>();
            resoureObject.OnBeforePickedUp();
        }
    }

    private Vector2 GetMousePosition2D()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2(mousePos.x, mousePos.y);
    }

    private GameObject GetTargetObject(RaycastHit2D clickHit, Vector2 mouseDownPosition)
    {
        // Check if the clicked object is a terrain tile
        if (clickHit.transform.gameObject.layer == Constants.Layer.GROUND)
        {
            // if yes, then break the terrain tile if it's the closest one to the player
            Vector2 direction = mouseDownPosition - new Vector2(transform.position.x, transform.position.y);

            // TODO: check why using ground layerMask doesn't work here
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, interactRange, groundLayer);

            GameObject closestObject = null;
            float closestDistance = float.MaxValue;
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.transform == null) continue;

                GameObject potentialTarget = hit.transform.gameObject;

                float distance = Vector2.Distance(hit.transform.position, transform.position);
                if (closestDistance > distance)
                {
                    closestObject = potentialTarget;
                    closestDistance = distance;
                }
            }

            if (clickHit.transform.gameObject == closestObject)
            {
                return closestObject;
            }
            else
            {
                return null;
            }
        }
        else
        {
            if (clickHit.transform.gameObject.tag == Constants.Tag.RESOURCE) return null;
            // if it's not a terrain tile, then break the clicked object
            return clickHit.transform.gameObject;
        }
    }

    private void ClickOnGameObject(GameObject target)
    {
        if (target == null) return;

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
