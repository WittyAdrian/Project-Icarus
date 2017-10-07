using UnityEngine;
using System.Collections;

public class DamageTextBehaviour : MonoBehaviour {

    public string displayText = "";

    float rotationX = 0F;
    Quaternion originalRotation;
    GameObject controller;

    // Use this for initialization
    void Start() {
        originalRotation = transform.localRotation;
        controller = GameObject.Find("CameraController");
        GetComponent<TextMesh>().text = displayText;
    }

    // Update is called once per frame
    void Update() {
        transform.LookAt(controller.transform);
        rotationX = transform.rotation.eulerAngles.y + 180;

        Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
        transform.localRotation = originalRotation * xQuaternion;

        transform.position += new Vector3(0, 0.02F, 0);
    }
}
