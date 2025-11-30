using UnityEngine;

public class Shell : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 40;

    [Header("Lifetime")]
    [SerializeField] private float lifeTime = 10f;

    [Header("Side")]
    [SerializeField] private bool isEnemyBullet = false;

    private Transform ownerRoot;

    public void Init(bool isEnemyBullet, Transform ownerRoot)
    {
        this.isEnemyBullet = isEnemyBullet;
        this.ownerRoot = ownerRoot;
    }

    private void OnEnable()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ownerRoot != null && other.transform.root == ownerRoot)
            return;

        Health targetHealth = other.GetComponent<Health>();
        if (targetHealth == null)
            targetHealth = other.GetComponentInParent<Health>();

        if (targetHealth == null)
            return;

        targetHealth.TakeDamage(damage, isEnemyBullet);

        Destroy(gameObject);
    }
}