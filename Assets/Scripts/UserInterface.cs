using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    //Variables for linking all UI elements with code
    public Button genButton;
    public Toggle showHeightmapToggle;

    public InputField seedInput;
    public InputField numOfDropsInput;
    public InputField inertiaInput;
    public InputField evaporationInput;
    public InputField depositionInput;
    public InputField radiusInput;
    public InputField erosionInput;

    public Text fpsText;
    public Text dropsText;

    public Slider comparisonSlider;

    //Other classes that need to be called or altered by the UI
    public MapGenerator mapGenerator;
    public DropGenerator dropGenerator;
    public MeshGenerator meshGenerator;

    //Heightmap texture GameObject and terrain GameObject
    public GameObject heightMapGO;
    public GameObject terrainMeshGO;

    //Variables used for calculating the FPS
    float fpsRefreshInterval = 0.25f;
    float fpsCounter = 0;
    float fpsTimer = 0;

    //Material for changing the comparison ratio value
    public Material terrainMaterial;

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.01f);
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        terrainMaterial.SetFloat("_ComparisonRatio", 0f);

        //Load input fields with current values
        numOfDropsInput.text = dropGenerator.numOfDrops.ToString();
        inertiaInput.text = dropGenerator.inertia.ToString();
        evaporationInput.text = dropGenerator.evaporation.ToString();
        depositionInput.text = dropGenerator.deposition.ToString();
        radiusInput.text = dropGenerator.radius.ToString();
        erosionInput.text = dropGenerator.erosion.ToString();

        //Add callback functions for all UI elements
        genButton.onClick.AddListener(delegate{StartErosion();});
        showHeightmapToggle.onValueChanged.AddListener(delegate {ShowHeightmap(showHeightmapToggle);});
        seedInput.onValueChanged.AddListener(delegate { CheckMinus(seedInput); });
        comparisonSlider.onValueChanged.AddListener(delegate { ChangeComp(); });

        numOfDropsInput.onValueChanged.AddListener(delegate {CheckMinus(numOfDropsInput);});
        inertiaInput.onValueChanged.AddListener(delegate {CheckMinus(inertiaInput);});
        evaporationInput.onValueChanged.AddListener(delegate {CheckMinus(evaporationInput);});
        depositionInput.onValueChanged.AddListener(delegate {CheckMinus(depositionInput);});
        radiusInput.onValueChanged.AddListener(delegate {CheckMinus(radiusInput);});
        erosionInput.onValueChanged.AddListener(delegate {CheckMinus(erosionInput);});

        //Wait so that the terrain mesh is constructed and there is no conflict with erosion starting when terrain is nonexistent
        yield return StartCoroutine("Wait");

        StartErosion();
    }

    public void Update()
    {
        //Calculate FPS every fpsRefreshInterval seconds
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
        dropsText.text = "Drops so far: " + dropGenerator.dropsSoFar.ToString();
    }

    //Method for making sure no negative number is being passed as parameter
    public void CheckMinus(InputField inputField)
    {
        if(inputField.text.Length != 0 && inputField.text[0] == '-')
        {
            inputField.text = inputField.text.Substring(1);
        }
    }

    string CheckForNull(string s)
    {
        if(s == "")
        {
            return "0";
        }
        return s;
    }

    public void StartErosion()
    {
        //Update the generator with the values from input fields
        dropGenerator.numOfDrops = int.Parse(CheckForNull(numOfDropsInput.text));
        dropGenerator.inertia = float.Parse(CheckForNull(inertiaInput.text.Replace('.', ',')));
        dropGenerator.evaporation = float.Parse(CheckForNull(evaporationInput.text.Replace('.', ',')));
        dropGenerator.deposition = float.Parse(CheckForNull(depositionInput.text.Replace('.', ',')));
        dropGenerator.radius = int.Parse(CheckForNull(radiusInput.text));
        dropGenerator.erosion = float.Parse(CheckForNull(erosionInput.text.Replace('.', ',')));

        mapGenerator.seed = int.Parse(CheckForNull(seedInput.text));

        Texture2D tex = mapGenerator.Generate();
        dropGenerator.StartSimulation(tex);
    }

    //Update shader property on slider change
    public void ChangeComp()
    {
        terrainMaterial.SetFloat("_ComparisonRatio", comparisonSlider.value);
    }

    //Enable or disable the heightmap or terrain based on toggle
    public void ShowHeightmap(Toggle toggle)
    {
        heightMapGO.SetActive(toggle.isOn);
        if(terrainMeshGO == null)
        {
            terrainMeshGO = GameObject.FindGameObjectsWithTag("Terrain")[0];
        }
        terrainMeshGO.SetActive(!toggle.isOn);
    }

}
