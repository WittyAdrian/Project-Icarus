using System.Collections.Generic;
using UnityEngine;

public class PlotBehaviour : MonoBehaviour {

    public int builders, radius;

    int reqWood, reqStone;
    float progStep, buildingHeight, assignTimer;

    GameObject building;
    Renderer buildingRenderer;
    Material buildingMaterial;
    List<GameObject> curWood, curStone;
    List<GameObject> assWood, assStone;

    void Start() {
        builders = 0;
        assignTimer = 1F;
        radius = 80;

        curWood = new List<GameObject>();
        curStone = new List<GameObject>();

        assWood = new List<GameObject>();
        assStone = new List<GameObject>();
    }

    void Update() {
        if (assignTimer <= 0) {
            foreach (GameObject npc in assWood) {
                var npcScript = npc.GetComponent<PersonBehaviour>();
                if (npcScript.getMyResource() != "wood" || !npcScript.isTargetingPlot(this.gameObject)) {
                    assWood.RemoveAll(a => a.GetInstanceID() == npc.GetInstanceID());
                }
            }

            foreach (GameObject npc in assStone) {
                var npcScript = npc.GetComponent<PersonBehaviour>();
                if (npcScript.getMyResource() != "stone" || !npcScript.isTargetingPlot(this.gameObject)) {
                    assWood.RemoveAll(a => a.GetInstanceID() == npc.GetInstanceID());
                }
            }

            if (assWood.Count < reqWood || assStone.Count < reqStone) {
                radius += 10;
            }

            assignTimer = 1F;
        } else {
            assignTimer -= Time.deltaTime;
        }
    }

    public int currentResources() {
        return curWood.Count + curStone.Count;
    }

    public bool assignPossible(string resource) {
        if (resource.ToLower() == "wood") {
            return assWood.Count < reqWood;
        } else if (resource.ToLower() == "stone") {
            return assStone.Count < reqStone;
        }

        return false;
    }

    public bool progressPossible(bool checkBuilders) {
        if (checkBuilders) {
            return builders < 2 && builders < curWood.Count + curStone.Count && (curWood.Count > 0 || curStone.Count > 0);
        }

        return curWood.Count > 0 || curStone.Count > 0;
    }

    public void addProgress() {
        if (building.transform.localScale.y == 0) {
            building.transform.position = transform.position - building.transform.localScale * .5F;
            building.transform.position += new Vector3(0, buildingHeight / 2 - .5F, 0);

            WorldController.addBuildingToMap(building.transform);
            WorldController.updateMap();
        }

        if (curWood.Count > 0) {
            Destroy(curWood[0].gameObject);
            curWood.RemoveAt(0);
        } else if (curStone.Count > 0) {
            Destroy(curStone[0].gameObject);
            curStone.RemoveAt(0);
        }

        if (reqWood <= 0 && reqStone <= 0) {
            completeBuilding();
        } else {
            building.transform.localScale += new Vector3(0, buildingHeight * (progStep / 100), 0);
        }
    }

    void completeBuilding() {
        if (building.name.Contains("Stockpile")) {
            WorldController.storageSpace += building.GetComponent<StockpileBehaviour>().maxStorage;
            WorldController.updateCounters();
        }

        buildingRenderer.material = buildingMaterial;
        building.layer = 0;
        building.transform.localScale = new Vector3(building.transform.localScale.x, buildingHeight, building.transform.localScale.z);

        Destroy(this.gameObject);
    }

    public void assignResource(string resource, GameObject assignObject, bool add = true) {
        if (resource.ToLower() == "wood") {
            if (add) {
                assWood.Add(assignObject);
            } else {
                assWood.RemoveAll(a => a.GetInstanceID() == assignObject.GetInstanceID());
            }
        } else if (resource.ToLower() == "stone") {
            if (add) {
                assStone.Add(assignObject);
            } else {
                assStone.RemoveAll(a => a.GetInstanceID() == assignObject.GetInstanceID());
            }
        }
    }

    public void addResource(GameObject resource, int assignID) {
        if (resource.name.Contains("Log")) {
            curWood.Add(resource);
            assWood.RemoveAll(a => a.GetInstanceID() == assignID);
            reqWood--;
        } else if (resource.name.Contains("Stone")) {
            curStone.Add(resource);
            assStone.RemoveAll(a => a.GetInstanceID() == assignID);
            reqStone--;
        }
    }

    public void setBuilding(GameObject myBuilding) {
        building = myBuilding;
        building.transform.position = new Vector3(-2000, -2000, -2000);

        buildingRenderer = building.GetComponent<Renderer>();
        buildingMaterial = buildingRenderer.material;
        buildingRenderer.material = (Material)Resources.Load("Materials/Outline");

        transform.localScale = Vector3.Scale(transform.localScale, building.transform.localScale);
        transform.localScale *= .5F;
        transform.localScale = new Vector3(transform.localScale.x, 1F, transform.localScale.z);
        transform.position += Vector3.Scale(transform.localScale * 5F, new Vector3(1, 0, 1));
        buildingHeight = building.transform.localScale.y;

        building.transform.localScale = Vector3.Scale(building.transform.localScale, new Vector3(1, 0, 1));
    }

    public void setRequirements(int wood, int stone) {
        reqWood = wood;
        reqStone = stone;

        progStep = 100 / (wood + stone);
    }

    public PlotBehaviour getBuildingPlot(Vector3 buildingPosition) {
        if (buildingPosition == building.transform.position) {
            return this;
        }

        return null;
    }

    public string getStatus() {
        string result = "";

        result += "Builders: " + builders + "\n\n";

        result += "Resources needed:\n";
        result += reqWood + " wood, " + reqStone + " stone\n\n";

        result += "Resources being delivered:\n";
        result += assWood.Count + " wood, " + assStone.Count + " stone\n\n";

        result += "Resources present:\n";
        result += curWood.Count + " wood, " + curStone.Count + " stone\n\n";

        return result;
    }
}
