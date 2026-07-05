using UnityEngine;

public class ScenePlayerPositioner : MonoBehaviour
{
    void Start()
    {
        var spawnPoint = GameObject.FindWithTag("SpawnPoint"); // ou GameObject.Find("SpawnPoint")
        if (spawnPoint == null)
        {
            Debug.LogWarning("Nenhum SpawnPoint encontrado nesta cena.");
            return;
        }

        var player = GameObject.FindWithTag("Player"); // a raiz do seu XR Origin
        if (player == null) return;

        player.transform.SetPositionAndRotation(spawnPoint.transform.position, spawnPoint.transform.rotation);
    }
}