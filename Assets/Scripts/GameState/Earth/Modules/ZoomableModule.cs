using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// R�sum� de ce que fait se script pour que tu comprends comment ca fonctionne.
/// </summary>
// Ce script rend un module cliquable via MouseInteractionManager.IInteractable.
// Quand on clique dessus :
// - il se d�place au centre de l'�cran
// - il grossit
// - un fond flou appara�t
// - les scripts interactables des enfants s'activent
// - les SpriteRenderer du module et de ses enfants apparaissent progressivement
// - les TextMeshPro du module et de ses enfants apparaissent progressivement
//
// Quand on le ferme :
// - il retourne � sa position d'origine
// - il reprend son �chelle d'origine
// - le fond flou dispara�t
// - les scripts interactables des enfants se d�sactivent
// - les SpriteRenderer du module et de ses enfants disparaissent progressivement
// - les TextMeshPro du module et de ses enfants disparaissent progressivement
//
// La fermeture peut se faire :
// - via la touche Escape / l'Input Action assign�e
// - via un clic ext�rieur g�r� par MouseInteractionManager <= (ducoup je les modifi� aussi)
public class ZoomableModule : MonoBehaviour, MouseInteractionManager.IInteractable
{
    [Header("R�f�rences")]

    [SerializeField] private Camera targetCamera;

    [SerializeField] private Transform zoomTarget;

    [SerializeField] private CanvasGroup blurBackground;

    [SerializeField] private Module moduleToStart;

    [Header("Input System")]

    [SerializeField] private InputActionReference closeAction;

    [Header("Zoom")]

    [SerializeField] private float zoomDuration = 0.35f;

    [SerializeField] private float zoomScaleMultiplier = 2f;

    [Tooltip("D�calage Z par rapport � la cam�ra quand le module est ouvert. Plus petit = plus proche de la cam�ra.")]
    [SerializeField] private float openedZOffsetFromCamera = 1f;

    [Header("D�tection auto")]

    [SerializeField] private bool autoFindChildInteractables = true;

    [Header("Sorting")]
    [SerializeField] private bool bringToFrontWhenOpened = true;
    [SerializeField] private int openedSortingOrderOffset = 100;

    [Header("Sprite au zoom")]
    [SerializeField] private SpriteRenderer mainSpriteRenderer;
    [SerializeField] private Sprite zoomedMainSprite;

    private Sprite originalMainSprite;
    private bool mainSpriteCached = false;

    private readonly Dictionary<SpriteRenderer, int> originalSpriteSortingOrders = new();
    private readonly Dictionary<Renderer, int> originalTextSortingOrders = new();
    private bool sortingOffsetApplied = false;

    [Header("Visuel")]

    [SerializeField] private bool hideVisualsWhenClosed = true;

    private readonly List<MonoBehaviour> childScriptsToEnable = new();

    private readonly List<SpriteRenderer> cachedSpriteRenderers = new();

    private readonly List<TMP_Text> cachedTmpTexts = new();

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
        // M�morisation des param�tre de d�part pour y revenir apr�s.
        startPosition = transform.position;
        startScale = transform.localScale;
        startRotation = transform.rotation;

        if (targetCamera == null)
            targetCamera = Camera.main;

        // Recherche automatiquement les enfants interactables
        if (autoFindChildInteractables)
            CacheChildInteractables();

        // Recherche automatiquement tous les �l�ments visuels du module et de ses enfants
        CacheVisualComponents();
        CacheOriginalSortingOrders();
        CacheMainSpriteIfNeeded();

        // Au d�marrage, d�sactive les scripts interactables des enfants pour qu'ils ne puissent pas �tre utilis�s tant que le module n'est pas ouvert
        SetChildScriptsState(false);

        // Au d�marrage, rend le module invisible visuellement tout en gardant ses colliders actifs
        if (hideVisualsWhenClosed)
            SetVisualAlpha(0f);

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
        // Si une animation est d�j� en cours, on ignore le clic
        if (isAnimating)
            return;

        // IMPORTANT : si le module est d�j� ouvert, un clic dessus ne le ferme pas
        if (isOpen)
            return;

        // Si un autre module est d�j� ouvert, on ne fait rien
        if (AnyModuleOpen)
            return;
        moduleToStart.OnStarted();
        StartCoroutine(OpenModuleRoutine());
    }

    public void OnHoverEnter()
    {

    }

    public void OnHoverExit()
    {

    }

    private void CacheOriginalSortingOrders()
    {
        originalSpriteSortingOrders.Clear();
        originalTextSortingOrders.Clear();

        for (int i = 0; i < cachedSpriteRenderers.Count; i++)
        {
            SpriteRenderer sr = cachedSpriteRenderers[i];
            if (sr != null && !originalSpriteSortingOrders.ContainsKey(sr))
                originalSpriteSortingOrders.Add(sr, sr.sortingOrder);
        }

        for (int i = 0; i < cachedTmpTexts.Count; i++)
        {
            TMP_Text tmp = cachedTmpTexts[i];
            if (tmp == null)
                continue;

            Renderer textRenderer = tmp.GetComponent<Renderer>() as Renderer;
            if (textRenderer != null && !originalTextSortingOrders.ContainsKey(textRenderer))
                originalTextSortingOrders.Add(textRenderer, textRenderer.sortingOrder);
        }
    }

    private void ApplySortingOffset()
    {
        if (!bringToFrontWhenOpened || sortingOffsetApplied)
            return;

        CacheOriginalSortingOrders();

        foreach (var pair in originalSpriteSortingOrders)
        {
            if (pair.Key != null)
                pair.Key.sortingOrder = pair.Value + openedSortingOrderOffset;
        }

        foreach (var pair in originalTextSortingOrders)
        {
            if (pair.Key != null)
                pair.Key.sortingOrder = pair.Value + openedSortingOrderOffset;
        }

        sortingOffsetApplied = true;
    }

    private void RestoreOriginalSortingOrders()
    {
        if (!sortingOffsetApplied)
            return;

        foreach (var pair in originalSpriteSortingOrders)
        {
            if (pair.Key != null)
                pair.Key.sortingOrder = pair.Value;
        }

        foreach (var pair in originalTextSortingOrders)
        {
            if (pair.Key != null)
                pair.Key.sortingOrder = pair.Value;
        }

        sortingOffsetApplied = false;
    }

    public void CloseModule()
    {
        // Refuse de fermer si le module n'est pas ouvert ou si une animation est d�j� en cours
        if (!isOpen || isAnimating)
            return;

        StartCoroutine(CloseModuleRoutine());
    }

    public static void CloseCurrentOpenModule()
    {
        // M�thode statique pratique pour fermer le module actuellement ouvert
        if (CurrentOpenModule != null)
            CurrentOpenModule.CloseModule();
    }

    private IEnumerator OpenModuleRoutine()
    {
        isAnimating = true;

        AnyModuleOpen = true;
        CurrentOpenModule = this;

        SetChildScriptsState(false);
        ApplySortingOffset();
        ApplyZoomedMainSprite();

        // On stocke l'�tat actuel de d�part
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

        // S'assure que le module est bien invisible au tout d�but de l'ouverture
        if (hideVisualsWhenClosed)
            SetVisualAlpha(0f);

        // Animation progressive
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;

            // t va de 0 � 1
            float t = Mathf.Clamp01(elapsed / zoomDuration);

            t = EaseOutCubic(t);

            // Interpolation position / taille / rotation
            transform.position = Vector3.Lerp(fromPos, toPos, t);
            transform.localScale = Vector3.Lerp(fromScale, toScale, t);
            transform.rotation = Quaternion.Slerp(fromRot, toRot, t);

            // Fade progressif du fond flou
            if (blurBackground != null)
                blurBackground.alpha = Mathf.Lerp(0f, 1f, t);

            // Fade progressif du module et de ses enfants
            if (hideVisualsWhenClosed)
                SetVisualAlpha(Mathf.Lerp(0f, 1f, t));

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

        if (hideVisualsWhenClosed)
            SetVisualAlpha(1f);

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

            // Fade progressif du module et de ses enfants vers l'invisible
            if (hideVisualsWhenClosed)
                SetVisualAlpha(Mathf.Lerp(1f, 0f, t));

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

        if (hideVisualsWhenClosed)
            SetVisualAlpha(0f);

        RestoreOriginalMainSprite();
        RestoreOriginalSortingOrders();

        isOpen = false;
        isAnimating = false;
        AnyModuleOpen = false;

        if (CurrentOpenModule == this)
            CurrentOpenModule = null;
    }

    private void CacheMainSpriteIfNeeded()
    {
        if (mainSpriteCached)
            return;

        if (mainSpriteRenderer == null)
            mainSpriteRenderer = GetComponent<SpriteRenderer>();

        if (mainSpriteRenderer != null)
        {
            originalMainSprite = mainSpriteRenderer.sprite;
            mainSpriteCached = true;
        }
    }

    private void ApplyZoomedMainSprite()
    {
        // On ne fait rien si aucun sprite de zoom n'a été assigné
        if (zoomedMainSprite == null)
            return;

        CacheMainSpriteIfNeeded();

        if (mainSpriteRenderer == null)
            return;

        mainSpriteRenderer.sprite = zoomedMainSprite;
    }

    private void RestoreOriginalMainSprite()
    {
        // Si aucun sprite de zoom n'était assigné, on n'a rien changé donc on sort
        if (zoomedMainSprite == null)
            return;

        CacheMainSpriteIfNeeded();

        if (mainSpriteRenderer == null)
            return;

        mainSpriteRenderer.sprite = originalMainSprite;
    }

    private void CacheChildInteractables()
    {
        // On vide d'abord la liste au cas o� cette m�thode est relanc�e
        childScriptsToEnable.Clear();

        // On lance une vraie recherche r�cursive
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

    private void CacheVisualComponents()
    {
        cachedSpriteRenderers.Clear();
        cachedTmpTexts.Clear();

        CacheVisualComponentsRecursive(transform);
    }

    private void CacheVisualComponentsRecursive(Transform current)
    {
        if (current == null)
            return;

        SpriteRenderer spriteRenderer = current.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && !cachedSpriteRenderers.Contains(spriteRenderer))
            cachedSpriteRenderers.Add(spriteRenderer);

        TMP_Text tmpText = current.GetComponent<TMP_Text>();
        if (tmpText != null && !cachedTmpTexts.Contains(tmpText))
            cachedTmpTexts.Add(tmpText);

        for (int i = 0; i < current.childCount; i++)
        {
            CacheVisualComponentsRecursive(current.GetChild(i));
        }
    }

    private void SetVisualAlpha(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);

        for (int i = 0; i < cachedSpriteRenderers.Count; i++)
        {
            if (cachedSpriteRenderers[i] == null)
                continue;

            Color color = cachedSpriteRenderers[i].color;
            color.a = alpha;
            cachedSpriteRenderers[i].color = color;
        }

        for (int i = 0; i < cachedTmpTexts.Count; i++)
        {
            if (cachedTmpTexts[i] == null)
                continue;

            Color color = cachedTmpTexts[i].color;
            color.a = alpha;
            cachedTmpTexts[i].color = color;
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