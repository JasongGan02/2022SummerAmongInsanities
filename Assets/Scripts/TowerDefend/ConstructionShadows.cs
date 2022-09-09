using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionShadows : MonoBehaviour
{
    [SerializeField]
    private GameObject m_tarTower;
    private uint m_collisionCount = 0;
    private float m_placeDistance = 0;
    private SpriteRenderer m_shadowSpriteRenderer;
    private Color m_originalColor;
    private bool m_placeable = false;
    void Start()
    {
        m_shadowSpriteRenderer = GetComponent<SpriteRenderer>();
        m_originalColor = m_shadowSpriteRenderer.color;
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlaceStatus();
    }

    public void StartUp(float placeDistance){
        m_placeDistance = placeDistance;
    }
    
    public bool GetPlaceStatus(){
        return m_placeable;
    }

    public void PlaceTower(){
        Instantiate(m_tarTower, transform.position, transform.rotation);
    }

    private void UpdatePlaceStatus(){
        float distance = Vector3.Distance(transform.position, GameObject.FindWithTag("Player").transform.position);
        if(distance <= m_placeDistance && m_collisionCount == 0){
            m_shadowSpriteRenderer.color = new Color(m_originalColor.r/2, m_originalColor.g, m_originalColor.b/2, m_originalColor.a);
            m_placeable = true;
        }else{
            m_shadowSpriteRenderer.color = new Color(m_originalColor.r, m_originalColor.g/2, m_originalColor.b/2, m_originalColor.a);
            m_placeable = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        ++m_collisionCount;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        --m_collisionCount;
    }
}
