using UnityEngine;
using System.Collections;

public class StockpileBehaviour : MonoBehaviour {
    
    public int maxStorage;

    void Start() {
        maxStorage = 30;
    }

    public bool allowStorage() {
        WorldController.checkAvailable();
        return !GetComponent<Renderer>().material.name.Contains("Outline") && WorldController.stockpileAvailable;
    }

    public bool storedResources() {
        WorldController.checkAvailable();
        return !GetComponent<Renderer>().material.name.Contains("Outline") && WorldController.resourcesAvailable;
    }
}
