using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HudBehaviour : MonoBehaviour {

    public static string activeAbility = "";
    static float? flashTimer;

    string activeCategory = "";
    Dictionary<string, List<GameObject>> buttonList;

    // Use this for initialization
    void Start() {
        buttonList = new Dictionary<string, List<GameObject>>();
        flashTimer = null;

        // Adding functionality to build buttons
        BindButton("BuildStockpileButton", "buildStockpile");
        BindButton("BuildHouseButton", "buildHouse");
        BindButton("BuildBarracksButton", "buildBarracks");

        // Adding functionality to ability buttons
        BindButton("PickupButton", "pickup");
        BindButton("KillButton", "kill");

        // Configuring category buttons
        SetCategory("BuildingsButton", "BuildButton");
        SetCategory("AbilitiesButton", "AbilityButton");

        // Configuring context buttons
        BindContextButton("BarracksButton");

        // Adding all the buttons to the buttonList
        BuildList();

        GameObject.Find("BackButton").GetComponent<Button>().onClick.AddListener(() => {
            foreach (var btn in buttonList[activeCategory]) {
                btn.SetActive(false);
            }

            foreach (var btn in buttonList["CategoryButton"]) {
                btn.SetActive(true);
            }

            buttonList["BackButton"][0].SetActive(false);
        });

        GameObject.Find("BackButton").gameObject.SetActive(false);
    }

    public void hideButtons() {
        foreach (var list in buttonList) {
            if (list.Key == "BackButton") {
                list.Value[0].SetActive(true);
            } else {
                foreach (var btn in list.Value) {
                    btn.SetActive(false);
                }
            }
        }
    }

    public void showCategory(string categoryLabel) {
        hideButtons();
        foreach (var btn in buttonList[categoryLabel]) {
            btn.SetActive(true);
        }

        activeCategory = categoryLabel;
    }

    public void ShowButtons(bool show) {
        foreach (var list in buttonList) {
            if (list.Key == "CategoryButton") {
                foreach (var btn in list.Value) {
                    btn.SetActive(show);
                }
            } else {
                foreach (var btn in list.Value) {
                    btn.SetActive(false);
                }
            }
        }
    }

    void BuildList() {
        AddCategoryToList("BuildButton");
        AddCategoryToList("AbilityButton");
        AddCategoryToList("CategoryButton");
        AddCategoryToList("BarracksButton");

        List<GameObject> list = new List<GameObject>();
        list.Add(GameObject.Find("BackButton").gameObject);
        buttonList.Add("BackButton", list);
    }

    void AddCategoryToList(string categoryLabel) {
        buttonList.Add(categoryLabel, new List<GameObject>());
        foreach (GameObject btn in GameObject.FindGameObjectsWithTag(categoryLabel)) {
            var list = buttonList[categoryLabel];
            list.Add(btn);
            if (categoryLabel != "CategoryButton") {
                btn.gameObject.SetActive(false);
            }
            buttonList[categoryLabel] = list;
        }
    }

    void SetCategory(string buttonName, string categoryLabel) {
        GameObject.Find(buttonName).GetComponent<Button>().onClick.AddListener(() => {
            showCategory(categoryLabel);
        });
    }

    void BindButton(string buttonName, string parameter) {
        GameObject.Find(buttonName).GetComponent<Button>().onClick.AddListener(() => {
            if (buttonName.Contains("Pickup")) {
                Cursor.SetCursor((Texture2D)Resources.Load("Textures/pickupCursor"), new Vector2(7, 7), CursorMode.Auto);
            } else if (buttonName.Contains("Kill")) {
                Cursor.SetCursor((Texture2D)Resources.Load("Textures/killCursor"), new Vector2(6, 6), CursorMode.Auto);
            } else {
                Cursor.SetCursor((Texture2D)Resources.Load("Textures/cursor"), Vector2.zero, CursorMode.Auto);
            }

            SetActiveAbility(parameter);
        });
    }

    void BindContextButton(string categoryLabel) {
        foreach (var btn in GameObject.FindGameObjectsWithTag(categoryLabel)) {
            string unit = btn.GetComponentInChildren<Text>().text;
            Debug.Log("Binding " + unit);

            btn.GetComponent<Button>().onClick.AddListener(() => {
                if (categoryLabel == "BarracksButton") {
                    WorldController.activeBuilding.GetComponent<BarracksBehaviour>().Train(unit);
                }
            });
        }
    }

    void Update() {
        if (flashTimer <= 0F && flashTimer != null) {
            GameObject.Find("Background3").GetComponent<RawImage>().color = new Color(.192F, .192F, .192F); //49-49-49
            flashTimer = null;
        } else {
            flashTimer -= Time.deltaTime;
        }

        GameObject.Find("PositionText").GetComponent<Text>().text = Mathf.Floor(GameObject.Find("CameraController").transform.position.x) + ", " + Mathf.Floor(GameObject.Find("CameraController").transform.position.z);
    }

    void SetActiveAbility(string ability) {
        activeAbility = ability;
    }

    public static void setTutorialMessage(string message) {
        GameObject.Find("TutorialText").GetComponent<Text>().text = message;
    }

    public static void FlashResources() {
        GameObject.Find("Background3").GetComponent<RawImage>().color = new Color(.9F, 0F, 0F);
        flashTimer = 2F;
    }
}
