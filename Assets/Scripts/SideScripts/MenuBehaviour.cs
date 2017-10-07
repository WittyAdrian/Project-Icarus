using UnityEngine;
using System.Collections;

public class MenuBehaviour : MonoBehaviour {

    AudioSource thisAudio;

	// Use this for initialization
	void Start () {
        // Set cursor
        Cursor.lockState = CursorLockMode.Confined;

        // Play backgroundmusic
        thisAudio = GetComponent<AudioSource>();
        AudioClip clip = (AudioClip)Resources.Load("Sounds/mainmenu");
        thisAudio.clip = clip;
        thisAudio.loop = true;
        thisAudio.Play();
    }
}
