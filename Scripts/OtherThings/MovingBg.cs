using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBg : MonoBehaviour
{
    [SerializeField] private Vector2 _speedMovement;
    private Vector2 _offset;
    private Material _material;

    private Rigidbody2D _playerRb;

    private void Awake()
    {
        _material = GetComponent<SpriteRenderer>().material;
        _playerRb = GameObject.FindGameObjectWithTag("Finish").GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if(_playerRb == null) return;

        _offset = (_playerRb.velocity.x * 0.1f) * Time.deltaTime * _speedMovement;
        _material.mainTextureOffset += _offset;
    }
}
