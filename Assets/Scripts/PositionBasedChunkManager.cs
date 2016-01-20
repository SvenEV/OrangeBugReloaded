using UnityEngine;
using OrangeBugReloaded.Core;
using System.Collections.Generic;

public class PositionBasedChunkManager : MonoBehaviour
{
    private Dictionary<Point, GameObject> _overlays = new Dictionary<Point, GameObject>();
    private ChunkLoader _chunkLoader;
    private Point _centerPoint;
    private Transform _overlayContainer;

    [Range(0, 20)]
    public int radius = 2;

    public Sprite chunkLoadingSprite;
    public Sprite chunkLoadedSprite;

    public ChunkLoader ChunkLoader
    {
        get { return _chunkLoader; }
        set
        {
            if (!Equals(_chunkLoader, value))
            {
                if (_chunkLoader != null)
                {
                    _chunkLoader.Chunks.ItemAdded -= OnChunkAdded;
                    _chunkLoader.Chunks.ItemRemoved -= OnChunkRemoved;
                    _chunkLoader.ChunksLoading.ItemAdded -= OnChunkLoadingStarted;
                    _chunkLoader.ChunksLoading.ItemRemoved -= OnChunkLoadingCompleted;
                }

                foreach (var p in _overlays.Keys)
                    DestroyOverlay(p);

                if (value != null)
                {
                    value.Chunks.ItemAdded += OnChunkAdded;
                    value.Chunks.ItemRemoved += OnChunkRemoved;
                    value.ChunksLoading.ItemAdded += OnChunkLoadingStarted;
                    value.ChunksLoading.ItemRemoved += OnChunkLoadingCompleted;
                }

                _chunkLoader = value;
            }
        }
    }

    private void OnChunkLoadingCompleted(KeyValuePair<Point, ChunkLoader.LoadingChunkToken> item)
    {
        Dispatcher.Run(() => DestroyOverlay(item.Key));
    }

    private void OnChunkLoadingStarted(KeyValuePair<Point, ChunkLoader.LoadingChunkToken> item)
    {
        Dispatcher.Run(() => CreateOverlay(item.Key, chunkLoadingSprite));
    }

    private void OnChunkRemoved(KeyValuePair<Point, Chunk> item)
    {
        Dispatcher.Run(() => DestroyOverlay(item.Key));
    }

    private void OnChunkAdded(KeyValuePair<Point, Chunk> item)
    {
        Dispatcher.Run(() => CreateOverlay(item.Key, chunkLoadedSprite));
    }

    private void Start()
    {
        _overlayContainer = new GameObject("Overlays").transform;
    }

    private void Update()
    {
        if (_chunkLoader == null)
            return;

        var center = new Point((int)transform.position.x, (int)transform.position.y) / Chunk.Size;

        for (var y = _centerPoint.Y - radius; y <= _centerPoint.Y + radius; y++)
            for (var x = _centerPoint.X - radius; x <= _centerPoint.X + radius; x++)
                if (x < center.X - radius || x > center.X + radius ||
                    y < center.Y - radius || y > center.Y + radius)
                    ChunkLoader.UnloadAsync(new Point(x, y), true); // Intended fire and forget

        for (var y = center.Y - radius; y <= center.Y + radius; y++)
            for (var x = center.X - radius; x <= center.X + radius; x++)
                ChunkLoader.TryGetAsync(new Point(x, y)); // Intended fire and forget

        _centerPoint = center;
    }

    private void DestroyOverlay(Point index)
    {
        GameObject overlay;

        if (_overlays.TryGetValue(index, out overlay))
        {
            Destroy(overlay);
            _overlays.Remove(index);
        }
    }

    private void CreateOverlay(Point index, Sprite sprite)
    {
        if (_overlays.ContainsKey(index))
            return;

        var go = new GameObject("Overlay");
        go.transform.parent = _overlayContainer;
        go.transform.position = new Vector3(
            index.X * Chunk.Size + Chunk.Size / 2f - .5f,
            index.Y * Chunk.Size + Chunk.Size / 2f - .5f);
        go.transform.localScale = new Vector3(Chunk.Size, Chunk.Size, 1);

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = -100;

        _overlays.Add(index, go);
    }
}
