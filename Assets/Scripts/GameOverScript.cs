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

    private void Awake()
    {
        hqCharacter = FindObjectOfType<HQCharacterAI>();
        enemyManager = FindObjectOfType<EnemyManager>();
    }

    private void FixedUpdate()
    {
        if(hqCharacter.IsDead())
        {
            timer.Stop();
            textGameOver.enabled = true;
            enemyManager.StopAllEnemies();
            enemyManager.enabled = false;
        }
    }
}
