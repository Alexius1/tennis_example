using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;

public enum Area {
  FieldSingle0 = 0,
  FieldSingle1 = 1,
  ServiceBox0L = 2,
  ServiceBox1L = 3,
  ServiceBoxOR = 4,
  ServiceBox1R = 5,
  Net = 6,
  Out = 7
}

public class GameLogic : NetworkBehaviour {
  //Score is saved as such: Bits 0:1 Points, 2:5 Games Set 1, 6:9 Set 2, 10:13 Set 3
  [Networked]
  [Capacity (2)] // Sets the fixed capacity of the collection
  NetworkArray<int> Score { get; } = MakeInitializer (new int[] { 0, 0 });
  private int currentSet = 0;
  private const int SETMASK = 0xFFFC;
  private const int FIRSTBYTEMASK = 0x000F;
  private readonly int[] tenniscounting = { 0, 15, 30, 40 };
  [SerializeField]
  private TextMeshProUGUI[] scoreUI = new TextMeshProUGUI[2];

  [SerializeField]
  private BoxCollider[] areas; //assign in same order as Area struct

  private Area expectedHit; //FieldSingle0 & FieldSingle1 stand for all 3 respective areas

  private void UpdateScore (int player) {
    if (Score[player] % 4 < 3) {
      Score.Set (player, Score[player] + 1);
      return;
    }

    Score.Set (player, Score[player] + (1 << (2 + currentSet * 4)));
    //reset current game
    Score.Set (0, Score[0] & SETMASK);
    Score.Set (1, Score[1] & SETMASK);

    if (Score[player] >> (2 + currentSet * 4) != 7)
      return;
    currentSet++;

    //the player to score the last point in the set wins if they already won one
    if (currentSet == 2 && ((Score[player] >> 2) & FIRSTBYTEMASK) == 7 ||
      currentSet == 3) {
      print ("Player " + player + " won.");
      ResetGamestate ();
    }
  }

  private void IncreaseScore (int player) {
    UpdateScore (player);
    UpdateScoreUI (player);
  }

  private void UpdateScoreUI (int player) {
    scoreUI[player].text = ScoreToString (Score[player]);
  }

  private string ScoreToString (int score) {
    return ((score >> 2) & FIRSTBYTEMASK) + " " + ((score >> 6) & FIRSTBYTEMASK) + " " + ((score >> 10) & FIRSTBYTEMASK) + " " + tenniscounting[score & ~SETMASK];
  }

  public void ResetGamestate () {
    Score.Set (0, 0);
    Score.Set (1, 0);
    UpdateScoreUI (0);
    UpdateScoreUI (1);
  }

  public bool BallHit (Area hit) {
    //TODO maybe add logic for over the net serves
    if (hit == Area.Net || hit == Area.Out) {
      IncreaseScore ((int) expectedHit & 1);
      return false;
    } else if (expectedHit == Area.FieldSingle0 || expectedHit == Area.FieldSingle1) {
      if (((int) expectedHit & 1) != ((int) hit & 1)) { //did the ball land on the wrong side of the net
        IncreaseScore ((int) expectedHit & 1);
        return false;
      }
    } else if (expectedHit != hit) { //a serve was expected
      IncreaseScore ((int) expectedHit & 1);
      return false;
    }
    expectedHit = (Area) ((int) ~expectedHit & 1); //go to other side
    return true;
  }

  //Method for collision with ground
  public bool BallHit (Vector3 position) {
    for (int i = 0; i < areas.Length; i++) {
      if (areas[i].bounds.Contains (transform.position))
        return BallHit ((Area) i);
    }
    Debug.LogError ("Error, should not be reached.");
    return false;
  }
}