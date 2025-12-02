using System;
using System.Collections.Generic;
using UnityEngine;


public enum BTState
{
    Success,
    Failure,
    Running
}

public abstract class BTNode
{
    public abstract BTState Tick();
}

public class BTSelector : BTNode
{
    private readonly List<BTNode> _children;

    public BTSelector(params BTNode[] children)
    {
        _children = new List<BTNode>(children);
    }

    public override BTState Tick()
    {
        foreach (var child in _children)
        {
            var state = child.Tick();
            if (state == BTState.Success || state == BTState.Running)
                return state;
        }
        return BTState.Failure;
    }
}

public class BTSequence : BTNode
{
    private readonly List<BTNode> _children;

    public BTSequence(params BTNode[] children)
    {
        _children = new List<BTNode>(children);
    }

    public override BTState Tick()
    {
        bool anyRunning = false;

        foreach (var child in _children)
        {
            var state = child.Tick();
            if (state == BTState.Failure)
                return BTState.Failure;
            if (state == BTState.Running)
                anyRunning = true;
        }

        return anyRunning ? BTState.Running : BTState.Success;
    }
}

public class BTCondition : BTNode
{
    private readonly Func<bool> _predicate;

    public BTCondition(Func<bool> predicate)
    {
        _predicate = predicate;
    }

    public override BTState Tick()
    {
        return _predicate() ? BTState.Success : BTState.Failure;
    }
}

public class BTAction : BTNode
{
    private readonly Func<BTState> _action;

    public BTAction(Func<BTState> action)
    {
        _action = action;
    }

    public override BTState Tick()
    {
        return _action();
    }
}


public class BTRewardFairy : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform visualRoot;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float fleeSpeed = 3f;
    public float detectionRadius = 10f;
    public float comfortableDistance = 5f;
    public float wanderRadius = 10f;
    public float wanderInterval = 3f;

    [Header("Pickup Effect")]
    public int healAmount = 30;
    public bool destroyOnPickup = true;

    [Header("Bounds")]
    public Transform centerPoint;
    public float maxDistanceFromCenter = 20f;

    private Vector3 _center;
    private Vector3 _currentTarget;
    private float _wanderTimer;
    private BTNode _root;

    private void Start()
    {
        if (centerPoint != null)
            _center = centerPoint.position;
        else
            _center = transform.position;

        PickNewWanderTarget();

        var fleeSequence = new BTSequence(
            new BTCondition(PlayerInRange),
            new BTAction(FleeFromPlayer)
        );

        var wanderAction = new BTAction(WanderAroundCenter);

        _root = new BTSelector(fleeSequence, wanderAction);
    }

    private void Update()
    {
        if (_root != null)
        {
            _root.Tick();
        }
    }


    private bool PlayerInRange()
    {
        if (player == null) return false;
        float dist = Vector3.Distance(transform.position, player.position);
        return dist < detectionRadius;
    }


    private BTState FleeFromPlayer()
    {
        if (player == null) return BTState.Failure;

        Vector3 toPlayer = transform.position - player.position;
        toPlayer.y = 0f;
        float sqrDist = toPlayer.sqrMagnitude;
        if (sqrDist < 0.001f)
        {
            toPlayer = transform.right;
        }

        float dist = Mathf.Sqrt(sqrDist);

        if (dist > comfortableDistance)
        {
            return BTState.Success;
        }

        Vector3 fleeDir = toPlayer.normalized;

        Vector3 side = Vector3.Cross(Vector3.up, fleeDir);
        fleeDir = (fleeDir * 0.8f + side * 0.2f).normalized;

        MoveInDirection(fleeDir, fleeSpeed);

        return BTState.Running;
    }


    private BTState WanderAroundCenter()
    {
        _wanderTimer -= Time.deltaTime;

        float distFromCenter = Vector3.Distance(transform.position, _center);
        if (distFromCenter > maxDistanceFromCenter)
        {
            _currentTarget = _center;
            _wanderTimer = wanderInterval;
        }
        else if (_wanderTimer <= 0f || ReachedTarget(_currentTarget))
        {
            PickNewWanderTarget();
        }

        Vector3 toTarget = _currentTarget - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.001f)
            return BTState.Success;

        MoveInDirection(toTarget.normalized, moveSpeed);
        return BTState.Running;
    }

    private void PickNewWanderTarget()
    {
        Vector2 rand = UnityEngine.Random.insideUnitCircle * wanderRadius;
        _currentTarget = _center + new Vector3(rand.x, 0f, rand.y);
        _wanderTimer = wanderInterval;
    }

    private bool ReachedTarget(Vector3 target)
    {
        Vector3 diff = target - transform.position;
        diff.y = 0f;
        return diff.sqrMagnitude < 0.5f * 0.5f;
    }


    private void MoveInDirection(Vector3 dir, float speed)
    {
        if (dir.sqrMagnitude < 0.001f) return;

        Vector3 step = dir.normalized * speed * Time.deltaTime;
        transform.position += step;

        if (visualRoot != null)
        {
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
                visualRoot.rotation = Quaternion.Slerp(visualRoot.rotation, look, 10f * Time.deltaTime);
            }
        }
        else
        {
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, 10f * Time.deltaTime);
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var hp = other.GetComponent<Health>();
        if (hp != null && !hp.IsEnemy)
        {
            hp.Heal(healAmount);
        }

        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 c = (centerPoint != null) ? centerPoint.position : transform.position;
        Gizmos.DrawWireSphere(c, wanderRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(c, maxDistanceFromCenter);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
#endif
}