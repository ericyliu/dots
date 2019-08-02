using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Dot
{
  public int id;
  public Color color;
  public RectTransform transform;

  public Dot(int id, Color color)
  {
    this.id = id;
    this.color = color;
  }
}

public class Game : MonoBehaviour
{
  public RectTransform dotContainer;
  public GameObject dotPrefab;
  public int width = 6;
  public int height = 6;
  public float dotFallSpeed = 5f;
  List<List<Dot>> board;
  System.Random dotGenerator;
  Color[] colors = new Color[]{
    Color.red,
    Color.green,
    Color.yellow,
    Color.magenta,
  };
  List<Dot> selected = new List<Dot>();
  int nextId = 0;

  public void OnPointerUp()
  {
    if (this.selected.Count > 1)
    {
      // on loop
      if (this.selected[0].id == this.selected[this.selected.Count - 1].id)
      {
        Color color = this.selected[0].color;
        this.selected = new List<Dot>();
        this.board.ForEach(column => column.ForEach(dot =>
        {
          if (dot.color == color) this.selected.Add(dot);
        }));
      }
      this.selected.ForEach(dot => this.RemoveDot(dot, this.FindDotPosition(dot).x));
    }
    this.selected = new List<Dot>();
  }

  void Start()
  {
    this.NewGame();
  }

  void Update()
  {
    for (int i = 0; i < this.board.Count; i++)
    {
      for (int j = 0; j < this.board[i].Count; j++)
      {
        var dot = this.board[i][j];
        this.SetDotSelected(dot);
        if (dot.transform == null) this.SpawnDotObject(dot, i, j);
        else this.DropDotObject(dot, j);
      }
    }
  }

  void SetDotSelected(Dot dot)
  {
    if (dot.transform == null) return;
    var outline = dot.transform.Find("Outline");
    if (this.selected.FindIndex(d => d.id == dot.id) > -1) outline.gameObject.SetActive(true);
    else outline.gameObject.SetActive(false);
  }

  void NewGame()
  {
    this.nextId = 0;
    this.dotGenerator = new System.Random();
    this.board = new List<List<Dot>>();
    foreach (Transform dot in dotContainer)
    {
      GameObject.Destroy(dot.gameObject);
    }
    for (int i = 0; i < this.width; i++)
    {
      this.board.Add(new List<Dot>());
      for (int j = 0; j < this.height; j++)
      {
        this.SpawnNewDot(i);
      }
    }
  }

  void SpawnNewDot(int column)
  {
    if (this.board[column].Count >= this.height)
    {
      throw new Exception("dot cannot be dropped, column is full");
    }
    this.board[column].Add(new Dot(this.nextId++, this.GetRandomColor()));
  }

  void SpawnDotObject(Dot dot, int column, int row)
  {
    var startingPosition = new Vector2(this.CalculateDotX(column), this.dotContainer.rect.height);
    dot.transform = Instantiate(
      this.dotPrefab,
      startingPosition,
      Quaternion.identity,
      this.dotContainer
    ).GetComponent<RectTransform>();
    dot.transform.Find("Image").GetComponent<Image>().color = dot.color;
    var entry = new EventTrigger.Entry();
    entry.eventID = EventTriggerType.PointerEnter;
    entry.callback.AddListener(data => this.SelectDot(data, dot, true));
    dot.transform.GetComponent<EventTrigger>().triggers.Add(entry);
    var entry2 = new EventTrigger.Entry();
    entry2.eventID = EventTriggerType.PointerDown;
    entry2.callback.AddListener(data => this.SelectDot(data, dot));
    dot.transform.GetComponent<EventTrigger>().triggers.Add(entry2);
    var entry3 = new EventTrigger.Entry();
    entry3.eventID = EventTriggerType.PointerUp;
    entry3.callback.AddListener(data => this.OnPointerUp());
    dot.transform.GetComponent<EventTrigger>().triggers.Add(entry3);
    dot.transform.anchoredPosition = startingPosition;
  }

  void DropDotObject(Dot dot, int row)
  {
    var currentPosition = dot.transform.anchoredPosition;
    var targetPosition = new Vector2(
      currentPosition.x,
      this.CalculateDotY(row)
    );
    if (currentPosition.y <= targetPosition.y)
    {
      dot.transform.anchoredPosition = targetPosition;
      return;
    }
    dot.transform.anchoredPosition = new Vector3(
      currentPosition.x,
      currentPosition.y - (this.dotFallSpeed * Time.deltaTime)
    );
  }

  void SelectDot(BaseEventData data, Dot dot, bool drag = false)
  {
    var position = this.FindDotPosition(dot);
    var pointerData = (PointerEventData)data;
    if (drag && !pointerData.dragging) return;
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
      if (
        Math.Abs(lastSelectedPosition.x - position.x) +
        Math.Abs(lastSelectedPosition.y - position.y) > 1
      ) return;
      if (lastSelectedDot.color != dot.color) return;
      if (this.selected.FindIndex(v => v.id == dot.id) > -1) return;
    }
    this.selected.Add(dot);
  }

  void RemoveDot(Dot dot, int column)
  {
    GameObject.Destroy(dot.transform.gameObject);
    this.board[column].Remove(dot);
    this.SpawnNewDot(column);
  }

  float CalculateDotX(int column)
  {
    float step = this.dotContainer.rect.width / this.width;
    return (column * step) + (step / 2);
  }

  float CalculateDotY(int row)
  {
    float step = this.dotContainer.rect.height / this.height;
    return (row * step) + (step / 2);
  }

  Color GetRandomColor()
  {
    return this.colors[this.dotGenerator.Next(0, this.colors.Length)];
  }

  Vector2Int FindDotPosition(Dot dot)
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
}
