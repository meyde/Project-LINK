using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class TectonicPlatesModule : Module
{
    private int moduleId = 4;
    [Header("Keyboard Reading")] 
    [SerializeField] private InputAction writing;
    [SerializeField] private InputAction backspace;
    [SerializeField] private InputAction enter;
    [Header("game objects to change")]
    [SerializeField] private GameObject[] colorPlates;
    [SerializeField] private GameObject[] letters;
    [Header("Data")]
    [SerializeField] private Sprite[] lettersSprite;
    [SerializeField] private int[] keyLocations;
    [SerializeField] private Color[] correctColors;

    [SerializeField] private GameManagerLocal gm;
    private int ledState;
    private bool autoLose;
    private void Awake()
    {
        keyLocations = new int[6]{ 1, 4, 7, 10, 12, 14};
        correctColors = new Color[4] { Color.green, Color.blue, Color.pink, Color.red };
    }
    private void OnEnable()
    {
        enter.Enable();
        writing.Enable();
        backspace.Enable();
        enter.performed += OnEnter;
        backspace.performed += OnBackSpace;
        writing.performed += OnLetterPress;
    }
    private void OnDisable()
    {
        writing.Disable();
        backspace.Disable();
        enter.Disable();
        enter.performed -= OnEnter;
        backspace.performed -= OnBackSpace;
        writing.performed -= OnLetterPress;
    }

    public override void OnStarted()
    {
        int currReg = gm.currentRegion;
        CEventRuntimeData? activeEvent = null;
        foreach (CEventRuntimeData cEventData in gm.gmn.events)
        {
            if (cEventData.state != 1) { continue; }
            CatastrophicEvent cEvent = gm.allEvents[cEventData.eventId];
            if( cEvent.region == currReg && cEventData.module1Option != -1 && cEvent.modules2.Contains(moduleId))
            {
                Debug.Log($"event {cEvent.eventId} is occuring in this region, need this module and has done its first module");

                activeEvent = cEventData;
            }
        }
        if ( !activeEvent.HasValue )
        {
            Debug.Log($" No event occuring found in this region that has done it's first module. any validation henceforth will FAIL");
            autoLose = true;
        }
    }
    private void OnEnter(InputAction.CallbackContext context)
    {
        Debug.Log("enter");
    }
    public void OnBackSpace(InputAction.CallbackContext context)
    {
        Debug.Log("backspaced");
    }

    private void OnLetterPress(InputAction.CallbackContext context)
    {
        Debug.Log("Letter pressed: " + context.control.displayName);
    }

    public void Randomizer()
    {
        foreach (GameObject colorPlate in colorPlates)
        { 
            colorPlate.GetComponent<SpriteRenderer>().color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        }

        foreach (GameObject letter in letters)
        {
            letter.GetComponent<SpriteRenderer>().sprite = lettersSprite[Random.Range(0, lettersSprite.Count())];
        }
    }

    public void coloredFixer(int color, int[] code)
    {
        switch (color)
        {
            case 0:
                //Code order: numericals
                int[] codePlace = new int[3] { keyLocations[Random.Range(0,6)], keyLocations[Random.Range(0, 6)], keyLocations[Random.Range(0, 6)] };
                Array.Sort(codePlace);
                int codeIndex = 0;
                for (int i=0; i<6; i++)
                {
                    if (i == codePlace[codeIndex])
                    {
                        colorPlates[i].GetComponent<SpriteRenderer>().color = correctColors[i];
                        letters[i].GetComponent<SpriteRenderer>().sprite = lettersSprite[code[codeIndex]];
                        codeIndex++;
                    }
                    else
                    {
                        
                    }

                }
                break;
            case 1:
                break;
            case 2:
                break;

        }
    }


}
