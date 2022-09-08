﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.Networking;

public class TrenchTimeMatchManager : MatchManager
{
    public bool matchTimeEnded = false;
    private GameObject[] players;
    private PlayerStats playerStats;
    private bool playerDead;
    protected override void Start()
    {
        base.Start();
        playerDead = false;
        playerStats = GameObject.FindGameObjectWithTag("DDOL").GetComponent<PlayerStats>();

    }

    protected override void Update()
    {
        if (isServer && _networkingManager.numPlayers == GameObject.FindGameObjectsWithTag("Player").Length)
        {
            gameReady = true;
        }
        if (gameReady && !matchStarting)
        {
            FindObjectOfType<TrenchTime>().setPlayersConnected(GameObject.FindGameObjectsWithTag("Player").Length);
            matchStarting = true;
            _uiManager.StartMatch(4f);
            StartCoroutine(matchStart());
        }

        if (matchTimeEnded)
        {
            FindObjectOfType<TrenchTime>().timeEnded = true;
            if (isServer) {
                matchTimeEnded = false;
            }
        }

        if (matchStarting && !playerDead && isClient && !isServer)
        {
            int authorityPlayer = 0;
            players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                if (player.GetComponent<PlayerMoveset>().hasAuthority)
                {
                    authorityPlayer++;
                }
            }

            if (authorityPlayer == 0)
            {
                playerDead = true;
                UIManager _uiManager = GameObject.FindWithTag("UIManager").GetComponent<UIManager>();
                playerStats.death++;
                _uiManager.setInfoBoxText("YOU DIED");        
                _uiManager.setInfoBoxActive(true);   
            }

        }
        
    }

    protected override IEnumerator matchStart()
    {
        return base.matchStart();
    }

    protected override void showPlayerUI()
    {
        base.showPlayerUI();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            for (int j = 0; j < players[i].transform.childCount; j++)
            {
                if (players[i].transform.GetChild(j).tag.Equals("Weapon"))
                {
                    GameObject oldWeapon = players[i].transform.GetChild(j).gameObject;
                    Weapon newWeapon = (Weapon) oldWeapon.AddComponent(typeof(Pistol));
                    oldWeapon.GetComponent<Shoot>().updateWeapon(newWeapon);
                    if (players[i].gameObject.GetComponent<PlayerMoveset>().hasAuthority)
                    {
                        _uiManager.setWeaponImage("Pistol");
                        _uiManager.setWeaponActive(true);
                    }

                }
            }
        }

    }

    public override void timeEnded()
    {
        if (!matchWon)
        {
            base.timeEnded();
            if (isServer)
            {
                GameObject[] spawners = GameObject.FindGameObjectsWithTag("Spawner");
                for (int i = 0; i < spawners.Length; i++)
                {
                    spawners[i].GetComponent<Spawner>().StopSpawning();
                }
            }
            matchTimeEnded = true;
        }
        

    }

    protected override IEnumerator resetChallenge()
    {
        yield return new WaitForSeconds(2f);
        GameObject.FindObjectOfType<TrenchTime>().endChallenge(false);
    }

    
}
