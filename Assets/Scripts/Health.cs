using UnityEngine;
public class Health : MonoBehaviour
{
    public float max = 100, cur = 100; public System.Action onDead;
    public void Apply(float dmg) { cur -= dmg; if (cur <= 0) { onDead?.Invoke(); Destroy(gameObject); } }
}
