using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GardenPool : MonoBehaviour
{

    public static GardenPool instance;


    public Transform worldReference;
    public Transform stickersAreaReference;

    public GameObject gnomePrefab;
    public GameObject bootyBoxPrefab;
    public GameObject bombPrefab;
    public GameObject gnomeStickerPrefab;
    public GameObject bombStickerPrefab;

    //offers
    public GameObject offerPrefab;
    public List<GameObject> offersLv1;
    public List<GameObject> offersLv2;
    public List<GameObject> offersLv3;
    public List<GameObject> offersLv4;
    public List<GameObject> offersLv5;

    //stickers
    public List<GameObject> bootyBoxStickerPrefabs;

    public int poolSize;


    [Header("Debug")]
    [SerializeField] Queue<GameObject> gnomesPool;
    [SerializeField] Queue<GameObject> bootyBoxesPool;
    [SerializeField] Queue<GameObject> bombsPool;
    [SerializeField] Queue<GameObject> offersPool;
    [SerializeField] Queue<GameObject> gnomeStickersPool;
    [SerializeField] Queue<GameObject> bombStickersPool;
    [SerializeField] Queue<GameObject> bootyBoxStickersPool;
    [SerializeField] int currentCount;


    //asset IDs
    public System.Guid gnomeAssetId
    {
        get; set;
    }
    public System.Guid bootyBoxAssetId
    {
        get; set;
    }
    public System.Guid bombAssetId
    {
        get; set;
    }
    public System.Guid offerAssetId
    {
        get; set;
    }

    public System.Guid bombStickerAssetId
    {
        get; set;
    }

    public System.Guid gnomeStickerAssetId
    {
        get; set;
    }

    public Dictionary<Vector3Int, System.Guid> bootyBoxStickerAssetIds
    {
        get; set;
    }

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

    void InitializeAssetsIds()
    {
        gnomeAssetId = gnomePrefab.GetComponent<NetworkIdentity>().assetId;
        bootyBoxAssetId = bootyBoxPrefab.GetComponent<NetworkIdentity>().assetId;
        bombAssetId = bombPrefab.GetComponent<NetworkIdentity>().assetId;
        offerAssetId = offerPrefab.GetComponent<NetworkIdentity>().assetId;
        gnomeStickerAssetId = gnomeStickerPrefab.GetComponent<NetworkIdentity>().assetId;
        bombStickerAssetId = bombStickerPrefab.GetComponent<NetworkIdentity>().assetId;

        bootyBoxStickerAssetIds = new Dictionary<Vector3Int, System.Guid>();

        foreach (GameObject sticker in bootyBoxStickerPrefabs)
        {
            bootyBoxStickerAssetIds.Add(sticker.GetComponent<Sticker>().cellSize, sticker.GetComponent<NetworkIdentity>().assetId);
        }
    }

    void RegisterSpawnHandlers()
    {
        //actors
        ClientScene.RegisterSpawnHandler(gnomeAssetId, SpawnActor, UnSpawnActor);
        ClientScene.RegisterSpawnHandler(bootyBoxAssetId, SpawnActor, UnSpawnActor);
        ClientScene.RegisterSpawnHandler(bombAssetId, SpawnActor, UnSpawnActor);
        ClientScene.RegisterSpawnHandler(offerAssetId, SpawnActor, UnSpawnActor);
        ClientScene.RegisterSpawnHandler(offerAssetId, SpawnOffer, UnSpawnOffer);

        //stickers
        ClientScene.RegisterSpawnHandler(gnomeStickerAssetId, SpawnActor, UnSpawnActor);
        ClientScene.RegisterSpawnHandler(bombStickerAssetId, SpawnOffer, UnSpawnOffer);

        foreach (KeyValuePair<Vector3Int, System.Guid> v in bootyBoxStickerAssetIds)
        {
            ClientScene.RegisterSpawnHandler(v.Value, SpawnActor, UnSpawnActor);
        }
    }

    void InitializePool()
    {
        //pools
        gnomesPool = new Queue<GameObject>();
        bootyBoxesPool = new Queue<GameObject>();
        bombsPool = new Queue<GameObject>();
        offersPool = new Queue<GameObject>();

        gnomeStickersPool = new Queue<GameObject>();
        bombStickersPool = new Queue<GameObject>();
        bootyBoxStickersPool = new Queue<GameObject>();

        GameObject prefab;

        //gnomes
        for (int i = 0; i < 4; i++)
        {
            prefab = CreateNew(gnomeAssetId);
            gnomesPool.Enqueue(prefab);
        }

        //booty
        for (int i = 0; i < 7; i++)
        {
            prefab = CreateNew(bootyBoxAssetId);
            bootyBoxesPool.Enqueue(prefab);
        }

        //bombs
        for (int i = 0; i < 5; i++)
        {
            prefab = CreateNew(bombAssetId);
            bombsPool.Enqueue(prefab);
        }

        //UI

        //offers
        for (int i = 0; i < 4; i++)
        {
            prefab = CreateNew(offerAssetId);
            offersPool.Enqueue(prefab);
        }
        
        //gnome stickers
        for (int i = 0; i < 4; i++)
        {
            prefab = CreateNew(gnomeStickerAssetId);
            gnomeStickersPool.Enqueue(prefab);
        }

        //bomb stickers
        for (int i = 0; i < 10; i++)
        {
            prefab = CreateNew(bombStickerAssetId);
            bombStickersPool.Enqueue(prefab);
        }

        //booty stickers
        foreach (GameObject sticker in bootyBoxStickerPrefabs)
        {
            System.Guid _id;
            bootyBoxStickerAssetIds.TryGetValue(sticker.GetComponent<Sticker>().cellSize, out _id);

            prefab = CreateNew(_id);
            bombsPool.Enqueue(prefab);
        }
    }

    GameObject CreateNew(System.Guid assetId)
    {
        GameObject prefab = GetPrefab(assetId);

        // use this object as parent so that objects dont crowd hierarchy
        GameObject actor = Instantiate(prefab, worldReference);
        actor.name = prefab.name;
        actor.SetActive(false);
        return actor;
    }
    
    GameObject GetPrefab(System.Guid assetId)
    {
        if (assetId == gnomeAssetId)
            return gnomePrefab;

        if (assetId == bootyBoxAssetId)
            return bootyBoxPrefab;

        if (assetId == bombAssetId)
            return bombPrefab;

        if (assetId == offerAssetId)
            return offerPrefab;

        if (assetId == gnomeStickerAssetId)
            return gnomeStickerPrefab;
        
        if (assetId == bombStickerAssetId)
            return bombStickerPrefab;

        if (bootyBoxStickerAssetIds.ContainsValue(assetId))
        {
            foreach (GameObject _stickerPrefab in bootyBoxStickerPrefabs)
            {
                if (assetId == _stickerPrefab.GetComponent<NetworkIdentity>().assetId)
                    return _stickerPrefab;
            }
        }

        return null;
    }
    
    Queue<GameObject> GetTargetPool(System.Guid assetId)
    {
        if (assetId == gnomeAssetId)
        {
            return gnomesPool;
        }

        if (assetId == bootyBoxAssetId)
        {
            return bootyBoxesPool;
        }

        if (assetId == bombAssetId)
        {
            return bombsPool;
        }

        if (assetId == offerAssetId)
        {
            return offersPool;
        }

        if (assetId == gnomeStickerAssetId)
        {
            return gnomeStickersPool;
        }

        if (assetId == bombStickerAssetId)
        {
            return bombStickersPool;
        }

        if (bootyBoxStickerAssetIds.ContainsValue(assetId))
        {
            return bootyBoxStickersPool;
        }

        return null;
    }

    public GameObject GetFromPool(System.Guid assetId)
    {
        //check pool
        Queue<GameObject> targetPool = GetTargetPool(assetId);

        if (targetPool == null)
            Debug.Log("Pool not found");

        //dequeue object or create new
        GameObject poolable = targetPool.Count > 0 ? targetPool.Dequeue() : CreateNew(assetId);

        if (poolable == null)
            Debug.LogError("Could not grab game object from pool, nothing available");

        poolable.SetActive(true);
        return poolable;
    }

    public GameObject SpawnActor(Vector3 position, System.Guid assetId)
    {
        return GetFromPool(assetId);
    }

    public void UnSpawnActor(GameObject spawned)
    {
        Debug.Log("Re-pooling game object " + spawned.name);
        spawned.SetActive(false);
    }

    public GameObject SpawnOffer(Vector3 position, System.Guid assetId)
    {
        return GetFromPool(assetId);
    }

    public void UnSpawnOffer(GameObject spawned)
    {
        Debug.Log("Re-pooling game object " + spawned.name);
        spawned.SetActive(false);
    }

    public GameObject GetRandomOfferByRound(int round)
    {
        // 20+     - OfferLv5
        if (round > 20)
            return offersLv5.ToArray()[Random.Range(0, offersLv5.Count)];

        // 10 - 20 - OfferLv4
        if (round > 10)
            return offersLv4.ToArray()[Random.Range(0, offersLv4.Count)];

        // 5 - 10  - OfferLv3
        if (round > 5)
            return offersLv3.ToArray()[Random.Range(0, offersLv3.Count)];

        // 3 - 5   - OfferLv2
        if (round > 2)
            return offersLv2.ToArray()[Random.Range(0, offersLv2.Count)];

        // 1 - 2   - OfferLv1
        return offersLv1.ToArray()[Random.Range(0, offersLv1.Count)];
    }

}
