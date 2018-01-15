using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;

// has maid specific & enemy specific variables not found in MaidBase & EnemyBase
public class ToonBase : MonoBehaviour
{
    public GameObject pf_damageDoneText;
    protected Image hpBar;
    protected GameManager gm;

    /* maidGear
     * [ HEAD ]
     * [ NECK ]
     * [ TORSO ]
     * [ ARMS ]
     * [ HANDS ] either rings, or hands
     * [ RING1 ] not both
     * [ RING2 ]
     * [ TWOHANDED ] either twohanded
     * [ MAINHAND ] or main/off hand
     * [ OFFHAND ] not both
     * [ LEGS ]
     * [ FEET ]
     * 
     */
    protected readonly int HEAD = 0, NECK = 1, TORSO = 2, ARMS = 3, HANDS = 4;
    protected readonly int LEGS = 4, FEET = 5;

    // GM has the stats key
    protected float[] stats = new float[GameManager.statArraySize];
    protected bool anEnemy = false;

    /*/ enemy specific variables
    protected float xp_value = 0;
    protected float coin_value = 0;
    protected float shared_xp_value = 0;    
    protected float healthRegenPercentPerTick;
    protected float healthRegenDelay;
    */

    // shared variables
    protected static float baseHitpool = 10;
    // protected float hitpool;
    protected static float baseAttack = 3;
    protected int currentFloor;

    protected void ToonInit()
    {
        gm = Camera.main.GetComponent<GameManager>();
        stats[GameManager.HP] = stats[GameManager.HPL];
        hpBar = transform.Find("Toon_Canvas").Find("HealthBar").Find("Health").GetComponent<Image>();
        stats[GameManager.FS] = 0;
    }

    protected void ToonBaseUpdate()
    {
        hpBar.fillAmount = stats[GameManager.HP] / stats[GameManager.HPL];
    }

    // called from MaidBase.cs from collision event for debug
    protected void Foo(Collision2D collision)
    {
        // Debug.Log(collision.gameObject.tag);
        if (collision.gameObject.tag == "Floor_Collider")
        {
            // Debug.Log("ToonBase.Foo()");
            if (stats[GameManager.FS] == 0)
                stats[GameManager.FS] = collision.gameObject.GetComponentInParent<FloorManager>().GetFloorNumber();
            else
            {
                stats[GameManager.FC] = collision.gameObject.GetComponentInParent<FloorManager>().GetFloorNumber();
                Debug.Log(stats[GameManager.FC] - stats[GameManager.FS]);
            }
        }
    }

    protected void InitDamageText(string text)
    {
        GameObject go = Instantiate(pf_damageDoneText);
        RectTransform goRect = go.GetComponent<RectTransform>();
        go.transform.SetParent(transform.Find("Toon_Canvas"));
        goRect.transform.localPosition = pf_damageDoneText.transform.localPosition;
        goRect.transform.localScale = pf_damageDoneText.transform.localScale;
        goRect.transform.localRotation = pf_damageDoneText.transform.localRotation;

        go.GetComponent<Text>().text = text;
        Destroy(go.gameObject, .5f);
    }

    // gets info from victim of attack
    protected void SetAttacking(Collision2D collision)
    {
        // jumps from getting hit
        transform.Translate(Vector2.up * 10 * Time.deltaTime);

        collision.gameObject.GetComponent<ToonBase>().SetAttacked(stats);
    }

    public float[] SetAttacked(float[] attackerStats)
    {
        stats[GameManager.HP] -= attackerStats[GameManager.ATK];

        // display damage done after modifiers
        InitDamageText(attackerStats[GameManager.ATK].ToString());

        // Debug.Log("Id: " + stats[ID] + "\nHealth " + stats[HP]);
        if (stats[GameManager.HP] <= 0f)
            Death(attackerStats);

        ToonBaseUpdate();

        return stats;
    }

    public void SetCurrentFloor(int currentFloor_)
    {
        currentFloor = currentFloor_;
    }

    protected virtual void Death(float[] attackerStats)
    {
        // Debug.Log("Apply kill credit to Maid: " + attackerStats[ID] + "\nEnemy is worth " + stats[XPV] + " xp");
        // gm.ApplyKillCredit(attackerStats, stats, 100f);
        if (this.stats[GameManager.ID] == -1)
            gm.RemoveActiveTapMaid();

        Destroy(gameObject, 0.25f);
    }

    public void SetHitpool(float hitpool_)
    {
        stats[GameManager.HPL] = hitpool_;
        stats[GameManager.HP] = hitpool_;
    }

    public static float GetBaseAttack() { return baseAttack; }

    public void SetAttack(float attack_) { stats[GameManager.ATK] = attack_; }

    public void SetId(int id_) { stats[GameManager.ID] = id_; }

    public float[] GetStats() { return stats; }
}
