using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Cinemachine;
using static UnityEngine.LightAnchor;

public class PlayerController : MonoBehaviour
{
    [Header("Player References")]
    PlayerInput playerInput;
    Rigidbody2D playerRb;
    [SerializeField] GameObject unarmedSprite;
    [SerializeField] GameObject gunSprite;
    [SerializeField] GameObject shotgunSprite;
    [SerializeField] GameObject gunEffectSprite;
    [SerializeField] GameObject shotgunEffectSprite;
    [SerializeField] GameObject reticle;
    [SerializeField] Transform aimPointPos;
    [SerializeField] Transform shootPointPos;
    [SerializeField] Transform cameraPointPos;
    [SerializeField] GameObject fireRateSprite;
    [SerializeField] GameObject damageSprite;
    [SerializeField] GameObject pierceSprite;
    Transform reticlePos;
    SpriteRenderer unarmedSpriteRenderer;
    SpriteRenderer gunSpriteRenderer;
    SpriteRenderer shotgunSpriteRenderer;
    SpriteRenderer gunEffectSpriteRenderer;
    SpriteRenderer shotgunEffectSpriteRenderer;
    Animator unarmedAnim;
    Animator gunAnim;
    Animator shotgunAnim;

    [Header("External References")]
    [SerializeField] GameObject gunBulletPrefab;
    [SerializeField] GameObject shotgunBulletPrefab;
    [SerializeField] CinemachineVirtualCamera virtualCamera;

    [Header("Input")]
    Vector2 moveInput;
    bool isGamepad;
    bool isRunning;
    bool isDodging;
    bool isDodgingPassive;
    bool canDodge;
    bool canShoot;
    public bool holdGun;
    public bool holdShotgun;
    float isShooting;
    [SerializeField] float cameraDistance;

    [Header("Player Stats")]
    [SerializeField] float speed;
    [SerializeField] float dodgeSpeed;
    [SerializeField] float dodgeSpeedPassive;
    [SerializeField] float dodgeDistance;
    [SerializeField] float dodgeCoolDown;
    [SerializeField] float gunFireRate;
    [SerializeField] float shotgunFireRate;
    [SerializeField] float reticleDistance;
    public bool gun;
    public bool shotgun;
    public bool death;
    bool damaged;
    bool jumpDamaged;
    bool invencible;
    public bool canInteract;
    public bool interacted;

    [Header("Variables temporales")]
    string triggerShootName;
    Animator shootAnim;
    SpriteRenderer tempSpriteRenderer;
    bool isJumping;
    bool fireRateAmplified;
    Vector2 jumpDirection;
    Vector2 ContactPoint;
    Color originalColor;

    void Start()
    {
        // Obtener las referencias necesarias al inicio
        playerInput = GetComponent<PlayerInput>();
        playerRb = GetComponent<Rigidbody2D>();
        reticlePos = reticle.GetComponent<Transform>();
        unarmedSpriteRenderer = unarmedSprite.GetComponent<SpriteRenderer>();
        gunSpriteRenderer = gunSprite.GetComponent<SpriteRenderer>();
        shotgunSpriteRenderer = shotgunSprite.GetComponent<SpriteRenderer>();
        gunEffectSpriteRenderer = gunEffectSprite.GetComponent<SpriteRenderer>();
        shotgunEffectSpriteRenderer = shotgunEffectSprite.GetComponent<SpriteRenderer>();
        unarmedAnim = unarmedSprite.GetComponent<Animator>();
        gunAnim = gunSprite.GetComponent<Animator>();
        shotgunAnim = shotgunSprite.GetComponent<Animator>();
        originalColor = unarmedSpriteRenderer.color;

        // Establecer que el jugador puede disparar y esquivar de incio
        canShoot = true;
        canDodge = true;
        damaged = false;

        GameManager.Instance.pierce = false;
        GameManager.Instance.damageAmplified = false;
    }

    void Update()
    {
        if (!death)
        {
            // Llamamiento a las funciones de seguimiento de la retícula y apuntar hacia el ratón
            if (!GameManager.Instance.onAnimation2)
            {
                ReticleFollow();
                MouseAim();
                // Llamamiento a función de disparo
                if (!GameManager.Instance.onAnimation) { Shoot(); }
                Damaged();
                HitBoxCoolDown();
            }
            else { aimPointPos.up = new Vector2(0 ,1); playerRb.velocity = Vector2.zero; }
        }
        else
        {
            // Llamamiento a la función de muerte del jugador, las funciones de control se inhabilitan durante la muerte
            Death();
        }

        // Llamamiento a la función para el sistema de animaciones
        AnimationSystem();
    }

    void FixedUpdate()
    {
        if (!death && !jumpDamaged && !GameManager.Instance.onAnimation && !GameManager.Instance.onAnimation2)
        {
            // Llamamiento a función de movimiento
            Move();
        }     
    }

    // Función para mover al jugador
    void Move()
    {
        if (!isDodging && !isDodgingPassive) 
        {
            // Cogemos los valores del input asignado a Move y con ellos movemos el RB del player según los valores recibidos y la velocidad
            moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
            playerRb.velocity = moveInput * speed;

            // Dependiendo de si nos movemos o no, le damos valor a isRunning lo cual servirá para iniciar las animaciones de correr si es verdadero
            if (moveInput.x == 0 && moveInput.y == 0) { isRunning = false; }
            else { isRunning = true; }
        }
    }

    // Función para esquivar
    public void Dodge(InputAction.CallbackContext context)
    {
        if (context.started && !death && !damaged && !GameManager.Instance.onAnimation2) 
        {
            // Solo podemos esquivar una vez hayamos encontrado la primera arma
            if ( (canDodge) && (holdGun || holdShotgun) )
            {
                canDodge = false;
                invencible = true;

                AudioManager.Instance.PlaySFX(8);

                // Damos de nuevo el control sobre el RB al movimiento pasado el tiempo configurado en dodgeDistance;
                Invoke(nameof(AllowMove), dodgeDistance);

                // Cogemos los valores del input asignado a Move ya que si nos movemos queremos esquivar hacia el mismo sentido del movimiento
                moveInput = playerInput.actions["Move"].ReadValue<Vector2>();

                // Si no nos estamos moviendo, el esquive será "pasivo" y se realizará hacia donde estemos apuntando
                if (moveInput == Vector2.zero) { isDodgingPassive = true; playerRb.AddForce(aimPointPos.transform.up * dodgeSpeedPassive); }

                // Si nos movemos, el esquive se realizará hacia la dirección en la que caminamos
                else { isDodging = true; playerRb.AddForce(moveInput * dodgeSpeed); }

                //Tiempo de enfriamiento del esquive
                Invoke(nameof(RestartDodge), dodgeCoolDown);
            }
        }
    }

    // Función para reiniciar la condición de poder esquivar cuando es llamada
    void RestartDodge()
    {
        canDodge = true;
    }

    // Función para volver a darle el control a Move sobre el RB y poder volver a disparar después de esquivar
    void AllowMove()
    {
        invencible = false;
        isDodging = false;
        isDodgingPassive = false;
    }

    // Función para apuntar con el ratón
    void MouseAim()
    {
        if (!isGamepad)
        {
            virtualCamera.Follow = cameraPointPos.transform;
            virtualCamera.m_Lens.OrthographicSize = 3.5f;

            // Obtener el input del ratón y transformarlo a posición en World
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(playerInput.actions["Aim"].ReadValue<Vector2>());

            // Calcular la dirección en la que apunta el ratón
            Vector2 direction = mousePos - (Vector2)aimPointPos.transform.position;

            // Rotar el apuntado del personaje para que su transform.up apunte en la dirección del ratón
            aimPointPos.transform.up = direction;
        }
    }

    // Función para apuntar con el joystick
    void JoystickAim()
    {
        if (isGamepad)
        {
            virtualCamera.Follow = cameraPointPos.transform;
            virtualCamera.m_Lens.OrthographicSize = cameraDistance;

            // Obtener el input del joystick
            Vector2 lookInput = playerInput.actions["Aim"].ReadValue<Vector2>();

            // Comprobación de que el input está recibiendo valores, si no recibe, no ejecuta el giro a 0 y se queda en la última posición en la que miramos
            if (lookInput != Vector2.zero)
            {
                // Calcular la dirección en la que apunta el joystick
                Vector2 direction = new Vector2(lookInput.x, lookInput.y).normalized;

                // Rotar el apuntado del personaje para que su transform.up apunte en la dirección del joystick
                aimPointPos.transform.up = direction;
            }
        }
    }

    // Función para hacer que la retícula siga al ratón o al giro del Joystick
    void ReticleFollow()
    {
        reticle.SetActive(true);

        if (!isGamepad)
        {
            // Si no se está usando un gamepad, la retícula sigue al ratón
            reticlePos.transform.SetParent(null);
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(playerInput.actions["Aim"].ReadValue<Vector2>());
            reticlePos.transform.position = mousePos;
        }
        else if (isGamepad)
        {
            // Si se está usando un gamepad, la retícula sigue al lookPoint con un offset de distancia ajustable hacia adelante 
            reticlePos.transform.SetParent(aimPointPos.transform);
            Vector2 reticleOffset = new Vector2(0, reticleDistance);
            reticlePos.transform.localPosition = reticleOffset;
            reticlePos.transform.up = new Vector2(1,0);
        }
    }

    // Función de disparo
    void Shoot()
    {
        // Obtenemos el input del disparo
        isShooting = playerInput.actions["Shoot"].ReadValue<float>();

        // Solo podremoos disparar si recibimos input de disparo y el player lleva arma
        if ((isShooting > 0) && (holdGun || holdShotgun))
        {
            // podremos disparar si se ha cumplido el tiempo de enfriamineto y no estemos en medio del esquive
            if (canShoot && !isDodging && !isDodgingPassive)
            {
                // Si usamos Gamepad activamos vibración de mando durante el disparo
                // if (isGamepad) { Gamepad.current.SetMotorSpeeds(0.25f, 0.25f); }

                canShoot = false;

                //La bala y la cadencia de tiro será distinta según el arma que usemos, instanciamos bala y establecemos la cadencia
                if (holdGun)
                {
                    AudioManager.Instance.PlaySFX(4);

                    Instantiate(gunBulletPrefab, shootPointPos.position, shootPointPos.rotation);
                    Invoke(nameof(RestartShoot), gunFireRate);
                    Invoke(nameof(ShootAnimOff), .5f);

                    // Activamos las animaciones de fogueo necesarias
                    gunEffectSprite.SetActive(true);
                    shootAnim = gunEffectSprite.GetComponent<Animator>();
                    shootAnim.SetTrigger(triggerShootName);
                }
                else if (holdShotgun)
                {
                    AudioManager.Instance.PlaySFX(0);

                    Instantiate(shotgunBulletPrefab, shootPointPos.position, shootPointPos.rotation);
                    Invoke(nameof(RestartShoot), shotgunFireRate);
                    Invoke(nameof(ShootAnimOff), .5f);

                    // Activamos las animaciones de fogueo necesarias
                    shotgunEffectSprite.SetActive(true);
                    shootAnim = shotgunEffectSprite.GetComponent<Animator>();
                    shootAnim.SetTrigger(triggerShootName);
                }
            }

            // Desactivamos la vibración cuando no disparamos
            //else { Gamepad.current.SetMotorSpeeds(0, 0); }
        }
    }

    // Función para reiniciar la condición de poder disparar cuando es llamada
    void RestartShoot()
    {
        canShoot = true;

        // El prefab de balas de escopeta es un empty que expulsa varias balas, una vez disparadas, el empty se desactivará en la jerarquía
        GameObject[] shotgunBulletsEmpty = GameObject.FindGameObjectsWithTag("ShotgunBullet");
        foreach (GameObject shotgunBulletEmpty in shotgunBulletsEmpty)
        {
            Destroy(shotgunBulletEmpty);
        }

        shootAnim = gunEffectSprite.GetComponent<Animator>();
        shootAnim.SetTrigger(triggerShootName);
    }

    void ShootAnimOff()
    {
        gunEffectSprite.SetActive(false);
        shotgunEffectSprite.SetActive(false);
    }

    // Función para cambiar entre armas 
    public void ChangeWeapon(InputAction.CallbackContext context)
    {
        if (context.started && !GameManager.Instance.onAnimation2)
        {
            if(!holdGun && !holdShotgun & gun) { holdGun = true; holdShotgun = false; }
            else if (holdGun && gun && shotgun) { holdGun = false; holdShotgun = true; }
            else if (holdShotgun && gun && shotgun) { holdGun = true; holdShotgun = false; }
        }
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            interacted = true;
        }
    }

    // Función para manejar cambios de dispositivo
    public void OnDeviceChange()
    {
        // Verificar si el control es mediante gamepad
        isGamepad = playerInput.currentControlScheme.Equals("Gamepad");
    }

    // Función para manejar el sistema de animaciones
    void AnimationSystem()
    {
        // Creamos variable string para almacenar el nombre de la animación que se ejecutará
        string triggerName = null;

        // Creamos bool para almacenar si queremos voltear o no el sprite dependiendo de si mira a izquierda o derecha
        bool flipOn = false;

        //Distinguimos si estamos muertos o esquivando mientras nos movemos ya que el sistema de animaciones será distinto al de movimiento/iddle

        if (!death)
        {
            if (!isDodging)
            {
                // Obtener las coordenadas de la mira
                float aimX = aimPointPos.transform.up.x;
                float aimY = aimPointPos.transform.up.y;

                // Sacamos el ángulo en el que apunta el vector de look, con Mathf.Atan2(X,Y) sacamos los radianes y multiplicándolo con Mathf.Rad2Deg lo convertimos a grados
                float aimAngle = Mathf.Atan2(aimX, aimY) * Mathf.Rad2Deg;

                // Verificar si el jugador está mirando hacia la derecha
                bool isFacingRight = (aimAngle > 0);

                // Establecer qué animación activar según el ángulo en el que apuntamos
                if (aimAngle >= -22.5 && aimAngle <= 22.5)
                {
                    // Animación Iddle/Run apuntando hacia arriba
                    triggerName = isRunning ? "runUp" : "iddleUp";
                    // Si hacemos un esquive sin movernos, se activa la animación de esquivar hacia arriba
                    if (isDodgingPassive) { triggerName = "dodgeUp"; }

                    // Animación de fogueo hacia arriba
                    triggerShootName = "gunUp";

                    //Cuando miramos arriba y abajo no volteamos sprites nunca
                    flipOn = false;
                }
                else if ((aimAngle > 22.5 && aimAngle <= 67.5) || (aimAngle < -22.5 && aimAngle >= -67.5))
                {
                    // Animación Iddle/Run apuntando diagonalmente hacia arriba
                    triggerName = isRunning ? "runSideUp" : "iddleSideUp";
                    // Si hacemos un esquive sin movernos, se activa la animación de esquivar diagonalmente hacia arriba
                    if (isDodgingPassive) { triggerName = "dodgeSideUp"; }

                    // Animación de fogueo hacia arriba diagonalmente
                    triggerShootName = "gunSideUp";

                    // Voltear el sprite si no está mirando hacia la derecha
                    flipOn = !isFacingRight;
                }
                else if ((aimAngle > 67.5 && aimAngle <= 112.5) || (aimAngle < -67.5 && aimAngle >= -112.5))
                {
                    // Animación Iddle/Run apuntando en horizontal
                    triggerName = isRunning ? "runSide" : "iddleSide";
                    // Si hacemos un esquive sin movernos, se activa la animación de esquivar en horizontal
                    if (isDodgingPassive) { triggerName = "dodgeSide"; }

                    // Animación de fogueo lateral
                    triggerShootName = "gunSide";

                    // Voltear el sprite si no está mirando hacia la derecha
                    flipOn = !isFacingRight;
                }
                else if ((aimAngle > 112.5 && aimAngle <= 157.5) || (aimAngle < -112.5 && aimAngle >= -157.5))
                {
                    // Animación Iddle/Run apuntando diagonalmente hacia abajo
                    triggerName = isRunning ? "runSideDown" : "iddleSideDown";
                    // Si hacemos un esquive sin movernos, se activa la animación de esquivar diagonalmente hacia abajo
                    if (isDodgingPassive) { triggerName = "dodgeSideDown"; }

                    // Animación de fogueo hacia abajo diagonalmente
                    triggerShootName = "gunSideDown";

                    // Voltear el sprite si no está mirando hacia la derecha
                    flipOn = !isFacingRight;
                }
                else
                {
                    // Animación Iddle/Run apuntando hacia abajo
                    triggerName = isRunning ? "runDown" : "iddleDown";
                    // Si hacemos un esquive sin movernos, se activa la animación de esquivar hacia abajo
                    if (isDodgingPassive) { triggerName = "dodgeDown"; }

                    // Animación de fogueo hacia abajo
                    triggerShootName = "gunDown";

                    //Cuando miramos arriba y abajo no volteamos sprites nunca
                    flipOn = false;
                }
            }
            else
            {
                // Sacamos el ángulo en el que apunta el input de movimiento, con Mathf.Atan2(X,Y) sacamos los radianes y multiplicándolo con Mathf.Rad2Deg lo convertimos a grados
                float inputAngle = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg;

                // Verificar si el jugador está yendo hacia la derecha
                bool isMovingRight = (moveInput.x > 0);

                // Establecer qué animación Dodge activar según el input que recibimos
                if (inputAngle >= -22.5 && inputAngle < 22.5)
                {
                    // Animación Dodge apuntando hacia arriba
                    triggerName = "dodgeUp";

                    //Cuando miramos arriba y abajo no volteamos sprites nunca
                    flipOn = false;
                }
                else if ((inputAngle >= 22.5 && inputAngle <= 67.5) || (inputAngle < -22.5 && inputAngle >= -67.5))
                {
                    // Animación Dodge apuntando diagonalmente hacia arriba
                    triggerName = "dodgeSideUp";

                    // Voltear el sprite si no está mirando hacia la derecha
                    flipOn = !isMovingRight;
                }
                else if ((inputAngle > 67.5 && inputAngle <= 112.5) || (inputAngle < -67.5 && inputAngle >= -112.5))
                {
                    // Animación Dodge apuntando en horizontal
                    triggerName = "dodgeSide";

                    // Voltear el sprite si no está mirando hacia la derecha
                    flipOn = !isMovingRight;
                }
                else if ((inputAngle > 112.5 && inputAngle <= 157.5) || (inputAngle < -112.5 && inputAngle >= -157.5))
                {
                    // Animación Dodge apuntando diagonalmente hacia abajo
                    triggerName = "dodgeSideDown";

                    // Voltear el sprite si no está mirando hacia la derecha
                    flipOn = !isMovingRight;
                }
                else
                {
                    // Animación Dodge apuntando hacia abajo
                    triggerName = "dodgeDown";

                    //Cuando miramos arriba y abajo no volteamos sprites nunca
                    flipOn = false;
                }
            }
        }
        else
        {
            // Si estamos muertos, se almacena la animación de muerte y no volteamos el sprite
            triggerName = "death";
            flipOn = false;
        }

        // Creamos variables temporales de Animator y SpriteRenderer para almacear los sprites y animaciones según si estamos desarmados o con distintas armas
        Animator tempAnim = null;
        tempSpriteRenderer = null;
        SpriteRenderer shootSpriteRender = null;

        // Dependiendo si estamos desarmados o con distintas armas activamos un sptrite u otro y desactivamos los demás
        if (!holdGun && !holdShotgun) 
        { 
            unarmedSprite.SetActive(true);
            gunSprite.SetActive(false);
            shotgunSprite.SetActive(false);
            tempAnim = unarmedAnim; tempSpriteRenderer = unarmedSpriteRenderer; 
        }
        else if (gun && holdGun && !holdShotgun) 
        {
            unarmedSprite.SetActive(false);
            gunSprite.SetActive(true);
            shotgunSprite.SetActive(false);
            tempAnim = gunAnim; tempSpriteRenderer = gunSpriteRenderer; shootSpriteRender = gunEffectSpriteRenderer;
        }
        else if (gun && shotgun && !holdGun && holdShotgun) 
        {
            unarmedSprite.SetActive(false);
            gunSprite.SetActive(false);
            shotgunSprite.SetActive(true);
            tempAnim = shotgunAnim; tempSpriteRenderer = shotgunSpriteRenderer; shootSpriteRender = shotgunEffectSpriteRenderer;
        }
        else if (gun && shotgun && holdGun && holdShotgun) 
        {
            holdGun = false; 
        }

        // Activar la animación correspondiente
        tempAnim.SetTrigger(triggerName);

        //Volteamos el sprite si se han dado las condiciones
        tempSpriteRenderer.flipX = flipOn;
        if (!canShoot && isShooting > 0) { shootSpriteRender.flipX = flipOn; }
    }
    
    private void Damaged()
    {
        if (jumpDamaged && !isJumping)
        {
            StartCoroutine(DamagedJump());
        }
    }

    IEnumerator DamagedJump()
    {
        isJumping = true;
        playerRb.AddForce(-jumpDirection * 5, ForceMode2D.Impulse);

        yield return new WaitForSeconds(.15f);
        jumpDamaged = false;

        isJumping = false;
        yield return null;
    }

    void HitBoxCoolDown()
    {
        if (damaged && !invencible && !death) 
        {
            damaged = false;
            invencible = true;
            AudioManager.Instance.PlaySFX(2);
            StartCoroutine(HitBoxReturn()); 
        }
    }

    IEnumerator HitBoxReturn()
    {

        Color alpha0 = new Color(1f, 1f, 1f, 0f);

        unarmedSpriteRenderer.color = alpha0;
        yield return new WaitForSeconds(.165f);

        tempSpriteRenderer.color = originalColor;
        yield return new WaitForSeconds(.165f);

        tempSpriteRenderer.color = alpha0;
        //tempSpriteRenderer.color = Color.red;
        yield return new WaitForSeconds(.165f);

        tempSpriteRenderer.color = originalColor;
        yield return new WaitForSeconds(.165f);

        tempSpriteRenderer.color = alpha0;
        //tempSpriteRenderer.color = Color.red;
        yield return new WaitForSeconds(.165f);

        tempSpriteRenderer.color = originalColor;
        yield return new WaitForSeconds(.165f);

        tempSpriteRenderer.color = alpha0;
        //tempSpriteRenderer.color = Color.red;
        yield return new WaitForSeconds(.165f);

        tempSpriteRenderer.color = originalColor;
        yield return new WaitForSeconds(.165f);

        tempSpriteRenderer.color = alpha0;
        //tempSpriteRenderer.color = Color.red;
        yield return new WaitForSeconds(.165f);

        tempSpriteRenderer.color = originalColor;
        yield return new WaitForSeconds(.165f);

        tempSpriteRenderer.color = alpha0;
        //tempSpriteRenderer.color = Color.red;
        yield return new WaitForSeconds(.165f);

        unarmedSpriteRenderer.color = originalColor;
        gunSpriteRenderer.color = originalColor;
        shotgunSpriteRenderer.color = originalColor;
        yield return new WaitForSeconds(.165f);

        invencible = false;

    }

    private void Death()
    {
        playerRb.velocity = Vector3.zero; 

        // Desactivamos reticula y hacemos que la cámara se centre directamente en el jugador
        reticle.SetActive(false);
        virtualCamera.Follow = transform;
        virtualCamera.m_Lens.OrthographicSize = 2;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Explosion"))
        {
            if (!invencible)
            {
                GameManager.Instance.health -= 25;

                jumpDirection = other.transform.position - transform.position;

                jumpDirection.Normalize();

                jumpDamaged = true;
            }
        }
        if (other.CompareTag("Enemy2Bullet"))
        {
            if (!invencible)
            {
                GameManager.Instance.health -= 15;

                jumpDirection = other.transform.position - transform.position;

                jumpDirection.Normalize();

                damaged = true;
            }
        }
        if (other.CompareTag("Enemy3BulletSingle"))
        {
            if (!invencible)
            {
                GameManager.Instance.health -= 10;

                jumpDirection = other.transform.position - transform.position;

                jumpDirection.Normalize();

                damaged = true;
            }
        }
        if (other.CompareTag("FireRate"))
        {
            if (!fireRateAmplified)
            {
                AudioManager.Instance.PlaySFX(7);
                other.gameObject.SetActive(false);
                StartCoroutine(FireRate());
            }
        }
        if (other.CompareTag("Damage"))
        {
            if (!GameManager.Instance.damageAmplified)
            {
                AudioManager.Instance.PlaySFX(7);
                other.gameObject.SetActive(false);
                StartCoroutine(Damage());
            }
        }
        if (other.CompareTag("Pierce"))
        {
            if (!GameManager.Instance.pierce)
            {
                AudioManager.Instance.PlaySFX(7);
                other.gameObject.SetActive(false);
                StartCoroutine(Pierce());
            }
        }
        if (other.CompareTag("Health"))
        {
            if (GameManager.Instance.health < 85)
            {
                AudioManager.Instance.PlaySFX(6);
                other.gameObject.SetActive(false);
                GameManager.Instance.health += 33;
            }
        }
    }

    IEnumerator FireRate()
    {
        fireRateAmplified = true;
        float normalGunFireRate = gunFireRate;
        float normalShotgunFireRate = shotgunFireRate;

        gunFireRate = gunFireRate /2;
        shotgunFireRate = shotgunFireRate /2;

        fireRateSprite.SetActive(true);

        yield return new WaitForSeconds(10);

        fireRateSprite.SetActive(false);

        gunFireRate = normalGunFireRate;
        shotgunFireRate = normalShotgunFireRate;
        fireRateAmplified = false;

        yield return null;   
    }

    IEnumerator Damage()
    {
        GameManager.Instance.damageAmplified = true;

        damageSprite.SetActive(true);

        yield return new WaitForSeconds(10);

        GameManager.Instance.damageAmplified = false;

        damageSprite.SetActive(false);

        yield return null;
    }

    IEnumerator Pierce()
    {
        GameManager.Instance.pierce = true;

        pierceSprite.SetActive(true);

        yield return new WaitForSeconds(10);

        GameManager.Instance.pierce = false;

        pierceSprite.SetActive(false);

        yield return null;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Interactuable"))
        {
            canInteract = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Interactuable"))
        {
            canInteract = false;
        }
    }
}

