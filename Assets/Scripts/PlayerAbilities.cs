using UnityEngine;

public class PlayerAbilities : MonoBehaviour {

    Terrain thisTerrain;
    Camera thisCamera;

    GameObject outlineBuild = null;
    Material outlineMaterial = null;

    Collider holding = null;

    void Start() {
        thisTerrain = Terrain.activeTerrain;
        thisCamera = GameObject.Find("Camera").GetComponent<Camera>();
    }

    void Update() {
        if (Input.mousePosition.y > 50) {
            if (holding != null) {
                if (Input.GetMouseButton(0)) {
                    Ray ray = thisCamera.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hitPoint;

                    int ignoreLayer = ~(1 << 2);
                    if (Physics.Raycast(ray, out hitPoint, float.PositiveInfinity, ignoreLayer)) {
                        holding.transform.position = hitPoint.point + new Vector3(0, 4, 0);
                    }
                } else {
                    holding.GetComponent<Rigidbody>().useGravity = true;
                    holding.gameObject.layer = 0;
                    holding = null;
                }
            } else if (Input.GetMouseButtonDown(0)) {
                Ray ray = thisCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitPoint;

                if (Physics.Raycast(ray, out hitPoint)) {
                    if (HudBehaviour.activeAbility != "") {
                        switch (HudBehaviour.activeAbility) {
                            case "buildStockpile":
                                buildAction(4, 1);
                                break;
                            case "buildHouse":
                                buildAction(8, 2);
                                break;
                            case "buildBarracks":
                                buildAction(6, 2);
                                break;
                            case "pickup":
                                pickup(hitPoint.collider);
                                break;
                            case "kill":
                                kill(hitPoint.collider);
                                break;
                        }
                    } else {
                        WorldController.selectEntity(hitPoint.collider);
                    }
                }
            } else if (outlineBuild != null) {
                Ray ray = thisCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitPoint;

                int groundLayer = 1 << 8;
                if (Physics.Raycast(ray, out hitPoint, float.PositiveInfinity, groundLayer)) {
                    Vector3 targetPos = WorldController.getGrid(hitPoint.point);

                    float thisHeight = thisTerrain.terrainData.GetHeight((int)targetPos.x, (int)targetPos.z) + thisTerrain.transform.position.y + outlineBuild.transform.localScale.y / 2;
                    outlineBuild.transform.position = new Vector3(targetPos.x, thisHeight, targetPos.z);
                }
            } else if (HudBehaviour.activeAbility != "") {
                switch (HudBehaviour.activeAbility) {
                    case "buildStockpile":
                        outlineBuild = GameObject.Instantiate(Resources.Load("Prefabs/Stockpile")) as GameObject;
                        break;
                    case "buildHouse":
                        outlineBuild = GameObject.Instantiate(Resources.Load("Prefabs/TownCenter")) as GameObject;
                        break;
                    case "buildBarracks":
                        outlineBuild = GameObject.Instantiate(Resources.Load("Prefabs/Barracks")) as GameObject;
                        break;
                }

                if (outlineBuild != null) {
                    var outlineRenderer = outlineBuild.GetComponent<Renderer>();
                    outlineMaterial = outlineRenderer.material;
                    outlineRenderer.material = (Material)Resources.Load("Materials/Outline");

                    outlineBuild.GetComponent<BoxCollider>().enabled = false;
                }
            }
        }

        if (Input.GetMouseButtonDown(1)) {
            if (outlineBuild != null) {
                Destroy(outlineBuild.gameObject);
                outlineBuild = null;
            }

            HudBehaviour.activeAbility = "";
            Cursor.SetCursor((Texture2D)Resources.Load("Textures/cursor"), Vector2.zero, CursorMode.Auto);
        }
    }

    void pickup(Collider target) {
        if (target.name.Contains("Person")) {
            target.GetComponent<Rigidbody>().useGravity = false;
            target.gameObject.layer = 2;
            holding = target;
        }
    }

    void kill(Collider target) {
        if (target.name.Contains("Person")) {
            target.GetComponent<PersonBehaviour>().hurt(9000);
        } else if (target.tag == "Hostile") {
            target.GetComponent<HostileBehaviour>().hurt(9000);
        }
    }

    void buildAction(int wood, int stone) {
        GameObject plot = GameObject.Instantiate(Resources.Load("Prefabs/BuildingPlot")) as GameObject;
        //outlineBuild.transform.position += new Vector3(0, plot.transform.localScale.y / 2, 0);
        plot.transform.position = outlineBuild.transform.position;// - new Vector3(0, outlineBuild.transform.localScale.y / 2, 0);
        plot.transform.position = new Vector3(plot.transform.position.x, thisTerrain.terrainData.GetHeight((int)plot.transform.position.x, (int)plot.transform.position.z) + 0.5F, plot.transform.position.z);

        PlotBehaviour plotScript = plot.GetComponent<PlotBehaviour>();
        plotScript.setRequirements(wood, stone);

        var outlineRenderer = outlineBuild.GetComponent<Renderer>();
        outlineRenderer.material = outlineMaterial;

        outlineBuild.GetComponent<BoxCollider>().enabled = true;

        plotScript.setBuilding(outlineBuild);
        outlineBuild = null;

        if (!Input.GetKey(KeyCode.LeftShift)) {
            HudBehaviour.activeAbility = "";
        }
    }

    //void explosion(Vector3 position, float force, float radius) {
    //    var npcs = GameObject.FindGameObjectsWithTag("Person");
    //    //Debug.Log("Explosion! " + npcs.Length + " npcs found.");
    //    foreach (GameObject npc in npcs) {
    //        PersonBehaviour npcBehaviour = npc.GetComponent<PersonBehaviour>();
    //        npcBehaviour.addForce(force, position, radius);
    //    }
    //}

    //void paintTerrain(Vector2 position) {
    //    float[,,] element = new float[1, 1, 2]; // create a temp 1x1x2 array
    //                                            // splatmapData[y, x, 0] = element[0, 0, 0] = 0; // set the element and
    //                                            // splatmapData[y, x, 1] = element[0, 0, 1] = 1; // update splatmapData
    //    element[0, 0, 0] = 0;
    //    element[0, 0, 1] = 0;
    //    thisTerrain.terrainData.SetAlphamaps((int)position.x, (int)position.y, element); // update only the selected terrain element
    //}

    //void spawnPrefab(string name, Vector3 position) {
    //    GameObject spawn = GameObject.Instantiate(Resources.Load("Prefabs/" + name)) as GameObject;
    //    position += new Vector3(0, spawn.transform.localScale.y / 2, 0);
    //    spawn.transform.position = position;
    //}

    //void increaseTerrain(Vector3 position, int brushSize) {
    //    float[,] heightSetting = new float[brushSize, brushSize];
    //    float newHeight = (position.y + 1) / TerrainGeneration.totalTerrainSize;

    //    for (int i = 0; i < brushSize; i++) {
    //        for (int j = 0; j < brushSize; j++) {
    //            heightSetting[i, j] = newHeight;
    //        }
    //    }

    //    thisTerrain.terrainData.SetHeights((int)position.x - brushSize / 2, (int)position.y - brushSize / 2, heightSetting);
    //}

    //void spawnTree(Vector3 position) {
    //    var tree = new TreeInstance();
    //    tree.position = position;
    //    tree.prototypeIndex = Random.Range(0, 2);

    //    tree.heightScale = 1;
    //    tree.widthScale = 1;

    //    TerrainGeneration.treesAssignment.Add(thisTerrain.terrainData.treeInstanceCount, 0);
    //    thisTerrain.AddTreeInstance(tree);
    //}
}