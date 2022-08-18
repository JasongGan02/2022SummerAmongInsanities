using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public int range = 1;
    private Animator animator;

    [Header("hold to interact setting")]
    public float waitTime = 1.0f;
    private float timeStamp = float.MaxValue;
    private GameObject targetObject;

    private delegate void TimerCompleteHandler();

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

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
        animator.SetBool("MeleeTool", false);
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseDownPosition = GetMousePosition2D();
            if (Vector2.Distance(mouseDownPosition, transform.position) > range) return;

            RaycastHit2D clickHit = Physics2D.Raycast(mouseDownPosition, Vector2.zero);
            if (clickHit.transform != null)
            {
                targetObject = GetTargetObject(clickHit, mouseDownPosition);
                StartTimer();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            ResetTimer();
        }

        if (IsTimerCompleted())
        {
            ClickOnGameObject(targetObject);

            Vector2 mouseDownPosition = GetMousePosition2D();
            if (Vector2.Distance(mouseDownPosition, transform.position) <= range)
            {
                RaycastHit2D clickHit = Physics2D.Raycast(mouseDownPosition, Vector2.zero);
                if (clickHit.transform != null)
                {
                    targetObject = GetTargetObject(clickHit, mouseDownPosition);
                    StartTimer();
                }
            }       
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
            // if yes, then break the closest terrain tile to the player
            Vector2 direction = mouseDownPosition - new Vector2(transform.position.x, transform.position.y);

            // TODO: check why using ground layerMask doesn't work here
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, range);

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
            return closestObject;
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
        animator.SetBool("MeleeTool", true);

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
