# Cell Evolution ML

Proyecto desarrollado en Unity para experimentar con Machine Learning y Reinforcement Learning mediante Unity ML-Agents.

## Descripción

El proyecto consiste en un videojuego en el cual diferentes células intentan sobrevivir frente a un jugador humano.

Durante cada ronda aparecen 10 células con diferentes tamaños, colores y posiciones.

El jugador dispone de aproximadamente 10 segundos para localizar y eliminar mediante clic la mayor cantidad posible de células.

Las células utilizan Unity ML-Agents para modificar características relacionadas con su apariencia y aprender mediante recompensas.

Actualmente se utilizan:

- Color RGB.
- Tamaño.

El sistema utiliza Reinforcement Learning mediante PPO (Proximal Policy Optimization).

## Estado del proyecto

El proyecto se encuentra EN DESARROLLO.

Actualmente se encuentran implementados:

- Generación de células.
- Posiciones aleatorias.
- Colores iniciales aleatorios.
- Tamaños iniciales aleatorios.
- Límites mínimos y máximos de tamaño.
- Sistema de rondas.
- Temporizador.
- Detección de clics.
- Eliminación de células.
- Sistema de puntuación.
- Integración con Unity ML-Agents.
- Agentes para las células.
- Observaciones del agente.
- Acciones continuas.
- Sistema de recompensas.
- Configuración inicial de PPO.
- Comunicación Unity-Python.
- Primeras pruebas de entrenamiento.
- Sistema inicial de métricas.

## Funcionamiento del Machine Learning

Cada célula funciona como un agente.

El agente recibe siete observaciones:

1. R de la célula.
2. G de la célula.
3. B de la célula.
4. R del fondo.
5. G del fondo.
6. B del fondo.
7. Tamaño normalizado.

Dispone de cuatro acciones continuas:

1. Modificar R.
2. Modificar G.
3. Modificar B.
4. Modificar tamaño.

Sistema de recompensas actual:

- Célula eliminada por el jugador: -1.
- Célula que sobrevive hasta terminar la ronda: +1.

Las diferentes células utilizan el mismo comportamiento `CellBehavior`, por lo que contribuyen al entrenamiento de una política compartida.

## Tecnologías

- Unity 6000.5.5f1
- C#
- Unity ML-Agents
- Python 3.10.12
- ML-Agents 1.1.0
- PyTorch 2.2.x
- PPO
- Conda / Miniconda

## Requisitos

Para abrir y modificar el proyecto se recomienda utilizar la misma versión de Unity utilizada durante el desarrollo:

Unity 6000.5.5f1

También es necesario disponer de Unity ML-Agents.

Las dependencias de Unity se encuentran registradas mediante los archivos de la carpeta `Packages`.

Para realizar entrenamiento es necesario configurar adicionalmente el entorno Python.

## Configuración del entorno Python

Se recomienda crear un entorno independiente mediante Conda.

Crear el entorno:

```bash
conda create -n mlagents python=3.10.12
```

Activarlo:

```bash
conda activate mlagents
```

Instalar una versión compatible de PyTorch 2.2.x.

Posteriormente instalar:

```bash
python -m pip install mlagents==1.1.0
```

Durante el desarrollo fue necesario utilizar una versión de `setuptools` compatible con `pkg_resources`:

```bash
python -m pip install setuptools==69.5.1
```

Comprobar ML-Agents:

```bash
mlagents-learn --help
```

Si se muestra correctamente la ayuda del comando, el entorno está preparado.

## Abrir el proyecto

Clonar el repositorio:

```bash
git clone URL_DEL_REPOSITORIO
```

Entrar a la carpeta:

```bash
cd MachineLearning
```

Abrir esta carpeta desde Unity Hub.

Unity reconstruirá automáticamente archivos locales como `Library`, por lo que la primera apertura puede tardar.

## Entrenamiento

Activar el entorno:

```bash
conda activate mlagents
```

Desde la raíz del proyecto ejecutar:

```bash
mlagents-learn Assets/ML/Config/CellTraining.yaml --run-id=CellTest01
```

Esperar a que ML-Agents indique que está esperando la conexión de Unity.

Después ejecutar la escena mediante Play en Unity.

Durante el entrenamiento el jugador debe intentar eliminar normalmente la mayor cantidad posible de células.

Para detener el entrenamiento:

1. Detener Play en Unity.
2. Presionar `Ctrl + C` en la terminal.

Para continuar posteriormente un entrenamiento existente se puede utilizar `--resume`.

## Estructura principal

```text
Assets/
├── ML/
│   ├── Config/
│   │   └── CellTraining.yaml
│   └── Models/
│
├── Prefabs/
│   └── Cell.prefab
│
├── Scenes/
│   └── Game.unity
│
└── Scripts/
    ├── Game/
    ├── Cells/
    ├── Player/
    └── ML/
```

## Scripts principales

### Cell.cs

Administra las características físicas y visuales de cada célula:

- Color.
- Tamaño.
- Límites.
- Estado.
- Eliminación.

### CellAgent.cs

Contiene la lógica relacionada con ML-Agents:

- Observaciones.
- Acciones.
- Recompensas.
- Episodios.
- Solicitud de decisiones.

### CellSpawner.cs

Administra:

- Creación de células.
- Posiciones aleatorias.
- Variación inicial.
- Lista de células activas.
- Células supervivientes.

### CellClickHandler.cs

Detecta los clics del jugador mediante Raycast 2D.

### RoundManager.cs

Controla:

- Inicio de ronda.
- Temporizador.
- Fin de ronda.
- Inicio de la siguiente ronda.

### ScoreManager.cs

Administra la puntuación del jugador.

### TrainingMetrics.cs

Registra información relacionada con el comportamiento de las células durante las rondas.

## Behavior Parameters

El prefab de la célula utiliza:

```text
Behavior Name: CellBehavior
Vector Observation Space Size: 7
Stacked Vectors: 1
Continuous Actions: 4
Behavior Type: Default
```

No es necesario utilizar `Decision Requester` con la implementación actual, ya que las decisiones se solicitan mediante código.

## Configuración PPO

La configuración del entrenamiento se encuentra en:

```text
Assets/ML/Config/CellTraining.yaml
```

El nombre:

```text
CellBehavior
```

debe coincidir exactamente con el `Behavior Name` configurado en Unity.

## Pendiente de desarrollo

El proyecto todavía requiere:

- Completar y validar las métricas.
- Realizar entrenamientos más extensos.
- Analizar la evolución de las recompensas.
- Analizar porcentaje de supervivencia.
- Comparar tamaño promedio.
- Comparar distancia de color respecto al fondo.
- Evaluar el impacto de los estados iniciales aleatorios.
- Ajustar hiperparámetros si es necesario.
- Seleccionar el modelo entrenado definitivo.
- Incorporar el modelo final dentro de Unity.
- Ejecutar inferencia sin depender de Python.
- Optimizar el proceso de entrenamiento.
- Eliminar mensajes Debug innecesarios.
- Mejorar la interfaz final.
- Realizar pruebas finales.

## Importante

No subir al repositorio las carpetas generadas automáticamente por Unity, especialmente:

```text
Library/
Temp/
Logs/
Obj/
UserSettings/
```

Tampoco debe subirse un entorno Conda o entorno virtual de Python.

Las carpetas principales necesarias para compartir un proyecto Unity son:

```text
Assets/
Packages/
ProjectSettings/
```

Los resultados temporales de entrenamiento se mantienen fuera del repositorio mediante `.gitignore`.

Cuando exista un modelo entrenado definitivo, deberá almacenarse dentro de:

```text
Assets/ML/Models/
```

para poder utilizarlo posteriormente desde Unity.