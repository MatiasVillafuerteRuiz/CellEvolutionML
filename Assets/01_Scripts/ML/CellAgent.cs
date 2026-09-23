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
    [Tooltip("Cada célula toma UNA sola decisión de color por ronda (al nacer). Este valor debe ser lo bastante grande (>=1) para que, en ese único paso, la red pueda llevar el color aleatorio inicial hasta cualquier punto del espacio RGB si así lo aprendió.")]
    [SerializeField] private float colorChangeSpeed = 1.2f;
    [SerializeField] private float sizeChangeSpeed = 0.20f;

    [Header("Recompensa por camuflaje (shaping)")]
    [Tooltip("Recompensa pequeña y continua que empuja a la célula a parecerse al fondo, además del +1/-1 de sobrevivir o ser detectada. Ayuda a que el aprendizaje converja más rápido.")]
    [SerializeField] private bool useRewardShaping = true;
    [SerializeField] private float shapingWeight = 0.01f;

    [Header("Depuración")]
    [Tooltip("Si está activo, imprime en consola cada acción/paso. Desactívalo durante el entrenamiento real: con miles de pasos satura la consola y ralentiza todo.")]
    [SerializeField] private bool verboseLogging = false;

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
        // Cada ronda es un episodio nuevo. La célula nace con un color
        // aleatorio (asignado por el CellSpawner) y el CellSpawner pide
        // UNA única decisión (RequestAdaptation) justo al crearla.
        //
        // A propósito NO volvemos a pedir decisiones durante la ronda:
        // el camuflaje no debe verse "en vivo" cambiando de color mientras
        // transcurre la ronda. Lo que sí mejora es la calidad de esa única
        // decisión a medida que la red se entrena a lo largo de muchas
        // rondas (episodios): con el tiempo, la política aprende a elegir
        // mejor color/tamaño de entrada dado el color de fondo, aunque el
        // punto de partida siga siendo aleatorio cada vez.
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

        if (verboseLogging)
        {
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
        }

        ApplyColorAction(
            redAction,
            greenAction,
            blueAction
        );

        ApplySizeAction(sizeAction);

        if (useRewardShaping)
        {
            ApplyCamouflageShaping();
        }
    }

    // RECOMPENSA CONTINUA POR PARECERSE AL FONDO
    private void ApplyCamouflageShaping()
    {
        Color cellColor = cell.CellColor;

        float colorDistance =
            Vector3.Distance(
                new Vector3(cellColor.r, cellColor.g, cellColor.b),
                new Vector3(backgroundColor.r, backgroundColor.g, backgroundColor.b)
            );

        // Distancia máxima posible entre dos colores RGB es sqrt(3).
        float normalizedDistance = Mathf.Clamp01(colorDistance / 1.732f);

        // Cuanto más lejos del color de fondo, mayor penalización (pequeña).
        AddReward(-normalizedDistance * shapingWeight);
    }
    // CAMBIAR COLOR

    private void ApplyColorAction(
        float redAction,
        float greenAction,
        float blueAction)
    {
        Color oldColor =
            cell.CellColor;

        Color newColor = new Color(oldColor.r + redAction * colorChangeSpeed, oldColor.g + greenAction * colorChangeSpeed, oldColor.b + blueAction * colorChangeSpeed, 1f);

        cell.SetColor(newColor);

        if (verboseLogging)
        {
            Debug.Log(gameObject.name + " | COLOR -> Antes: " + oldColor + " | Después: " + cell.CellColor);
        }
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

        if (verboseLogging)
        {
            Debug.Log(gameObject.name + " | SIZE -> Antes: " + oldSize.ToString("F2") + " | Después: " + cell.Size.ToString("F2"));
        }
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
            TrainingMetrics.Instance.RegisterSurvivor(cell.Size, cell.CellColor, backgroundColor);
        }

        Debug.Log(
            gameObject.name +
            " | Sobrevivió | Reward: +1"
        );

        EndEpisode();
    }
}