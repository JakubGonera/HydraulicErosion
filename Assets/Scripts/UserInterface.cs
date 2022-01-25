using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    public Button genButton;
    public MapGenerator mapGenerator;

    // Start is called before the first frame update
    void Start()
    {
        genButton.onClick.AddListener(this.Generate);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void Generate()
    {
        mapGenerator.Generate();
    }
}
