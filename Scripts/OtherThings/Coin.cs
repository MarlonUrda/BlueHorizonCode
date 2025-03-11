using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    private string _currentAnimation;
    private Animator _anim;

    const string PICKUP = "Pick";

    public AudioClip _coinClip;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {

            GetComponent<Collider2D>().enabled = false;

            GameManager.Instance.AddPoint();
            AudioManager.Instance.PlaySound(_coinClip);
            ChangeAnimation(PICKUP);
            StartCoroutine(DestroyAfterAnimation());
        }
    }

    private void ChangeAnimation(string newAnimation)
    {
        if (_currentAnimation == newAnimation) return;

        Debug.Log("Changing animation to: " + newAnimation);

        _anim.Play(newAnimation);
        _currentAnimation = newAnimation;
    }

    private IEnumerator DestroyAfterAnimation()
    {
        // Espera hasta que la animación termine
        yield return new WaitForSeconds(_anim.GetCurrentAnimatorStateInfo(0).length);
        Destroy(gameObject);
    }
}
