using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public GameObject pf_tapMaid;
    public GameObject[] pf_maid;
    public GameObject[] pf_enemy;
    // public GameObject[] pf_mirrorEnemy;
    public GameObject[] pf_floor;
    public GameObject[] pf_mirror;
    public GameObject pf_maidStatPanel;
    public Sprite[] sprite_floors;
    public Sprite[] sprite_floorBorders;

    // player variables
    private float vaultValue = 30;
    private float xpVaultValue = 0;

    // these will replace pf_enemy[]
    // these should be loaded from file at some point to make life easy when adding/removing enemies
    private GameObject[] pf_boss;
    private GameObject[] pf_midBoss;
    private GameObject[] pf_toughMob;
    private GameObject[] pf_trashMob;

    private GameObject statPanel;
    private float floorHeight = 3.6f;

    // this array is given to FloorManager with mobs to initialize
    private GameObject[] pf_newFloorEnemies;

    // used for development hierarchy organization in the unity editor
    private Transform floorHolder, maidHolder;

    // this is used for every new floor thats instantiated
    private int nextTopFloor;
    private GameObject newFloor;
    private string floor;

    private Text[] text_HUD = new Text[3];

    // [0]name; [1]hp; [2]attack; [3]lvl
    private Text[] text_tapMaid_panel = new Text[4];
    private Text[,] text_maidPanel = new Text[7, 4];

    protected Image tapMaidxpBar;
    protected Image[] arrivalBar = new Image[7];
    protected Image[] xpBar = new Image[7];

    // maid UI elements    
    // private Text[] text_maidLevel = new Text[7];
    private Text[] text_maidPower = new Text[3];
    private Text text_tapMaidLvLUpWithCoins;
    private Text[] text_LvLUpWithCoins = new Text[7];

    // MAIN ARRAY KEY
    /*
    * [ ATK ] attack - 1
    * [ CNTR ] counter - 11
    * [ CV ] coin_value - 6
    * [ FC ] current floor - 14
    * [ FS ] starting floor - 15
    * [ HP ] hitpoints - 3
    * [ HPL ] hitpool - 0
    * [ ID ] id - 4
    * [ LVL ] lvl - 2
    * [ RGD ] regen_delay - 12
    * [ RGPT ] regen_per_tick - 13
    * [ RST ] respawn_time - 10    
    * [ SXPV ] shared_xp_value - 7
    * [ XPC ] xp_current - 8
    * [ XPTL ] xp_toLvL - 9
    * [ XPV ] xp_value - 5
    */
    public static readonly int HPL = 0, ATK = 1, LVL = 2, HP = 3, ID = 4;
    public static readonly int XPV = 5, CV = 6, SXPV = 7, XPC = 8, XPTL = 9;
    public static readonly int RST = 10, CNTR = 11, RGD = 12, RGPT = 13, FC = 14;
    public static readonly int FS = 15;
    public static readonly int statArraySize = 17;

    // MAIN ARRAYS
    private static float[] tapMaidBaseStats;
    private float[] tapMaidStats;
    private static float[,] maidBaseStats;
    private float[,] maidStats;

    private int maxActiveTapMaids = 9;
    private int activeTapMaids;

    // private int tapMaidCounter;
    // private int[] maidCounter = new int[7];

    // private readonly float[] MaidRespawnDelay = new float[7] { 4.5f, 5.1f, 5.3f, 7.4f, 7.8f, 8.2f, 8.9f, };
    // private float[] maid_respawnTimer = new float[7];

    private bool[] maidUnlocked = new bool[7] { false, false, false, false, false, false, false };
    private bool[] maidSpawning = new bool[7] { true, true, true, true, true, true, true };


    // how many floors are alowed to be active, and the floor maids spawn on/monsters start spawning at
    private int activeFloors = 6;
    private int spawningFloor = 0;
    private bool statPanelExpanded = true;

    // NEEDS OVERHAUL
    // Lvls stands for floor level, or number
    private int bossLvls = 5;
    private int midBossLvls = 4;
    private int toughMobLvls = 3;

    void Start()
    {
        SetUpHUD();
        SetUpMaidStats();
        // holders are for heirchy in engine environment, keeps things neat        
        floorHolder = new GameObject("Floors").transform;
        maidHolder = new GameObject("Maids").transform;

        // Maid stuff setup
        // sets the timer for when each maid will spawn
        // tapMaidCounter = 0;
        for (int maidId = 0; maidId < 7; maidId++)
        {
            maidStats[maidId, RST] = maidBaseStats[maidId, RST];
            // maidCounter[maidId] = 0;
        }

        // xp goals
        tapMaidStats[XPTL] = 10;
        for (int maidId = 0; maidId < pf_maid.Length; maidId++)
            maidStats[maidId, XPTL] = 10 + 1.15f * maidId;

        // there aren't any floors yet, first floor will be floor 0
        nextTopFloor = 0;
        spawningFloor = 0;

        activeTapMaids = 0;

        // floor prefab order matters, the first generated floor must be NewFloor otherwise the game breaks
        NewFloor();
    }

    void Update()
    {
        // cycle through all maid timers, spawn maid if needed & reset timer
        for (int maidId = 0; maidId < pf_maid.Length; maidId++)
        {
            if (maidUnlocked[maidId] && maidSpawning[maidId])
            {
                if (maidStats[maidId, RST] > 0)
                    maidStats[maidId, RST] -= Time.deltaTime;
                else
                {
                    SummonMaid(pf_maid[maidId], maidId);
                    maidStats[maidId, RST] = GetMaidRespawnTime(maidId);
                }
            }
        }
        UpdateHUD();
    }

    void SetUpHUD()
    {
        text_HUD[0] = transform.Find("Canvas").Find("HUD").Find("SharedXp").GetComponent<Text>();
        text_HUD[1] = transform.Find("Canvas").Find("HUD").Find("Vault").GetComponent<Text>();
        text_HUD[2] = transform.Find("Canvas").Find("HUD").Find("ActiveTapMaids").GetComponent<Text>();

        // Stat Panels // [0]name; [1]hp; [2]attack; [3]lvl
        // TapMaid
        text_tapMaid_panel[0] = transform.Find("Canvas").Find("HUD").Find("MaidPanelAnchor").Find("TapMaidPanel").Find("Name_txt").GetComponent<Text>();
        text_tapMaid_panel[1] = transform.Find("Canvas").Find("HUD").Find("MaidPanelAnchor").Find("TapMaidPanel").Find("Hitpool_txt").GetComponent<Text>();
        text_tapMaid_panel[2] = transform.Find("Canvas").Find("HUD").Find("MaidPanelAnchor").Find("TapMaidPanel").Find("Attack_txt").GetComponent<Text>();
        text_tapMaid_panel[3] = transform.Find("Canvas").Find("HUD").Find("MaidPanelAnchor").Find("TapMaidPanel").Find("LvL_txt").GetComponent<Text>();
        tapMaidxpBar = transform.Find("Canvas").Find("HUD").Find("MaidPanelAnchor").Find("TapMaidPanel").Find("Xp_Bar").Find("Percent").GetComponent<Image>();
        text_tapMaidLvLUpWithCoins = transform.Find("Canvas").Find("HUD").Find("MaidPanelAnchor").Find("TapMaidPanel").Find("LevelUp_button").Find("LevelUpCost_txt").GetComponent<Text>();

        // maids
        for (int maidId = 0; maidId < 3; maidId++)
        {
            string panelName = "MaidPanel_" + maidId;
            text_maidPanel[maidId, 0] = transform.Find("Canvas").Find("HUD").Find("MaidPanelAnchor").Find(panelName).Find("Name_txt").GetComponent<Text>();
            text_maidPanel[maidId, 1] = transform.Find("Canvas").Find("HUD").Find("MaidPanelAnchor").Find(panelName).Find("Hitpool_txt").GetComponent<Text>();
            text_maidPanel[maidId, 2] = transform.Find("Canvas").Find("HUD").Find("MaidPanelAnchor").Find(panelName).Find("Attack_txt").GetComponent<Text>();
            text_maidPanel[maidId, 3] = transform.Find("Canvas").Find("HUD").Find("MaidPanelAnchor").Find(panelName).Find("LvL_txt").GetComponent<Text>();
            arrivalBar[maidId] = transform.Find("Canvas").Find("HUD").Find("MaidPanelAnchor").Find(panelName).Find("Arrival_Bar").Find("Percent").GetComponent<Image>();
            xpBar[maidId] = transform.Find("Canvas").Find("HUD").Find("MaidPanelAnchor").Find(panelName).Find("Xp_Bar").Find("Percent").GetComponent<Image>();
            // Debug.Log(maidId);
            text_maidPower[maidId] = transform.Find("Canvas").Find("HUD").Find("MaidPanelAnchor").Find(panelName).Find("Power_button").Find("Text").GetComponent<Text>();
            text_LvLUpWithCoins[maidId] = transform.Find("Canvas").Find("HUD").Find("MaidPanelAnchor").Find(panelName).Find("LevelUp_button").Find("LevelUpCost_txt").GetComponent<Text>();
        }

        statPanel = GameObject.Find("MaidPanelAnchor");
    }

    void UpdateHUD()
    {
        // add vaultValue to UI
        text_HUD[1].text = ": " + ((int)vaultValue).ToString();
        text_HUD[0].text = ""; // "sxp: " + ((int)xpVaultValue).ToString();        
        text_HUD[2].text = ": " + activeTapMaids + " / " + maxActiveTapMaids;

        // statpanel update
        text_tapMaid_panel[0].text = "Tap Maid";
        text_tapMaid_panel[1].text = "Hitpool: " + tapMaidStats[HPL];
        text_tapMaid_panel[2].text = "Attack: " + tapMaidStats[ATK];
        text_tapMaid_panel[3].text = "LeveL: " + tapMaidStats[LVL];
        tapMaidxpBar.fillAmount = tapMaidStats[XPC] / tapMaidStats[XPTL];
        text_tapMaidLvLUpWithCoins.text = "Level up\n" + GetCoinCost((int)tapMaidStats[LVL]) + "\ncoins";

        for (int maidId = 0; maidId < 3; maidId++)
        {
            switch (maidId)
            {
                case 0:
                    text_maidPanel[maidId, 0].text = "Damage Maid";
                    break;
                case 1:
                    text_maidPanel[maidId, 0].text = "Tank Maid";
                    break;
                case 2:
                    text_maidPanel[maidId, 0].text = "Healer Maid";
                    break;
            }
            text_maidPanel[maidId, 1].text = "Hitpool: " + maidStats[maidId, HPL];
            text_maidPanel[maidId, 2].text = "Attack: " + maidStats[maidId, ATK];
            text_maidPanel[maidId, 3].text = "LeveL: " + maidStats[maidId, LVL];
            arrivalBar[maidId].fillAmount = maidStats[maidId, RST] / maidBaseStats[maidId, RST];
            xpBar[maidId].fillAmount = maidStats[maidId, XPC] / maidStats[maidId, XPTL];
            text_LvLUpWithCoins[maidId].text = "Level up\n" + GetCoinCost((int)maidStats[maidId, LVL]) + "\ncoins";
        }

        // maid on off switch
        for (int maidId = 0; maidId < text_maidPower.Length; maidId++)
        {
            if (maidSpawning[maidId])
                text_maidPower[maidId].text = "O\nN";
            else
                text_maidPower[maidId].text = "O\nF\nF";
        }

        // stats
        // number of maids summoned per maid type
        // max amount any tank has tanked, average amount ''
        // tank should only tank a hit if the maid being protected survives the hit
    }

    private void SummonMaid(GameObject maid, int maidId)
    {
        floor = "Floor_" + spawningFloor;
        GameObject.Find(floor).GetComponent<FloorManager>().GenerateMaid(maid, maidId);
    }

    public float[] GetEnemyStats(int fn)
    {
        float[] eStats = new float[statArraySize];

        eStats[ATK] = 10 + 5 * Mathf.Pow(1.09f, fn); // attack
        eStats[CV] = 1 + Mathf.Pow(1.07f, fn);// 2 + fn; // coin value
        eStats[HPL] = 10 + 10 * Mathf.Pow(1.15f, fn); // hitpool
        eStats[RGD] = 1.15f; // regen delay
        // eStats[RGPT] = 0.05f; // regen percent per tick
        eStats[SXPV] = 1 + fn / 5; // shared xp value
        eStats[XPV] = 10 + fn * 1.17f; // xp value

        // generic stat modifier based on the floor number the enemy is on
        for (int stat = 0; stat < eStats.Length; stat++)
            if (stat != ID)
                eStats[stat] += fn * eStats[stat] * 1.11f;

        return eStats;
    }

    private void SetUpMaidStats()
    {
        tapMaidBaseStats = new float[statArraySize];
        maidBaseStats = new float[7, statArraySize];

        tapMaidStats = new float[statArraySize];
        maidStats = new float[7, statArraySize];

        // tap maid
        tapMaidStats[HPL] = tapMaidBaseStats[HPL] = 10;
        tapMaidStats[ATK] = tapMaidBaseStats[ATK] = 10;
        tapMaidStats[LVL] = tapMaidBaseStats[LVL] = 1;
        tapMaidStats[ID] = tapMaidBaseStats[ID] = -1;
        tapMaidStats[CNTR] = tapMaidBaseStats[CNTR] = 0;

        // this loop used to set up non unique starting variables
        for (int maidId = 0; maidId < 7; maidId++)
        {
            maidStats[maidId, LVL] = maidBaseStats[maidId, LVL] = 0;
            maidStats[maidId, CNTR] = maidBaseStats[maidId, CNTR] = 0;
        }

        // damage maid
        maidStats[0, HPL] = maidBaseStats[0, HPL] = 10f;
        maidStats[0, ATK] = maidBaseStats[0, ATK] = 14f;
        maidStats[0, ID] = maidBaseStats[0, ID] = 0;
        maidBaseStats[0, RST] = 4.5f;


        // tank maid
        maidStats[1, HPL] = maidBaseStats[1, HPL] = 17f;
        maidStats[1, ATK] = maidBaseStats[1, ATK] = 7f;
        maidStats[1, ID] = maidBaseStats[1, ID] = 1;
        maidBaseStats[1, RST] = 4.7f;

        // heal maid
        maidStats[2, HPL] = maidBaseStats[2, HPL] = 9;
        maidStats[2, ATK] = maidBaseStats[2, ATK] = 9;
        maidStats[2, ID] = maidBaseStats[2, ID] = 2;
        maidBaseStats[2, RST] = 4.9f;

        // not used in game, but still used in code to make implementation easier
        maidStats[3, HPL] = maidBaseStats[3, HPL] = 10;
        maidStats[3, ATK] = maidBaseStats[3, ATK] = 10;
        maidStats[3, ID] = maidBaseStats[3, ID] = 3;
        maidBaseStats[3, RST] = 10f;

        maidStats[4, HPL] = maidBaseStats[4, HPL] = 9;
        maidStats[4, ATK] = maidBaseStats[4, ATK] = 6;
        maidStats[4, ID] = maidBaseStats[4, ID] = 4;
        maidBaseStats[4, RST] = 10f;

        maidStats[5, HPL] = maidBaseStats[5, HPL] = 10;
        maidStats[5, ATK] = maidBaseStats[5, ATK] = 7;
        maidStats[5, ID] = maidBaseStats[5, ID] = 5;
        maidBaseStats[5, RST] = 10f;

        maidStats[6, HPL] = maidBaseStats[6, HPL] = 11;
        maidStats[6, ATK] = maidBaseStats[6, ATK] = 8;
        maidStats[6, ID] = maidBaseStats[6, ID] = 6;
        maidBaseStats[6, RST] = 10f;
    }

    // used by floor manager when a maid is spawned on said manager
    public float[] GetMaidStats(int maidId)
    {
        float[] mStats = new float[2];

        // This converts 2d array to 1d array for requesting meathod
        // mStats[0]hitpool; mStats[1]attack
        // maidStats[0]hitpool; [1]attack; [2]lvl
        if (maidId != -1)
        {
            for (int stat = 0; stat < mStats.Length; stat++)
                mStats[stat] = maidStats[maidId, stat];
            return mStats;
        }
        else
            return tapMaidStats;
    }

    public void CheckIfTopFloor(int floorNumber)
    {// checks if reported floor is top or not
     // if so, make a new floor

        // this block calls to make a new floor
        if (floorNumber == nextTopFloor - 1)
        {
            NewFloor();
            /*
            if (nextTopFloor % 2 == 0)
                NewFloor();
            else
                NewMirror();
            */
        }

        if (spawningFloor + activeFloors < nextTopFloor)
        {
            spawningFloor = nextTopFloor - activeFloors;
            for (int i = 0; i < spawningFloor; i++)
            {
                floor = "Floor_" + i;
                GameObject.Find(floor).GetComponent<FloorManager>().LockFloor();
            }

        }
    }

    public int GetMaidLevel(int maidId)
    {
        if (maidId != -1)
            return (int)maidStats[maidId, 2];
        else
            return (int)tapMaidStats[2];
    }

    public int GetTopFloor()
    {
        return nextTopFloor - 1;
    }

    // even numbered floors, we start at floor 0
    private void NewFloor()
    {
        SetUpNewFloorEnemies(nextTopFloor);
        if (nextTopFloor % 2 == 0)
            newFloor = (GameObject)Instantiate(pf_floor[0], new Vector2(0f, (nextTopFloor * floorHeight) - 12.7f), Quaternion.identity);
        else
            newFloor = (GameObject)Instantiate(pf_mirror[0], new Vector2(0f, (nextTopFloor * floorHeight) - 12.7f), Quaternion.identity);
        newFloor.name = "Floor_" + nextTopFloor;
        newFloor.transform.Find("Sprite_Floor").GetComponent<SpriteRenderer>().sprite = sprite_floors[Random.Range(0, sprite_floors.Length)];
        newFloor.transform.Find("Sprite_FloorBorder").GetComponent<SpriteRenderer>().sprite = sprite_floorBorders[Random.Range(0, sprite_floorBorders.Length)];
        newFloor.GetComponent<FloorManager>().SetupFloor(nextTopFloor, pf_tapMaid, pf_newFloorEnemies);
        newFloor.transform.SetParent(floorHolder);
        nextTopFloor += 1;
    }

    /*/ odd numbered floors
    private void NewMirror()
    {
        SetUpNewFloorEnemies(nextTopFloor); // SetUpNewMirrorFloorEnemies(nextTopFloor);
        newFloor = (GameObject)Instantiate(pf_mirror[0], new Vector2(0f, (nextTopFloor * floorHeight) - 12.7f), Quaternion.identity);
        newFloor.name = "Floor_" + nextTopFloor;
        newFloor.transform.Find("Sprite_Floor").GetComponent<SpriteRenderer>().sprite = sprite_floors[Random.Range(0, sprite_floors.Length)];
        newFloor.transform.Find("Sprite_FloorBorder").GetComponent<SpriteRenderer>().sprite = sprite_floorBorders[Random.Range(0, sprite_floorBorders.Length)];
        newFloor.GetComponent<FloorManager>().SetupFloor(nextTopFloor, pf_tapMaid, pf_newFloorEnemies);
        newFloor.transform.SetParent(floorHolder);
        nextTopFloor += 1;
    }
    */

    private void SetUpNewFloorEnemies(int fN)
    {
        if (fN != 0)
        {
            // tough mobs unless something higher in the food chain
            if (fN % toughMobLvls == 0 && fN % bossLvls != 0 && fN % midBossLvls != 0)
            {
                pf_newFloorEnemies = new GameObject[2] { pf_enemy[Random.Range(0, pf_enemy.Length)], pf_enemy[Random.Range(0, pf_enemy.Length)] };
            }

            // mid boss unless something higher in the food chain
            else if (fN % midBossLvls == 0 && fN % bossLvls != 0)
            {
                pf_newFloorEnemies = new GameObject[2] { pf_enemy[Random.Range(0, pf_enemy.Length)], pf_enemy[Random.Range(0, pf_enemy.Length)] };
            }

            // boss is currently highest in food chain
            else if (fN % bossLvls == 0)
            {
                pf_newFloorEnemies = new GameObject[3] { pf_enemy[Random.Range(0, pf_enemy.Length)], pf_enemy[Random.Range(0, pf_enemy.Length)], pf_enemy[Random.Range(0, pf_enemy.Length)] };
            }

            // regular floors get one enemy
            else
            {
                pf_newFloorEnemies = new GameObject[1] { pf_enemy[Random.Range(0, pf_enemy.Length)] };
            }
        }
        // for the very first floor, floor 0
        else
            pf_newFloorEnemies = new GameObject[1] { pf_enemy[Random.Range(0, pf_enemy.Length)] };
    }

    private float GetMaidRespawnTime(int maidId)
    {
        float modifiedRespawnTime = maidBaseStats[maidId, RST];
        return modifiedRespawnTime;
    }

    public void CycleMaidPower(int i)
    {
        maidSpawning[i] = !maidSpawning[i];
    }

    public void ApplyKillCredit(int maidId, float[] defeatedStats, float percentOwed)
    {
        switch (maidId)
        {
            case -1:
                tapMaidStats[XPC] += (defeatedStats[XPV] * .5f) * percentOwed;
                // while loop lets maid lvl up more than once if enough xp was obtained
                while (tapMaidStats[XPC] >= tapMaidStats[XPTL])
                {
                    MaidLevelUpFromKill(-1);
                    tapMaidStats[XPC] = tapMaidStats[XPC] - tapMaidStats[XPTL];
                    tapMaidStats[XPTL] = (tapMaidStats[LVL] * 10) + (tapMaidStats[LVL] * 3.14f);
                }
                vaultValue += defeatedStats[CV] * percentOwed;
                return;

            case 0: // damage
                maidStats[maidId, XPC] += defeatedStats[XPV] * percentOwed;
                break;

            case 1: // tank
                maidStats[maidId, XPC] += (defeatedStats[XPV] * 3 / 4) * percentOwed;
                break;

            case 2: // healer
                maidStats[maidId, XPC] += (defeatedStats[XPV] * 5 / 4) * percentOwed;
                break;

            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            case 6:
                break;
        }

        // while loop lets maid lvl up more than once if enough xp was obtained
        while (maidStats[maidId, XPC] >= maidStats[maidId, XPTL])
        {// level up maid, and reset xp variables for next lvl
            MaidLevelUpFromKill(maidId);
            maidStats[maidId, XPC] = maidStats[maidId, XPC] - maidStats[maidId, XPTL];
            maidStats[maidId, XPTL] = (maidStats[maidId, LVL] * 10) + (maidStats[maidId, LVL] * Mathf.Pow(1.1f, maidStats[maidId, LVL]));
        }

        // Debug.Log(defeatedStats[CV] + "*" + percentOwed);
        vaultValue += defeatedStats[CV] * percentOwed;
        // Debug.Log(vaultValue);
        xpVaultValue += defeatedStats[SXPV] * percentOwed;
    }

    public void MaidLevelUpFromKill(int maidId)
    {
        if (maidId != -1) // non tapMaid
        {
            // lvl up
            maidStats[maidId, LVL] += 1; // lvl
            maidStats[maidId, HPL] += maidBaseStats[maidId, HPL] * Mathf.Pow(1.065f, maidStats[maidId, LVL]); //maidStats[maidId, LVL]; // hitpool
            maidStats[maidId, ATK] = maidBaseStats[maidId, ATK] * Mathf.Pow(1.065f, maidStats[maidId, LVL]); // maidStats[maidId, LVL]; // attack
            maidStats[maidId, CNTR] = 0;
        }
        else // tapMaid
        {
            tapMaidStats[LVL] += 1; // lvl
            tapMaidStats[0] = tapMaidBaseStats[HPL] * Mathf.Pow(1.05f, tapMaidStats[LVL]); // hitpool
            tapMaidStats[1] = tapMaidBaseStats[ATK] * Mathf.Pow(1.05f, tapMaidStats[LVL]); // tapMaidStats[LVL]; // attack
            tapMaidStats[CNTR] = 0;
        }
    }

    // lvl up evenly accross all maids
    // get average lvl, lvl up lowest lvl, repeat

    public void MaidLevelUpWithCoins(int maidId)
    {
        // TODO:
        // enough coins check
        // remove coins for payment

        if (maidId != -1) // non tapMaid
        {
            if (vaultValue >= GetCoinCost((int)maidStats[maidId, LVL]))
            {
                maidUnlocked[maidId] = true;

                maidStats[maidId, LVL] += 1; // lvl
                maidStats[maidId, HPL] = maidBaseStats[maidId, HPL] * maidStats[maidId, LVL]; // hitpool
                maidStats[maidId, ATK] = maidBaseStats[maidId, ATK] * maidStats[maidId, LVL]; // attack

                vaultValue -= GetCoinCost(((int)maidStats[maidId, LVL] - 1));
            }
        }
        else // tapMaid
        {
            if (vaultValue >= GetCoinCost((int)tapMaidStats[LVL]))
            {
                tapMaidStats[LVL] += 1; // lvl
                tapMaidStats[0] = tapMaidBaseStats[HPL] * tapMaidStats[LVL]; // hitpool
                tapMaidStats[1] = tapMaidBaseStats[ATK] * tapMaidStats[LVL]; // attack

                vaultValue -= GetCoinCost((int)tapMaidStats[LVL] - 1);
            }
        }
    }

    private int GetCoinCost(int lvl)
    {
        if (lvl != 0)
            return (int)(lvl * 11.7f) + (lvl * lvl);
        return 10;
    }

    public void MaidLevelUpWithSharedXp(int maidId)
    {
        // TODO:
        // enough shared xp check
        // remove shared xp for payment

        if (maidId != -1) // non tapMaid
        {
            maidUnlocked[maidId] = true;

            // lvl up
            maidStats[maidId, LVL] += 1; // lvl
            maidStats[maidId, HPL] = maidBaseStats[maidId, HPL] * maidStats[maidId, LVL]; // hitpool
            maidStats[maidId, ATK] = maidBaseStats[maidId, ATK] * maidStats[maidId, LVL]; // attack
        }
        else // tapMaid
        {
            tapMaidStats[LVL] += 1; // lvl
            tapMaidStats[0] = tapMaidBaseStats[HPL] * tapMaidStats[LVL]; // hitpool
            tapMaidStats[1] = tapMaidBaseStats[ATK] * tapMaidStats[LVL]; // attack
        }
    }

    public GameObject GetNewEnemy()
    {
        return pf_enemy[Random.Range(0, pf_enemy.Length)];
    }

    /*
    public GameObject GetNewMirrorEnemy()
    {
        return pf_mirrorEnemy[Random.Range(0, pf_mirrorEnemy.Length)];
    }
    */

    /*
    public float GetEnemyHealthRegenDelay()
    {
        // add enemy modifiers here
        return enemyHealthRechargeDelay;
    }

    public float GetEnemyHealthRegenPercentPerTick()
    {
        // add enemy modifiers here
        return enemyHealthRegenPercentPerTick;
    }
    */

    public float GetEnemyRespawnTime()
    {
        // add enemy respawn time modifiers here
        return 3f; // enemyBaseRespawnTime;
    }

    public GameObject GetTapMaid()
    {
        return pf_tapMaid;
    }

    public void AddMaidToHolder(GameObject maid)
    {
        maid.transform.SetParent(maidHolder);
    }

    public Transform GetMaidHolder()
    {
        return maidHolder;
    }

    public int GetMaidCounter(int maidId)
    {
        if (maidId != -1)
        {
            maidStats[maidId, CNTR] += 1;
            return (int)maidStats[maidId, CNTR] - 1;
        }
        else
        {
            tapMaidStats[CNTR] += 1;
            return (int)tapMaidStats[CNTR] - 1;
        }
    }

    public int GetMaxActiveTapMaids()
    {
        return maxActiveTapMaids;
    }

    public int GetActiveTapMaids()
    {
        return activeTapMaids;
    }

    public void AddActiveTapMaid()
    {
        activeTapMaids += 1;
    }

    public void RemoveActiveTapMaid()
    {
        activeTapMaids -= 1;
    }

    public void ShowHideStatPanel()
    {
        if (statPanelExpanded)
            statPanel.transform.position = new Vector2(0f, statPanel.transform.position.y);
        else
            statPanel.transform.position = new Vector2(15, statPanel.transform.position.y);

        statPanelExpanded = !statPanelExpanded;
    }
}
