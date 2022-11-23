using Fusion;
using UnityEngine;

public enum BallControll {
  None,
  ServingLeft,
  ServingRight,
  Normal
}

public class Player : NetworkBehaviour {
  private NetworkCharacterControllerPrototype _cc;
  [SerializeField] private PhysxBall _physxBallPrefab;
  private Vector3 _forward;
  private BallControll _ballControll = BallControll.None;
  [SerializeField] private Camera _camera;
  private GameLogic _gameLogic;

  private void Awake () {
    _cc = GetComponent<NetworkCharacterControllerPrototype> ();
    _gameLogic = FindObjectOfType<GameLogic>();
  }
  private void Start () {
    if (_cc.HasInputAuthority) {
      GetComponentInChildren<Camera> ().enabled = true;
      GetComponentInChildren<AudioListener> ().enabled = true;
    }
    _forward = transform.forward;
  }
  public override void FixedUpdateNetwork () {
    if (GetInput (out NetworkInputData data)) {
      Vector3 inputDirection = Quaternion.Euler (0, data.movementInput.angle, 0) * _forward * data.movementInput.magnitude;
      if (_ballControll == BallControll.None)
        _cc.Move (inputDirection * 5 * Runner.DeltaTime);
      else if (_ballControll != BallControll.Normal) {
        inputDirection.Scale (new Vector3 (1, 0, 0));
        _cc.Move (inputDirection * 5 * Runner.DeltaTime);
      }

      _camera.transform.parent.localEulerAngles = new Vector3 (-Mathf.Cos (data.mousePosition.angle / 180 * Mathf.PI),
        Mathf.Sin (data.mousePosition.angle / 180 * Mathf.PI), 0) * data.mousePosition.magnitude * 15;
      if (_ballControll != BallControll.None) {
        if (data.buttons.IsSet (ButtonInputs.MOUSEL)) {
          ThrowBall (10);
        }
      }
    }
  }

  private void ThrowBall (int force) {
    if (Runner.IsServer) {
      Quaternion centerAdjustment = Quaternion.Euler (-45, 0, 0);
      Runner.Spawn (_physxBallPrefab,
        transform.position + _forward, Quaternion.identity,
        Object.InputAuthority, (runner, o) => {
          o.GetComponent<PhysxBall> ().Init (_camera.transform.parent.localRotation * centerAdjustment * _forward * force, _gameLogic);
        });
    }
    _ballControll = BallControll.None;
  }

  public void CatchBall () {
    _ballControll = BallControll.Normal;
  }
}