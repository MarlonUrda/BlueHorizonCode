using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int CoinsAmount { get { return _coinsAmount;  } }
    private int _coinsAmount;

    public HUD _hud;

    public int lives = 3;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.Log("There is already a GameManager instance in the scene");
        }

    }

    public void AddPoint()
    {
        _coinsAmount++;
        _hud.UpdatePoints(CoinsAmount);
    }

    public void RemoveLife()
    {
        lives--;
        _hud.DeactivateLife(lives);
    }
}
