using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class CellAgent : Agent
{
    [Header("Referencias")]
    [SerializeField] private Cell cell;

    [Header("Fondo")]
    [SerializeField] private Camera mainCamera;

    [Header("Velocidad de adaptación")]
    [SerializeField] private float colorChangeSpeed = 0.25f;
    [SerializeField] private float sizeChangeSpeed = 0.20f;

    private Color backgroundColor;
    public Color BackgroundColor => backgroundColor;
    public override void Initialize()
    {
        if (cell == null)
        {
            cell = GetComponent<Cell>();
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            backgroundColor =
                mainCamera.backgroundColor;
        }

        Debug.Log(
            "CellAgent inicializado en: " +
            gameObject.name
        );
    }
    public override void OnEpisodeBegin()
    {
        // Por ahora no hacemos nada aquí.
        // El CellSpawner será quien solicite
        // la adaptación.
    }

    // OBSERVACIONES

    public override void CollectObservations(
        VectorSensor sensor)
    {
        if (cell == null)
            return;

        Color currentColor =
            cell.CellColor;

        // Color de la célula
        sensor.AddObservation(currentColor.r);
        sensor.AddObservation(currentColor.g);
        sensor.AddObservation(currentColor.b);

        // Color del fondo
        sensor.AddObservation(backgroundColor.r);
        sensor.AddObservation(backgroundColor.g);
        sensor.AddObservation(backgroundColor.b);

        // Tamaño normalizado
        float normalizedSize =
            Mathf.InverseLerp(
                cell.MinSize,
                cell.MaxSize,
                cell.Size
            );

        sensor.AddObservation(normalizedSize);
    }

    // ACCIONES
    public override void OnActionReceived(
        ActionBuffers actions)
    {
        if (cell == null)
        {
            Debug.LogError(
                "Cell es NULL en CellAgent."
            );

            return;
        }

        float redAction =
            Mathf.Clamp(
                actions.ContinuousActions[0],
                -1f,
                1f
            );

        float greenAction =
            Mathf.Clamp(
                actions.ContinuousActions[1],
                -1f,
                1f
            );

        float blueAction =
            Mathf.Clamp(
                actions.ContinuousActions[2],
                -1f,
                1f
            );

        float sizeAction =
            Mathf.Clamp(
                actions.ContinuousActions[3],
                -1f,
                1f
            );

        Debug.Log(
            gameObject.name +
            " | ACCIONES -> R: " +
            redAction.ToString("F2") +
            " G: " +
            greenAction.ToString("F2") +
            " B: " +
            blueAction.ToString("F2") +
            " Size: " +
            sizeAction.ToString("F2")
        );

        ApplyColorAction(
            redAction,
            greenAction,
            blueAction
        );

        ApplySizeAction(sizeAction);
    }
    // CAMBIAR COLOR

    private void ApplyColorAction(
        float redAction,
        float greenAction,
        float blueAction)
    {
        Color oldColor =
            cell.CellColor;

        Color newColor = new Color(oldColor.r +redAction * colorChangeSpeed,oldColor.g +greenAction * colorChangeSpeed, oldColor.b + blueAction * colorChangeSpeed, 1f );

        cell.SetColor(newColor);

        Debug.Log(gameObject.name +" | COLOR -> Antes: " +oldColor +" | Después: " +cell.CellColor);
    }
    // CAMBIAR TAMAÑO
    private void ApplySizeAction(
        float sizeAction)
    {
        float oldSize =
            cell.Size;

        float newSize =
            oldSize +
            sizeAction * sizeChangeSpeed;

        cell.SetSize(newSize);

        Debug.Log(gameObject.name +" | SIZE -> Antes: " +oldSize.ToString("F2") +" | Después: " +cell.Size.ToString("F2"));
    }
    // HEURISTIC

    public override void Heuristic(
        in ActionBuffers actionsOut)
    {
        ActionSegment<float> actions =
            actionsOut.ContinuousActions;

        // Por defecto no hacemos nada.
        actions[0] = 0f;
        actions[1] = 0f;
        actions[2] = 0f;
        actions[3] = 0f;

        if (Input.GetKey(KeyCode.R))
        {
            actions[0] = 1f;
        }

        if (Input.GetKey(KeyCode.G))
        {
            actions[1] = 1f;
        }

        if (Input.GetKey(KeyCode.B))
        {
            actions[2] = 1f;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            actions[3] = -1f;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            actions[3] = 1f;
        }
    }
    // SOLICITAR ADAPTACIÓN
    public void RequestAdaptation()
    {
        RequestDecision();
    }
    // RECOMPENSAS
    public void OnClickedByPlayer()
    {
        AddReward(-1f);

        if (TrainingMetrics.Instance != null)
        {
            TrainingMetrics.Instance
                .RegisterClickedCell();
        }

        Debug.Log(
            gameObject.name +
            " | Eliminada | Reward: -1"
        );

        EndEpisode();
    }

    public void OnSurvivedRound()
    {
        AddReward(1f);

        if (TrainingMetrics.Instance != null)
        {
            TrainingMetrics.Instance.RegisterSurvivor(cell.Size,cell.CellColor,backgroundColor);
        }

        Debug.Log(
            gameObject.name +
            " | Sobrevivió | Reward: +1"
        );

        EndEpisode();
    }
}