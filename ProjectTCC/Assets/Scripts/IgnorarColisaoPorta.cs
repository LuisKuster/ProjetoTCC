using UnityEngine;

public class IgnorarColisaoPorta : MonoBehaviour
{
    [SerializeField] Transform porta;
    [SerializeField] Transform moldura;

    void Awake()
    {
        var colsPorta = porta.GetComponentsInChildren<Collider>();
        var colsMoldura = moldura.GetComponentsInChildren<Collider>();

        foreach (var p in colsPorta)
            foreach (var m in colsMoldura)
                Physics.IgnoreCollision(p, m, true);
    }
}