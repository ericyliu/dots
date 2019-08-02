using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Dot
{
  public Color color;
  public RectTransform transform;

  public Dot(Color color)
  {
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
        if (dot.transform == null) this.SpawnDotObject(dot, i, j);
        else this.DropDotObject(dot, j);
      }
    }
  }

  void NewGame()
  {
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
    this.board[column].Add(new Dot(this.GetRandomColor()));
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
    dot.transform.GetComponent<Image>().color = dot.color;
    var entry = new EventTrigger.Entry();
    entry.eventID = EventTriggerType.PointerClick;
    entry.callback.AddListener(data => this.RemoveDot(dot, column));
    dot.transform.GetComponent<EventTrigger>().triggers.Add(entry);
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
}
