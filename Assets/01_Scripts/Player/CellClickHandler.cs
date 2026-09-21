using UnityEngine;

public class CellClickHandler : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private ScoreManager scoreManager;

    private void Update()
    {
        DetectClick();
    }

    private void DetectClick()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Vector2 mouseWorldPosition =mainCamera.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPosition,Vector2.zero);

        if (hit.collider == null)
            return;

        Cell cell = hit.collider.GetComponent<Cell>();

        if (cell == null)
            return;

        scoreManager.AddPoint();

        cell.Eliminate();
    }
}