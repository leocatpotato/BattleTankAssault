using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class TankController : MonoBehaviour
{
    public float accel = 16f;
    public float maxSpeed = 12f;
    public float turnSpeed = 70f;
    public float brakeDrag = 4f;
    Rigidbody rb; float h, v;
    void Awake() { rb = GetComponent<Rigidbody>(); rb.centerOfMass = new Vector3(0, -0.4f, 0); }
    void Update() { v = Input.GetAxisRaw("Vertical"); h = Input.GetAxisRaw("Horizontal"); }
    void FixedUpdate()
    {
        Vector3 fwd = transform.forward * v * accel;
        rb.AddForce(fwd, ForceMode.Acceleration);

        var vel = rb.linearVelocity; vel.y = 0;
        if (vel.magnitude > maxSpeed)
        {
            var capped = vel.normalized * maxSpeed; rb.linearVelocity = new Vector3(capped.x, rb.linearVelocity.y, capped.z);
        }

        float turn = h * turnSpeed * Mathf.Clamp01(vel.magnitude / (maxSpeed * 0.6f) + 0.4f);
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0, turn * Time.fixedDeltaTime, 0));

        rb.linearDamping = Mathf.Lerp(rb.linearDamping, Mathf.Abs(v) > 0.1f ? 0.5f : brakeDrag, 5f * Time.fixedDeltaTime);
    }
}
