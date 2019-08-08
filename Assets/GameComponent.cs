using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameComponent : MonoBehaviour
{
  public RectTransform dotContainer;
  public GameObject dotPrefab;
  public GameObject connectorPrefab;
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
    this.game.OnDotSpawn = this.SpawnDotObject;
    this.game.OnDotDespawn = this.DespawnDot;
    this.game.OnConnectDot = this.SpawnConnector;
    this.game.OnDisconnectDot = this.RemoveConnector;
    this.game.NewGame();
  }

  void SetDotStatus(Dot dot)
  {
    if (dot.transform == null) return;
    var outline = dot.transform.Find("Outline");
    if (this.game.IsDotSelected(dot)) outline.gameObject.SetActive(true);
    else outline.gameObject.SetActive(false);
  }

  void SpawnDotObject(Dot dot)
  {
    var startingPosition = new Vector2(
      this.GetDotPosition(dot).x,
      this.dotContainer.rect.height
    );
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

  void DespawnDot(Dot dot)
  {
    if (dot.connector != null) GameObject.Destroy(dot.connector.gameObject);
    if (dot.transform != null) GameObject.Destroy(dot.transform.gameObject);
  }

  void SpawnConnector(Dot from, Dot to)
  {
    from.connector = Instantiate(
      this.connectorPrefab,
      this.dotContainer
    ).GetComponent<RectTransform>();
    var fromCoord = this.game.FindDotCoordinates(from);
    var toCoord = this.game.FindDotCoordinates(to);
    var fromPos = this.GetDotPosition(from);
    var toPos = this.GetDotPosition(to);
    var center = (this.GetDotPosition(from) + this.GetDotPosition(to)) / 2f;
    var distance = Vector2.Distance(fromPos, toPos);
    var vertical = fromCoord.x == toCoord.x;
    from.connector.anchoredPosition = center;
    from.connector.sizeDelta = new Vector2(
      vertical ? 20f : distance,
      vertical ? distance : 20f
    );
    from.connector.SetSiblingIndex(0);
  }

  void RemoveConnector(Dot dot)
  {
    if (dot.connector == null) return;
    GameObject.Destroy(dot.connector.gameObject);
  }

  void DropDotObject(Dot dot)
  {
    var row = this.game.FindDotCoordinates(dot).y;
    var currentPosition = dot.transform.anchoredPosition;
    var targetPosition = new Vector2(
      currentPosition.x,
      this.GetDotPosition(dot).y
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

  Vector2 GetDotPosition(Dot dot)
  {
    var coord = this.game.FindDotCoordinates(dot);
    var xStep = this.dotContainer.rect.width / this.game.width;
    var yStep = this.dotContainer.rect.height / this.game.height;
    return new Vector2(
      (coord.x * xStep) + (xStep / 2),
      (coord.y * yStep) + (yStep / 2)
    );
  }
}
