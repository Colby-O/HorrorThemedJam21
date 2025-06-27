using System;
using UnityEngine;

namespace HTJ21
{
    public class SwitchBloodNumbers : MonoBehaviour
    {
        [SerializeField] private string _number = "232";

        [SerializeField] private Sprite[] _numberSprites;
        private void Start()
        {
            foreach (Transform group in transform)
            {
                foreach (Transform number in group)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Transform digit = number.GetChild(i);
                        SpriteRenderer sr = digit.GetComponent<SpriteRenderer>();
                        sr.sprite = _numberSprites[(int)char.GetNumericValue(_number[i])];
                    }
                }
            }
        }
    }
}
