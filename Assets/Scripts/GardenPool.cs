using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolableInfo
{
    public GameObject prefab;
    public Transform container;
    public int poolSize;
}

public class GardenPool : MonoBehaviour
{
    public static GardenPool instance;
    
    [Header("Parent references")]
    public Transform worldReference;
    public Transform stickersAreaReference;

    [Header("Actor references")]
    public GameObject gnomePrefab;
    public GameObject bootyBoxPrefab;
    public GameObject bootyPrefab;
    public GameObject bombPrefab;

    //offers
    [Header("offer references")]
    public GameObject offerPrefab;

    //move to server
    public List<GameObject> offersLv1;
    public List<GameObject> offersLv2;
    public List<GameObject> offersLv3;
    public List<GameObject> offersLv4;
    public List<GameObject> offersLv5;

    //stickers
    [Header("Sticker references")]
    public GameObject gnomeStickerPrefab;
    public GameObject bombStickerPrefab;
    public List<GameObject> bootyBoxStickerPrefabs;

    //network pools
    [SerializeField] Queue<GameObject> gnomesPool;
    [SerializeField] Queue<GameObject> bootyPool;
    [SerializeField] Queue<GameObject> bootyBoxesPool;
    [SerializeField] Queue<GameObject> bombsPool;

    //local pools
    [SerializeField] Queue<GameObject> offersPool;
    [SerializeField] Queue<GameObject> gnomeStickersPool;
    [SerializeField] Queue<GameObject> bombStickersPool;
    [SerializeField] Queue<GameObject> bootyBoxStickersPool;
    [SerializeField] int currentCount;
    
    //asset IDs
    System.Guid gnomeAssetId {get; set; }
    System.Guid bootyAssetId { get; set; }
    System.Guid bootyBoxAssetId { get; set; }
    System.Guid bombAssetId { get; set; }

    // Handles requests to spawn / unspawn game objects on the client
    public delegate GameObject SpawnDelegate(Vector3 position, System.Guid assetId);
    public delegate void UnSpawnDelegate(GameObject spawned);

    void Awake()
    {
        if (instance != null)
            Destroy(this);

        instance = this;
    }

    void Start()
    {
        //init assets ids
        InitializeAssetsIds();

        //instantiate pool objects
        InitializePool();

        //register spawn handlers
        RegisterSpawnHandlers();
    }

    //asset ids
    void InitializeAssetsIds()
    {
        gnomeAssetId = gnomePrefab.GetComponent<NetworkIdentity>().assetId;
        bootyBoxAssetId = bootyBoxPrefab.GetComponent<NetworkIdentity>().assetId;
        bootyAssetId = bootyPrefab.GetComponent<NetworkIdentity>().assetId;
        bombAssetId = bombPrefab.GetComponent<NetworkIdentity>().assetId;
    }

    void RegisterSpawnHandlers()
    {
        //actors
        ClientScene.RegisterSpawnHandler(gnomeAssetId, SpawnActor, UnSpawnActor);
        ClientScene.RegisterSpawnHandler(bootyBoxAssetId, SpawnActor, UnSpawnActor);
        ClientScene.RegisterSpawnHandler(bootyAssetId, SpawnActor, UnSpawnActor);
        ClientScene.RegisterSpawnHandler(bombAssetId, SpawnActor, UnSpawnActor);
    }

    void InitializePool()
    {
        //pools
        gnomesPool = new Queue<GameObject>();
        bootyBoxesPool = new Queue<GameObject>();
        bootyPool = new Queue<GameObject>();
        bombsPool = new Queue<GameObject>();
        offersPool = new Queue<GameObject>();

        gnomeStickersPool = new Queue<GameObject>();
        bombStickersPool = new Queue<GameObject>();
        bootyBoxStickersPool = new Queue<GameObject>();

        GameObject prefab;

        //gnomes
        for (int i = 0; i < 1; i++)
        {
            prefab = CreateNewActor(gnomeAssetId);
            gnomesPool.Enqueue(prefab);
        }

        //booty
        for (int i = 0; i < 7; i++)
        {
            prefab = CreateNewActor(bootyBoxAssetId);
            bootyBoxesPool.Enqueue(prefab);
        }

        //bombs
        for (int i = 0; i < 1; i++)
        {
            prefab = CreateNewActor(bombAssetId);
            bombsPool.Enqueue(prefab);
        }

        //UI

        //offers
        for (int i = 0; i < 4; i++)
        {
            prefab = CreateNewSticker(offerPrefab);
            offersPool.Enqueue(prefab);
        }
        
        //gnome stickers
        for (int i = 0; i < 4; i++)
        {
            prefab = CreateNewSticker(gnomeStickerPrefab);
            gnomeStickersPool.Enqueue(prefab);
        }

        //bomb stickers
        for (int i = 0; i < 10; i++)
        {
            prefab = CreateNewSticker(bombStickerPrefab);
            bombStickersPool.Enqueue(prefab);
        }

        //booty stickers
        foreach (GameObject sticker in bootyBoxStickerPrefabs)
        {
            prefab = CreateNewSticker(sticker);
            bootyBoxStickersPool.Enqueue(prefab);
        }
    }

    GameObject CreateNewActor(System.Guid assetId)
    {
        GameObject prefab = GetPrefab(assetId);

        // use this object as parent so that objects dont crowd hierarchy
        GameObject actor = Instantiate(prefab, worldReference);
        actor.name = prefab.name;
        actor.SetActive(false);
        return actor;
    }

    GameObject CreateNewSticker(GameObject prefab)
    {
        // use this object as parent so that objects dont crowd hierarchy
        GameObject actor = Instantiate(prefab, worldReference);
        actor.name = prefab.name;
        actor.SetActive(false);
        return actor;
    }

    GameObject GetPrefab(System.Guid assetId)
    {
        if (assetId == bombAssetId)
            return bombPrefab;

        if (assetId == gnomeAssetId)
            return gnomePrefab;

        if (assetId == bootyAssetId)
            return bootyPrefab;

        if (assetId == bootyBoxAssetId)
            return bootyBoxPrefab;
        
        return null;
    }
    
    Queue<GameObject> GetTargetNetworkPool(System.Guid assetId)
    {
        if (assetId == gnomeAssetId)
            return gnomesPool;

        if (assetId == bootyAssetId)
            return bootyPool;

        if (assetId == bootyBoxAssetId)
            return bootyBoxesPool;

        if (assetId == bombAssetId)
            return bombsPool;
        
        return null;
    }
    
    //GET FROM LOCAL POOL
    public GameObject GetFromPool(GameObject prefab)
    {
        Queue<GameObject> targetPool = GetTargetLocalPool(prefab);

        //dequeue object or create new
        GameObject poolable = targetPool.Count > 0 ? targetPool.Dequeue() : CreateNewSticker(prefab);

        if (poolable == null)
            Debug.LogError("Could not grab game object from pool, nothing available");

        poolable.SetActive(true);
        return poolable;
    }

    //GET FROM NETWORK POOL
    public GameObject GetFromPool(System.Guid assetId)
    {
        //check pool
        Queue<GameObject> targetPool = GetTargetNetworkPool(assetId);

        if (targetPool == null)
            Debug.Log("Pool not found");

        //dequeue object or create new
        GameObject poolable = targetPool.Count > 0 ? targetPool.Dequeue() : CreateNewActor(assetId);

        if (poolable == null)
            Debug.LogError("Could not grab game object from pool, nothing available");

        poolable.SetActive(true);
        return poolable;
    }

    public GameObject SpawnActor(SpawnMessage msg)
    {
        return GetFromPool(msg.assetId);
    }

    public void UnSpawnActor(GameObject spawned)
    {
        PutBackInNetworkPool(spawned, spawned.GetComponent<NetworkIdentity>().assetId);
        Debug.Log("Re-pooling game object " + spawned.name);
    }


    public void PutBackInLocalPool(GameObject spawned)
    {
        // disable object
        spawned.SetActive(false);

        //check pool
        Queue<GameObject> targetPool = GetTargetLocalPool(spawned);

        offersPool.Enqueue(spawned);
    }

    Queue<GameObject> GetTargetLocalPool(GameObject prefab)
    {
        if (prefab.GetComponent<TreatOffer>())
            return offersPool;
        else if (prefab.GetComponent<GnomeUISticker>())
            return gnomeStickersPool;
        else if (prefab.GetComponent<BootyUISticker>())
            return bootyBoxStickersPool;
        else if (prefab.GetComponent<BombUISticker>())
            return bombStickersPool;

        return null;
    }

    //put back in pool
    public void PutBackInNetworkPool(GameObject spawned, System.Guid _assetId)
    {
        // disable object
        spawned.SetActive(false);

        // add back to pool
        GetTargetNetworkPool(_assetId).Enqueue(spawned);
    }

    public GameObject GetRandomOfferByRound(int round)
    {
        // 20+     - OfferLv5
        //if (round > 20)
        //    return offersLv5.ToArray()[Random.Range(0, offersLv5.Count)];
        //
        //// 10 - 20 - OfferLv4
        //if (round > 10)
        //    return offersLv4.ToArray()[Random.Range(0, offersLv4.Count)];
        //
        //// 5 - 10  - OfferLv3
        //if (round > 5)
        //    return offersLv3.ToArray()[Random.Range(0, offersLv3.Count)];
        //
        //// 3 - 5   - OfferLv2
        //if (round > 2)
        //    return offersLv2.ToArray()[Random.Range(0, offersLv2.Count)];

        // 1 - 2   - OfferLv1
        return offersLv1.ToArray()[Random.Range(0, offersLv1.Count)];
    }

}
