using System;
using System.Collections.Generic;
using UnityEngine;

public class GameTest
{
  public void Run()
  {
    this.TestSelectNormal();
    this.TestSelectSquare();
    Debug.Log("Tests Passed");
  }

  void TestSelectNormal()
  {
    var game = new Game(6, 6);
    game.NewGame();
    var dotsToSelect = new List<Dot>(){
      game.board[0][0],
      game.board[0][1],
      game.board[1][1],
      game.board[1][0]
    };
    dotsToSelect.ForEach(dot => dot.color = Color.red);
    dotsToSelect.ForEach(dot => game.SelectDot(dot));
    game.OnSelectionFinish();
    dotsToSelect.ForEach(dot => this.Assert(!this.IsDotOnBoard(dot, game)));
  }

  void TestSelectSquare()
  {
    var game = new Game(6, 6);
    game.NewGame();
    var dotsToSelect = new List<Dot>(){
      game.board[0][0],
      game.board[0][1],
      game.board[1][1],
      game.board[1][0],
      game.board[0][0]
    };
    dotsToSelect.ForEach(dot => dot.color = Color.red);
    var redDots = new List<Dot>();
    game.board.ForEach(col =>
      col.ForEach(dot =>
      {
        if (dot.color == Color.red)
        {
          redDots.Add(dot);
        }
      })
    );
    dotsToSelect.ForEach(dot => game.SelectDot(dot));
    game.OnSelectionFinish();
    redDots.ForEach(dot => this.Assert(!this.IsDotOnBoard(dot, game)));
  }

  bool IsDotOnBoard(Dot dot, Game game)
  {
    return game.board.Find(col =>
      col.Find(dotOnBoard => dotOnBoard.id == dot.id) != null
    ) != null;
  }

  void Assert(bool condition)
  {
    if (condition) return;
    throw new Exception("Assertion failed.");
  }
}
