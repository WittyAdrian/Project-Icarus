using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour {

    Terrain thisTerrain;
    TerrainData thisData;

    public static int totalTerrainSize = 1025;
    public static Dictionary<int, int> treesAssignment;

    int chunks = 200, detailSize = 2050, chunkSize, totalSize, forestSize = 50, forestDispersion = 20, forestDistance = 40;
    float dispersion = .001F, detailDispersion = .0003F, startHeight;

    float[,] heightSetting, chunkMap;
    List<Vector2> forestLocations = new List<Vector2>(), formationLocations = new List<Vector2>();

    void Start() {
        // Retrieving objects
        thisTerrain = GetComponent<Terrain>();
        thisData = thisTerrain.terrainData;

        // Initialization values
        totalSize = totalTerrainSize;
        thisData.treeInstances = new TreeInstance[0];
        treesAssignment = new Dictionary<int, int>();
        thisTerrain.detailObjectDistance = 185F;
        thisTerrain.treeDistance = 200F;
        chunkMap = new float[chunks + 1, chunks + 1];
        heightSetting = new float[totalSize, totalSize];
        chunkSize = totalSize / chunks;
        startHeight = .25F;//Random.Range(.1F, .25F);

        // Executing terrain generation
        //platues(true, false);
        //polygons(true, false);
        layers(true, 0);
        //addWalls();
        addTrees(0.029F, 0.006F, 80);
        addDetails();

        // Centering camera
        GameObject controller = GameObject.Find("CameraController");
        controller.transform.position = transform.position;
        controller.transform.position += new Vector3(totalSize / 2, heightSetting[totalSize / 2, totalSize / 2] * totalSize + 25, totalSize / 2);
        controller.GetComponent<CameraControl>().setOriginHeight(controller.transform.position.y);

        // Setting size and heightmap
        thisData.heightmapResolution = totalSize;
        thisData.size = new Vector3(totalSize, totalSize, totalSize);
        thisData.SetHeights(0, 0, heightSetting);

        thisTerrain.Flush();
        WorldController.updateMap();
        WorldController.updateViewarea(controller.transform.position);
    }

    void polygons(bool evenSpread, bool bumpy) {
        for (int i = 0; i <= chunks; i++) {
            for (int j = 0; j <= chunks; j++) {
                if (i == 0) {
                    if (j == 0) {
                        chunkMap[i, j] = startHeight;
                    } else {
                        float lastHeight = chunkMap[i, j - 1];
                        chunkMap[i, j] = Random.Range(lastHeight - dispersion, lastHeight + dispersion);
                    }
                } else {
                    if (j == 0) {
                        float lastHeight = chunkMap[i - 1, j];
                        chunkMap[i, j] = Random.Range(lastHeight - dispersion, lastHeight + dispersion);
                    } else {
                        float lastHeight = (chunkMap[i - 1, j] + chunkMap[i, j - 1]) / 2;
                        float thisDispersion = dispersion;
                        if (evenSpread) {
                            thisDispersion = (dispersion - Mathf.Abs(chunkMap[i - 1, j] - chunkMap[i, j - 1])) / 2;
                        }
                        chunkMap[i, j] = Random.Range(lastHeight - thisDispersion, lastHeight + thisDispersion);
                    }
                }
            }
        }

        for (int i = 0; i < totalSize; i++) {
            for (int j = 0; j < totalSize; j++) {
                int x = Mathf.FloorToInt(i / chunkSize), y = Mathf.FloorToInt(j / chunkSize);

                float left = (chunkMap[x + 1, y] - chunkMap[x, y]) / chunkSize * (i % chunkSize) + chunkMap[x, y];
                float right = (chunkMap[x + 1, y + 1] - chunkMap[x, y + 1]) / chunkSize * (i % chunkSize) + chunkMap[x, y + 1];

                float heightResult = (right - left) / chunkSize * (j % chunkSize) + left;

                if (bumpy) {
                    heightSetting[i, j] = Random.Range(heightResult - detailDispersion, heightResult + detailDispersion);
                } else {
                    heightSetting[i, j] = heightResult;
                }
            }
        }
    }

    void platues(bool evenSpread, bool bumpy) {
        for (int i = 0; i < chunks; i++) {
            for (int j = 0; j < chunks; j++) {
                if (i == 0) {
                    if (j == 0) {
                        chunkMap[i, j] = startHeight;
                    } else {
                        float lastHeight = chunkMap[i, j - 1];
                        chunkMap[i, j] = Random.Range(lastHeight - dispersion, lastHeight + dispersion);
                    }
                } else {
                    if (j == 0) {
                        float lastHeight = chunkMap[i - 1, j];
                        chunkMap[i, j] = Random.Range(lastHeight - dispersion, lastHeight + dispersion);
                    } else {
                        float lastHeight = (chunkMap[i - 1, j] + chunkMap[i, j - 1]) / 2;
                        float thisDispersion = dispersion;
                        if (evenSpread) {
                            thisDispersion = (dispersion - Mathf.Abs(chunkMap[i - 1, j] - chunkMap[i, j - 1])) / 2;
                        }
                        chunkMap[i, j] = Random.Range(lastHeight - thisDispersion, lastHeight + thisDispersion);
                    }
                }
            }
        }

        for (int i = 0; i < totalSize; i++) {
            for (int j = 0; j < totalSize; j++) {
                int x = Mathf.FloorToInt(i / chunkSize), y = Mathf.FloorToInt(j / chunkSize);
                if (bumpy) {
                    heightSetting[i, j] = Random.Range(chunkMap[x, y] - detailDispersion, chunkMap[x, y] + detailDispersion);
                } else {
                    heightSetting[i, j] = chunkMap[x, y];
                }
            }
        }
    }

    void layers(bool evenSpread, int formations) {
        for (int i = 0; i < totalSize; i++) {
            for (int j = 0; j < totalSize; j++) {
                heightSetting[i, j] = startHeight;
            }
        }

        while (formationLocations.Count < formations) {
            Vector2 newLocation = new Vector2(Random.Range(0, totalSize), Random.Range(0, totalSize));

            foreach (Vector2 loc in formationLocations) {
                if (Mathf.Abs(Vector2.Distance(newLocation, loc)) < 250F) {
                    newLocation = new Vector2(-1, -1);
                }
            }

            if (newLocation.x != -1) {
                createFormation(newLocation);
            }
        }
    }

    void createFormation(Vector2 position) {
        int formDirection = Random.value < .5F ? 1 : -1;
        float hillHeight = Random.Range(6, 10);
        int width = Random.Range(15, 30), height = Random.Range(15, 30);

        while (hillHeight > 0) {
            //Debug.Log("On " + hillHeight + " starting at " + ((int)position.x - width / 2) + ", " + ((int)position.y - height / 2) + " going to " + ((int)position.x + width) + ", " + ((int)position.y + height));
            for (int i = (int)position.x - width / 2; i < (int)position.x + width; i++) {
                bool started = false, ended = false;
                for (int j = (int)position.y - height / 2; j < (int)position.y + height; j++) {
                    if (j < totalSize - 1 && i < totalSize - 1 && j > 1 && i > 1 && false) {
                        if (i == (int)position.x - width / 2) {
                            if (!started) {
                                started = Random.value < (j - position.y + height / 2) / (height / 2);
                            } else if (!ended) {
                                ended = Random.value < (j - position.y + height / 2) / (height / 2) - 1;
                            }
                        } else {
                            if (i < (int)position.x + width / 2) {
                                // Can only get larger
                                if (!started) {
                                    if (heightSetting[i - 1, j] == startHeight + (hillHeight / totalSize) * formDirection || heightSetting[i, j + 1] == startHeight + ((hillHeight + 1) / totalSize) * formDirection) { // Check above/below layer
                                        started = true;
                                    } else {
                                        started = Random.value < (j - position.y + height / 2) / (height / 2);
                                    }
                                } else if (!ended) {
                                    if (heightSetting[i - 1, j] != startHeight + (hillHeight / totalSize) * formDirection && heightSetting[i, j - 1] != startHeight + ((hillHeight + 1) / totalSize) * formDirection) { // Check above/below layer
                                        ended = Random.value < (j - position.y + height / 2) / (height / 2) - 1;
                                    }
                                }
                            } else {
                                // Can only get smaller
                                if (!started) {
                                    if (heightSetting[i, j + 1] == startHeight + ((hillHeight + 1) / totalSize) * formDirection) {  // Check above/below layer
                                        started = true;
                                    } else if (heightSetting[i - 1, j] == startHeight + (hillHeight / totalSize) * formDirection) {
                                        started = Random.value < (j - position.y + height / 2) / (height / 2);
                                    }
                                } else if (!ended) {
                                    if (heightSetting[i, j - 1] != startHeight + ((hillHeight + 1) / totalSize) * formDirection) {  // Check above/below layer
                                        if (heightSetting[i - 1, j] != startHeight + (hillHeight / totalSize) * formDirection) {
                                            ended = true;
                                        } else {
                                            ended = Random.value < (j - position.y + height / 2) / (height / 2) - 1;
                                        }
                                    }
                                }
                            }
                        }

                        if (started && !ended && heightSetting[i, j] == startHeight) {
                            heightSetting[i, j] = startHeight + (hillHeight / totalSize) * formDirection;
                        }
                    }

                    if (j < totalSize - 1 && i < totalSize - 1 && j > 1 && i > 1 && heightSetting[i, j] == startHeight) {
                        heightSetting[i, j] = startHeight + (hillHeight / totalSize) * formDirection;
                    }
                }
            }

            width += Random.Range(15, 30);
            height += Random.Range(15, 30);
            hillHeight--;
        }

        formationLocations.Add(position);
    }

    void addTrees(float treePercentage, float rockPercentage, int forests) {
        // treePercentage is between 0 and 1        
        for (float i = 0; i < totalSize; i += 5) {
            for (float j = 0; j < totalSize; j += 5) {
                float spawnChance = Random.value;

                if (spawnChance < treePercentage) {
                    var tree = new TreeInstance();
                    tree.position = new Vector3(i / totalSize, heightSetting[(int)i, (int)j], j / totalSize);
                    if (spawnChance < rockPercentage) {
                        tree.prototypeIndex = 3;
                        WorldController.worldMap[(int)i, (int)j] = 'r';
                    } else {
                        tree.prototypeIndex = Random.Range(0, 3);
                        WorldController.worldMap[(int)i, (int)j] = 't';
                    }

                    //tree.rotation = Random.Range(0, 360);                    
                    // tree.lightmapColor = new Color32(255, 255, 255, 255);

                    tree.heightScale = 1;
                    tree.widthScale = 1;

                    treesAssignment.Add(thisTerrain.terrainData.treeInstanceCount, 0);
                    thisTerrain.AddTreeInstance(tree);
                }
            }
        }

        int timeoutCheck = 100;
        while (forestLocations.Count < forests) {
            Vector2 newLocation = new Vector2(Random.Range(0, totalSize), Random.Range(0, totalSize));

            foreach (Vector2 loc in forestLocations) {
                if (Mathf.Abs(Vector2.Distance(newLocation, loc)) < forestDistance) {
                    newLocation = new Vector2(-1, -1);
                }
            }


            if (newLocation.x != -1) {
                timeoutCheck = 100;
                spawnForest(newLocation);
            }

            timeoutCheck--;
            if (timeoutCheck <= 0) {
                Debug.Log("Breaking forest generation.");
                break;
            }
        }
    }

    void spawnForest(Vector2 position) {
        int forestWidth = Random.Range(forestSize - forestDispersion, forestSize + forestDispersion),
            forestHeight = Random.Range(forestSize - forestDispersion, forestSize + forestDispersion);

        int startX = (int)position.x - forestWidth / 2,
            startY = (int)position.y - forestHeight / 2;

        for (float i = startX; i < startX + forestWidth; i += Random.Range(2F, 4F)) {
            for (float j = startY; j < startY + forestHeight; j += Random.Range(2F, 4F)) {
                if (j < totalSize && i < totalSize && j > 0 && i > 0) {
                    float spawnChance = Random.value;

                    if (spawnChance > Mathf.Abs(Vector2.Distance(new Vector2(i, j), position)) / ((forestWidth + forestHeight) / 2 / 2)) {
                        var tree = new TreeInstance();
                        tree.position = new Vector3(Random.Range(i - 1F, i + 1F) / totalSize, heightSetting[(int)i, (int)j], j / totalSize);
                        tree.prototypeIndex = Random.Range(0, 3);
                        // tree.lightmapColor = new Color32(255, 255, 255, 255);

                        tree.heightScale = 1;
                        tree.widthScale = 1;

                        treesAssignment.Add(thisTerrain.terrainData.treeInstanceCount, 0);
                        thisTerrain.AddTreeInstance(tree);
                        WorldController.worldMap[(int)i, (int)j] = 't';
                    }
                }
            }
        }

        forestLocations.Add(position);
    }

    void addDetails() {
        // Old values were 960, 80
        thisData.SetDetailResolution(detailSize, 16);

        int[,] fullDetails = new int[detailSize, detailSize];
        int[,] halfDetails = new int[detailSize, detailSize];
        //int[,] quarterDetails = new int[detailSize, detailSize];
        //int[,] tenthDetails = new int[detailSize, detailSize];
        for (int i = 0; i < detailSize; i++) {
            for (int j = 0; j < detailSize; j++) {
                fullDetails[i, j] = 1;

                float chance = Random.value;
                if (chance < .5F) {
                    halfDetails[i, j] = 1;
                }

                //if (chance < .25F) {
                //    quarterDetails[i, j] = 1;
                //}

                //if (chance < .1F) {
                //    tenthDetails[i, j] = 1;
                //}
            }
        }

        //thisData.SetDetailLayer(0, 0, 0, halfDetails);
        //thisData.SetDetailLayer(0, 0, 1, fullDetails);
        thisData.SetDetailLayer(0, 0, 2, halfDetails);
        //thisData.SetDetailLayer(0, 0, 3, tenthDetails);
        //thisData.SetDetailLayer(0, 0, 4, quarterDetails);
        //thisData.SetDetailLayer(0, 0, 5, fullDetails);
        thisData.SetDetailLayer(0, 0, 6, fullDetails);
    }

    void addWalls() {
        for (int i = 0; i <= 1; i++) {
            for (int k = 0; k < 5; k++) {
                for (int j = 0; j < totalSize; j++) {
                    heightSetting[j, i * (totalSize - 6) + k] = heightSetting[j, i * (totalSize - 6) + k] * (1 + (5 - k) / 10);
                }
            }
        }

        for (int i = 0; i <= 1; i++) {
            for (int k = 0; k < 5; k++) {
                for (int j = 0; j < totalSize; j++) {
                    heightSetting[i * (totalSize - 6) + k, j] = heightSetting[j, i * (totalSize - 6) + k] * (1 + (5 - k) / 10);
                }
            }
        }
    }
}
