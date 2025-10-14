using UnityEngine;
public class Shell : MonoBehaviour
{
    public float life = 3f; public float damage = 40f; public float blastRadius = 3.5f; public float blastForce = 900f;
    void Start() { Destroy(gameObject, life); }
    void OnCollisionEnter(Collision c)
    {
        foreach (var col in Physics.OverlapSphere(transform.position, blastRadius))
        {
            if (col.attachedRigidbody)
            {
                col.attachedRigidbody.AddExplosionForce(blastForce, transform.position, blastRadius, 0.5f, ForceMode.Impulse);
            }
            if (col.TryGetComponent<Health>(out var h)) { h.Apply(damage); }
        }
        Destroy(gameObject);
    }
}
