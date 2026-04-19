using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.InputSystem;

public class IntroCinematicManager : MonoBehaviour
{
    [Header("Cinematic")]
    [SerializeField] private GameObject cinematicPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("UI")]
    [SerializeField] private GameObject characterSelectionCanvas;

    [Header("Input")]
    [SerializeField] private InputActionReference skipAction;

    private GameObject currentCinematic;
    private PlayableDirector currentDirector;
    private bool isPlaying = false;

    private void Start()
    {
        if (characterSelectionCanvas != null)
            characterSelectionCanvas.SetActive(false);

        PlayCinematic();
    }

    private void OnEnable()
    {
        if (skipAction != null && skipAction.action != null)
        {
            skipAction.action.performed += OnSkipPressed;
            skipAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (skipAction != null && skipAction.action != null)
        {
            skipAction.action.performed -= OnSkipPressed;
            skipAction.action.Disable();
        }
    }

    private void PlayCinematic()
    {
        if (cinematicPrefab == null)
        {
            ShowUI();
            return;
        }

        Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        pos.z = -5f;

        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        currentCinematic = Instantiate(cinematicPrefab, pos, rot);

        currentDirector = currentCinematic.GetComponent<PlayableDirector>();

        if (currentDirector != null)
        {
            currentDirector.stopped += OnCinematicFinished;
            currentDirector.Play();
        }
        else
        {
            Debug.LogWarning("Aucun PlayableDirector trouvé sur la cinématique.");
            ShowUI();
            DestroyCurrentCinematic();
            return;
        }

        isPlaying = true;
    }

    private void OnSkipPressed(InputAction.CallbackContext context)
    {
        if (!context.performed || !isPlaying)
            return;

        SkipCinematic();
    }

    private void SkipCinematic()
    {
        if (currentDirector != null)
            currentDirector.stopped -= OnCinematicFinished;

        DestroyCurrentCinematic();
        ShowUI();
    }

    private void OnCinematicFinished(PlayableDirector director)
    {
        if (director != null)
            director.stopped -= OnCinematicFinished;

        DestroyCurrentCinematic();
        ShowUI();
    }

    private void DestroyCurrentCinematic()
    {
        if (currentCinematic != null)
        {
            Destroy(currentCinematic);
            currentCinematic = null;
        }

        currentDirector = null;
    }

    private void ShowUI()
    {
        isPlaying = false;

        if (characterSelectionCanvas != null)
            characterSelectionCanvas.SetActive(true);
    }
}