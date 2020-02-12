using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallaxer : MonoBehaviour
{
    // Start is called before the first frame update
    class PoolObject{
        public Transform transform;
        public bool inUse;

        public PoolObject(Transform t){
           transform = t; 
        }

        public void Use(){
            inUse = true;
        }  

        public void Dispose(){
            inUse = false;
        }  
    }

    [System.Serializable]
    public struct YSpawnRange {
        public float min;
        public float max;
    }

    public GameObject Prefab;
    public int poolSize;
    public float spawnRate;
    public float shiftSpeed; 

    public YSpawnRange ySpawnRange;
    public Vector3 defaultSpawnPosition;
    public bool spawnImmediate;
    public Vector3 immediateSpawnPosition;
    public Vector2 targetAspectRatio;

    float spawnTimer;
    float targetAspect;
    PoolObject[] poolObjects;

    GameManager gameManager;

    void Awake(){
        Configure();
    }

    void Start(){
       gameManager = GameManager.Instance; 
    }

    void OnEnable(){
        GameManager.OnGameOverConfirmed += OnGameOverConfirmed;
    }

    void OnDisable(){
        GameManager.OnGameOverConfirmed -= OnGameOverConfirmed;
    }

    void OnGameOverConfirmed(){
        for(int i = 0; i < poolObjects.Length; i++){
            poolObjects[i].Dispose();
            // transform.localPosition
            poolObjects[i].transform.localPosition = Vector3.one * 1000;
        }

        if(spawnImmediate){
            SpawnImmediate();
        }
    }

    void Update(){
        if(gameManager.GameOver) return;

        Shift();

        spawnTimer += Time.deltaTime;
        if(spawnTimer > spawnRate){
            Spawn();
            spawnTimer = 0;
        } 
    }

    void Configure(){
        targetAspect = targetAspectRatio.x / targetAspectRatio.y;
        poolObjects = new PoolObject[poolSize];
        for(int i = 0; i < poolObjects.Length; i++){
            GameObject gameObject = Instantiate(Prefab) as GameObject;
            gameObject.SetActive(true);
            Transform t = gameObject.transform;
            t.SetParent(transform);
            t.position = Vector3.one * 1000;
            poolObjects[i] = new PoolObject(t);
        }

        if(spawnImmediate){
            SpawnImmediate();
        }
    }

    void Spawn(){
        Transform t = GetPoolObject();
        if(t == null) return;
        Vector3 position = Vector3.zero;
        position.x = (defaultSpawnPosition.x * Camera.main.aspect) / targetAspect;
        position.y = Random.Range(ySpawnRange.min, ySpawnRange.max);
        t.position = position;
    }

    void SpawnImmediate(){
        Transform t = GetPoolObject();
        if(t == null) return;
        Vector3 position = Vector3.zero;
        position.x = (immediateSpawnPosition.x * Camera.main.aspect) / targetAspect;
        position.y = Random.Range(ySpawnRange.min, ySpawnRange.max);
        t.position = position;
        Spawn();
    }

    void Shift(){
        for(int i = 0; i < poolObjects.Length; i++){
            poolObjects[i].transform.localPosition += -Vector3.right * shiftSpeed * Time.deltaTime;
            CheckDisposeObject(poolObjects[i]);
        }
    }

    void CheckDisposeObject(PoolObject poolObject){
        // transform.localPosition
        if(poolObject.transform.localPosition.x < (-defaultSpawnPosition.x * Camera.main.aspect) / targetAspect){
            poolObject.Dispose();
            // dont visible
            poolObject.transform.localPosition = Vector3.one * 1000;
        }
    }

    Transform GetPoolObject(){
       for(int i = 0; i < poolObjects.Length; i++){
           if(!poolObjects[i].inUse){
               poolObjects[i].Use();
               return poolObjects[i].transform;
           }
       } 
       return null;
    }
}
