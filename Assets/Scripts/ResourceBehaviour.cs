using UnityEngine;

public class ResourceBehaviour : MonoBehaviour {

    public bool assigned = false, inStock = false;

    Vector3 originalScale;

    // Use this for initialization
    void Start() {
        originalScale = transform.localScale;
    }

    public void placeInPlot(GameObject targetPlot) {
        // GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<BoxCollider>().enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;

        // var plotScript = targetPlot.GetComponent<PlotBehaviour>();

        this.transform.localScale = originalScale;
        this.transform.rotation = Quaternion.identity;

        this.transform.parent = targetPlot.transform;
        this.transform.position += new Vector3(Random.Range(-2F, 2F), 0, Random.Range(-2F, 2F));

        //this.transform.position = targetPlot.transform.position + new Vector3(targetPlot.transform.localScale.x / 2 * -1 + .5F, 1, targetPlot.transform.localScale.z / 2 * -1 + .5F);

        //this.transform.position += new Vector3(plotScript.currentResources() % targetPlot.transform.localScale.x,
        //                                        Mathf.Floor(plotScript.currentResources() / (targetPlot.transform.localScale.x * targetPlot.transform.localScale.z)) * originalScale.y,
        //                                        Mathf.Floor(plotScript.currentResources() / targetPlot.transform.localScale.x) % targetPlot.transform.localScale.z);

        inStock = true;
        assigned = false;
    }

    public void placeInStock(GameObject targetStock) {
        if (gameObject.name.Contains("Log")) {
            WorldController.woodCount++;
        } else if (gameObject.name.Contains("Stone")) {
            this.transform.position += new Vector3(0, .5F, 0);
            WorldController.stoneCount++;
        }

        WorldController.updateCounters();
        Destroy(this.gameObject);
    }
}
