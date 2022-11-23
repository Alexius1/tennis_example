using Fusion;
using UnityEngine;

public class BallSpawner : NetworkBehaviour {
  [SerializeField] private PhysxBall _physxBallPrefab;
  [Networked] private TickTimer delay { get; set; }
  [SerializeField] private GameLogic _gameLogic;

  public override void FixedUpdateNetwork () {
    if (delay.ExpiredOrNotRunning (Runner)) {
      delay = TickTimer.CreateFromSeconds (Runner, 2);
      Runner.Spawn (_physxBallPrefab,
        transform.position, Quaternion.identity,
        Object.InputAuthority, (runner, o) => {
          o.GetComponent<PhysxBall> ().Init (10 * transform.forward, _gameLogic);
        });
    }
  }
}