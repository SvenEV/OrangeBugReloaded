using UnityEngine;
using OrangeBugReloaded.Core;
using OrangeBugReloaded.Core.LocalSinglePlayer;

public class GameController : MonoBehaviour
{
    public CameraFollow CameraFollow;
    public MapViewModel MapVM;
    public MapEditor Editor;

    private PositionBasedChunkManager _chunkManager;
    private bool _isInEditMode = false;
    private Map _map;

    public static GameController Instance { get; private set; }

    public bool IsInEditMode
    {
        get { return _isInEditMode; }
        set
        {
            if (value)
            {
                // Switch to edit mode
                Editor.gameObject.SetActive(true);
                Editor.OpenDesigner(_map);
            }
            else
            {
                // Switch to play mode
                Editor.gameObject.SetActive(false);
            }

            _isInEditMode = value;
        }
    }

    private void Start()
    {
        //Logger.Logged += s => Debug.Log(s);
        Logger.Info += s => Debug.Log(s);
        Logger.Warning += s => Debug.LogWarning(s);
        Logger.Error += s => Debug.LogError(s);

        if (Instance != null)
            Debug.LogError("There are multiple GameControllers in this scene");

        Instance = this;

        var storage = new BasicJsonStorage(@"C:\users\svenv\desktop\OrangeBugWorld.json");

        Map.CreateAsync(storage).ContinueWith(t =>
        {
            _map = t.Result;

            Dispatcher.Run(() =>
            {
                _chunkManager = CameraFollow.GetComponent<PositionBasedChunkManager>();
                _chunkManager.ChunkLoader = _map.ChunkLoader;

                MapVM.LocationsProvider = _map;

                var startPoint = new Point(
                    PlayerPrefs.GetInt("PlayerX", 0),
                    PlayerPrefs.GetInt("PlayerY", 0));

                Camera.main.transform.position = new Vector3(startPoint.X, startPoint.Y, -10);

                IsInEditMode = false;
            });
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            IsInEditMode = true;
        }

        //if (IsInEditMode && Input.GetKeyDown(KeyCode.F) && _map.Player != null)
        //    _editor.CursorPosition = _map.Player.GetComponent<EntityViewModel>().Object.Owner.Position;
    }

    public void ToggleDesigner()
    {
        if (IsInEditMode)
            Editor.BeginCloseDesigner();
        else
            IsInEditMode = true;
    }
}
