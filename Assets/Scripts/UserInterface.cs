using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    public Button genButton;
    public Toggle showHeightmapToggle;
    public InputField seedInput;
    public Text fpsText;

    public MapGenerator mapGenerator;
    public DropGenerator dropGenerator;
    public MeshGenerator meshGenerator;

    public GameObject heightMapGO;
    public GameObject terrainMeshGO;

    float fpsRefreshInterval = 0.25f;
    float fpsCounter = 0;
    float fpsTimer = 0;


    IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.01f);
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        genButton.onClick.AddListener(delegate
        {
            StartErosion();
        });
        showHeightmapToggle.onValueChanged.AddListener(delegate {
            ShowHeightmap(showHeightmapToggle);
        });
        seedInput.onValueChanged.AddListener(delegate {
            ChangeSeed();
        });

        yield return StartCoroutine("Wait");

        StartErosion();
    }

    public void Update()
    {
        if(fpsTimer > fpsRefreshInterval)
        {
            fpsText.text = "FPS: " + fpsCounter / fpsRefreshInterval;
            fpsTimer = 0;
            fpsCounter = 0;
        }
        else
        {
            fpsCounter++;
            fpsTimer += Time.deltaTime;
        }
    }

    public void ChangeSeed()
    {
        mapGenerator.seed = int.Parse(seedInput.text);
    }

    public void ShowHeightmap(Toggle toggle)
    {
        heightMapGO.SetActive(toggle.isOn);
        if(terrainMeshGO == null)
        {
            terrainMeshGO = GameObject.FindGameObjectsWithTag("Terrain")[0];
        }
        terrainMeshGO.SetActive(!toggle.isOn);
    }

    public void StartErosion()
    {
        Texture2D tex = mapGenerator.Generate();
        dropGenerator.StartSimulation(tex);
    }
}
