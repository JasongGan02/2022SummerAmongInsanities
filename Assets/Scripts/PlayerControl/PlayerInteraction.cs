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
    public GameObject[] raycastStartingPoints;
    private float timeStamp = float.MaxValue;
    private GameObject targetObject;
    private Rigidbody2D rb;
    private Playermovement pm;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        pm = GetComponent<Playermovement>();
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
            if (Vector2.Distance(mouseDownPosition, transform.position) <= interactRange)
            {
                RaycastHit2D clickHit = Physics2D.Raycast(mouseDownPosition, Vector2.zero);
                if (clickHit.transform != null)
                {
                    GameObject tempTargetObject = clickHit.transform.gameObject;
                    if (tempTargetObject.GetComponent<BreakableObject>() != null && CanInteractWith(tempTargetObject, mouseDownPosition))
                    {
                        animator.SetBool(Constants.Animator.MELEE_TOOL, true);
                        if (clickHit.transform.gameObject != targetObject)
                        {
                            targetObject = tempTargetObject;
                            pm.excavateCoeff = 0.1f;
                            StartTimer();
                        }
                    }
                    else
                    {
                        
                        ResetMeleeAnimationAndTimer();
                    }
                }
                else
                {
                    
                    ResetMeleeAnimationAndTimer();
                }
            } 
            else
            {
                
                ResetMeleeAnimationAndTimer();
            }

            
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            ResetMeleeAnimationAndTimer();
        }

        if (IsTimerCompleted())
        {
            ClickOnGameObject(targetObject);
            StartTimer();
        }
    }

    private void ResetMeleeAnimationAndTimer()
    {
        targetObject = null;
        animator.SetBool(Constants.Animator.MELEE_TOOL, false);
        pm.excavateCoeff=1f;

        ResetTimer();
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

    private bool CanInteractWith(GameObject tempTargetObject, Vector2 mouseDownPosition)
    {
        if (tempTargetObject == null) return false;

        if (tempTargetObject.layer == Constants.Layer.GROUND)
        {
            foreach (GameObject raycastStartingPoint in raycastStartingPoints)
            {
                Vector2 direction = mouseDownPosition - new Vector2(raycastStartingPoint.transform.position.x, raycastStartingPoint.transform.position.y);
                RaycastHit2D hit = Physics2D.Raycast(raycastStartingPoint.transform.position, direction, interactRange, groundLayer);
                if (hit.transform != null && hit.transform.gameObject == tempTargetObject)
                {
                    return true;
                }
            }
            return false;
        }
        else
        {
            return !tempTargetObject.CompareTag(Constants.Tag.RESOURCE);
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
