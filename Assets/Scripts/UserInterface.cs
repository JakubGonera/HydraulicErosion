using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    public Button genButton;
    public Toggle showHeightmapToggle;
    public InputField seedInput;

    public MapGenerator mapGenerator;
    public GameObject heightMapGO;

    // Start is called before the first frame update
    void Start()
    {
        genButton.onClick.AddListener(this.Generate);
        showHeightmapToggle.onValueChanged.AddListener(delegate {
            ShowHeightmap(showHeightmapToggle);
        });
        seedInput.onValueChanged.AddListener(delegate {
            ChangeSeed();
        });
    }

    public void ChangeSeed()
    {
        mapGenerator.seed = int.Parse(seedInput.text);
    }

    public void ShowHeightmap(Toggle toggle)
    {
        heightMapGO.SetActive(toggle.isOn);
    }

    public void Generate()
    {
        mapGenerator.Generate();
    }
}
