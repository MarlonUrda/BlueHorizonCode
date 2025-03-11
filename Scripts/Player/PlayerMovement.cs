using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public PlayerMovementStats movementStats;
    [SerializeField] private Collider2D _feetCollider;
    [SerializeField] private Collider2D _bodyCollider;

    private Rigidbody2D _rb;
    private Animator _anim;
    public AudioManager _audioManager;
    public AudioClip _jumpSound;
    public AudioClip _walkSound;
    public AudioClip _hurtSound;

    private string currentAnimation;

    // Variables de movimiento
    private Vector2 _movementVelocity;
    private bool _isFacingRight;
    private bool _canMove = true;

    // Variables para el chequeo de colisiones
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private bool _isGrounded;
    private bool _bumpedHead;

    //Variables para el salto
    public float VerticalVelocity { get; private set; }
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private int _jumpsUsed;
    private bool _canJump;

    // Variables de Apex
    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;

    // Jump Buffer
    private float _jumpBufferTimer;
    private bool _jumpReleaseDuringBuffer;

    // Variables del salto de coyote
    private float _coyoteTimer;

    const string IDLE = "Idle";
    const string WALK = "Walk";
    const string RUN = "Run";
    const string JUMP = "Jump";
    const string HURT = "Hurt";
    const string DEATH = "Dead";

    // Al momento de entrar en escena se captura el componente Rigidbody2D
    private void Awake()
    {
        _isFacingRight = true;

        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
    }

    private void Update()
    {
        CountTimers();
        JumpChecks();
    }

    private void FixedUpdate()
    {
        CollisionsCheck();
        Jump();
        if(_isGrounded)
        {
            Move(movementStats.GroundAcceleration, movementStats.GroundDeceleration, InputManager.Movement);
        } else
        {
            Move(movementStats.AirAcceleration, movementStats.AirDeceleration, InputManager.Movement); 
        }
    }

    #region Movement

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if(!_canMove) return;

        if (moveInput != Vector2.zero)
        {
            // Chequea si el jugador puede girar
            TurnCheck(moveInput);

            Vector2 targetVelocity = Vector2.zero;

            if(InputManager.RunIsHeld)
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * movementStats.runSpeed;
                ChangeAnimation(RUN);
            } else
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * movementStats.walkSpeed;
                ChangeAnimation(WALK);
            }

            _movementVelocity = Vector2.Lerp(_movementVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            _rb.velocity = new Vector2(_movementVelocity.x, _rb.velocity.y);
        }

        else if (moveInput == Vector2.zero)
        {
            _movementVelocity = Vector2.Lerp(_movementVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            _rb.velocity = new Vector2(_movementVelocity.x, _rb.velocity.y);
            ChangeAnimation(IDLE);
        }
    }

    private void TurnCheck(Vector2 moveInput)
    {
        if(_isFacingRight && moveInput.x < 0)
        {
            Turn(false);
        }
        else if (!_isFacingRight && moveInput.x > 0)
        {
            Turn(true);
        }
    }

    private void Turn(bool tunrRight)
    {
        if(tunrRight)
        {
            _isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        } else
        {
            _isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
    }

    #endregion

    #region Jump
    private void JumpChecks()
    {
        //Al momento de presionar el boton de salto
        if(InputManager.JumpWasPressed)
        {
            _jumpBufferTimer = movementStats.JumpBufferTime;
            _jumpReleaseDuringBuffer = false;
        }
        //Cuando soltamos el boton de salto
        if(InputManager.JumpWasReleased)
        {
            if(_jumpBufferTimer > 0f)
            {
                _jumpReleaseDuringBuffer = true;
            }

            if(_isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = movementStats.TimeUpwardsCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }
        //Iniciar salto considerando el buffer de salto y el intervalo de coyote
        if(_jumpBufferTimer > 0f && !_isJumping && (_isGrounded || _coyoteTimer > 0f))
        {
            if (_canJump)
            {
                InitiateJump(1);
                _audioManager.PlaySound(_jumpSound);
                ChangeAnimation(JUMP);

                if (_jumpReleaseDuringBuffer)
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }
        //Doble salto
        else if(_jumpBufferTimer > 0f && _isJumping && _jumpsUsed < movementStats.NumberOfJumpsAllowed)
        {
            if (_canJump)
            {
                _isFastFalling = false;
                InitiateJump(1);
                _audioManager.PlaySound(_jumpSound);
                ChangeAnimation(JUMP);
            }
        }
        //Salto aereo despues de que el intervalo de coyote haya pasado
        else if (_jumpBufferTimer > 0f && _isFalling && _jumpsUsed < movementStats.NumberOfJumpsAllowed - 1)
        {
            if (_canJump)
            {
                InitiateJump(2);
                _isFastFalling = false;
                _audioManager.PlaySound(_jumpSound);
                ChangeAnimation(JUMP);
            }
            
        }
        //Aterrizar
        if((_isJumping || _isFalling) && VerticalVelocity <= 0f)
        {
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;
            _jumpsUsed = 0;
            VerticalVelocity = Physics2D.gravity.y;
            _canJump = true;
        }
    }

    private void InitiateJump(int numberOfJumpsUsed)
    {
        if(!_isJumping)
        {
            _isJumping = true;
        }

        _jumpBufferTimer = 0f;
        _jumpsUsed += numberOfJumpsUsed;
        VerticalVelocity = movementStats.InitialJumpVelocity;

        if(_jumpsUsed >= movementStats.NumberOfJumpsAllowed)
        {
            _canJump = false;
        }
    }

    private void Jump()
    {
        //Aplicar gravedad
        if(_isJumping)
        {
            //Chequear rebote con la cabeza
            if (_bumpedHead)
            {
                _isFastFalling = true;
            }

            //Gravedad ascendiendo
            if (VerticalVelocity >= 0f)
            {
                //Control de Apex
                _apexPoint = Mathf.InverseLerp(movementStats.InitialJumpVelocity, 0f, VerticalVelocity);

                if (_apexPoint >= movementStats.ApexTrheshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0f;
                    }

                    if (_isPastApexThreshold)
                    {
                        _timePastApexThreshold += Time.fixedDeltaTime;
                        if (_timePastApexThreshold < movementStats.ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        }
                        else
                        {
                            VerticalVelocity -= 0.01f;
                        }
                    }
                }

                //Gravedad ascendiendo pero sin haber pasado el limite (Apex)
                else
                {
                    VerticalVelocity += movementStats.Gravity * Time.fixedDeltaTime;
                    if (_isPastApexThreshold)
                    {
                        _isPastApexThreshold = false;
                    }
                }
            }
            //Gravedad descendiendo
            else if (!_isFastFalling)
            {
                VerticalVelocity += movementStats.Gravity * movementStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }

            else if (VerticalVelocity < 0f)
            {
                if (!_isFalling)
                {
                    _isFalling = true;
                }
            }
        }

        //Jump cut
        if(_isFastFalling)
        {
            if(_fastFallTime == movementStats.TimeUpwardsCancel)
            {
                VerticalVelocity += movementStats.Gravity * movementStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (_fastFallTime < movementStats.TimeUpwardsCancel)
            {
                VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, (_fastFallTime / movementStats.TimeUpwardsCancel));
            }
            _fastFallTime += Time.fixedDeltaTime;
        }

        //Gravedad en caida normal
        if (!_isGrounded && !_isJumping)
        {
            if(!_isFalling)
            {
                _isFalling = true;
            }

            VerticalVelocity += movementStats.Gravity * Time.fixedDeltaTime;
        }
        //Restringir la velocidad de caida
        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -movementStats.MaxFallSpeed, 50f);

        _rb.velocity = new Vector2(_rb.velocity.x, VerticalVelocity);
    }
    #endregion

    #region Timers
    private void CountTimers()
    {
        _jumpBufferTimer -= Time.deltaTime;

        if (!_isGrounded)
        {
            _coyoteTimer -= Time.deltaTime;
        }
        else
        {
            _coyoteTimer = movementStats.JumpCoyoteTime;
        }
    }
    #endregion

    #region Collision Checks

    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(_feetCollider.bounds.center.x, _feetCollider.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_feetCollider.bounds.size.x, movementStats.groundCheckDistance);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, movementStats.groundCheckDistance, movementStats.groundLayer);

        if (_groundHit.collider != null)
        {
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }
    }

    private void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(_feetCollider.bounds.center.x, _bodyCollider.bounds.max.y);
        Vector2 boxCastSize = new Vector2(_feetCollider.bounds.size.x * movementStats.HeadWidth, movementStats.HeadDetectionLength);

        _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, movementStats.HeadDetectionLength, movementStats.groundLayer);

        if(_headHit.collider != null)
        {
            _bumpedHead = true;
        }
        else
        {
            _bumpedHead = false;
        }
    }

    private void CollisionsCheck()
    {
        IsGrounded();
        BumpedHead();
    }

    #endregion

    #region Hit
    public void ApplyForce()
    {
        _canMove = false;

        VerticalVelocity = movementStats.InitialJumpVelocity;
        _rb.velocity = new Vector2(_rb.velocity.x, 2.5f);

        StartCoroutine(CanMoveAgain());
    }

    public void hitByEnemy()
    {
        _canMove = false;

        if(_rb.velocity.x > 0)
        {
            _audioManager.PlaySound(_hurtSound);
            ChangeAnimation(HURT);
            _rb.velocity = new Vector2(_rb.velocity.x, 1f);
        } else
        {
            _audioManager.PlaySound(_hurtSound);
            ChangeAnimation(HURT);
            _rb.velocity = new Vector2(_rb.velocity.x, 1f);
        }

        StartCoroutine(CanMoveAgain());

    }

    public void End()
    {
        StartCoroutine(PlayerDie());
        SceneManager.LoadScene("GameOver");
    }

    private IEnumerator PlayerDie()
    {
        Debug.Log("Corrutina de muerte iniciada");
        _rb.velocity = Vector2.zero;
        ChangeAnimation(DEATH);

        yield return new WaitForSeconds(_anim.GetCurrentAnimatorStateInfo(0).length);

        Destroy(gameObject);
    }

    private IEnumerator CanMoveAgain()
    {
        yield return new WaitForSeconds(0.1f);

        while (!_isGrounded)
        {
            yield return null;
        }

        _canMove = true;
    }
    #endregion

    #region Animation
    private void ChangeAnimation(string newAnimation)
    {
        Debug.Log(newAnimation);
        if(currentAnimation == newAnimation) return;

        _anim.Play(newAnimation);
        currentAnimation = newAnimation;
    }
    #endregion
}
