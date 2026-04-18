using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
static class RandomExtensions
{
    public static void Shuffle<T>(this System.Random rng, T[] array)
    {
        //Credit: StackOverflow
        int n = array.Length;
        while (n > 1)
        {
            int k = rng.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }
}
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
    private int[] keyLocations;
    [SerializeField] private Sprite[] correctColors; 

    [SerializeField] private GameManagerLocal gm;
    private int ledState;
    private bool autoLose;
    private int[] code;
    private string[] codeWritten= new string[3] {"_","_","_"};
    private int currentLetterInd = 0;

    CEventRuntimeData? activeEvent = null;


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
            return;
        }
        Debug.Log($"Trying to solve event {activeEvent.Value.eventId}");
        ledState = activeEvent.Value.module1Option;
        Debug.Log($" ledState: {ledState}");
        Randomizer();
        ColoredFixer(ledState);
    }

    private void OnEnter(InputAction.CallbackContext context)
    {
        Debug.Log("enter");
        if (autoLose)
        {
            Debug.Log("no event found for this module, trying to lose oldest");
            gm.EndModuleCheck(moduleId, false, -1);
        }
        else
        {
            int eventId = activeEvent.Value.eventId;
            if (currentLetterInd != 2)
            {
                Debug.Log("not enough letters. Losing the event.");
                gm.EndModuleCheck(moduleId, false, eventId);
            }

            else
            {
                Debug.Log("enough Letters. Verifying.");
                string[] stringCode = new string[code.Length];
                for (int i = 0; i < stringCode.Length; i++)
                {
                    stringCode[i] = ((char)('A' + code[i])).ToString();
                    Debug.Log($"number {code[i]} converted to string {stringCode[i]}");
                    if (stringCode[i] != codeWritten[i] )
                    {
                        Debug.Log($"mismatch: expected {stringCode[i]}, got {codeWritten[i]}. Losing the event.");
                        gm.EndModuleCheck(moduleId, false, eventId);
                        return;
                    }
                }
                Debug.Log("End verify, all good. Validating event.");
                gm.EndModuleCheck(moduleId,true, eventId);

            }
            
        }
        

    }
    public void OnBackSpace(InputAction.CallbackContext context)
    {
        Debug.Log("backspaced");
        if (currentLetterInd == 0)
        {
            Debug.Log("already have no letters"); return;
        }
        codeWritten[currentLetterInd] = "_";
        currentLetterInd--;
    }

    private void OnLetterPress(InputAction.CallbackContext context)
    {
        Debug.Log("Letter pressed: " + context.control.displayName);

        if (currentLetterInd == 2) { Debug.Log("already have 3 letters"); return; }
        codeWritten[currentLetterInd] = context.control.displayName.ToUpper();
        currentLetterInd++;
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

    public void ColoredFixer(int color)
    {
        code = new int[3] { Random.Range(0, 25), Random.Range(0, 25), Random.Range(0, 25) };
        if (color == 2) { Array.Sort(code); }
        int[] keyLocId = new int[3] { Random.Range(0, 6), Random.Range(0, 6), Random.Range(0, 6) };
        Array.Sort(keyLocId);
        int[] codePlace = new int[3] { keyLocations[keyLocId[0]], keyLocations[keyLocId[1]], keyLocations[keyLocId[2]] };
        switch (color)
        {

            case 0:
                //Code order: numericals, blue code

                keyLocations = new int[6] { 0, 3, 6, 9, 11, 13 };
                break;
            case 1:
                //Code order: alphabetical, green code
                keyLocations = new int[6] { 0, 9, 13, 6, 3, 11 };
                break;
            case 2:
                //Code order: , pink code
                keyLocations = new int[6] { 0, 3, 6, 9, 11, 13 };
                var rng = new System.Random();
                rng.Shuffle(keyLocations);
                break;

        }
        int codeIndex = 0;
        for (int i = 0; i < 6; i++)
        {
            int keyLocation = keyLocations[i];
            if (keyLocation == codePlace[codeIndex])
            {
                colorPlates[keyLocation].GetComponent<SpriteRenderer>().sprite = correctColors[i];
                letters[keyLocation].GetComponent<SpriteRenderer>().sprite = lettersSprite[code[codeIndex]];
                codeIndex++;
            }
            else
            {
                SpriteRenderer cpsr = colorPlates[keyLocation].GetComponent<SpriteRenderer>();
                SpriteRenderer lsr = letters[keyLocation].GetComponent<SpriteRenderer>();
                while (cpsr.sprite == correctColors[i] || lsr == lettersSprite[code[codeIndex]])
                {
                    cpsr.sprite = correctColors[Random.Range(0, 4)];
                    lsr.sprite = lettersSprite[Random.Range(0, 25)];
                }
            }
        }
    }


}
