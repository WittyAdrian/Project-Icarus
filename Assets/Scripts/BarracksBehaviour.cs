using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BarracksBehaviour : MonoBehaviour {

    Dictionary<string, int> trainingCooldown, resourceCost;
    List<string> trainingQueue;
    float trainTimer;

    // Use this for initialization
    void Start() {
        trainingCooldown = new Dictionary<string, int>();
        resourceCost = new Dictionary<string, int>();
        trainingQueue = new List<string>();

        InitializeCost("Knight", 6, 2, 4);
        InitializeCost("Archer", 8, 4, 1);
        InitializeCost("Mage", 11, 3, 3);

        // trainingCooldown.Add("Knight", 6);
        // trainingCooldown.Add("Archer", 8);
        // trainingCooldown.Add("Mage", 11);

        // resourceCost.Add("KnightWood", 2);
        // resourceCost.Add("KnightStone", 4);
        // resourceCost.Add("ArcherWood", 4);
        // resourceCost.Add("ArcherStone", 1);
        // resourceCost.Add("MageWood", 3);
        // resourceCost.Add("MageStone", 3);

        trainTimer = 0F;
    }

    void InitializeCost(string unit, int cooldown, int wood, int stone) {
        trainingCooldown.Add(unit, cooldown);
        resourceCost.Add(unit + "Wood", wood);
        resourceCost.Add(unit + "Stone", stone);
    }

    // Update is called once per frame
    void Update() {
        if (trainTimer <= 0F) {
            if (trainingQueue.Count > 0) {
                var spawn = GameObject.Instantiate(Resources.Load("Prefabs/Person")) as GameObject;

                var spawnBehaviour = (PersonBehaviour)spawn.GetComponent(typeof(PersonBehaviour));
                spawnBehaviour.roleForce = trainingQueue[0];

                spawn.transform.position = transform.position + new Vector3(1.5F, -1F, 5F);

                trainingQueue.RemoveAt(0);

                if (trainingQueue.Count > 0) {
                    trainTimer = trainingCooldown[trainingQueue[0]];
                }
            }
        } else {
            trainTimer -= Time.deltaTime;
        }
    }

    public void Train(string unit) {
        if (WorldController.woodCount >= resourceCost[unit + "Wood"] && WorldController.stoneCount >= resourceCost[unit + "Stone"]) {
            Debug.Log("Training a new " + unit + " during " + trainingCooldown[unit] + " seconds");

            WorldController.woodCount -= resourceCost[unit + "Wood"];
            WorldController.stoneCount -= resourceCost[unit + "Stone"];
            WorldController.updateCounters();

            if (trainTimer <= 0F) {
                trainTimer = trainingCooldown[unit];
            }

            trainingQueue.Add(unit);
        } else {
            HudBehaviour.FlashResources();
            Debug.Log("Cannot train " + unit + ". Not enough resources.");
        }
    }

    public string GetStatus() {
        string result = "";

        string training = trainingQueue.Count > 0 ? trainingQueue[0] : "nothing";
        result += "Currently training: " + training + "\n";
        result += "Time remaining: " + Mathf.Floor(trainTimer) + "\n\n";

        result += " Training costs:\n";
        result += "Knight: " + resourceCost["KnightWood"] + " wood, " + resourceCost["KnightStone"] + " stone\n";
        result += "Archer: " + resourceCost["ArcherWood"] + " wood, " + resourceCost["ArcherStone"] + " stone\n";
        result += "Mage: " + resourceCost["MageWood"] + " wood, " + resourceCost["MageStone"] + " stone\n";

        return result;
    }
}
