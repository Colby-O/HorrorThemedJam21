using UnityEngine;

namespace HTJ21
{
    public class ShowInRange : MonoBehaviour
    {
        [SerializeField] private GameObject _obj;
        [SerializeField] private float _dstMax;
        [SerializeField] private float _dstMin;

        private void Update()
        {
            if (!_obj || !HTJ21GameManager.CurrentControllable || HTJ21GameManager.IsPaused) return;
            float dst = Vector3.Distance(HTJ21GameManager.CurrentControllable.transform.position, transform.position);
            _obj.gameObject.SetActive(dst < _dstMax && dst > _dstMin);
        }
    }
}
