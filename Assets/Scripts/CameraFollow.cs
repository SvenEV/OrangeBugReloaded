using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Vector3 _targetPosition;

    public Vector3 distanceToTarget;

    public List<Transform> Targets = new List<Transform>();

    private void Start()
    {

    }

    private void LateUpdate()
    {
        if (Targets.Any())
        {
            var minX = Targets.Min(p => p.position.x);
            var maxX = Targets.Max(p => p.position.x);
            var minY = Targets.Min(p => p.position.y);
            var maxY = Targets.Max(p => p.position.y);

            _targetPosition = new Vector3(
                Mathf.Lerp(minX, maxX, .5f),
                Mathf.Lerp(minY, maxY, .5f), 0);

            transform.position = Vector3.Lerp(
                transform.position,
                _targetPosition + distanceToTarget, 2 * Time.deltaTime);
        }
    }
}
