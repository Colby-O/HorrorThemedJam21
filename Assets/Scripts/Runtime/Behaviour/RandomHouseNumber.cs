using UnityEngine;

namespace HTJ21
{
    public class RandomHouseNumber : MonoBehaviour
    {
        void Start()
        {
            TMPro.TMP_Text text = transform.Find("HouseNumber").GetComponent<TMPro.TMP_Text>();
            int number;
            do
            {
                number = Random.Range(0, 100);
            } while (number == 41);

            text.text = number.ToString();
        }
    }
}
