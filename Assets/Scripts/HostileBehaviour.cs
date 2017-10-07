using UnityEngine;
using System.Collections;

public class HostileBehaviour : MonoBehaviour {

    public int moveSpeed = 3, jumpHeight = 4;
    public GameObject bloodCube;

    int health = 20;

    string behaviourState = "idle";
    int moveLength = 0;
    float rotateDegrees = 0F;

    float rotationX = 0F, rotationY = 0F, attackTimer = 0F, idleTimer = 0F;
    Quaternion originalRotation, xQuaternion;
    Rigidbody thisBody;
    //Renderer thisRenderer;

    Object damageTextReference;
    GameObject /*ground,*/ target, damageText;
    PersonBehaviour targetBehaviour;

    // Use this for initialization
    void Start() {
        // Initialize variables
        originalRotation = transform.localRotation;
        //ground = GameObject.Find("Ground");
        thisBody = GetComponent<Rigidbody>();

        damageTextReference = Resources.Load("DamageText");

        // Randomize appearance
        transform.localScale = new Vector3(transform.localScale.x * Random.Range(0.8f, 1.2f), transform.localScale.y * Random.Range(1f, 1.5f), transform.localScale.z * Random.Range(0.8f, 1.2f));
    }

    // Update is called once per frame
    void Update() {
        // Movement cycle
        if (hasTarget() && isGrounded()) {
            transform.LookAt(target.transform);
            transform.position += transform.forward * Time.deltaTime * (moveSpeed * 2);

            if (Vector3.Distance(target.transform.position, transform.position) < transform.localScale.z + 0.5F && attackTimer <= 0) {
                target.GetComponent<PersonBehaviour>().hurt(Random.Range(2, 4));

                attackTimer = Random.Range(1, 2);
            } else {
                attackTimer -= 1 * Time.deltaTime;
            }
        } else if (isGrounded()) {
            if (idleTimer <= 0) {
                switch (behaviourState) {
                    case "idle":
                        transform.localRotation = originalRotation * xQuaternion;
                        //float changeState = Random.value * 100;
                        behaviourState = "moving";
                        moveLength = 100;
                        break;
                    case "moving":
                        transform.position += transform.forward * Time.deltaTime * moveSpeed;
                        moveLength--;
                        if (moveLength <= 0) {
                            rotateDegrees = Random.value * 360 - 180;
                            behaviourState = "rotate";
                        }
                        break;
                    case "rotate":
                        if (rotateDegrees < 1 && rotateDegrees > -1) {
                            idleTimer = 10;
                            behaviourState = "idle";
                            rotateDegrees = 0F;
                        } else {
                            if (rotateDegrees > 0) {
                                rotateDegrees--;
                                rotationX += 1F;
                            } else {
                                rotateDegrees++;
                                rotationX -= 1F;
                            }

                            xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
                            transform.localRotation = originalRotation * xQuaternion;
                        }
                        break;
                }
            } else {
                idleTimer -= 60 * Time.deltaTime;
            }
        }

        if (!thisBody.useGravity) {
            if (idleTimer <= 0) {
                idleTimer = 60;
                thisBody.useGravity = true;
            } else {
                idleTimer -= 60 * Time.deltaTime;
            }

            rotationY += 1F;
            rotationX += 1F;
            xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.right);
            transform.localRotation = originalRotation * xQuaternion * yQuaternion;
        }

        // Falling of the world kills it
        if (transform.position.y < -50) {
            Destroy(this.gameObject);
        }

        // No health kills it
        if (health <= 0) {
            bleed(Random.Range(10, 15));
            Destroy(this.gameObject);
        }
    }

    bool hasTarget() {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Person");

        if (targets.Length < 1) {
            return false;
        }

        // Get target by closest distance
        this.target = null;
        foreach (GameObject target in targets) {
            var distance = Vector3.Distance(target.transform.position, transform.position);
            if (distance < 30F && (this.target == null || distance < Vector3.Distance(this.target.transform.position, transform.position))) {
                this.target = target;
            }
        }

        if (this.target != null) {
            return true;
        }

        return false;
    }

    bool isGrounded() {
        var groundCast = Physics.RaycastAll(new Ray(transform.position, Vector3.down), transform.localScale.y + 0.5F);
        foreach (RaycastHit hit in groundCast) {
            if (hit.transform.gameObject.name.Contains("Ground")) {
                return true;
            }
        }
        return false;
    }

    public void hurt(int damage) {
        damageText = GameObject.Instantiate(damageTextReference) as GameObject;
        damageText.GetComponent<DamageTextBehaviour>().displayText = damage.ToString();
        damageText.transform.position = transform.position + new Vector3(0F, 1F, 0F);

        this.health -= damage;
        bleed(damage);
    }

    void bleed(int amount) {
        if (amount > 5) {
            amount = 5;
        }

        for (int i = 0; i < amount; i++) {
            Instantiate(bloodCube, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);
        }
    }
}
