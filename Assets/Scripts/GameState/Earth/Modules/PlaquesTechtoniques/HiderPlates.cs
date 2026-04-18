using System.Collections.Generic;
using UnityEngine;

public class HiderPlates : MonoBehaviour
{
    [Header("Covers dans l'ordre logique 0 -> 15")]
    [SerializeField] private HiderPlatesInteract[] blockers;

    [Header("Grille")]
    [SerializeField] private int gridWidth = 4;
    [SerializeField] private int gridHeight = 4;
    [SerializeField] private int startEmptyIndex = 15;

    private int emptyBlocker;

    private void Awake()
    {
        SetupBlockers();
        ResetBlockers();
    }

    private void SetupBlockers()
    {
        if (blockers == null || blockers.Length == 0)
            return;

        for (int i = 0; i < blockers.Length; i++)
        {
            if (blockers[i] == null)
                continue;

            blockers[i].SetManager(this, i);
            blockers[i].neighboors = GetNeighbors(i);
        }
    }

    public void ResetBlockers()
    {
        if (blockers == null || blockers.Length == 0)
            return;

        emptyBlocker = Mathf.Clamp(startEmptyIndex, 0, blockers.Length - 1);

        for (int i = 0; i < blockers.Length; i++)
        {
            if (blockers[i] == null)
                continue;

            blockers[i].gameObject.SetActive(i != emptyBlocker);
        }

        Debug.Log($"[HIDER] Reset. Trou initial = {emptyBlocker}");
    }

    public void TryChangingDeactivatedBlocker(int blockerId)
    {
        if (blockers == null || blockers.Length == 0)
            return;

        if (blockerId < 0 || blockerId >= blockers.Length)
            return;

        if (blockerId == emptyBlocker)
            return;

        HiderPlatesInteract clickedBlocker = blockers[blockerId];
        if (clickedBlocker == null)
            return;

        Debug.Log($"[HIDER] Click reçu sur index {blockerId}, objet = {clickedBlocker.name}, trou = {emptyBlocker}");

        int[] neighbors = clickedBlocker.neighboors;
        if (neighbors == null || neighbors.Length == 0)
            return;

        bool canMove = false;
        foreach (int n in neighbors)
        {
            if (n == emptyBlocker)
            {
                canMove = true;
                break;
            }
        }

        if (!canMove)
        {
            Debug.Log($"[HIDER] {clickedBlocker.name} n'est pas voisin du trou.");
            return;
        }

        if (blockers[emptyBlocker] != null)
            blockers[emptyBlocker].gameObject.SetActive(true);

        clickedBlocker.gameObject.SetActive(false);
        emptyBlocker = blockerId;

        Debug.Log($"[HIDER] Nouveau trou = {emptyBlocker}");
    }

    private int[] GetNeighbors(int index)
    {
        List<int> result = new List<int>();

        int row = index / gridWidth;
        int col = index % gridWidth;

        if (row > 0) result.Add(index - gridWidth);              // haut
        if (row < gridHeight - 1) result.Add(index + gridWidth); // bas
        if (col > 0) result.Add(index - 1);                      // gauche
        if (col < gridWidth - 1) result.Add(index + 1);          // droite

        return result.ToArray();
    }
}