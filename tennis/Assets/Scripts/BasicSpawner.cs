using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Collections;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks {
  private NetworkRunner _runner;
  private NetworkButtons _buttons;
  [SerializeField] private NetworkPrefabRef _playerPrefab;
  private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject> ();

  [SerializeField] private Vector3[] _playerStartingPositions;

  private void OnGUI () {
    if (_runner == null) {
      if (GUI.Button (new Rect (0, Screen.height - 80, 200, 40), "Host")) {
        OpenLobby (GameMode.Host);
      }
      if (GUI.Button (new Rect (0, Screen.height - 40, 200, 40), "Join")) {
        OpenLobby (GameMode.Client);
      }

    }
  }

  public void OnPlayerJoined (NetworkRunner runner, PlayerRef player) {
    if (runner.IsServer) {
      if (_playerStartingPositions.Length <= _spawnedCharacters.Count)
        Debug.LogError ("No starting position for player " + (_spawnedCharacters.Count + 1));
      Vector3 spawnPosition = _playerStartingPositions[_spawnedCharacters.Count];

      NetworkObject networkPlayerObject = runner.Spawn (_playerPrefab, spawnPosition, Quaternion.identity, player);

      if (networkPlayerObject.transform.position.z > 0)
        networkPlayerObject.transform.Rotate (new Vector3 (0, 180, 0));
      // Keep track of the player avatars so we can remove it when they disconnect
      _spawnedCharacters.Add (player, networkPlayerObject);

      if (_spawnedCharacters.Count == 2) {
        GameLogic.Instance.StartGame ();
      }
    }
    this.GetComponentInChildren<Camera> ().enabled = false;
    this.GetComponentInChildren<AudioListener> ().enabled = false;
  }

  public void OnPlayerLeft (NetworkRunner runner, PlayerRef player) {
    // Find and remove the players avatar
    if (_spawnedCharacters.TryGetValue (player, out NetworkObject networkObject)) {
      runner.Despawn (networkObject);
      _spawnedCharacters.Remove (player);
    }
  }
  public void OnInput (NetworkRunner runner, NetworkInput input) {
    var data = new NetworkInputData ();

    Vector2 direction = new Vector2 ();

    direction += Vector2.up * Input.GetAxisRaw ("Vertical");
    direction += Vector2.right * Input.GetAxisRaw ("Horizontal");

    data.movementInput.angle = Vector2.Angle (Vector2.up, direction) * Mathf.Sign (direction.x);
    data.movementInput.magnitude = Mathf.Min (direction.magnitude, 1);

    direction = NormalizeMousePosition (Input.mousePosition);

    data.mousePosition.angle = Vector2.Angle (Vector2.up, direction) * Mathf.Sign (direction.x);
    data.mousePosition.magnitude = direction.magnitude;

    data.buttons = _buttons;
    _buttons.SetAllUp ();

    input.Set (data);
  }
  public void OnInputMissing (NetworkRunner runner, PlayerRef player, NetworkInput input) { }
  public void OnShutdown (NetworkRunner runner, ShutdownReason shutdownReason) { }
  public void OnConnectedToServer (NetworkRunner runner) { }
  public void OnDisconnectedFromServer (NetworkRunner runner) { }
  public void OnConnectRequest (NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
  public void OnConnectFailed (NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
  public void OnUserSimulationMessage (NetworkRunner runner, SimulationMessagePtr message) { }
  public void OnSessionListUpdated (NetworkRunner runner, List<SessionInfo> sessionList) { }
  public void OnCustomAuthenticationResponse (NetworkRunner runner, Dictionary<string, object> data) { }
  public void OnHostMigration (NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
  public void OnReliableDataReceived (NetworkRunner runner, PlayerRef player, System.ArraySegment<byte> data) { }
  public void OnSceneLoadDone (NetworkRunner runner) {
    this.GetComponentInChildren<Camera> ().enabled = false;
    this.GetComponentInChildren<AudioListener> ().enabled = false;
  }
  public void OnSceneLoadStart (NetworkRunner runner) { }

  async void OpenLobby (GameMode mode) {
    _runner = gameObject.AddComponent<NetworkRunner> ();
    _runner.ProvideInput = true;
    _buttons.SetAllUp ();

    await _runner.StartGame (new StartGameArgs () {
      GameMode = mode,
        SessionName = "Test",
        Scene = SceneManager.GetActiveScene ().buildIndex,
        SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault> ()
    });
  }

  private void Update () {
    if (Input.GetMouseButtonDown (0))
      _buttons.Set (ButtonInputs.MOUSEL, true);
    if (Input.GetMouseButtonDown (0))
      _buttons.Set (ButtonInputs.MOUSER, true);
    if (Input.GetKeyDown (KeyCode.R))
      _buttons.Set (ButtonInputs.R, true);
    if (Input.GetKeyDown (KeyCode.Q))
      _buttons.Set (ButtonInputs.Q, true);
  }

  //pixel coords -> -0.5 - 0.5
  private Vector2 NormalizeMousePosition (Vector2 mousePosition) {
    return new Vector2 (mousePosition.x / Screen.width - 0.5f, mousePosition.y / Screen.height - 0.5f);
  }
}