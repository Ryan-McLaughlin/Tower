using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FloorManager : MonoBehaviour
{
    public GameObject pf_onClickMaid;
    public Transform enemyHolder;

    // private Vector2[] enemy_spawnPoint = new[] { new Vector2(3.6f, 0.1f), new Vector2(4.4f, 0.1f), new Vector2(6.6f, 0.1f) };
    // private Vector2[] m_enemy_spawnPoint = new[] { new Vector2(-3.6f, 0.1f), new Vector2(-4.4f, 0.1f), new Vector2(-6.6f, 0.1f) };
    private Vector2 position;

    private GameObject instance;
    private int floorNumber = 2;
    private bool locked = false, initialized = false;
    private float x, y;

    /* stat.us/istic variables
     * [ ATK ] attack
     * [ CV ] coin_value
     * [ HP ] hitpoints
     * [ HPL ] hitpool
     * [ ID ] id
     * [ LVL ] lvl
     * [ SXPV ] shared_xp_value
     * [ XPC ] xp_current
     * [ XPTL ] xp_toLvL
     * [ XPV ] xp_value
     */
    protected readonly int HPL = 0, ATK = 1, LVL = 2, HP = 3, ID = 4;
    protected readonly int XPV = 5, CV = 6, SXPV = 7, XPC = 8, XPTL = 9;
    // private float[] eStats = new float[10];

    // enemy fpArray is given by GM
    private GameObject[] pf_enemyA;
    private GameObject[] enemyA;
    private int[] maidCounter;

    private float[] respawnTimer;
    private GameManager gm;
    private Text text;

    private List<GameObject> mList;

    void Update()
    {
        // this block is for summoning enemies
        if (!locked && initialized)
        {
            for (int i = 0; i < pf_enemyA.Length; i++)
            {
                if (enemyA[i] == null && respawnTimer[i] <= 0)
                    respawnTimer[i] = gm.GetEnemyRespawnTime();

                if (respawnTimer[i] > 0)
                {
                    respawnTimer[i] -= Time.deltaTime;

                    // bang, new enemy
                    if (respawnTimer[i] <= 0)
                    {
                        pf_enemyA[i] = gm.GetNewEnemy();
                        NewFloorEnemy(i);
                    }
                }
            }
        }
    }

    void OnMouseDown()
    {
        if (!initialized)
            Debug.Log("Floor " + floorNumber + " not initialized");
        TapMaid();
    }

    private void InitFloor()
    {// makes a new floor and adds it to floorHolder which is owned by the GM

        position = new Vector2(gameObject.GetComponent<Transform>().position.x, gameObject.GetComponent<Transform>().position.y);
        for (int i = 0; i < pf_enemyA.Length; i++)
            NewFloorEnemy(i);
    }

    private void NewFloorEnemy(int enemyNumber)
    {// makes a new enemy and adds it to enemyHolder which is owned by the GM
        if (floorNumber % 2 == 0)
            enemyA[enemyNumber] = (GameObject)Instantiate(pf_enemyA[enemyNumber], position + new Vector2(3.6f - (1.6f * enemyNumber), .1f), Quaternion.Euler(89, 0, 0));
        else
        {
            enemyA[enemyNumber] = (GameObject)Instantiate(pf_enemyA[enemyNumber], position + new Vector2(-3.6f + (1.6f * enemyNumber), .1f), Quaternion.Euler(89, 0, 0));
            enemyA[enemyNumber].GetComponent<SpriteRenderer>().flipX = true;
            //enemyA[enemyNumber].transform.eulerAngles = Quaternion.Euler(89, 0, 0);
        }
        FinishEnemy(enemyNumber);
    }

    private void FinishEnemy(int ePosition)
    {// so this block isn't repeated in NewMirrorEnemy and NewFloorEnemy it exists here
        enemyA[ePosition].name = pf_enemyA[ePosition].name + " --> pos: " + ePosition;
        enemyA[ePosition].transform.SetParent(enemyHolder);

        // get enemy stats for initialization
        float[] eStats = gm.GetEnemyStats(floorNumber);

        // run enemy initialization
        enemyA[ePosition].GetComponent<Enemy>().Init(eStats, floorNumber);
    }

    private void NewMaid(GameObject maid, int maidId)
    {// makes a new maid and adds her to maidHolder which is owned by the GM       

        // Debug.Log(maid.name);
        instance = (GameObject)Instantiate(maid, position + new Vector2(Random.Range(-2.9f, -3.5f), Random.Range(1f, 1.5f)), Quaternion.identity);
        instance.name = maid.name;
        MaidBase mb = instance.GetComponent<MaidBase>();
        // mb.name = maid.name;

        float[] mStats = gm.GetMaidStats(maidId);
        // Debug.Log(mb.name);
        mb.SetHitpool(mStats[HPL]);
        mb.SetAttack(mStats[ATK]);

        mb.SetCurrentFloor(floorNumber);
        mb.SetId(maidId);
        instance.name = maid.name + " lvl:" + gm.GetMaidLevel(maidId) + " #" + gm.GetMaidCounter(maidId);

        // hierarchy placement
        instance.transform.SetParent(gm.GetMaidHolder());
    }

    private void NewMirrorMaid(GameObject maid, int maidId)
    {// makes a new mirror maid and adds her to maidHolder which is owned by the GM

        instance = (GameObject)Instantiate(maid, position + new Vector2(Random.Range(2.9f, 3.5f), Random.Range(1f, 1.5f)), Quaternion.identity);
        MaidBase mb = instance.GetComponent<MaidBase>();

        float[] mStats = gm.GetMaidStats(maidId);
        mb.SetAttack(mStats[HPL]);
        mb.SetHitpool(mStats[ATK]);

        mb.SetCurrentFloor(floorNumber);
        mb.SetId(maidId);
        instance.name = maid.name + " lvl:" + gm.GetMaidLevel(maidId) + " #" + gm.GetMaidCounter(maidId);

        // hierarchy placement
        instance.transform.SetParent(gm.GetMaidHolder());
    }

    public void TapMaid()
    {// tapMaid is seperate from the nonTapMaid array to avoid confusion when coding
     // therefore it made sense to give it it's own 'in-between' summon function called from OnMouseDown()

        if (!locked && gm.GetActiveTapMaids() < gm.GetMaxActiveTapMaids())
        {
            // regular floor code
            if (floorNumber % 2 == 0)
                NewMaid(pf_onClickMaid, -1);

            // mirror floor code
            else
                NewMirrorMaid(pf_onClickMaid, -1);

            gm.AddActiveTapMaid();
        }
        else if (gm.GetActiveTapMaids() < gm.GetMaxActiveTapMaids())
            Debug.Log("Max number of Tap Maids reached!");
        else if (locked)
            Debug.Log("Floor " + floorNumber + " is locked!");
    }

    public void GenerateMaid(GameObject maid, int maidId)
    {
        // regular floor code
        if (floorNumber % 2 == 0)
            NewMaid(maid, maidId);
        // mirror floor code
        else
            NewMirrorMaid(maid, maidId);
    }

    public void SetupFloor(int fn, GameObject maid, GameObject[] enemies)
    {// called from GM

        gm = Camera.main.GetComponent<GameManager>();
        this.floorNumber = fn;
        text = transform.Find("Floor_Canvas").Find("Floor_Text").GetComponent<Text>();
        text.text = floorNumber.ToString();
        enemyHolder = new GameObject("Enemies").transform;
        enemyHolder.SetParent(transform);
        pf_onClickMaid = maid;
        pf_enemyA = new GameObject[enemies.Length];
        enemyA = new GameObject[enemies.Length];
        pf_enemyA = enemies;
        mList = new List<GameObject>();

        respawnTimer = new float[pf_enemyA.Length];

        InitFloor();

        /*/ regular floor code
        if (floorNumber % 2 == 0)
            InitFloor();
        // mirror floor code
        else
            InitMirror();
        */

        initialized = true;
    }

    public void SetEnemies(GameObject[] inEnemy)
    {
        pf_enemyA = inEnemy;
    }

    public int GetFloorNumber()
    {
        return floorNumber;
    }

    public Vector2 GetPosition()
    {
        return position;
    }

    public void LockFloor()
    {
        if (locked)
            return;

        locked = true;
        for (int i = 0; i < enemyA.Length; i++)
            if (enemyA[i] != null)
                enemyA[i].GetComponent<Enemy>().End();
    }

    public void AddMaidToList(GameObject maid_)
    {
        if (mList != null)
        {

            foreach (var maid in mList)
            {
                if (maid == null)
                {
                    mList.Remove(maid);
                    return;
                }
                if (maid == maid_)
                    return;
            }
        }
        mList.Add(maid_);
    }

    public void RemvoveMaidFromList(GameObject maid)
    {
        mList.Remove(maid);
    }
}
