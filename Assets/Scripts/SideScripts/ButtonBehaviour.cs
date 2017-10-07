using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonBehaviour : MonoBehaviour {

    public void playClick() {
        SceneManager.LoadScene("Game");
    }

    public void exitClick() {
        Application.Quit();
    }
}
