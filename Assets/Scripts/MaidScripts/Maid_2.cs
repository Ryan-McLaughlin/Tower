using System.Collections;
using UnityEngine;

public class Maid_2 : MaidBase
{// Heal Maid

    private readonly float[] BASESTAT = new float[] { 1, 0, 2.5f };
    private float[] healerStat;

    private readonly int HA = 1; // heal amount    
    private readonly int HCT = 1; // heal count
    private readonly int TTH = 2; // time till heal
    
    void Start()
    {
        speedX = 5.75f;
        base.Init();

        healerStat = new float[BASESTAT.Length];
        for (int i = 0; i < BASESTAT.Length; i++)
            healerStat[i] = BASESTAT[i];
    }

    new private void Update()
    {
        base.Update();

        // countdown timer for each heal
        if (healerStat[TTH] > 0)
            healerStat[TTH] -= Time.deltaTime;
        // heal
        else
        {
            SpellHeal();

            // update
            healerStat[HCT] += 1;
            // Debug.Log(this.name + ": heal #" + healerStat[HCT]);
            healerStat[TTH] = BASESTAT[TTH];
        }
    }

    private void SpellHeal()
    {

    }
}
