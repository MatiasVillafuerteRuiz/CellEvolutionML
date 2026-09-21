using UnityEngine;

public class Cell : MonoBehaviour
{
    [Header("Tamaño")]
    [SerializeField] private float size = 1f;
    [SerializeField] private float minSize = 0.4f;
    [SerializeField] private float maxSize = 1.2f;

    [Header("Color")]
    [SerializeField] private Color cellColor = Color.white;

    private bool isAlive = true;

    private CellSpawner cellSpawner;
    private SpriteRenderer spriteRenderer;

    public float Size => size;
    public Color CellColor => cellColor;
    public bool IsAlive => isAlive;

    public float MinSize => minSize;
    public float MaxSize => maxSize;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("La célula no tiene SpriteRenderer.");
        }
    }

    private void Start()
    {
        ApplySize();
        ApplyColor();
    }

    public void Initialize(CellSpawner spawner)
    {
        cellSpawner = spawner;
    }

    public void SetSize(float newSize)
    {
        size = Mathf.Clamp( newSize,minSize,maxSize);

        ApplySize();
    }

    private void ApplySize()
    {
        transform.localScale =
            Vector3.one * size;
    }
    public void SetColor(Color newColor)
    {
        cellColor = new Color(Mathf.Clamp01(newColor.r),Mathf.Clamp01(newColor.g), Mathf.Clamp01(newColor.b),1f);

        ApplyColor();
    }

    private void ApplyColor()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color = cellColor;
    }
    public void Eliminate()
    {
        if (!isAlive)
            return;

        isAlive = false;

        CellAgent agent =
            GetComponent<CellAgent>();

        if (agent != null)
        {
            agent.OnClickedByPlayer();
        }

        Debug.Log(
            "Célula eliminada por el jugador."
        );

        if (cellSpawner != null)
        {
            cellSpawner.NotifyCellEliminated(
                gameObject
            );
        }

        Destroy(gameObject);
    }
}