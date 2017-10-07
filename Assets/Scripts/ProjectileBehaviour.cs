using UnityEngine;
using System.Collections;

public class ProjectileBehaviour : MonoBehaviour {

    float moveSpeed = 0;
    GameObject target = null;//, Ground;

    // Use this for initialization
    void Start() {
        //Ground = GameObject.Find("Ground");	
    }

    // Update is called once per frame
    void Update() {
        if (!isGrounded()) {
            transform.position += transform.forward * Time.deltaTime * moveSpeed;

            if (target != null) {
                if (Vector3.Distance(target.transform.position, transform.position) < transform.localScale.z + 1.25F) {
                    target.GetComponent<HostileBehaviour>().hurt(Random.Range(8, 14));
                    Destroy(this.gameObject);
                }
            }
        }
    }

    public void Initialize(float speed, GameObject target) {
        this.moveSpeed = speed;
        this.target = target;
    }

    bool isGrounded() {
        var groundCast = Physics.RaycastAll(new Ray(transform.position, Vector3.down), transform.localScale.y + 0.5F);
        foreach (RaycastHit hit in groundCast) {
            if (hit.transform.gameObject.name == "Ground") {
                return true;
            }
        }
        return false;
    }

    void getHostile() {
        GameObject[] hostiles = GameObject.FindGameObjectsWithTag("Hostile");

        if (hostiles.Length < 1) {
            return;
        }

        this.target = null;
        foreach (GameObject hostile in hostiles) {
            var distance = Vector3.Distance(hostile.transform.position, transform.position);
            if (this.target == null || distance < Vector3.Distance(this.target.transform.position, transform.position)) {
                this.target = hostile;
            }
        }
    }
}
