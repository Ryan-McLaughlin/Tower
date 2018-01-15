using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Enemy : ToonBase
{
    protected float regenTimer = 0;    
    protected Vector2 rotationEuler;
    protected bool mirrorEnemy = false;
    protected bool hitByTapMaid = false;
    protected bool[] hitByMaid = new bool[7];

    // evolvo type (kinda like dragon type) rearranges its stats to better defend against the last thing that killed it

    // called from FloorManager.cs
    public void Init(float[] eStats, int floorNumber_)
    {
        stats = eStats;
        ToonInit();
        anEnemy = true;

        // meathod found in ToonBase
        SetCurrentFloor(floorNumber_);

        // used when awarding xp to maids
        for (int maidId = 0; maidId < 7; maidId++)
            hitByMaid[maidId] = false;
    }

    // regenerate health
    private void Regenerate()
    {
        // 0 ~ 5 percent hit points left
        if (stats[GameManager.HP] < 0.05f * stats[GameManager.HPL])
        {
            stats[GameManager.HP] += (stats[GameManager.HPL] * 0.002f);
        }

        // 5 ~ 10
        if (stats[GameManager.HP] < 0.1f * stats[GameManager.HPL])
        {
            stats[GameManager.HP] += (stats[GameManager.HPL] * 0.005f);
        }

        // 10 ~ 25
        if (stats[GameManager.HP] < 0.25f * stats[GameManager.HPL])
        {
            stats[GameManager.HP] += (stats[0] * 0.015f);
        }

        // 25 ~ 50
        else if (stats[GameManager.HP] < 0.5f * stats[GameManager.HPL])
        {
            stats[GameManager.HP] += (stats[GameManager.HPL] * 0.025f);
        }

        // 50 ~ 100
        else
            stats[GameManager.HP] += (stats[GameManager.HPL] * 0.075f);
        
        // reset timer after each regen tic
        regenTimer = 0;

        // health is capped it at 100%
        if (stats[GameManager.HP] > stats[GameManager.HPL])
            stats[GameManager.HP] = stats[GameManager.HPL];
    }

    void Update()
    {
        if (stats[GameManager.RGD] <= regenTimer)
           Regenerate();

        regenTimer += Time.deltaTime;

        // when enemy first generates they are laying down, they rotate into upright position
        if (transform.rotation.x > 0)
            transform.Rotate(Vector3.left * 90 * Time.deltaTime);

        ToonBaseUpdate();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {        
        // base.Foo(collision);
        if (collision.gameObject.tag == "Maid_")
        {
            SetAttacking(collision);
            if (collision.gameObject.GetComponent<ToonBase>().GetStats()[GameManager.ID] != -1)
                hitByMaid[(int)collision.gameObject.GetComponent<ToonBase>().GetStats()[GameManager.ID]] = true;
            else
                hitByTapMaid = true;

            regenTimer = 0;
        }
    }
    
    public void SetMirror()
    {
        mirrorEnemy = true;
    }

    // used when locking out floors, I think
    public void End()
    {
        Destroy(gameObject);
    }

    protected override void Death(float[] attackerStats)
    {
        // temp is number of different maids that hit enemy
        float temp = 0;

        if (hitByTapMaid == true)
            temp++;

        for (int maidId = 0; maidId < 7; maidId++)
            if (hitByMaid[maidId] == true)
                temp++;

        // change temp to percentage of credit to give to each maid
        temp = (1 / temp);

        // give xp (or not) to tap maid
        if (hitByTapMaid == true)
        {
            // Debug.Log("Maid_" + -1 + " gets " + temp + " * " + stats[XPV] + " xp");
            gm.ApplyKillCredit(-1, stats, temp);
        }

        // give xp (or not) to each maid
        for (int maidId = 0; maidId < 7; maidId++)
        {
            if (hitByMaid[maidId] == true)
            {
                // Debug.Log("Maid_" + maidId + " gets " + temp + " * " + stats[XPV] + " xp");
                gm.ApplyKillCredit(maidId, stats, temp);
            }
        }

        Destroy(gameObject, 0.25f);
    }
}
