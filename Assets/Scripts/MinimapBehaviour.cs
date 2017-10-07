using UnityEngine;
using System.Collections;

public class MinimapBehaviour : MonoBehaviour {
    
	// Use this for initialization
	void Start () {
        //transform.position.x = 
        Camera map = GetComponent<Camera>();
        map.rect = new Rect(0, 0, 0.1F, 0.1F);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
