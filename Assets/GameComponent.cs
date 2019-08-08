using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameComponent : MonoBehaviour
{
  public RectTransform dotContainer;
  public GameObject dotPrefab;
  Game game;
  float dotFallSpeed = 500f;

  void Start()
  {
    this.NewGame();
  }

  void Update()
  {
    var board = this.game.board;
    for (int i = 0; i < board.Count; i++)
    {
      for (int j = 0; j < board[i].Count; j++)
      {
        var dot = board[i][j];
        this.SetDotStatus(dot);
        this.DropDotObject(dot);
      }
    }
  }

  void NewGame()
  {
    foreach (Transform dot in dotContainer)
    {
      GameObject.Destroy(dot.gameObject);
    }
    this.game = new Game(6, 6);
    this.game.onDotSpawn = this.SpawnDotObject;
    this.game.NewGame();
  }

  void SetDotStatus(Dot dot)
  {
    if (dot.transform == null) return;
    var outline = dot.transform.Find("Outline");
    if (this.game.IsDotSelected(dot)) outline.gameObject.SetActive(true);
    else outline.gameObject.SetActive(false);
  }

  void SpawnDotObject(Dot dot, int column)
  {
    var startingPosition = new Vector2(this.CalculateDotX(column), this.dotContainer.rect.height);
    dot.transform = Instantiate(
      this.dotPrefab,
      startingPosition,
      Quaternion.identity,
      this.dotContainer
    ).GetComponent<RectTransform>();
    dot.transform.Find("Image").GetComponent<Image>().color = dot.color;
    var onClick = new EventTrigger.Entry();
    onClick.eventID = EventTriggerType.PointerDown;
    onClick.callback.AddListener(data => this.SelectDot(data, dot, true));
    dot.transform.GetComponent<EventTrigger>().triggers.Add(onClick);
    var onDrag = new EventTrigger.Entry();
    onDrag.eventID = EventTriggerType.PointerEnter;
    onDrag.callback.AddListener(data => this.SelectDot(data, dot));
    dot.transform.GetComponent<EventTrigger>().triggers.Add(onDrag);
    var onRelease = new EventTrigger.Entry();
    onRelease.eventID = EventTriggerType.PointerUp;
    onRelease.callback.AddListener(data => this.game.OnSelectionFinish());
    dot.transform.GetComponent<EventTrigger>().triggers.Add(onRelease);
    dot.transform.anchoredPosition = startingPosition;
  }

  void DropDotObject(Dot dot)
  {
    var row = this.game.FindDotPosition(dot).y;
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

  void SelectDot(BaseEventData data, Dot dot, bool first = false)
  {
    var pointerData = (PointerEventData)data;
    if (!first && !pointerData.dragging) return;
    this.game.SelectDot(dot);
  }

  float CalculateDotX(int column)
  {
    float step = this.dotContainer.rect.width / this.game.width;
    return (column * step) + (step / 2);
  }

  float CalculateDotY(int row)
  {
    float step = this.dotContainer.rect.height / this.game.height;
    return (row * step) + (step / 2);
  }
}
