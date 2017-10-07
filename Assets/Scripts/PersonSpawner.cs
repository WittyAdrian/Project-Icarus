using UnityEngine;

public class PersonSpawner : MonoBehaviour {

    PersonBehaviour spawnBehaviour;
    GameObject ground, spawn, townCenter;
    Renderer townRenderer;
    Terrain thisTerrain;
    TerrainData thisData;
    Camera thisCamera;

    // Use this for initialization
    void Start() {
        ground = GameObject.Find("Ground");
        thisTerrain = Terrain.activeTerrain;
        thisData = thisTerrain.terrainData;
        thisCamera = GameObject.Find("Camera").GetComponent<Camera>();

        townCenter = GameObject.Instantiate(Resources.Load("Prefabs/TownCenter")) as GameObject;
        townRenderer = townCenter.GetComponent<Renderer>();
        townRenderer.material = (Material)Resources.Load("Materials/Outline");
    }

    // Update is called once per frame
    void Update() {
        if (townRenderer.material.name.Contains("Outline")) {
            Ray ray = thisCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitPoint;

            int groundLayer = 1 << 8;
            if (Physics.Raycast(ray, out hitPoint, float.PositiveInfinity, groundLayer)) {
                Vector3 targetPos = WorldController.getGrid(hitPoint.point);

                float thisHeight = thisData.GetHeight((int)targetPos.x, (int)targetPos.z) + ground.transform.position.y + townCenter.transform.localScale.y / 2;
                townCenter.transform.position = new Vector3(targetPos.x, thisHeight, targetPos.z);
            }
            // int thisX = (int)((transform.position.x - ground.transform.position.x) * terrainOffset), thisZ = (int)((transform.position.z - ground.transform.position.z) * terrainOffset);
            // float thisHeight = thisData.GetHeight(thisX, thisZ);

            // townCenter.transform.position = new Vector3(transform.position.x, thisHeight + ground.transform.position.y + townCenter.transform.localScale.y / 2, transform.position.z);
        }

        if (Input.GetMouseButtonDown(0)) {
            if (townRenderer.material.name.Contains("Outline")) {
                //int thisX = (int)((transform.position.x - ground.transform.position.x) * terrainOffset), thisZ = (int)((transform.position.z - ground.transform.position.z) * terrainOffset);
                //float[,] heightMap = thisData.GetHeights(thisX - 8, thisZ - 6, 16, 16);
                //float townHeight = (townCenter.transform.position.y - ground.transform.position.y - 2) / 2000;

                //for (int i = 0; i < 16; i++) {
                //    for (int j = 0; j < 16; j++) {
                //        heightMap[i, j] = townHeight;
                //    }
                //}

                //thisData.SetHeights(thisX - 8, thisZ - 6, heightMap);
                townRenderer.material = (Material)Resources.Load("Materials/Metal");

                //GameObject stockpile = GameObject.Instantiate(Resources.Load("Prefabs/Stockpile")) as GameObject;
                //stockpile.transform.position = townRenderer.transform.position + new Vector3(10, -2, 0);

                WorldController.addBuildingToMap(townCenter.transform);
                //WorldController.addBuildingToMap(stockpile.transform);
                WorldController.updateMap();

                HudBehaviour.setTutorialMessage("");

                //int[,] noGrass = new int[(int)stockpile.transform.localScale.z * 2, (int)stockpile.transform.localScale.x * 2];
                //thisData.SetDetailLayer(((int)stockpile.transform.position.x - (int)thisTerrain.transform.position.x) * 2 - (int)stockpile.transform.localScale.x, ((int)stockpile.transform.position.z - (int)thisTerrain.transform.position.z) * 2 - (int)stockpile.transform.localScale.z, 2, noGrass);
                //thisData.SetDetailLayer(((int)stockpile.transform.position.x - (int)thisTerrain.transform.position.x) * 2 - (int)stockpile.transform.localScale.x, ((int)stockpile.transform.position.z - (int)thisTerrain.transform.position.z) * 2 - (int)stockpile.transform.localScale.z, 6, noGrass);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            SpawnPerson("Civilian", new Vector3(0, -1, 0));

            WorldController.citizenCount++;
            WorldController.updateCounters();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            SpawnPerson("Knight", new Vector3(0, -1, 0));
        }

        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            SpawnPerson("Archer", new Vector3(0, -1, 0));
        }

        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            SpawnPerson("Mage", new Vector3(0, -1, 0));
        }

        if (Input.GetKeyDown(KeyCode.F)) {
            for (int i = 0; i < 10; i++) {
                for (int j = 0; j < 10; j++) {
                    SpawnPerson("", new Vector3(i, -1, j));
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Q)) {
            spawn = GameObject.Instantiate(Resources.Load("Prefabs/Hound")) as GameObject;
            spawn.transform.position = transform.position - new Vector3(0, 1, 0);
        }
    }

    void SpawnPerson(string role, Vector3 offset) {
        spawn = GameObject.Instantiate(Resources.Load("Prefabs/Person")) as GameObject;

        spawn.GetComponent<PersonBehaviour>().roleForce = role;
        spawn.transform.position = transform.position + offset;
    }
}
