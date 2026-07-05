using UnityEngine;

public class PersistentPlayer : MonoBehaviour
{
    private static PersistentPlayer instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject); // já existe um player, não duplica
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}