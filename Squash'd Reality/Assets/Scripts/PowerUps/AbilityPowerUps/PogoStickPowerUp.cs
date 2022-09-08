﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PogoStickPowerUp : PowerUP
{
    protected override void Start()
    {
        base.Start();
    }

    public override void triggerEnter(Collider other)
    {
        UIManager uiManager = GameObject.FindWithTag("UIManager").GetComponent<UIManager>();
        if (other.tag == "Player")
        {
            other.GetComponent<AudioManager>().playPowerUpSound();
            PlayerMoveset dm =  other.gameObject.GetComponent<PlayerMoveset>();
            dm.setPogoStickActive();
            if (dm.hasAuthority)
            {
                uiManager.setPowerUpImage("PogoStickPowerUp");
                uiManager.setPowerUpName("Pogo stick");
                uiManager.setPowerUpButtonActive(true);
            }
            Destroy(gameObject);

        }
    }
}
