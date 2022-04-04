using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScript : MonoBehaviour
{
    public Text textGameOver;

    public TimerCustom timer;

    private HQCharacterAI hqCharacter;

    private EnemyManager enemyManager;
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip deathClip;
    private void Awake()
    {
        hqCharacter = FindObjectOfType<HQCharacterAI>();
        enemyManager = FindObjectOfType<EnemyManager>();
        audioSource = GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        if(hqCharacter.IsDead())
        {
            timer.Stop();
            textGameOver.enabled = true;
            enemyManager.StopAllEnemies();
            enemyManager.enabled = false;
            audioSource.Stop();
            audioSource.PlayOneShot(deathClip, 0.5f);
            this.enabled = false;
        }
    }
}
