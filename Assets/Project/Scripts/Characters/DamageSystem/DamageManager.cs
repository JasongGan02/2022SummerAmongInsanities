using UnityEngine;

public class DamageManager : MonoBehaviour
{
    private static DamageManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // ��֤�ö����ڳ����л�ʱ���ᱻ����
        }
        else
        {
            Destroy(gameObject);  // ��ֻ֤����һ�� GameManager
        }
    }

    // ���������ܿ��������ﶨ��
}
