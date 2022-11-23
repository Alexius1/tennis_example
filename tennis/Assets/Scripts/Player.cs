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
  [Networked] private BallControll BallControll { get; set; } = BallControll.None;
  [SerializeField] private Camera _camera;
  //private static readonly Vector3 BASELINEMIDDLE = new Vector3 (0, 0, 11.89f);
  //private static readonly float SINGLEFIELDWIDTH = 8.23f;

  private void Awake () {
    _cc = GetComponent<NetworkCharacterControllerPrototype> ();
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
      if (BallControll != BallControll.Normal)
        _cc.Move (inputDirection * 5 * Runner.DeltaTime);

      _camera.transform.parent.localEulerAngles = new Vector3 (-Mathf.Cos (data.mousePosition.angle / 180 * Mathf.PI),
        Mathf.Sin (data.mousePosition.angle / 180 * Mathf.PI), 0) * data.mousePosition.magnitude * 15;
      if (BallControll != BallControll.None) {
        if (data.buttons.IsSet (ButtonInputs.MOUSEL)) {
          ThrowBall (10);
        }
      }
    }
  }

  private void ThrowBall (int force) {
    if (HasStateAuthority) {
      Quaternion centerAdjustment = Quaternion.Euler (-25, 0, 0);
      Runner.Spawn (_physxBallPrefab,
        transform.position + _forward, Quaternion.identity,
        Object.InputAuthority, (runner, o) => {
          o.GetComponent<PhysxBall> ().Init (_camera.transform.parent.rotation * centerAdjustment * Vector3.forward * force);
        });
    }
    BallControll = BallControll.None;
  }

  public void CatchBall () {
    BallControll = BallControll.Normal;
  }

  public void Serve (bool left) {
    if (left) {
      BallControll = BallControll.ServingLeft;
    } else {
      BallControll = BallControll.ServingRight;
    }
  }

  public void Reset(){
    BallControll = BallControll.None;
  }
}