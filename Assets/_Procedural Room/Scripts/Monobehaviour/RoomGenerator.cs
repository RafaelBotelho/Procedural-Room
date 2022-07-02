using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    #region Variables / Components

    [Header("Room Settings")]
    [SerializeField] private int _xSize = 1;
    [SerializeField] private int _zSize = 1;
    [SerializeField] private float _tileSize = 0;
    [SerializeField] private int _numberOfDoors = 0;
    [SerializeField] private int _numberOfWindows = 0;

    [Header("Decoration Settings")]
    [SerializeField] private int _safeArea = 0;
    [SerializeField] private bool _useMaxWallDecoration = false;
    [SerializeField] private int _numberOfWallDecorations = 0;
    [SerializeField] private bool _useMaxPropDecoration = false;
    [SerializeField] private int _numberOfPropDecorations = 0;
    [SerializeField] private float _wallDecorationOffSet = 0;
    [SerializeField] private float _propDecorationOffSet = 0;

    [Header("Seeds")]
    [Range(0,99999)] [SerializeField] private int _roomSeed = 0;
    [Range(0,99999)] [SerializeField] private int _DecorationSeed = 0;

    [Header("Grid Origin")]
    [SerializeField] private Transform _originTransform = default;
    [SerializeField] private Vector3 _originPosition = Vector3.zero;
    
    [Header("Tile References")]
    [SerializeField] private List<GameObject> _floors = new List<GameObject>();
    [SerializeField] private List<GameObject> _walls = new List<GameObject>();
    [SerializeField] private List<GameObject> _doors = new List<GameObject>();
    [SerializeField] private List<GameObject> _windows = new List<GameObject>();
    
    [Header("Decoration References")]
    [SerializeField] private List<SO_Decorations> _wallDecorations = new List<SO_Decorations>();
    [SerializeField] private List<SO_Decorations> _propDecorations = new List<SO_Decorations>();
    
    private Grid<Transform> _roomGrid;

    private List<GameObject> _spawnedFloors = new List<GameObject>();
    private List<GameObject> _spawnedWalls = new List<GameObject>();
    private List<GameObject> _spawnedDoors = new List<GameObject>();
    private List<GameObject> _spawnedWindows = new List<GameObject>();

    private List<SpawnedDecoration> _spawnedWallDecorations = new List<SpawnedDecoration>();
    private List<SpawnedDecoration> _spawnedPropDecorations = new List<SpawnedDecoration>();

    #endregion

    #region Monobehaviour

#if UNITY_EDITOR
    void OnValidate() { UnityEditor.EditorApplication.delayCall += _OnValidate; }
    void _OnValidate() 
    {
        if (!this) return;
        if(!Application.isPlaying) return;
        
        ClearRoom();
        ClearDecorations();
        
        GenerateRoom();
        GenerateDecorations();
    }
#endif

    #endregion

    #region Methods

    #region Room

    private void GenerateRoom()
    {
        Random.InitState(_roomSeed);
        _roomGrid = new Grid<Transform>(_xSize, _zSize, _tileSize,
            _originTransform ? _originTransform.position : _originPosition);
        
        GenerateFloors();
        GenerateWalls();
        GenerateDoors();
        GenerateWindows();
    }

    private void GenerateFloors()
    {
        if(_floors.Count <= 0)return;
        
        var originPosition = _originTransform ? _originTransform.position : _originPosition;
        for (int x = 0; x < _xSize; x++)
        {
            for (int y = 0; y < _zSize; y++)
            {
                var floorPrefab = _floors[Random.Range(0, _floors.Count)];
                var spawnPosition = originPosition + new Vector3(x * _tileSize, 0, y * _tileSize);
                var spawnedFloor = Instantiate(floorPrefab, spawnPosition, Quaternion.identity);
                
                _spawnedFloors.Add(spawnedFloor);
                _roomGrid.SetValue(x, y, spawnedFloor.transform);
            }
        }
    }

    private void GenerateWalls()
    {
        if(_walls.Count <= 0)return;
        
        for (int x = 0; x < _xSize; x++)
        {
            var downTileTransform = _roomGrid.GetValue(x, 0);
            var upTileTransform = _roomGrid.GetValue(x, _zSize - 1);
            
            SpawnWall(downTileTransform.position, -downTileTransform.forward);
            SpawnWall(upTileTransform.position, upTileTransform.forward);
        }
        
        for (int y = 0; y < _zSize; y++)
        {
            var leftTileTransform = _roomGrid.GetValue(0, y);
            var rightTileTransform = _roomGrid.GetValue(_xSize - 1, y);
            
            SpawnWall(leftTileTransform.position, -leftTileTransform.right);
            SpawnWall(rightTileTransform.position, rightTileTransform.right);
        }
    }

    private void GenerateDoors()
    {
        if(_doors.Count <= 0)return;
        if(_spawnedWalls.Count <= 0) return;
        
        for (int i = 0; i < _numberOfDoors; i++)
        {
            var randomWall = _spawnedWalls[Random.Range(0, _spawnedWalls.Count)];
            var safeCheck = 30;

            while (_roomGrid.IsCornerTile(randomWall.transform.position +
                                          randomWall.transform.forward * (_tileSize * 0.5f)))
            {
                randomWall = _spawnedWalls[Random.Range(0, _spawnedWalls.Count)];
                safeCheck--;
                
                if(safeCheck <= 0)
                    return;
            }

            var doorPrefab = _doors[Random.Range(0, _doors.Count)];
            
            var spawnedDoor =
                Instantiate(doorPrefab, randomWall.transform.position, randomWall.transform.rotation);
            
            _spawnedWalls.Remove(randomWall);
            _spawnedDoors.Add(spawnedDoor);
            Destroy(randomWall);
        }
    }

    private void GenerateWindows()
    {
        if(_windows.Count <= 0) return;
        if(_spawnedWalls.Count <= 0) return;
        
        for (int i = 0; i < _numberOfWindows; i++)
        {
            var randomWall = _spawnedWalls[Random.Range(0, _spawnedWalls.Count)];
            var windowPrefab = _windows[Random.Range(0, _windows.Count)];
            
            var spawnedWindow =
                Instantiate(windowPrefab, randomWall.transform.position, randomWall.transform.rotation);
            
            _spawnedWalls.Remove(randomWall);
            _spawnedWindows.Add(spawnedWindow);
            Destroy(randomWall);
        }
    }

    private void ClearRoom()
    {
        foreach (var floor in _spawnedFloors)
            Destroy(floor);
        foreach (var wall in _spawnedWalls)
            Destroy(wall);
        foreach (var door in _spawnedDoors)
            Destroy(door);
        foreach (var window in _spawnedWindows)
            Destroy(window);
        
        _spawnedFloors.Clear();
        _spawnedWalls.Clear();
        _spawnedDoors.Clear();
        _spawnedWindows.Clear();
    }
    
    private void SpawnWall(Vector3 tilePosition,Vector3 direction)
    {
        var wallPrefab = _walls[Random.Range(0, _walls.Count)];
        var wallSpawned = Instantiate(wallPrefab, tilePosition + direction * (_tileSize * 0.5f), Quaternion.identity);

        wallSpawned.transform.LookAt(tilePosition);
        _spawnedWalls.Add(wallSpawned);
    }

    #endregion

    #region Decoration

    private void GenerateDecorations()
    {
        Random.InitState(_DecorationSeed);
        
        GeneratePropDecoration(out var propSpawned);
        GenerateWallDecoration(out var wallSpawned);
        
        while (propSpawned || wallSpawned)
        {
            GeneratePropDecoration(out propSpawned);
            GenerateWallDecoration(out wallSpawned);
        }
    }

    private void GenerateWallDecoration(out bool spawned)
    {
        spawned = false;
        if(_wallDecorations.Count <= 0) return;
        
        var safeCheck = 30;

        if (_spawnedWallDecorations.Count >= _numberOfWallDecorations && !_useMaxWallDecoration) return;
        
        while (safeCheck > 0)
        {
            var decoration = _wallDecorations[Random.Range(0, _wallDecorations.Count)];
            var wall = _spawnedWalls[Random.Range(0, _spawnedWalls.Count)].transform;
            var decorationPrefab = Instantiate(decoration.prefab, wall);
            
            decorationPrefab.transform.localPosition += decoration.positionOffSet;

            var validPosition = true;

            foreach (var spawnedPropDecoration in _spawnedPropDecorations)
            {
                if (Vector3.Distance(decorationPrefab.transform.position, spawnedPropDecoration.spawnedObject.transform.position) < spawnedPropDecoration.size ||
                    Vector3.Distance(decorationPrefab.transform.position, spawnedPropDecoration.spawnedObject.transform.position) < decoration.size)
                    validPosition = false;
            }
            
            foreach (var spawnedWallDecoration in _spawnedWallDecorations)
            {
                if (Vector3.Distance(decorationPrefab.transform.position, spawnedWallDecoration.spawnedObject.transform.position) < spawnedWallDecoration.size ||
                    Vector3.Distance(decorationPrefab.transform.position, spawnedWallDecoration.spawnedObject.transform.position) < decoration.size)
                    validPosition = false;
            }

            if (validPosition)
            {
                decorationPrefab.transform.localEulerAngles += decoration.rotationOffSet;
                _spawnedWallDecorations.Add(new SpawnedDecoration(decorationPrefab, decoration.size));
                spawned = true;
                break;
            }

            Destroy(decorationPrefab);
            safeCheck--;
        }
    }

    private void GeneratePropDecoration(out bool spawned)
    {
        spawned = false;
        
        if(_propDecorations.Count <= 0) return;
        
        var safeCheck = 30;
        var minPosition = _roomGrid.GetValue(_safeArea, _safeArea).position;
        var maxPosition = _roomGrid.GetValue((_xSize - 1) - _safeArea, (_zSize - 1) - _safeArea).position;

        if (_spawnedPropDecorations.Count >= _numberOfPropDecorations && !_useMaxPropDecoration) return;

        while (safeCheck > 0)
        {
            var decoration = _propDecorations[Random.Range(0, _propDecorations.Count)];

            var position =
                new Vector3(Random.Range(minPosition.x, maxPosition.x), 0 + _propDecorationOffSet,
                    Random.Range(minPosition.z, maxPosition.z)) +
                decoration.positionOffSet;
            
            position += decoration.positionOffSet;

            var validPosition = true;

            foreach (var spawnedPropDecoration in _spawnedPropDecorations)
            {
                if (Vector3.Distance(position, spawnedPropDecoration.spawnedObject.transform.position) < spawnedPropDecoration.size ||
                    Vector3.Distance(position, spawnedPropDecoration.spawnedObject.transform.position) < decoration.size)
                    validPosition = false;
            }

            foreach (var spawnedWallDecoration in _spawnedWallDecorations)
            {
                if (Vector3.Distance(position, spawnedWallDecoration.spawnedObject.transform.position) < spawnedWallDecoration.size ||
                    Vector3.Distance(position, spawnedWallDecoration.spawnedObject.transform.position) < decoration.size)
                    validPosition = false;
            }
            
            if (validPosition)
            {
                var decorationPrefab = Instantiate(decoration.prefab, position,
                    Quaternion.identity);

                if (decoration.allowRandomRotation)
                    decorationPrefab.transform.Rotate(new Vector3(0, 1, 0), Random.Range(0, 360));

                _spawnedPropDecorations.Add(new SpawnedDecoration(decorationPrefab, decoration.size));

                spawned = true;
                break;
            }

            safeCheck--;
        }
    }

    private void ClearDecorations()
    {
        foreach (var wallDecoration in _spawnedWallDecorations)
            Destroy(wallDecoration.spawnedObject);
        foreach (var propDecoration in _spawnedPropDecorations)
            Destroy(propDecoration.spawnedObject);
        
        _spawnedWallDecorations.Clear();
        _spawnedPropDecorations.Clear();
    }
    
    #endregion

    #endregion
}

public class SpawnedDecoration
{
    #region Variables

    private GameObject _spawnedObject = default;
    private float _size = 0;

    #endregion

    #region Properties

    public GameObject spawnedObject => _spawnedObject;
    public float size => _size;

    #endregion

    #region Constructor

    public SpawnedDecoration(GameObject decorationObject, float decorationSize)
    {
        _spawnedObject = decorationObject;
        _size = decorationSize;
    }

    #endregion
}