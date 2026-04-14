using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Résumé de ce que fait se script pour que tu comprends comment ca fonctionne.
/// </summary>
// Ce script rend un module cliquable via MouseInteractionManager.IInteractable.
// Quand on clique dessus :
// - il se déplace au centre de l'écran
// - il grossit
// - un fond flou apparaît
// - les scripts interactables des enfants s'activent
//
// Quand on le ferme :
// - il retourne à sa position d'origine
// - il reprend son échelle d'origine
// - le fond flou disparaît
// - les scripts interactables des enfants se désactivent
//
// La fermeture peut se faire :
// - via la touche Escape / l'Input Action assignée
// - via un clic extérieur géré par MouseInteractionManager <= (ducoup je les modifié aussi)
public class ZoomableModule : MonoBehaviour, MouseInteractionManager.IInteractable
{
    [Header("Références")]

    [SerializeField] private Camera targetCamera;

    [SerializeField] private Transform zoomTarget;

    [SerializeField] private CanvasGroup blurBackground;

    [Header("Input System")]

    [SerializeField] private InputActionReference closeAction;

    [Header("Zoom")]

    [SerializeField] private float zoomDuration = 0.35f;

    [SerializeField] private float zoomScaleMultiplier = 2f;

    [Tooltip("Décalage Z par rapport à la caméra quand le module est ouvert. Plus petit = plus proche de la caméra.")]
    [SerializeField] private float openedZOffsetFromCamera = 1f;

    [Header("Détection auto")]

    [SerializeField] private bool autoFindChildInteractables = true;

    private readonly List<MonoBehaviour> childScriptsToEnable = new();

    private Vector3 startPosition;

    private Vector3 startScale;

    // Rotation d'origine du module avant ouverture
    private Quaternion startRotation;

    private bool isOpen = false;

    private bool isAnimating = false;

    public static bool AnyModuleOpen = false;

    public static ZoomableModule CurrentOpenModule { get; private set; }

    public bool IsOpen => isOpen;

    public bool IsAnimating => isAnimating;

    private void Awake()
    {
        // Mémorisation des paramètre de départ pour y revenir après.
        startPosition = transform.position;
        startScale = transform.localScale;
        startRotation = transform.rotation;

        if (targetCamera == null)
            targetCamera = Camera.main;

        // Recherche automatiquement les enfants interactables
        if (autoFindChildInteractables)
            CacheChildInteractables();

        // Au démarrage, désactive les scripts interactables des enfants pour qu'ils ne puissent pas être utilisés tant que le module n'est pas ouvert
        SetChildScriptsState(false);

        if (blurBackground != null)
        {
            blurBackground.alpha = 0f;
            blurBackground.interactable = false;
            blurBackground.blocksRaycasts = false;
        }
    }

    private void OnEnable()
    {
        if (closeAction != null && closeAction.action != null)
        {
            closeAction.action.performed += OnCloseActionPerformed;
            closeAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (closeAction != null && closeAction.action != null)
        {
            closeAction.action.performed -= OnCloseActionPerformed;
            closeAction.action.Disable();
        }

        if (CurrentOpenModule == this)
            CurrentOpenModule = null;
    }

    private void OnDestroy()
    {
        if (closeAction != null && closeAction.action != null)
            closeAction.action.performed -= OnCloseActionPerformed;

        if (CurrentOpenModule == this)
            CurrentOpenModule = null;
    }

    private void OnCloseActionPerformed(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        CloseModule();
    }

    public void OnClick()
    {
        // Si une animation est déjà en cours, on ignore le clic
        if (isAnimating)
            return;

        // IMPORTANT : si le module est déjà ouvert, un clic dessus ne le ferme pas
        if (isOpen)
            return;

        // Si un autre module est déjà ouvert, on ne fait rien
        if (AnyModuleOpen)
            return;

        StartCoroutine(OpenModuleRoutine());
    }

    public void OnHoverEnter()
    {

    }

    public void OnHoverExit()
    {

    }

    public void CloseModule()
    {
        // Refuse de fermer si le module n'est pas ouvert ou si une animation est déjà en cours
        if (!isOpen || isAnimating)
            return;

        StartCoroutine(CloseModuleRoutine());
    }

    public static void CloseCurrentOpenModule()
    {
        // Méthode statique pratique pour fermer le module actuellement ouvert
        if (CurrentOpenModule != null)
            CurrentOpenModule.CloseModule();
    }

    private IEnumerator OpenModuleRoutine()
    {
        isAnimating = true;

        AnyModuleOpen = true;

        CurrentOpenModule = this;

        SetChildScriptsState(false);

        // On stocke l'état actuel de départ
        Vector3 fromPos = transform.position;
        Vector3 fromScale = transform.localScale;
        Quaternion fromRot = transform.rotation;

        // Calcule de la destination finale
        Vector3 toPos = GetTargetCenterPosition();

        // Calcule de la taille finale
        Vector3 toScale = startScale * zoomScaleMultiplier;

        // Rotation finale choisie : identity (pas de rotation) (si rotation d'un module pour tel events jsp)
        Quaternion toRot = Quaternion.identity;

        float elapsed = 0f;

        // Animation progressive
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;

            // t va de 0 à 1
            float t = Mathf.Clamp01(elapsed / zoomDuration);

            t = EaseOutCubic(t);

            // Interpolation position / taille / rotation
            transform.position = Vector3.Lerp(fromPos, toPos, t);
            transform.localScale = Vector3.Lerp(fromScale, toScale, t);
            transform.rotation = Quaternion.Slerp(fromRot, toRot, t);

            if (blurBackground != null)
                blurBackground.alpha = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        transform.position = toPos;
        transform.localScale = toScale;
        transform.rotation = toRot;

        if (blurBackground != null)
        {
            blurBackground.alpha = 1f;
            blurBackground.interactable = false;
            blurBackground.blocksRaycasts = false;
        }

        SetChildScriptsState(true);

        isOpen = true;
        isAnimating = false;
    }

    private IEnumerator CloseModuleRoutine()
    {
        isAnimating = true;

        SetChildScriptsState(false);

        Vector3 fromPos = transform.position;
        Vector3 fromScale = transform.localScale;
        Quaternion fromRot = transform.rotation;

        float elapsed = 0f;

        // Animation progressive
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / zoomDuration);
            t = EaseOutCubic(t);

            // Retour progressif vers la position, taille et rotation d'origine
            transform.position = Vector3.Lerp(fromPos, startPosition, t);
            transform.localScale = Vector3.Lerp(fromScale, startScale, t);
            transform.rotation = Quaternion.Slerp(fromRot, startRotation, t);

            if (blurBackground != null)
                blurBackground.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        transform.position = startPosition;
        transform.localScale = startScale;
        transform.rotation = startRotation;

        if (blurBackground != null)
        {
            blurBackground.alpha = 0f;
            blurBackground.interactable = false;
            blurBackground.blocksRaycasts = false;
        }

        isOpen = false;
        isAnimating = false;
        AnyModuleOpen = false;

        if (CurrentOpenModule == this)
            CurrentOpenModule = null;
    }

    private void CacheChildInteractables()
    {
        // On vide d'abord la liste au cas où cette méthode est relancée
        childScriptsToEnable.Clear();

        // On lance une vraie recherche récursive
        CacheChildInteractablesRecursive(transform);
    }

    private void CacheChildInteractablesRecursive(Transform current)
    {
        for (int i = 0; i < current.childCount; i++)
        {
            Transform child = current.GetChild(i);

            if (child == null)
                continue;

            MonoBehaviour[] behavioursOnChild = child.GetComponents<MonoBehaviour>();

            for (int j = 0; j < behavioursOnChild.Length; j++)
            {
                MonoBehaviour mb = behavioursOnChild[j];

                if (mb == null)
                    continue;

                if (mb is MouseInteractionManager.IInteractable)
                {
                    if (!childScriptsToEnable.Contains(mb))
                        childScriptsToEnable.Add(mb);
                }
            }

            CacheChildInteractablesRecursive(child);
        }
    }

    private Vector3 GetTargetCenterPosition()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        Vector3 targetPos;

        if (zoomTarget != null)
        {
            targetPos = zoomTarget.position;
        }
        else
        {
            float distanceFromCamera = Mathf.Abs(transform.position.z - targetCamera.transform.position.z);

            Vector3 screenCenter = new Vector3(
                Screen.width * 0.5f,
                Screen.height * 0.5f,
                distanceFromCamera
            );

            targetPos = targetCamera.ScreenToWorldPoint(screenCenter);
        }

        targetPos.z = targetCamera.transform.position.z + openedZOffsetFromCamera;

        return targetPos;
    }

    private void SetChildScriptsState(bool state)
    {
        for (int i = 0; i < childScriptsToEnable.Count; i++)
        {
            if (childScriptsToEnable[i] != null)
                childScriptsToEnable[i].enabled = state;
        }
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}