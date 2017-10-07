using System.Collections.Generic;
using UnityEngine;

public class DeleteTriggerBehaviour : MonoBehaviour {
    void OnTriggerEnter(Collider col) {
        Destroy(col.gameObject);
    }
}