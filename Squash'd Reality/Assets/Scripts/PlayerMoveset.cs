﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(AudioManager))]
public class PlayerMoveset : NetworkBehaviour
{
    private CharacterController controller;
    private AudioManager audioManager;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private static float initSpeed= 3.0f;
    private float playerSpeed = initSpeed;
    private float playerSpeedMultiplier = 1f;
    private float hoverboardMultiplier = 1.3f;
    [SerializeField] private float jumpHeight = 0.5f;
    private float jumpHeightMultiplier = 1f;
    private float pogoStickMultiplier = 1.3f;
    private float gravityValue = -9.81f;

    [SyncVar] public string playerName;
    [SyncVar(hook="_meshActiveChanged")] public bool meshActive;
    [SyncVar] public int life = 1;

    private Coroutine durationPowerup;

    private bool pogoStickActive;

    private bool pauseActive = false;

    public bool playerCanMove = true;

    private bool nameSetted = false;
    private PlayerStats playerStats;

    private bool isDead = false;
    
    //ALLY DAMAMGE
    private float BasicDamage = 6.7f;
    private float MediumDamage = 13.4f;
    private float HighDamage = 20f;
    private float allyLife = 20f;


    private bool isWalking = false;

    private NetworkAnimator _networkAnimator;
    private void Start()
    {
        isDead = false;
        if (isServer) {
            meshActive = true;
        }

        isWalking = false;
        playerCanMove = true;
        controller = gameObject.GetComponent<CharacterController>();
        controller.detectCollisions = false;
        playerStats = GameObject.FindGameObjectWithTag("DDOL").GetComponent<PlayerStats>();
        
        
        audioManager = GetComponent<AudioManager>();
        _networkAnimator = GetComponent<NetworkAnimator>();

        if (playerName == GameObject.FindGameObjectWithTag("DDOL").GetComponent<DDOL>().playerName)
        {
            if (SceneManager.GetActiveScene().name == "CookingTime")
            {
                audioManager.playMusicLevel(0);
            }else if (SceneManager.GetActiveScene().name == "DarkPuzzle")
            {
                audioManager.playMusicLevel(1);
            }else if (SceneManager.GetActiveScene().name == "TrenchTime")
            {
                audioManager.playMusicLevel(2);
            }else if (SceneManager.GetActiveScene().name == "ElectroPipeline")
            {
                audioManager.playMusicLevel(3);
            }else if (SceneManager.GetActiveScene().name == "Lobby")
            {
                audioManager.playMusicLevel(4);
            }  
        }
        
    }

    private void Update()
    {
        if (Input.GetButtonDown("Start") && !pauseActive)
        {
            pauseActive = true;
        }else if (Input.GetButtonDown("Start") && pauseActive)
        {
            pauseActive = false;
        }
        if (!nameSetted && hasAuthority)
        {
            nameSetted = true;
            playerStats.playerName = playerName;
        }
    }

    void FixedUpdate() {
        if (hasAuthority && !pauseActive && playerCanMove) {
            Move();
        }

        if (!playerCanMove)
        {
            float x =gameObject.transform.position.x;
            float y = 1.9f;
            float z = gameObject.transform.position.z;
            gameObject.transform.position = new Vector3(x,y,z);
        }
    }

    private void _meshActiveChanged(bool meshActive){
        //gameObject.GetComponent<MeshRenderer>().enabled = meshActive;
        gameObject.transform.GetChild(4).transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled = meshActive;
    }
    void Move(){
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            if (!isWalking)
            {
                _networkAnimator.animator.SetFloat("walkMultiplier",1.0f);
            }
            isWalking = true;
            //_networkAnimator.SetTrigger("walk");
           // _networkAnimator.animator.ResetTrigger("walk");
        }
        else if(isWalking)
        {
            isWalking = false;
            _networkAnimator.animator.SetFloat("walkMultiplier",0.0f);
            //deactivate
        }
       
        Vector3 moveRightStick = new Vector3(Input.GetAxis("Horizontal-Direction"), 0, -Input.GetAxis("Vertical-Direction"));
        if (move != Vector3.zero && groundedPlayer)
        {
            audioManager.playSteps();
        }
        controller.Move(move * Time.deltaTime * playerSpeed*playerSpeedMultiplier);

        if(moveRightStick != Vector3.zero){
            gameObject.transform.forward = moveRightStick;

        }else if (moveRightStick == Vector3.zero & move != Vector3.zero){
            gameObject.transform.forward = move;
        }

        // Changes the height position of the player..
        if (Input.GetButton("Jump") && groundedPlayer && !pogoStickActive) {
            audioManager.playJumpSound();
            _networkAnimator.SetTrigger("jump");
            _networkAnimator.animator.ResetTrigger("jump");
            playerVelocity.y += Mathf.Sqrt(jumpHeight * jumpHeightMultiplier * -4.0f * gravityValue);
        }

        if (pogoStickActive && groundedPlayer)
        {
            audioManager.playJumpSound();
            playerVelocity.y += Mathf.Sqrt(jumpHeight * jumpHeightMultiplier * -4.0f * gravityValue);

        }

        if (!groundedPlayer)
        {
            controller.slopeLimit = 90f;
        }
        else
        {
            controller.slopeLimit = 45f;
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
        
    }

    public void Die(string playerDead) {
        UIManager _uiManager = GameObject.FindWithTag("UIManager").GetComponent<UIManager>();
        PlayerStats playerStats = GameObject.FindWithTag("DDOL").GetComponent<PlayerStats>();
        if (hasAuthority)
        {
            _uiManager.setInfoBoxText("YOU DIED");
            playerStats.death++;
        }
        else
        {
            _uiManager.setInfoBoxText(playerDead + " DIED");
        }
        _uiManager.setInfoBoxActive(true);

    }

    //Use this for trench time
    public void TakeDamage(int damage, string shooterName)
    {
        UIManager _uiManager = GameObject.FindWithTag("UIManager").GetComponent<UIManager>();
        //disable Spartan Armor
        if (life == 2 && hasAuthority)
        {
            _uiManager.setPowerUpButtonActive(false);
        }

        if (isServer && !isDead)
        {
            life = life - damage;
            if (life <= 0)
            {
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                foreach (var player in players)
                {
                    PlayerMoveset pMoveset = player.GetComponent<PlayerMoveset>();
                    if (pMoveset.playerName == shooterName)
                    {
                        pMoveset.friendlyKilled(); 
                    }
                }     
                if (hasAuthority)
                {
                    isDead = true;
                    playerStats.death++;
                    _uiManager.setInfoBoxText("YOU DIED");        
                    _uiManager.setInfoBoxActive(true);   
                }
                GameObject.FindObjectOfType<TrenchTime>().setPlayerDead();
                if (shooterName != "enemy")
                {
                    Destroy(this.gameObject, 0.5f);
                }
                else
                {
                    Destroy(this.gameObject);
                }
            }   
        }else if (isClient && !isServer && shooterName!="enemy")
        {
            life = life - damage;
            if (life <= 0)
            {
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                foreach (var player in players)
                {
                    PlayerMoveset pMoveset = player.GetComponent<PlayerMoveset>();
                    if (pMoveset.playerName == shooterName)
                    {
                        pMoveset.friendlyKilled(); 
                    }
                }     
            }
        }
        
        
    }
    
    //-------------------------------------------POWER UPs SETTINGS---------------------------------------
    public void setSpartanArmorActive()
    {
        GameObject.FindWithTag("DDOL").GetComponent<PlayerStats>().powerUp++;
        resetPowerUpValues();
        life = 2;
    }

    public void setHoverboardActive()
    {
        GameObject.FindWithTag("DDOL").GetComponent<PlayerStats>().powerUp++;
        resetPowerUpValues();
        playerSpeedMultiplier = hoverboardMultiplier;
        durationPowerup = StartCoroutine(powerUpDuration());
    }

    public void setPogoStickActive()
    {
        GameObject.FindWithTag("DDOL").GetComponent<PlayerStats>().powerUp++;
        resetPowerUpValues();
        pogoStickActive = true;
        jumpHeightMultiplier = pogoStickMultiplier;
        durationPowerup = StartCoroutine(powerUpDuration());
    }
    public void resetPowerUpValues()
    {
        if (durationPowerup != null)
        {
            StopCoroutine(durationPowerup);

        }
        life = 1;
        playerSpeedMultiplier = 1f;
        jumpHeightMultiplier = 1.3f;
        pogoStickActive = false;
    }

    IEnumerator powerUpDuration()
    {
        yield return new WaitForSeconds(10f);
        UIManager _uiManager = GameObject.FindWithTag("UIManager").GetComponent<UIManager>();
        resetPowerUpValues();
        if (hasAuthority)
        {
         _uiManager.setPowerUpButtonActive(false);   
        }
    }
    //----------------------------------------------------TRIGGER BULLET------------------------------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
      
        if (other.tag == "Bullet" && other.GetComponent<Bullet>().shooterName!=playerName)
        {
            string bulletName = other.GetComponent<Bullet>().bulletName;
            if (bulletName == "BulletPistol")
            {
                allyLife -= BasicDamage;
            }else if (bulletName == "BulletShotgun")
            {
                allyLife -= BasicDamage;
            } else if (bulletName == "BulletAssaultRifle")
            {
                allyLife -= MediumDamage;
            }else if (bulletName== "BulletSniperRifle")
            {
                allyLife -= HighDamage;
            }else if (bulletName == "BulletSMG")
            {
                allyLife -= BasicDamage;
            }
            
            if (allyLife <= 0f)
            {
                TakeDamage(1, other.GetComponent<Bullet>().shooterName);
                allyLife = 20f;
            }
        }
        
    }

    //----------------------------------------------------SET OTHER STATS----------------------------------------------------------------------
    public void setCollectibleStats()
    {
        if (hasAuthority)
        {
            GameObject.FindWithTag("DDOL").GetComponent<PlayerStats>().collectible++;

        }
    }

    public void enemyKilled()
    {
        if (hasAuthority)
        {
            GameObject.FindWithTag("DDOL").GetComponent<PlayerStats>().antivirusKilled++;
        }
    }

    public void friendlyKilled()
    {
        if (hasAuthority)
        {
            GameObject.FindWithTag("DDOL").GetComponent<PlayerStats>().friendlyKill++;
        }
    }
    
}
