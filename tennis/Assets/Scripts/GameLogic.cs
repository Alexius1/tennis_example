using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
  public static GameLogic Instance { get; private set; }
  //Score is saved as such: Bits 0:1 Points, 2:5 Games Set 1, 6:9 Set 2, 10:13 Set 3
  [Networked (OnChanged = nameof (UpdateScoreUI))][Capacity (2)]
  NetworkArray<int> Score { get; } = MakeInitializer (new int[] { 0, 0 });
  public static void UpdateScoreUI (Changed<GameLogic> changed) {
    changed.Behaviour.UpdateScoreUI ();
  }
  private void UpdateScoreUI () {
    _scoreUI[0].text = ScoreToString (Score[0]);
    _scoreUI[1].text = ScoreToString (Score[1]);
  }

  [SerializeField] private TextMeshProUGUI[] _scoreUI = new TextMeshProUGUI[2];

  [Networked (OnChanged = nameof (UpdateServeUI))]
  public bool ServeSide0 { get; set; } = true;
  public static void UpdateServeUI (Changed<GameLogic> changed) {
    changed.Behaviour.UpdateServeUI ();
  }
  public void UpdateServeUI () {
    _serveUI[0].enabled = ServeSide0;
    _serveUI[1].enabled = !ServeSide0;
  }

  [SerializeField] private Image[] _serveUI = new Image[2];

  private int _currentSet = 0;
  private const int SETMASK = 0xFFFC;
  private const int FIRSTBYTEMASK = 0x000F;
  private readonly int[] _tenniscounting = { 0, 15, 30, 40 };
  [SerializeField] private BoxCollider[] _areas; //assign in same order as Area struct

  private Area _expectedHit; //FieldSingle0 & FieldSingle1 stand for all 3 respective areas
  private bool _leftServe = true;
  private Player[] _players;

  public void StartGame () {
    print ("Game starting");
    _players = FindObjectsOfType<Player> ();
    if (_players.Length != 2)
      Debug.LogError ("Not Implemented.");
    ResetGamestate ();
  }

  public void Awake () {
    Instance = this;
  }

  //increases score, returns true if serving side switches
  private bool UpdateScore (int player) {
    if (Score[player] % 4 < 3) {
      Score.Set (player, Score[player] + 1);
      return false;
    }

    Score.Set (player, Score[player] + (1 << (2 + _currentSet * 4)));
    //reset current game
    Score.Set (0, Score[0] & SETMASK);
    Score.Set (1, Score[1] & SETMASK);

    if (Score[player] >> (2 + _currentSet * 4) != 7)
      return true;
    _currentSet++;

    //the player to score the last point in the set wins if they already won one
    if (_currentSet == 2 && ((Score[player] >> 2) & FIRSTBYTEMASK) == 7 ||
      _currentSet == 3) {
      print ("Player " + player + " won.");
      ResetGamestate ();
    }
    return true;
  }

  public void ScorePoint (int player) {
    print ("Point for Player " + player);
    if (UpdateScore (player))
      ServeSide0 = !ServeSide0;
    _leftServe = !_leftServe;
    _expectedHit = (Area) ((ServeSide0?2 : 3) + (_leftServe?2 : 0));
    print ("Player " + (ServeSide0?0 : 1) + " is serving from the " + (_leftServe? "left.": "right."));
    print (_expectedHit);
    _players[ServeSide0 ? 0 : 1].Serve (_leftServe);
  }

  private string ScoreToString (int score) {
    return ((score >> 2) & FIRSTBYTEMASK) + " " + ((score >> 6) & FIRSTBYTEMASK) + " " + ((score >> 10) & FIRSTBYTEMASK) + " | " + _tenniscounting[score & ~SETMASK];
  }

  public void ResetGamestate () {
    Score.Set (0, 0);
    Score.Set (1, 0);
    UpdateScoreUI ();
    _leftServe = true;
    _players[0].Reset ();
    _players[1].Reset ();
    _players[0].Serve (_leftServe);
    _expectedHit = Area.ServiceBoxOR;
    UpdateServeUI ();
    print ("Player 0 is serving from the left.");
  }

  //returns false when a player has commited an error
  public bool BallHit (Area hit) {
    if (!Object.HasStateAuthority)
      Debug.LogError ("Should not be called on client.");
    print (hit + " was hit.");
    //TODO maybe add logic for over the net serves
    if (hit == Area.Net || hit == Area.Out) {
      ScorePoint ((int) ~_expectedHit & 1);
      return false;
    } else if (_expectedHit == Area.FieldSingle0 || _expectedHit == Area.FieldSingle1) {
      if (((int) _expectedHit & 1) != ((int) hit & 1)) { //did the ball land on the wrong side of the net
        ScorePoint ((int) ~_expectedHit & 1);
        return false;
      }
    } else if (_expectedHit != hit) { //a serve was expected
      ScorePoint ((int) ~_expectedHit & 1);
      return false;
    }
    SwitchExpectedSide ();
    print (_expectedHit);
    return true;
  }

  //Method for collision with ground
  public bool BallHit (Vector3 position) {
    for (int i = 0; i < _areas.Length; i++) {
      if (_areas[i].bounds.Contains (position)) {
        return BallHit ((Area) i);
      }
    }
    return BallHit (Area.Out);
  }

  public void SwitchExpectedSide () {
    _expectedHit = (Area) ((int) ~_expectedHit & 1); //go to other side
  }

  public override void FixedUpdateNetwork () {
    if (GetInput (out NetworkInputData data)) {
      if (data.buttons.IsSet (ButtonInputs.R))
        ResetGamestate ();
      if (data.buttons.IsSet (ButtonInputs.Q)) {
        _players[0].Reset ();
        _players[1].Reset ();
        ScorePoint (Random.Range (0, 2));
      }
    }
  }
}