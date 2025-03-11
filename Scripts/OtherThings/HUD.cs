using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    public TextMeshProUGUI CoinsText;
    public GameObject[] lives;

    private void Update()
    {
        CoinsText.text = GameManager.Instance.CoinsAmount.ToString();
    }

    public void UpdatePoints(int val)
    {
        CoinsText.text = val.ToString();
    }

    public void DeactivateLife(int index)
    {
        lives[index].SetActive(false);
    }

    public void ActivateLife(int index)
    {
        lives[index].SetActive(true);
    }
}
