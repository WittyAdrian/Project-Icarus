using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraControl : MonoBehaviour {

    int moveSpeed = 10, scrollSpeed = 40;
    float rotationX = 0F, rotationY = -65F, sensitivity = 1.5F, originalHeight;
    bool unlock = false;

    Quaternion originalRotation;
    AudioSource thisAudio;
    Terrain thisTerrain;

    void Start() {
        // Set cursor
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.SetCursor((Texture2D)Resources.Load("Textures/cursor"), Vector2.zero, CursorMode.Auto);

        // Initialize camera angle
        originalRotation = transform.localRotation;
        Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);
        transform.localRotation = originalRotation * yQuaternion;

        // Initialize values
        thisTerrain = Terrain.activeTerrain;

        // Play background music
        thisAudio = GetComponent<AudioSource>();
        AudioClip clip = (AudioClip)Resources.Load("Sounds/background_loop");
        thisAudio.clip = clip;
        thisAudio.loop = true;
        thisAudio.Play();
    }

    void Update() {
        // Camera look around
        if (Input.GetMouseButton(1)) {
            rotationX += Input.GetAxis("Mouse X") * sensitivity;
            if (unlock) {
                rotationY += Input.GetAxis("Mouse Y") * sensitivity;
            }
            updateCamera();
        }

        // Zoom in
        if (Input.GetAxis("Mouse ScrollWheel") > 0F) { // Forward
            if (unlock || transform.position.y > originalHeight - 15) {
                transform.position += transform.forward * Time.deltaTime * scrollSpeed;

                if (transform.position.y < originalHeight - 10) {
                    rotationY += 1F;
                    updateCamera();
                }
            }
        } else if (Input.GetAxis("Mouse ScrollWheel") < 0F) { // Backwards
            if (unlock || transform.position.y < originalHeight) {
                transform.position -= transform.forward * Time.deltaTime * scrollSpeed;

                if (transform.position.y < originalHeight - 10) {
                    rotationY -= 1F;
                    updateCamera();
                }
            }
        }

        // Movement by WASD or middle mouse
        if (Input.GetKey(KeyCode.W) || (Input.GetMouseButton(2) && Input.GetAxis("Mouse Y") < 0)) {
            if (insideBounds(transform.position + Vector3.Scale(transform.forward, new Vector3(1, 0, 1)) * Time.deltaTime * (moveSpeed * 2))) {
                transform.position += Vector3.Scale(transform.forward, new Vector3(1, 0, 1)) * Time.deltaTime * (moveSpeed * 2);
                WorldController.updateViewarea(transform.position);
            }
        }

        if (Input.GetKey(KeyCode.S) || (Input.GetMouseButton(2) && Input.GetAxis("Mouse Y") > 0)) {
            if (insideBounds(transform.position - Vector3.Scale(transform.forward, new Vector3(1, 0, 1)) * Time.deltaTime * (moveSpeed * 2))) {
                transform.position -= Vector3.Scale(transform.forward, new Vector3(1, 0, 1)) * Time.deltaTime * (moveSpeed * 2);
                WorldController.updateViewarea(transform.position);
            }
        }

        if (Input.GetKey(KeyCode.D) || (Input.GetMouseButton(2) && Input.GetAxis("Mouse X") < 0)) {
            if (insideBounds(transform.position + transform.right * Time.deltaTime * moveSpeed)) {
                transform.position += transform.right * Time.deltaTime * moveSpeed;
                WorldController.updateViewarea(transform.position);
            }
        }

        if (Input.GetKey(KeyCode.A) || (Input.GetMouseButton(2) && Input.GetAxis("Mouse X") > 0)) {
            if (insideBounds(transform.position - transform.right * Time.deltaTime * moveSpeed)) {
                transform.position -= transform.right * Time.deltaTime * moveSpeed;
                WorldController.updateViewarea(transform.position);
            }
        }

        // Unlock camera
        if (Input.GetKeyDown(KeyCode.V)) {
            unlock = !unlock;
            if (!unlock) {
                rotationY = -65F;
                updateCamera();
                transform.position = new Vector3(transform.position.x, originalHeight, transform.position.z);
            }
        }

        // Movement functions
        if (Input.GetKey(KeyCode.LeftShift)) {
            moveSpeed = 100;
        } else if (Input.GetMouseButton(2)) {
            moveSpeed = 50;
        } else if (moveSpeed != 10) {
            moveSpeed = 10;
        }

        // World functions
        if (Input.GetKey(KeyCode.Escape)) {
            //Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = true;
            SceneManager.LoadScene("MainMenu");
            //Application.Quit();
        }
    }

    bool insideBounds(Vector3 newPosition) {
        var worldBounds = new Rect(thisTerrain.transform.position.x, thisTerrain.transform.position.z, TerrainGeneration.totalTerrainSize, TerrainGeneration.totalTerrainSize);
        Vector2 position = new Vector2(newPosition.x, newPosition.z);
        return worldBounds.Contains(position);
    }

    void updateCamera() {
        if (!unlock) {
            rotationY = Mathf.Clamp(rotationY, -65F, -55F);
        }

        Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
        Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);
        transform.localRotation = originalRotation * xQuaternion * yQuaternion;

        WorldController.updateViewarea(transform.position);
    }

    public void setOriginHeight(float height) {
        this.originalHeight = height;
    }
}
