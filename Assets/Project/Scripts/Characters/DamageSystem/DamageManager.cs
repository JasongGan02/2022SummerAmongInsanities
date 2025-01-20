using UnityEngine;

public class DamageManager : MonoBehaviour
{
    private static DamageManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // 保证该对象在场景切换时不会被销毁
        }
        else
        {
            Destroy(gameObject);  // 保证只存在一个 GameManager
        }
    }

    // 其他管理功能可以在这里定义
}
