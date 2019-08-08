
using UnityEngine;

public class Dot
{
  public int id;
  public Color color;
  public RectTransform transform;
  public RectTransform connector;

  public Dot(int id, Color color)
  {
    this.id = id;
    this.color = color;
  }
}