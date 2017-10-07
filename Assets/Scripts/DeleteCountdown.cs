using UnityEngine;
using System.Collections;

public class DeleteCountdown : MonoBehaviour {

    public int countdown;
    public bool dispers;

    float countdownValue;

    void Start() {
        float countdownDispersion = countdown / 3;
        countdownValue = countdown;
        if (dispers && countdownDispersion > 0)
        {
            countdownValue += Random.Range(-countdownDispersion, countdownDispersion);
        }
    }
    
	// Update is called once per frame
	void Update () {
        if(countdownValue <= 0) {
            Destroy(this.gameObject);
        }
        countdownValue -= 1 * Time.deltaTime;
	}
}
