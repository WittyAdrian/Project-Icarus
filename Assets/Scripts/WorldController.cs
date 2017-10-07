using UnityEngine;
using UnityEngine.UI;

public class WorldController : MonoBehaviour {

    public static char[,] worldMap;
    public static bool stockpileAvailable = false, resourcesAvailable = false, woodAvailable = false, stoneAvailable = false,
        plotBuildingAvailable = false, plotResourcesAvailable = false, plotWoodAvailable = false, plotStoneAvailable = false;
    public static int citizenCount = 0, woodCount = 0, stoneCount = 0, storageSpace = 0;
    public static GameObject activeBuilding;
    public static HudBehaviour hudScript;

    static Texture2D mapTexture, viewareaTexture;
    static int minimapSize = 256;
    static Color[] viewareaReset;
    static float selectedTimer;
    static Collider selectedEntity;

    void Start() {
        // Reset values
        citizenCount = 0;
        woodCount = 0;
        stoneCount = 0;
        storageSpace = 0;

        stockpileAvailable = false;
        woodAvailable = false;
        stoneAvailable = false;
        plotBuildingAvailable = false;
        plotResourcesAvailable = false;
        plotWoodAvailable = false;
        plotStoneAvailable = false;

        // Initialization
        worldMap = new char[TerrainGeneration.totalTerrainSize, TerrainGeneration.totalTerrainSize];
        mapTexture = new Texture2D(minimapSize, minimapSize, TextureFormat.ARGB32, false);
        viewareaTexture = new Texture2D(minimapSize, minimapSize, TextureFormat.ARGB32, false);
        viewareaReset = new Color[minimapSize * minimapSize];
        hudScript = GameObject.Find("HUD").GetComponent<HudBehaviour>();

        selectedTimer = 0F;
        selectedEntity = null;

        for (int i = 0; i < TerrainGeneration.totalTerrainSize; i++) {
            for (int j = 0; j < TerrainGeneration.totalTerrainSize; j++) {
                worldMap[i, j] = 'e';
                if (i < minimapSize && j < minimapSize) {
                    viewareaReset[i + j] = Color.clear;
                }
            }
        }

        viewareaTexture.SetPixels(viewareaReset);
        viewareaTexture.Apply();

        GameObject.Find("Minimap").GetComponent<Image>().material.mainTexture = mapTexture;
        GameObject.Find("MinimapViewarea").GetComponent<Image>().material.mainTexture = viewareaTexture;
    }

    void Update() {
        if(selectedTimer <= 0F) {
            if(selectedEntity != null) {
                selectEntity(selectedEntity);
            }

            selectedTimer = .5F;
        } else {
            selectedTimer -= Time.deltaTime;
        }
    }

    public static void checkAvailable() {
        // storage space available
        stockpileAvailable = woodCount + stoneCount < storageSpace;

        // resource available in storage
        resourcesAvailable = woodCount + stoneCount > 0;
        woodAvailable = woodCount > 0;
        stoneAvailable = stoneCount > 0;

        // building plot availability
        plotBuildingAvailable = false;
        plotResourcesAvailable = false;
        plotWoodAvailable = false;
        plotStoneAvailable = false;
        var plots = GameObject.FindGameObjectsWithTag("Plot");
        foreach (var plot in plots) {
            if (plot != null) {
                var plotScript = plot.GetComponent<PlotBehaviour>();

                if (plotScript.progressPossible(true)) {
                    plotBuildingAvailable = true;
                }

                if (plotScript.assignPossible("wood")) {
                    plotWoodAvailable = true;
                    plotResourcesAvailable = true;
                }

                if (plotScript.assignPossible("stone")) {
                    plotStoneAvailable = true;
                    plotResourcesAvailable = true;
                }

                if (plotBuildingAvailable && plotWoodAvailable && plotStoneAvailable) {
                    break;
                }
            }
        }
    }

    public static void updateViewarea(Vector3 position) {
        position = position / 4;
        viewareaTexture.SetPixels(viewareaReset);

        int width = 12, lowHeight = 2, topHeight = 12;

        for (int j = (int)position.x - width; j < position.x + width; j++) {
            if (j > 0 && j < minimapSize && position.z + topHeight > 0 && position.z + topHeight < minimapSize) {
                viewareaTexture.SetPixel(j, (int)position.z + topHeight, new Color(1, 1, 1));
            }
        }

        for (int j = (int)position.x - width; j < position.x + width; j++) {
            if (j > 0 && j < minimapSize && position.z - lowHeight > 0 && position.z - lowHeight < minimapSize) {
                viewareaTexture.SetPixel(j, (int)position.z - lowHeight, new Color(1, 1, 1));
            }
        }

        for (int i = -1; i < 2; i += 2) {
            for (int j = (int)position.z - lowHeight; j < position.z + topHeight; j++) {
                if (j > 0 && j < minimapSize && position.x + width * i > 0 && position.x + width * i < minimapSize) {
                    viewareaTexture.SetPixel((int)position.x + width * i, j, new Color(1, 1, 1));
                }
            }
        }

        // for (int i = (int)position.x - 1; i < position.x + 2; i++) {
        // for (int j = (int)position.z - 1; j < position.z + 2; j++) {
        // viewareaTexture.SetPixel(i, j, new Color(0, 0, 0));
        // }
        // }

        viewareaTexture.Apply();
    }

    public static void addBuildingToMap(Transform building) {
        for (int i = (int)building.position.x; i < building.position.x + building.localScale.x; i++) {
            for (int j = (int)building.position.z; j < building.position.z + building.localScale.z; j++) {
                WorldController.worldMap[i, j] = 'b';
            }
        }
    }

    public static void removeResourceFromMap(Vector3 position, char resource) {
        var resFound = false;
        var offset = 0;

        while (!resFound && offset < 3) {
            for (int i = (int)position.x - offset; i < position.x + offset + 1; i++) {
                for (int j = (int)position.z - offset; j < position.z + offset + 1; j++) {
                    if (i > 0 && i < TerrainGeneration.totalTerrainSize && j > 0 && j < TerrainGeneration.totalTerrainSize) {
                        if (worldMap[i, j] == resource) {
                            worldMap[i, j] = 'e';
                            resFound = true;
                            break;
                        }
                    }
                }
            }

            offset++;
        }
    }

    public static void updateMap() {
        for (int i = 0; i < minimapSize; i++) {
            for (int j = 0; j < minimapSize; j++) {
                switch (getMinChar(new Vector2(i, j))) {
                    case 'e':   // Empty
                        mapTexture.SetPixel(i, j, new Color(.06F, .55F, .05F));
                        break;
                    case 't':   // Tree
                        mapTexture.SetPixel(i, j, new Color(.03F, .17F, .03F));
                        break;
                    case 'r':   // Rock
                        mapTexture.SetPixel(i, j, new Color(.6F, .6F, .6F));
                        break;
                    case 'b':   // Building or Blue
                        mapTexture.SetPixel(i, j, new Color(.06F, .18F, 1F));
                        break;
                }
            }
        }

        mapTexture.Apply();
    }

    public static void updateCounters() {
        GameObject.Find("CitizenCount").GetComponent<Text>().text = "Citizens: " + citizenCount;
        GameObject.Find("WoodCount").GetComponent<Text>().text = "Wood: " + woodCount;
        GameObject.Find("StoneCount").GetComponent<Text>().text = "Stone: " + stoneCount;
        GameObject.Find("StorageCount").GetComponent<Text>().text = "Storage: " + (storageSpace - woodCount - stoneCount);
    }

    static char getMinChar(Vector2 position) {
        int trees = 0, rocks = 0, buildings = 0;

        for (int i = (int)position.x * 4; i < position.x * 4 + 4; i++) {
            for (int j = (int)position.y * 4; j < position.y * 4 + 4; j++) {
                if (i < TerrainGeneration.totalTerrainSize && j < TerrainGeneration.totalTerrainSize) {
                    switch (worldMap[i, j]) {
                        case 'r':
                            rocks++;
                            break;
                        case 't':
                            trees++;
                            break;
                        case 'b':
                            buildings++;
                            break;
                    }
                }
            }
        }

        if (buildings > 0) {
            return 'b';
        } else if (rocks > 0) {
            return 'r';
        } else if (trees > 0) {
            return 't';
        }

        return 'e';
    }

    public static Vector3 getGrid(Vector3 position) {
        int gridSize = 2;

        float newX = position.x - (position.x % gridSize);
        float newY = position.y - (position.y % gridSize);
        float newZ = position.z - (position.z % gridSize);

        return new Vector3(newX, newY, newZ);
    }

    public static void selectEntity(Collider entity) {
        selectedTimer = .5F;
        selectedEntity = entity;

        string result = "";
        Renderer entityRenderer = entity.GetComponent<Renderer>();

        if (entity.name.Contains("BuildingPlot") || (entityRenderer != null && entityRenderer.material.name.Contains("Outline"))) {
            result += "Building plot\n\n";

            PlotBehaviour script = entity.GetComponent<PlotBehaviour>();

            if (script == null) {
                var plots = GameObject.FindGameObjectsWithTag("Plot");
                foreach (GameObject plot in plots) {
                    if (script == null) {
                        script = plot.GetComponent<PlotBehaviour>().getBuildingPlot(entity.transform.position);
                    }
                }
            }

            if (script != null) {
                result += script.getStatus();
            }
        } else if (entity.name.Contains("Person")) {
            result += "NPC\n\n";
            result += entity.GetComponent<PersonBehaviour>().getStatus();
        } else if (entity.name.Contains("TownCenter")) {
            result += "Town Center\n\n";
            result += "Citizens: " + entity.GetComponent<TownCenterBehaviour>().currentSpawns;
        } else if (entity.name.Contains("Stockpile")) {
            result += "Stockpile\n\n";
            result += "Provides " + entity.GetComponent<StockpileBehaviour>().maxStorage + " storage";
        } else if (entity.name.Contains("Workshop")) {
            result += "Workshop\n\n";
        } else if (entity.name.Contains("Barracks")) {
            result += "Barracks\n\n";
            result += entity.GetComponent<BarracksBehaviour>().GetStatus();

            hudScript.showCategory("BarracksButton");
            activeBuilding = entity.gameObject;
        } else if (!entity.name.Contains("Ground")) {
            if (entity.transform.parent != null) {
                selectEntity(entity.transform.parent.GetComponent<Collider>());
            }
            return;
        }

        GameObject.Find("DetailText").GetComponent<Text>().text = result;
    }
}
