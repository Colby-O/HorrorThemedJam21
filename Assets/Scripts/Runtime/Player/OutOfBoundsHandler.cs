using PlazmaGames.Attribute;
using PlazmaGames.Core;
using UnityEngine;

namespace HTJ21
{
    public class OutOfBoundsHandler : MonoBehaviour
    {
        [SerializeField] private float _verticalOffset;
        [SerializeField, ReadOnly] private GameObject _verticalBounds;


        private void Awake()
        {
            _verticalBounds = GameObject.FindWithTag("VerticalBounds");
        }

        private void LateUpdate()
        {
            if (_verticalBounds && transform.position.y < _verticalBounds.transform.position.y)
            {
                Vector3 newPos = GameManager.GetMonoSystem<IGPSMonoSystem>().GetClosestNodePositionToPoint(RoadwayHelper.GetRoadways(), transform.position);
                newPos.y += _verticalOffset;
                transform.position = newPos;
                transform.rotation = Quaternion.identity;

                if (TryGetComponent(out Rigidbody rb))
                {
                    rb.linearVelocity = Vector3.zero;
                }
            }
        }
    }
}
