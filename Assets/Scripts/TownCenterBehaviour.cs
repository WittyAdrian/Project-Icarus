using UnityEngine;

public class TownCenterBehaviour : MonoBehaviour {

    public int currentSpawns = 0;

    int maxSpawns = 10, spawnRate = 5;
    float spawnCooldown;

    PersonBehaviour spawnBehaviour;
    GameObject spawn;
    Renderer thisRenderer;

    //List<GameObject> currentSpawns;

    // Use this for initialization
    void Start() {
        thisRenderer = GetComponent<Renderer>();

        spawnCooldown = spawnRate;
        //currentSpawns = new List<GameObject>();
    }

    // Update is called once per frame
    void Update() {
        if (!thisRenderer.material.name.Contains("Outline")) {
            if (spawnCooldown <= 0 && currentSpawns < maxSpawns) {
                Spawn("Civilian");
                spawnCooldown = spawnRate;
            } else if (spawnCooldown > 0) {
                spawnCooldown -= 1 * Time.deltaTime;
            }
        }
    }

    void Spawn(string role) {
        spawn = GameObject.Instantiate(Resources.Load("Prefabs/Person")) as GameObject;
        currentSpawns++;

        spawnBehaviour = (PersonBehaviour)spawn.GetComponent(typeof(PersonBehaviour));
        spawnBehaviour.roleForce = role;
        spawnBehaviour.myTownCenter = this.gameObject;

        spawn.transform.position = transform.position + new Vector3(1.5F, -1F, 5F);

        WorldController.citizenCount++;
        WorldController.updateCounters();
    }
}
