using UnityEngine;

public class PlayerMovementLimiter : MonoBehaviour
{
    [SerializeField] private BoxCollider boundsCollider;

    private Bounds bounds;

    private void Start()
    {
        if (boundsCollider != null)
            bounds = boundsCollider.bounds;
    }

    private void LateUpdate()
    {
        if (boundsCollider == null) return;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, bounds.min.x, bounds.max.x);
        pos.y = Mathf.Clamp(pos.y, bounds.min.y, bounds.max.y);
        pos.z = Mathf.Clamp(pos.z, bounds.min.z, bounds.max.z);
        if (transform.position!=pos)
        {
            transform.position = pos;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (boundsCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(boundsCollider.bounds.center, boundsCollider.bounds.size);
        }
    }
#endif
}
