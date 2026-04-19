using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
static class RandomExtensions
{
    public static void Shuffle<T>(this System.Random rng, T[] array)
    {
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
    private const int HiddenCellIndex = 15;

    [Header("Keyboard Reading")]
    [SerializeField] private InputAction writing;
    [SerializeField] private InputAction backspace;
    [SerializeField] private InputAction enter;

    [Header("Board")]
    [SerializeField] private GameObject[] colorPlates;
    [SerializeField] private GameObject[] letters;
    [SerializeField] private GameObject[] covers;

    [Header("Validation Display")]
    [SerializeField] private TextMeshPro[] codeDisplay;

    [Header("Data")]
    [SerializeField] private Sprite[] lettersSprite;
    [SerializeField] private Sprite[] correctColors;
    private int[] colorCodes;
    [SerializeField] private TEcPlateCodeSO[] allCodes;
    [Header("References")]
    [SerializeField] private GameManagerLocal gm;

    [Header("Debug")]
    [SerializeField] private bool testMode = false;
    [SerializeField] private int testColorMode = 0;
    private int[] keyLocations;
    private int ledState;
    private int codeType;
    private bool autoLose;
    private int[] code;
    private string[] codeWritten = new string[3] { "_", "_", "_" };
    private int currentLetterInd = 0;

    private CEventRuntimeData? activeEvent = null;


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
        enter.performed -= OnEnter;
        backspace.performed -= OnBackSpace;
        writing.performed -= OnLetterPress;

        writing.Disable();
        backspace.Disable();
        enter.Disable();
    }

    public override void OnStarted()
    {
        ResetInputDisplay();
        ResetBoard();
        Randomizer();

        int currReg = gm.currentRegion;
        activeEvent = null;
        autoLose = false;

        foreach (CEventRuntimeData cEventData in gm.gmn.events)
        {
            if (cEventData.state != 1)
                continue;

            CatastrophicEvent cEvent = gm.allEvents[cEventData.eventId];

            if (cEvent.region == currReg &&
                cEventData.module1Option != -1 &&
                cEvent.modules2.Contains(moduleId))
            {
                activeEvent = cEventData;
                break;
            }
        }

        if (!activeEvent.HasValue)
        {
            
            Debug.Log("No event found in this region for this module. Any validation will fail.");
            autoLose = true;
            return;
        }

        Debug.Log($"Trying to solve event {activeEvent.Value.eventId}");
        ledState = activeEvent.Value.module1Option;
        Debug.Log($"ledState: {ledState}");
        foreach (TEcPlateCodeSO code in allCodes)
        {
            if(code.eventId == activeEvent.Value.eventId)
            {
                codeType = code.codeOptions[ledState];
                switch (ledState)
                {
                    case 0:
                        keyLocations = code.keyPositionsBlue;
                        colorCodes = code.colorPositionsBlue;
                        break;
                    case 1:
                        keyLocations = code.keyPositionsGreen;
                        colorCodes = code.colorPositionsGreen;
                        break;
                    case 2:
                        keyLocations = code.keyPositionsPink;
                        colorCodes = code.colorPositionsPink;
                        break;
                }
                
            }
        }
        ColoredFixer(codeType);
    }

    private void ResetBoard()
    {
        for (int i = 0; i < colorPlates.Length; i++)
        {
            if (colorPlates[i] == null)
                continue;

            SpriteRenderer plateSr = colorPlates[i].GetComponent<SpriteRenderer>();
            if (plateSr != null)
                plateSr.enabled = true;
        }

        for (int i = 0; i < letters.Length; i++)
        {
            if (letters[i] == null)
                continue;

            SpriteRenderer letterSr = letters[i].GetComponent<SpriteRenderer>();
            if (letterSr != null)
                letterSr.enabled = true;
        }

        ResetCovers();
    }

    private void ResetCovers()
    {
        if (covers == null || covers.Length == 0)
            return;

        for (int i = 0; i < covers.Length; i++)
        {
            if (covers[i] == null)
                continue;

            covers[i].SetActive(i != HiddenCellIndex);
        }
    }

    private void ResetInputDisplay()
    {
        currentLetterInd = 0;
        codeWritten = new string[3] { "_", "_", "_" };
        UpdateCodeDisplay();
    }

    private void UpdateCodeDisplay()
    {
        if (codeDisplay == null || codeDisplay.Length < 3)
            return;

        for (int i = 0; i < 3; i++)
        {
            if (codeDisplay[i] != null)
                codeDisplay[i].text = codeWritten[i];
        }
    }

    private void OnEnter(InputAction.CallbackContext context)
    {
        Debug.Log("Enter");

        if (autoLose)
        {
            Debug.Log("No event found for this module, losing oldest.");
            gm.EndModuleCheck(moduleId, false, -1);
            return;
        }

        int eventId = activeEvent.Value.eventId;

        if (currentLetterInd != 3)
        {
            Debug.Log("Not enough letters. Losing the event.");
            gm.EndModuleCheck(moduleId, false, eventId);
            return;
        }

        Debug.Log("Enough letters. Verifying.");
        string[] stringCode = new string[code.Length];

        for (int i = 0; i < stringCode.Length; i++)
        {
            stringCode[i] = ((char)('A' + code[i])).ToString();

            if (stringCode[i] != codeWritten[i])
            {
                Debug.Log($"Mismatch: expected {stringCode[i]}, got {codeWritten[i]}. Losing the event.");
                gm.EndModuleCheck(moduleId, false, eventId);
                return;
            }
        }

        Debug.Log("End verify, all good. Validating event.");
        gm.EndModuleCheck(moduleId, true, eventId);
    }

    public void OnBackSpace(InputAction.CallbackContext context)
    {
        if (currentLetterInd == 0)
        {
            Debug.Log("Already have no letters");
            return;
        }
            

        currentLetterInd--;
        codeWritten[currentLetterInd] = "_";
        UpdateCodeDisplay();

        Debug.Log("Backspace");
    }

    private void OnLetterPress(InputAction.CallbackContext context)
    {
        string pressedKey = context.control.displayName.ToUpper();

        if (currentLetterInd >= 3)
        {
            Debug.Log("Already have 3 letters");
            return;
        }
            

        if (string.IsNullOrWhiteSpace(pressedKey) || pressedKey.Length != 1)
            return;

        char c = pressedKey[0];
        if (c < 'A' || c > 'Z')
            return;

        codeWritten[currentLetterInd] = pressedKey;
        currentLetterInd++;
        UpdateCodeDisplay();

        Debug.Log($"Typed: {pressedKey}");
    }

    public void Randomizer()
    {
        Debug.Log($"correctColors={correctColors.Length}, lettersSprite={lettersSprite.Length}");

        for (int i = 0; i < colorPlates.Length; i++)
        {
            SpriteRenderer cpsr = colorPlates[i].GetComponent<SpriteRenderer>();
            SpriteRenderer lsr = letters[i].GetComponent<SpriteRenderer>();

            Debug.Log($"cell {i} | plateSR={(cpsr != null)} | letterSR={(lsr != null)}");

            if (cpsr != null && correctColors.Length > 0)
                cpsr.sprite = correctColors[Random.Range(0, correctColors.Length)];

            if (lsr != null && lettersSprite.Length > 0)
                lsr.sprite = lettersSprite[Random.Range(0, lettersSprite.Length)];
        }
    }

    public void ColoredFixer(int color)
    {
        code = new int[3]
        {
            Random.Range(0, 26),
            Random.Range(0, 26),
            Random.Range(0, 26)
        };

        if (color == 2)
            Array.Sort(code);
        if (color == 5) 
        {
            Array.Sort(code);
            Array.Reverse(code);
        }    


        int[] keyLocId = new int[3]
        {
            Random.Range(0, 6),
            Random.Range(0, 6),
            Random.Range(0, 6)
        };

        while (keyLocId[0] == keyLocId[1] || keyLocId[1] == keyLocId[2] || keyLocId[0] == keyLocId[2])
        {
            keyLocId[0] = Random.Range(0, 6);
            keyLocId[1] = Random.Range(0, 6);
            keyLocId[2] = Random.Range(0, 6);
        }

        Array.Sort(keyLocId);

        int[] codePlace = new int[3]
        {
            keyLocations[keyLocId[0]],
            keyLocations[keyLocId[1]],
            keyLocations[keyLocId[2]]
        };

        int codeIndex = 0;

        for (int i = 0; i < keyLocations.Length; i++)
        {
            int keyLocation = keyLocations[i];
            Debug.Log($"Current KeyLocation: {keyLocations[i]}");
            Debug.Log($"Current location to find: {codePlace[codeIndex]}");
            SpriteRenderer cpsr = colorPlates[keyLocation].GetComponent<SpriteRenderer>();
            SpriteRenderer lsr = letters[keyLocation].GetComponent<SpriteRenderer>();

            if (cpsr == null || lsr == null)
                continue;

            if (codeIndex < codePlace.Length && keyLocation == codePlace[codeIndex])
            {
                Debug.Log($"Placed right letter: {(char) ('A'+code[codeIndex])} with the right Color: {colorCodes[codeIndex]} ");
                cpsr.sprite = correctColors[colorCodes[codeIndex]];
                lsr.sprite = lettersSprite[code[codeIndex]];
                codeIndex++;
            }
            else
            {
                Sprite randomColor = correctColors[Random.Range(0, correctColors.Length)];
                Sprite randomLetter = lettersSprite[Random.Range(0, lettersSprite.Length)];

                int safety = 0;
                while (
                    codeIndex < codePlace.Length &&
                    (randomColor == correctColors[Mathf.Clamp(color, 0, correctColors.Length - 1)] ||
                     randomLetter == lettersSprite[code[Mathf.Min(codeIndex, code.Length - 1)]]) &&
                    safety < 20
                )
                {
                    randomColor = correctColors[Random.Range(0, correctColors.Length)];
                    randomLetter = lettersSprite[Random.Range(0, lettersSprite.Length)];
                    safety++;
                }

                cpsr.sprite = randomColor;
                lsr.sprite = randomLetter;
            }
        }
    }
}