using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid<TGridObject>
{
    #region Variables / Components

    private int _width = 0;
    private int _height = 0;
    private float _cellSize = 0;
    private  Vector3 _origin = Vector3.zero;
    private TGridObject[,] _gridArray;

    #endregion

    #region Properties

    public int width => _width;
    public int height => _height;
    public float cellSize => _cellSize;

    #endregion
    
    #region Constructor

    public Grid(int width, int height, float cellSize, Vector3 origin)
    {
        _width = width;
        _height = height;
        _cellSize = cellSize;
        _origin = origin;
        _gridArray = new TGridObject[width, height];
    }

    #endregion

    #region Methods

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, 0, y) * _cellSize + _origin;
    }

    public void GetXY(Vector3 worldPosition, out  int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - _origin).x / _cellSize);
        y = Mathf.FloorToInt((worldPosition - _origin).z / _cellSize);
    }

    public void SetValue(int x, int y, TGridObject value)
    {
        if (x >= 0 && y >= 0 && x < _width && y < _height)
            _gridArray[x, y] = value;
    }
    
    public void SetValue(Vector3 worldPosition, TGridObject value)
    {
        GetXY(worldPosition, out var x, out var y);
        SetValue(x, y, value);
    }
    
    public TGridObject GetValue(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < _width && y < _height)
             return _gridArray[x, y];
        
        return default;
    }
    
    public TGridObject GetValue(Vector3 worldPosition)
    {
        GetXY(worldPosition, out var x, out var y);
        return GetValue(x, y);
    }

    public bool IsCornerTile(Vector3 tilePosition)
    {
        if (GetWorldPosition(0,0) == tilePosition) return true;
        if (GetWorldPosition(_width - 1,0) == tilePosition) return true;
        if (GetWorldPosition(0,_height - 1) == tilePosition) return true;
        if (GetWorldPosition(_width - 1,_height - 1) == tilePosition) return true;
        
        return false;
    }
    
    #endregion
}
