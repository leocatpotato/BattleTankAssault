using UnityEngine;

public class Shell : MonoBehaviour
{
    public float life = 10f;
    public float damage = 40f;
    public float blastRadius = 3.5f;

    void Start()
    {
        Destroy(gameObject, life);
    }

    void OnCollisionEnter(Collision c)
    {
        foreach (var col in Physics.OverlapSphere(transform.position, blastRadius))
        {
            if (col.TryGetComponent<Health>(out var h))
            {
                h.Apply(damage);
            }
        }

        Destroy(gameObject);
    }
}