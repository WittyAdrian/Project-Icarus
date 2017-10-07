using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PersonBehaviour : MonoBehaviour {

    public int moveSpeed = 2, jumpHeight = 4;
    public GameObject myTownCenter = null;
    public string roleForce = "";

    string role = "Civilian";
    int health = 10, speed = 2;
    float hostileDetectRange = 15F, attackSpeed = 3F;
    List<GameObject> equipment = new List<GameObject>();

    string behaviourState = "idle", nextState = "";
    int moveLength = 0, rotateSpeed = 1, pathAngle = 20;
    float targetRotation = 0F, attackTimer = 0F, idleTimer = 0F, validTimer = 0F, standTimer = 0F, positionTimer = 4F;

    float rotationX = 0F;
    Quaternion originalRotation, xQuaternion, swordRotation;
    Vector3 originalScale, swordScale, lastPosition;
    Rigidbody thisBody;
    Renderer thisRenderer;
    LineRenderer thisLine;
    AudioSource thisAudio;

    Object damageTextReference;
    GameObject hostile, damageText;

    Terrain activeTerrain;
    string target = "";
    Vector3 targetPosition;
    GameObject targetObject, targetStructure;
    int targetIndex = -1, targetBound, obsModifier;
    bool obsTurned;

    void Start() {
        // Initialize stats
        health = Random.Range(10, 25);

        // Initialize variables
        originalRotation = transform.localRotation;
        lastPosition = transform.position;
        damageTextReference = Resources.Load("DamageText");
        activeTerrain = Terrain.activeTerrain;

        thisRenderer = GetComponent<Renderer>();
        thisAudio = GetComponent<AudioSource>();
        thisBody = GetComponent<Rigidbody>();

        thisLine = gameObject.AddComponent<LineRenderer>();
        thisLine.material = (Material)Resources.Load("Materials/Blue");
        thisLine.SetWidth(0.05f, 0.05f);
        thisLine.enabled = false;

        // Initialize Role
        if (roleForce == "") {
            role = "Civilian";
            float roleChance = Random.value;
            if (roleChance < .2F) {
                role = "Archer";
            } else if (roleChance < .5F) {
                role = "Knight";
            } else if (roleChance < .6F) {
                role = "Mage";
            }
        } else {
            role = roleForce;
        }
        initializeRole();

        // Randomize appearance
        transform.localScale = new Vector3(transform.localScale.x * Random.Range(0.8f, 1.2f), transform.localScale.y * Random.Range(1f, 1.5f), transform.localScale.z * Random.Range(0.8f, 1.2f));
        originalScale = transform.localScale;
        thisRenderer.material.color = new Color(Random.value, Random.value, Random.value);
    }

    void Update() {
        // Set speed modifier
        if (isCarrying()) {
            speed = 1;
        } else {
            speed = 2;
        }

        #region hostile interaction
        if (isGrounded() && seeHostile()) {
            transform.LookAt(hostile.transform);
            if (target != "") {
                disposeTarget();
            }

            if (behaviourState != "idle") {
                behaviourState = "idle";
            }

            switch (role) {
                case "Knight":
                    transform.position += transform.forward * Time.deltaTime * (moveSpeed * 2);

                    if (equipment[0].transform.rotation.eulerAngles.x != 90) {
                        Quaternion swordQ = Quaternion.AngleAxis(90, Vector3.right);
                        equipment[0].transform.localRotation = swordRotation * swordQ;

                        equipment[0].transform.localScale = new Vector3(swordScale.x, swordScale.y * 2.5F, swordScale.z / 2);
                    }

                    if (Vector3.Distance(hostile.transform.position, transform.position) < transform.localScale.z + 1.25F && attackTimer <= 0) {
                        hostile.GetComponent<HostileBehaviour>().hurt(Random.Range(4, 7));
                        hostile.GetComponent<Rigidbody>().AddForce(new Vector3(50, 0, 0), ForceMode.Impulse);

                        attackTimer = Random.Range(attackSpeed, attackSpeed * 1.5F);
                    } else {
                        attackTimer -= 1 * Time.deltaTime;
                    }
                    break;
                case "Archer":
                    if (Vector3.Distance(hostile.transform.position, transform.position) < 10F) {
                        rotationX = transform.rotation.eulerAngles.y + 180;

                        xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
                        transform.localRotation = originalRotation * xQuaternion;

                        transform.position += transform.forward * Time.deltaTime * (moveSpeed * 2);
                    } else if (attackTimer <= 0) {
                        Object reference = Resources.Load("Prefabs/Arrow");
                        GameObject arrow = GameObject.Instantiate(reference) as GameObject;

                        arrow.transform.position = transform.position + (transform.forward * 2);
                        arrow.transform.rotation = transform.rotation;
                        arrow.transform.localRotation *= Quaternion.AngleAxis(transform.rotation.eulerAngles.x - 45, Vector3.right);

                        //Rigidbody arrowBody = arrow.GetComponent<Rigidbody>();
                        //Vector3 v3Force = 500 * transform.forward;
                        //arrowBody.AddRelativeForce(v3Force);
                        ////arrowBody.AddForce(transform.forward * 50);

                        arrow.GetComponent<ProjectileBehaviour>().Initialize(Vector3.Distance(hostile.transform.position, transform.position) * 0.7F, hostile);

                        attackTimer = Random.Range(attackSpeed, attackSpeed * 1.5F);
                    } else {
                        attackTimer -= 1 * Time.deltaTime;
                    }
                    break;
                case "Mage":
                    hostile.GetComponent<Rigidbody>().useGravity = false;

                    if (hostile.transform.position.y < transform.position.y + 8) {
                        hostile.transform.position = new Vector3(hostile.transform.position.x, hostile.transform.position.y + (1F * Time.deltaTime), hostile.transform.position.z);
                    } else {
                        hostile.transform.position = new Vector3(hostile.transform.position.x, hostile.transform.position.y, hostile.transform.position.z);
                    }

                    if (!thisLine.enabled) {
                        thisLine.enabled = true;
                    }

                    thisLine.SetPositions(new Vector3[] { findChildElement(equipment[0], "Cube").transform.position, hostile.transform.position });
                    break;
                default:
                    rotationX = transform.rotation.eulerAngles.y + 180;

                    xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
                    transform.localRotation = originalRotation * xQuaternion;

                    transform.position += transform.forward * Time.deltaTime * (moveSpeed * 2);
                    break;
            }
        } else {
            if (role == "Knight" && equipment[0].transform.localScale != swordScale) {
                equipment[0].transform.localScale = swordScale;
                equipment[0].transform.localRotation = swordRotation;
            } else if (role == "Mage" && thisLine.enabled) {
                thisLine.enabled = false;
            }
        }
        #endregion

        #region movement and interaction
        if (isGrounded() && !seeHostile()) {
            if (idleTimer <= 0) {
                switch (behaviourState) {
                    case "idle":    /// IDLE STATE
                        #region idle state
                        if (!isArmed() && seeTarget()) {
                            //if (target == "plot") {
                            //    var test = GameObject.Instantiate(Resources.Load("Prefabs/TestCube")) as GameObject;
                            //    test.transform.position = targetPosition;
                            //}

                            behaviourState = "target";
                        } else {
                            disposeTarget();
                            idleTimer = 60;
                        }
                        break;
                    #endregion
                    case "target":  /// TARGET STATE
                        #region target state
                        if (target == "resource") {
                            if (hasValidTarget()) {
                                targetPosition = targetObject.transform.position;
                            } else {
                                disposeTarget();
                            }
                        }
                        transform.LookAt(targetPosition);
                        transform.eulerAngles = new Vector3(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

                        if (Mathf.Abs(Vector3.Distance(lastPosition, transform.position)) > 1F) {
                            positionTimer = 4F;
                            lastPosition = transform.position;
                        } else if (positionTimer <= 0) {
                            positionTimer = 4F;
                            transform.position -= transform.forward;
                            lastPosition = transform.position;

                            behaviourState = "jump";
                            nextState = "target";
                        } else {
                            positionTimer -= Time.deltaTime;
                        }


                        if (Mathf.Abs(Vector3.Distance(targetPosition, transform.position)) < targetBound) {
                            if (target == "tree" || (target == "plot" && targetObject == null)) {
                                idleTimer = 180;
                            }
                            if (target == "stone") {
                                idleTimer = 360;
                            }

                            behaviourState = "interacting";
                        } else {
                            if (seeObstacle()) {
                                if (transform.rotation.eulerAngles.y < 45 || transform.rotation.eulerAngles.y > 315) {
                                    obsModifier = targetPosition.x > transform.position.x ? 1 : -1; // North
                                } else if (transform.rotation.eulerAngles.y < 135) {
                                    obsModifier = targetPosition.z < transform.position.z ? 1 : -1; // East
                                } else if (transform.rotation.eulerAngles.y < 225) {
                                    obsModifier = targetPosition.x < transform.position.x ? 1 : -1; // South
                                } else {
                                    obsModifier = targetPosition.z > transform.position.z ? 1 : -1; // West
                                }

                                // obsModifier = 1;
                                // if (Mathf.Abs(targetPosition.z - transform.position.z) > Mathf.Abs(targetPosition.x - transform.position.x)) {
                                // if (targetPosition.x < transform.position.x) {
                                // obsModifier = -1;
                                // }
                                // } else {
                                // if (targetPosition.z < transform.position.z) {
                                // obsModifier = -1;
                                // }
                                // }

                                // if (270 > transform.rotation.eulerAngles.y && transform.rotation.eulerAngles.y > 90) {
                                // obsModifier *= -1;
                                // }

                                behaviourState = "obstacle";
                                nextState = "target";
                                obsTurned = false;
                            } else {
                                transform.position += transform.forward * Time.deltaTime * (moveSpeed * speed);
                            }

                            if (validTimer <= 0) {
                                if (!this.hasValidTarget()) {
                                    disposeTarget();
                                    behaviourState = "idle";
                                }
                                validTimer = .5F;
                            } else {
                                validTimer -= Time.deltaTime;
                            }
                        }
                        break;
                    #endregion
                    case "interacting": /// INTERACTING STATE
                        #region interacting state
                        switch (target) {
                            case "tree":
                            case "stone":
                                var trees = activeTerrain.terrainData.treeInstances;
                                var resourcePos = trees[targetIndex].position * TerrainGeneration.totalTerrainSize;

                                if (target == "tree") {
                                    WorldController.removeResourceFromMap(resourcePos, 't');
                                    trees[targetIndex].prototypeIndex = 4;

                                    for (int i = 1; i <= 2; i++) {
                                        GameObject log = GameObject.Instantiate(Resources.Load("Prefabs/Log")) as GameObject;
                                        log.transform.position = transform.position + transform.up + transform.forward * i;
                                    }
                                } else if (target == "stone") {
                                    WorldController.removeResourceFromMap(resourcePos, 'r');
                                    trees[targetIndex] = new TreeInstance();

                                    for (int i = 1; i <= 5; i++) {
                                        GameObject stone = GameObject.Instantiate(Resources.Load("Prefabs/Stone")) as GameObject;
                                        stone.transform.position = transform.position + new Vector3(Random.Range(0, 3), Random.Range(0, 3), Random.Range(0, 3));
                                    }
                                }

                                WorldController.updateMap();
                                activeTerrain.terrainData.treeInstances = trees;
                                break;
                            case "resource":
                                pickupItem(targetObject, new Vector3(0F, 0F, 1F));
                                break;
                            case "stockpile":
                                if (isCarrying()) {
                                    dropItem(targetObject);
                                    if (targetStructure != null && targetStructure.GetComponent<StockpileBehaviour>().allowStorage()) {
                                        targetObject.GetComponent<ResourceBehaviour>().placeInStock(targetStructure);
                                    }
                                    targetObject = null;
                                } else {
                                    WorldController.checkAvailable();
                                    if (WorldController.woodAvailable || WorldController.stoneAvailable) {
                                        GameObject resource = null;
                                        if (WorldController.woodAvailable && WorldController.plotWoodAvailable) {
                                            resource = GameObject.Instantiate(Resources.Load("Prefabs/Log")) as GameObject;

                                            WorldController.woodCount--;
                                        } else if (WorldController.stoneAvailable && WorldController.plotStoneAvailable) {
                                            resource = GameObject.Instantiate(Resources.Load("Prefabs/Stone")) as GameObject;

                                            WorldController.stoneCount--;
                                        }

                                        if (resource != null) {
                                            resource.transform.position = transform.position + transform.right;

                                            targetObject = resource;
                                            pickupItem(targetObject, new Vector3(0F, 0F, 1F));

                                            WorldController.updateCounters();
                                        }
                                    }
                                }
                                break;
                            case "plot":
                                if (targetStructure != null) {
                                    var plotScript = targetStructure.GetComponent<PlotBehaviour>();

                                    if (isCarrying()) {
                                        dropItem(targetObject);

                                        targetObject.GetComponent<ResourceBehaviour>().placeInPlot(targetStructure);
                                        plotScript.addResource(targetObject, this.gameObject.GetInstanceID());

                                        targetObject = null;
                                    } else {
                                        if (plotScript.progressPossible(false)) {
                                            plotScript.addProgress();
                                        }

                                        if (plotScript.builders > 0) {
                                            plotScript.builders--;
                                        }
                                    }
                                }
                                break;
                        }

                        behaviourState = "idle";
                        target = "";
                        break;
                    #endregion
                    case "obstacle":    /// OBSTACLE STATE
                        #region obstacle state
                        if (obsModifier == 1 || obsModifier == -1) {
                            if (!obsTurned) {
                                if (obsModifier == 1) {
                                    targetRotation = transform.rotation.eulerAngles.y - (transform.rotation.eulerAngles.y % 90) + 90;
                                } else {
                                    targetRotation = transform.rotation.eulerAngles.y - (transform.rotation.eulerAngles.y % 90);
                                }

                                rotateSpeed = 3;
                                moveLength = 10;
                                behaviourState = "rotate";
                                nextState = "obstacle";
                            } else {
                                RaycastHit[] obstacleCast;
                                if (obsModifier == 1) {
                                    obstacleCast = Physics.RaycastAll(new Ray(transform.position, transform.right * -1), 2.5F);
                                } else {
                                    obstacleCast = Physics.RaycastAll(new Ray(transform.position, transform.right), 2.5F);
                                }

                                if (obstacleCast.Length > 0) {
                                    transform.position += transform.forward * Time.deltaTime * (moveSpeed * speed);
                                } else if (moveLength > 0) {
                                    transform.position += transform.forward * Time.deltaTime * (moveSpeed * speed);
                                    moveLength--;
                                } else {
                                    var currentRotation = transform.rotation;
                                    transform.LookAt(targetPosition);
                                    targetRotation = transform.rotation.eulerAngles.y;
                                    transform.rotation = currentRotation;

                                    obsModifier *= -1;
                                    rotateSpeed = 3;
                                    nextState = "target";
                                    behaviourState = "rotate";
                                }
                            }
                        } else {
                            behaviourState = "idle";
                        }
                        break;
                    #endregion
                    case "move":    /// MOVE STATE
                        #region move state
                        transform.position += transform.forward * Time.deltaTime * (moveSpeed * speed);
                        moveLength--;
                        if (moveLength <= 0) {
                            behaviourState = nextState;
                        }
                        break;
                    #endregion
                    case "rotate":  /// ROTATE STATE
                        #region rotate state
                        if (Mathf.Abs(targetRotation - transform.rotation.eulerAngles.y) > rotateSpeed) {
                            if (obsModifier == 1) {
                                xQuaternion = Quaternion.AngleAxis(transform.rotation.eulerAngles.y + rotateSpeed, Vector3.up);
                                transform.localRotation = originalRotation * xQuaternion;
                            } else if (obsModifier == -1) {
                                xQuaternion = Quaternion.AngleAxis(transform.rotation.eulerAngles.y - rotateSpeed, Vector3.up);
                                transform.localRotation = originalRotation * xQuaternion;
                            } else {
                                behaviourState = "idle";
                            }
                        } else {
                            xQuaternion = Quaternion.AngleAxis(targetRotation, Vector3.up);
                            transform.localRotation = originalRotation * xQuaternion;
                            obsTurned = true;
                            behaviourState = nextState;
                        }

                        // if (rotateDegrees < 1 && rotateDegrees > -1) {
                        // behaviourState = "idle";
                        // rotateDegrees = 0F;
                        // } else {
                        // if (rotateDegrees > 0) {
                        // rotateDegrees--;
                        // rotationX += 1F;
                        // } else {
                        // rotateDegrees++;
                        // rotationX -= 1F;
                        // }

                        // xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
                        // transform.localRotation = originalRotation * xQuaternion;
                        // }
                        break;
                    #endregion
                    case "jump":    /// JUMP STATE
                        transform.localScale = originalScale;
                        thisBody.AddForce(new Vector3(0, 10, 0), ForceMode.Impulse);
                        idleTimer = 10;
                        behaviourState = "jumping";
                        break;
                    case "jumping": /// JUMPING STATE
                        idleTimer = 10;
                        behaviourState = nextState;
                        break;
                }
            } else {
                if (behaviourState == "jump") {
                    transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y - .05f, transform.localScale.z);
                }
                idleTimer -= 60 * Time.deltaTime;
            }
        } else {
            if (behaviourState == "jumping") {
                transform.position += transform.forward * Time.deltaTime * (moveSpeed * 3);
            }
        }
        #endregion

        // Start standing upright if not already doing so
        //if (isGrounded() && transform.localRotation.eulerAngles.z != 0) {
        //    if (standTimer <= 0) {
        //        transform.rotation.eulerAngles *= new Vector3(1, 1, 0);
        //        standTimer = 1F;
        //    } else if (standTimer > 0) {
        //        standTimer -= Time.deltaTime;
        //    }
        //}

        #region death detection
        // Falling of the world kills it
        // No health kills it
        if (health <= 0 || transform.position.y < -50) {
            int items = equipment.Count;
            for (int i = 0; i < items; i++) {
                dropEquipment(0);
            }

            damageTextReference = null;

            bleed(Random.Range(12, 20));

            if (myTownCenter != null) {
                myTownCenter.GetComponent<TownCenterBehaviour>().currentSpawns--;
            }

            if (target != "") {
                disposeTarget();
            }

            if (isCarrying()) {
                if (targetObject != null) {
                    targetObject.GetComponent<ResourceBehaviour>().assigned = false;
                    if (targetObject.transform.parent == this.transform) {
                        dropItem(targetObject);
                    }
                    targetObject = null;
                }
            }

            WorldController.citizenCount--;
            WorldController.updateCounters();

            Destroy(this.gameObject);
        }
        #endregion
    }

    void initializeRole() {
        switch (role.ToLower()) {
            case "knight":
                equipItem("Sword", new Vector3(0.7F, 0, 0.7F));
                swordRotation = equipment[0].transform.localRotation;
                swordScale = equipment[0].transform.localScale;
                equipItem("Shield", new Vector3(-0.7F, 0, 0.7F));
                equipItem("Helmet", new Vector3(0, 0.55F, 0));
                this.health = 15;
                break;
            case "archer":
                equipItem("Bow", new Vector3(0.7F, 0, 0.7F));
                equipItem("Quiver", new Vector3(0, 0.2F, -0.9F));
                hostileDetectRange = 30F;
                break;
            case "mage":
                equipItem("Staff", new Vector3(0.7F, 0, 0.7F));
                break;
        }
    }

    void equipItem(string prefab, Vector3 localPos) {
        GameObject instance = GameObject.Instantiate(Resources.Load("Prefabs/" + prefab)) as GameObject;
        equipment.Add(instance);

        equipment[equipment.Count - 1].GetComponent<BoxCollider>().enabled = false;
        equipment[equipment.Count - 1].GetComponent<Rigidbody>().isKinematic = true;

        equipment[equipment.Count - 1].transform.parent = this.transform;
        equipment[equipment.Count - 1].transform.localPosition = localPos;
    }

    void pickupItem(GameObject item, Vector3 localPos) {
        item.GetComponent<BoxCollider>().enabled = false;
        item.GetComponent<Rigidbody>().isKinematic = true;

        item.transform.parent = this.transform;
        item.transform.localPosition = localPos;
    }

    void dropItem(GameObject item) {
        item.GetComponent<BoxCollider>().enabled = true;
        item.GetComponent<Rigidbody>().isKinematic = false;

        item.transform.parent = null;
    }

    void dropEquipment(int index) {
        equipment[index].transform.parent = null;

        equipment[index].GetComponent<BoxCollider>().enabled = true;
        equipment[index].GetComponent<Rigidbody>().isKinematic = false;

        equipment.RemoveAt(index);
    }

    public void hurt(int damage) {
        damageText = GameObject.Instantiate(damageTextReference) as GameObject;
        damageText.GetComponent<DamageTextBehaviour>().displayText = damage.ToString();
        damageText.transform.position = transform.position + new Vector3(0F, 1F, 0F);

        switch (role) {
            case "Mage":
                playSound("mage_hurt");
                break;
        }

        this.health -= damage;
        bleed(damage);
    }

    public void addForce(float force, Vector3 position, float radius) {
        thisBody.AddExplosionForce(force, position, radius);
    }

    void playSound(string soundName) {
        AudioClip clip = (AudioClip)Resources.Load("Sounds/" + soundName);
        thisAudio.PlayOneShot(clip);
    }

    bool isArmed() {
        return role == "Knight" || role == "Archer" || role == "Mage";
    }

    bool seeHostile() {
        GameObject[] hostiles = GameObject.FindGameObjectsWithTag("Hostile");

        if (hostiles.Length < 1) {
            return false;
        }

        this.hostile = null;
        foreach (GameObject hostile in hostiles) {
            var distance = Vector3.Distance(hostile.transform.position, transform.position);
            if (distance < hostileDetectRange && (this.hostile == null || distance < Vector3.Distance(this.hostile.transform.position, transform.position))) {
                this.hostile = hostile;
            }
        }

        if (this.hostile != null) {
            return true;
        }

        return false;
    }

    bool seeTarget() {
        WorldController.checkAvailable();

        float distance = 99999;
        GameObject targetPlot = null;

        // If carrying a resource
        if (isCarrying()) {
            #region closest building plot
            // Get closest building plot
            if (WorldController.plotResourcesAvailable) {
                distance = 99999;
                foreach (GameObject plot in GameObject.FindGameObjectsWithTag("Plot")) {
                    if (plot.GetComponent<PlotBehaviour>().assignPossible(getMyResource())) {
                        targetPosition = plot.transform.position; //getClosestBuildingPoint(plot.transform);
                        if (plot.GetComponent<PlotBehaviour>().radius > Mathf.Abs(Vector3.Distance(targetPosition, transform.position)) && distance > Mathf.Abs(Vector3.Distance(targetPosition, transform.position))) {
                            distance = Mathf.Abs(Vector3.Distance(targetPosition, transform.position));
                            targetPlot = plot;
                        }
                    }
                }

                if (targetPlot != null) {
                    targetPlot.GetComponent<PlotBehaviour>().assignResource(getMyResource(), this.gameObject);

                    targetStructure = targetPlot;
                    targetPosition = getClosestBuildingPoint(targetPlot.transform);
                    target = "plot";
                    targetBound = 2;
                    return true;
                }
            }
            #endregion

            #region closest available stockpile
            // Get closest available stockpile
            if (WorldController.stockpileAvailable) {
                distance = 99999;
                GameObject targetStock = null;
                foreach (GameObject stockpile in GameObject.FindGameObjectsWithTag("Stockpile")) {
                    targetPosition = stockpile.transform.position;
                    if (distance > Mathf.Abs(Vector3.Distance(targetPosition, transform.position)) && stockpile.GetComponent<StockpileBehaviour>().allowStorage()) {
                        distance = Mathf.Abs(Vector3.Distance(targetPosition, transform.position));
                        targetStock = stockpile;
                    }
                }

                if (targetStock != null) {
                    targetStructure = targetStock;
                    targetPosition = targetStock.transform.position;
                    target = "stockpile";
                    targetBound = 5;
                    return true;
                }
            }
            #endregion
        } else {
            #region closest building plot for building
            // Get closest building plot that allows building
            if (WorldController.plotBuildingAvailable) {
                distance = 99999;
                foreach (GameObject plot in GameObject.FindGameObjectsWithTag("Plot")) {
                    if (plot.GetComponent<PlotBehaviour>().progressPossible(true)) {
                        targetPosition = plot.transform.position; //getClosestBuildingPoint(plot.transform);
                        if (plot.GetComponent<PlotBehaviour>().radius > Mathf.Abs(Vector3.Distance(targetPosition, transform.position)) && distance > Mathf.Abs(Vector3.Distance(targetPosition, transform.position))) {
                            distance = Mathf.Abs(Vector3.Distance(targetPosition, transform.position));
                            targetPlot = plot;
                        }
                    }
                }

                if (targetPlot != null && targetPlot.GetComponent<PlotBehaviour>().progressPossible(true)) {
                    targetPlot.GetComponent<PlotBehaviour>().builders++;

                    targetStructure = targetPlot;
                    targetPosition = getClosestBuildingPoint(targetPlot.transform);
                    target = "plot";
                    targetBound = 2;
                    return true;
                }
            }
            #endregion

            #region closest resource if available
            // Get closest resource if any are available
            if (WorldController.stockpileAvailable || WorldController.plotResourcesAvailable) {
                var resources = GameObject.FindGameObjectsWithTag("Resource");

                distance = 99999;
                targetObject = null;
                foreach (var res in resources) {
                    var resBehaviour = res.GetComponent<ResourceBehaviour>();
                    if (!resBehaviour.assigned && !resBehaviour.inStock && (WorldController.stockpileAvailable || (WorldController.plotResourcesAvailable && resourceNeeded(res)))) {
                        targetPosition = res.transform.position;
                        if (distance > Mathf.Abs(Vector3.Distance(targetPosition, transform.position))) {
                            distance = Mathf.Abs(Vector3.Distance(targetPosition, transform.position));
                            targetObject = res;
                        }
                    }
                }

                if (targetObject != null) {
                    var resBehaviour = targetObject.GetComponent<ResourceBehaviour>();
                    if (!resBehaviour.assigned && !resBehaviour.inStock) {
                        targetPosition = targetObject.transform.position;
                        resBehaviour.assigned = true;
                        target = "resource";
                        targetBound = 2;
                        return true;
                    }
                }
            }
            #endregion

            #region stored resources if available and needed
            // If stored resources are available and needed
            if ((WorldController.woodAvailable && WorldController.plotWoodAvailable) || (WorldController.stoneAvailable && WorldController.plotStoneAvailable)) {
                distance = 99999;
                GameObject targetStock = null;
                foreach (GameObject stockpile in GameObject.FindGameObjectsWithTag("Stockpile")) {
                    targetPosition = stockpile.transform.position;
                    if (distance > Mathf.Abs(Vector3.Distance(targetPosition, transform.position)) && stockpile.GetComponent<StockpileBehaviour>().storedResources()) {
                        distance = Mathf.Abs(Vector3.Distance(targetPosition, transform.position));
                        targetStock = stockpile;
                    }
                }

                if (targetStock != null) {
                    targetPosition = targetStock.transform.position;
                    foreach (GameObject plot in GameObject.FindGameObjectsWithTag("Plot")) {
                        if (plot.GetComponent<PlotBehaviour>().radius > Mathf.Abs(Vector3.Distance(plot.transform.position, targetPosition))) {
                            targetStructure = targetStock;
                            target = "stockpile";
                            targetBound = 5;
                            return true;
                        }
                    }
                }
            }
            #endregion

            #region closest tree or stone
            // Get closest tree or stone to cut it down
            if (WorldController.stockpileAvailable || WorldController.plotResourcesAvailable) {
                TreeInstance[] trees = activeTerrain.terrainData.treeInstances;

                if (trees.Length < 1) {
                    return false;
                }

                distance = 99999;
                for (int i = 0; i < trees.Length; i++) {
                    if (TerrainGeneration.treesAssignment[i] < 1 && (WorldController.stockpileAvailable || (WorldController.plotResourcesAvailable && ((trees[i].prototypeIndex == 3 && WorldController.plotStoneAvailable) || (trees[i].prototypeIndex < 3 && WorldController.plotWoodAvailable))))) {
                        targetPosition = Vector3.Scale(trees[i].position, activeTerrain.terrainData.size) + activeTerrain.transform.position;
                        if (distance > Mathf.Abs(Vector3.Distance(targetPosition, transform.position))) {
                            distance = Mathf.Abs(Vector3.Distance(targetPosition, transform.position));
                            targetIndex = i;
                        }
                    }
                }

                if (TerrainGeneration.treesAssignment[targetIndex] < 1) {
                    TerrainGeneration.treesAssignment[targetIndex]++;
                    targetPosition = Vector3.Scale(trees[targetIndex].position, activeTerrain.terrainData.size) + activeTerrain.transform.position;
                    target = "tree";
                    if (trees[targetIndex].prototypeIndex == 3) {
                        target = "stone";
                    }

                    targetBound = 2;
                    return true;
                }
            }
            #endregion
        }

        return false;
    }

    Vector3 getClosestBuildingPoint(Transform building) {
        Vector3 buildingSize = building.localScale * 10F, result = building.position;

        if (transform.position.x < building.position.x) {
            if (transform.position.z < building.position.z) {
                if (transform.position.x > building.position.x - buildingSize.x) {
                    result = new Vector3(transform.position.x, transform.position.y, building.position.z - buildingSize.z); // Bottom side
                } else if (transform.position.z > building.position.z - buildingSize.z) {
                    result = new Vector3(building.position.x - buildingSize.x, transform.position.y, transform.position.z); // Left side
                } else {
                    result = new Vector3(building.position.x - buildingSize.x, transform.position.y, building.position.z - buildingSize.z); // Bottom-left corner
                }
            } else {
                if (transform.position.x > building.position.x - buildingSize.x) {
                    result = new Vector3(transform.position.x, transform.position.y, building.position.z); // Top side
                } else {
                    result = new Vector3(building.position.x - buildingSize.x, transform.position.y, building.position.z); // Top-left corner
                }
            }
        } else {
            if (transform.position.z < building.position.z) {
                if (transform.position.z > building.position.z - buildingSize.z) {
                    result = new Vector3(building.position.x, transform.position.y, transform.position.z); // Right side
                } else {
                    result = new Vector3(building.position.x, transform.position.y, building.position.z - buildingSize.z); // Bottom-right corner
                }
            } else {
                result = new Vector3(building.position.x, transform.position.y, building.position.z); // Top-Right corner
            }
        }

        return result;
    }

    bool hasValidTarget() {
        switch (target) {
            case "stone":
            case "tree":
                return TerrainGeneration.treesAssignment[targetIndex] < 2;
            case "resource":
                return targetObject != null && (WorldController.stockpileAvailable || WorldController.plotResourcesAvailable);
            case "plot":
                if (targetStructure != null) {
                    if (isCarrying()) {
                        return targetStructure.GetComponent<PlotBehaviour>().progressPossible(false);
                    } else {
                        return targetStructure.GetComponent<PlotBehaviour>().progressPossible(true);
                    }
                }
                break;
            case "stockpile":
                if (targetStructure != null) {
                    return targetStructure.GetComponent<StockpileBehaviour>().allowStorage();
                }
                break;
        }

        return false;
    }

    void disposeTarget() {
        switch (target) {
            case "stone":
            case "tree":
                TerrainGeneration.treesAssignment[targetIndex]--;
                break;
            case "plot":
                if (targetStructure != null) {
                    var plotScript = targetStructure.GetComponent<PlotBehaviour>();

                    if (isCarrying()) {
                        plotScript.assignResource(getMyResource(), this.gameObject, false);
                    } else {
                        if (plotScript.builders > 0) {
                            plotScript.builders--;
                        }
                    }
                }
                break;
            case "resource":
                if (targetObject != null) {
                    targetObject.GetComponent<ResourceBehaviour>().assigned = false;
                    if (targetObject.transform.parent == this.transform) {
                        dropItem(targetObject);
                    }
                    targetObject = null;
                }
                break;
        }

        // if (targetObject != null) {
        // targetObject.GetComponent<ResourceBehaviour>().assigned = false;
        // if (targetObject.transform.parent == this.transform) {
        // dropItem(targetObject);
        // }
        // targetObject = null;
        // }

        target = "";
    }

    public string getStatus() {
        string result = "";

        result += "Role: " + role + "\n";
        result += "Health: " + health + "\n\n";

        result += "Behaviour: " + behaviourState + "\n";
        result += "Target: " + target + "\n";
        result += "Position: " + transform.position + "\n";
        if (targetPosition != null) {
            result += "Going to: " + targetPosition + "\n";
        }
        result += "Rotation: " + transform.rotation.eulerAngles + "\n";

        if (isCarrying()) {
            result += "Holding: " + targetObject.name;
        }

        return result;
    }

    bool seeObstacle() {
        if (transform.rotation.eulerAngles.y % 90 < pathAngle || transform.rotation.eulerAngles.y % 90 > 90 - pathAngle) {
            var obstacleCast = Physics.RaycastAll(new Ray(transform.position, transform.forward), 1F);
            foreach (RaycastHit ray in obstacleCast) {
                if (ray.collider.tag != "Resource" && !ray.collider.name.Contains("Cube")) {
                    return true;
                }
            }
        }

        return false;
    }

    bool resourceNeeded(GameObject resource) {
        if (resource.name.Contains("Log")) {
            return WorldController.plotWoodAvailable;
        } else if (resource.name.Contains("Stone")) {
            return WorldController.plotStoneAvailable;
        }

        return false;
    }

    public string getMyResource() {
        if (targetObject != null && targetObject.transform.parent == this.transform) {
            if (targetObject.name.Contains("Log")) {
                return "wood";
            } else if (targetObject.name.Contains("Stone")) {
                return "stone";
            }
        }
        return "";
    }

    public bool isTargetingPlot(GameObject plot) {
        return target == "plot" && isCarrying() && targetStructure.GetInstanceID() == plot.GetInstanceID();
    }

    bool isCarrying() {
        return targetObject != null && targetObject.transform.parent == this.transform;
    }

    Transform findChildElement(GameObject parent, string name) {
        foreach (Transform elem in parent.GetComponentInChildren<Transform>()) {
            if (elem.name == name) {
                return elem;
            }
        }

        return null;
    }

    bool isGrounded() {
        var groundCast = Physics.RaycastAll(new Ray(transform.position, Vector3.down), transform.localScale.y + 0.5F);
        return groundCast.Length > 0;
    }

    void bleed(int amount) {
        if (amount > 5) {
            amount = 5;
        }

        for (int i = 0; i < amount; i++) {
            var blood = GameObject.Instantiate(Resources.Load("Prefabs/BloodCube")) as GameObject;
            blood.transform.position = transform.position;
        }
    }
}