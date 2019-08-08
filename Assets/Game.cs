using System;
using System.Collections.Generic;
using UnityEngine;

public class Game
{
  public List<List<Dot>> board;
  public Action<Dot, int> onDotSpawn;
  public int width = 6;
  public int height = 6;
  int nextId = 0;
  List<Dot> selected = new List<Dot>();
  System.Random dotGenerator;
  Color[] colors = new Color[]{
    Color.red,
    Color.green,
    Color.yellow,
    Color.magenta,
  };

  public Game(int width, int height)
  {
    this.nextId = 0;
    this.width = width;
    this.height = height;
  }

  public void NewGame()
  {
    this.dotGenerator = new System.Random();
    this.board = new List<List<Dot>>();
    for (int i = 0; i < this.width; i++)
    {
      this.board.Add(new List<Dot>());
      for (int j = 0; j < this.height; j++)
      {
        this.SpawnNewDot(i);
      }
    }
  }

  public void SelectDot(Dot dot)
  {
    if (this.selected.Count > 1)
    {
      // unselect dots
      var secondToLastDot = this.selected[this.selected.Count - 2];
      if (dot.id == secondToLastDot.id)
      {
        this.selected.RemoveAt(this.selected.Count - 1);
        return;
      }
      // detect loop
      if (this.selected[0].id == this.selected[this.selected.Count - 1].id) return;
      // create loop
      if (this.selected[0].id == dot.id)
      {
        this.selected.Add(dot);
        return;
      }
    }
    // valid selection check
    if (this.selected.Count > 0)
    {
      var lastSelectedDot = this.selected[this.selected.Count - 1];
      var lastSelectedPosition = this.FindDotPosition(lastSelectedDot);
      var position = this.FindDotPosition(dot);
      if (
        Math.Abs(lastSelectedPosition.x - position.x) +
        Math.Abs(lastSelectedPosition.y - position.y) > 1
      ) return;
      if (lastSelectedDot.color != dot.color) return;
      if (this.selected.FindIndex(v => v.id == dot.id) > -1) return;
    }
    this.selected.Add(dot);
  }

  public void OnSelectionFinish()
  {
    if (this.selected.Count > 1)
    {
      // on loop
      if (this.selected[0].id == this.selected[this.selected.Count - 1].id)
      {
        this.selected = this.GetAllColor(this.selected[0].color);
      }
      this.selected.ForEach(dot => this.RemoveDot(dot, this.FindDotPosition(dot).x));
    }
    this.selected = new List<Dot>();
  }

  public bool IsDotSelected(Dot dot)
  {
    return this.selected.FindIndex(d => d.id == dot.id) > -1;
  }

  public Vector2Int FindDotPosition(Dot dot)
  {
    int row = -1;
    int column = this.board.FindIndex(c =>
    {
      row = c.FindIndex(d => d.id == dot.id);
      if (row == -1) return false;
      return true;
    });
    if (column == -1)
    {
      throw new Exception("dot does not exist");
    }
    return new Vector2Int(column, row);
  }

  List<Dot> GetAllColor(Color color)
  {
    var dots = new List<Dot>();
    this.board.ForEach(column => column.ForEach(dot =>
    {
      if (dot.color == color) dots.Add(dot);
    }));
    return dots;
  }

  public void RemoveDot(Dot dot, int column)
  {
    GameObject.Destroy(dot.transform.gameObject);
    this.board[column].Remove(dot);
    this.SpawnNewDot(column);
  }

  void SpawnNewDot(int column)
  {
    if (this.board[column].Count >= this.height)
    {
      throw new Exception("dot cannot be dropped, column is full");
    }
    var dot = new Dot(this.nextId++, this.GetRandomColor());
    this.board[column].Add(dot);
    if (this.onDotSpawn != null) this.onDotSpawn(dot, column);
  }

  Color GetRandomColor()
  {
    return this.colors[this.dotGenerator.Next(0, this.colors.Length)];
  }
}