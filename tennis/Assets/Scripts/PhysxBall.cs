using Fusion;
using UnityEngine;

public class PhysxBall : NetworkBehaviour {
  [Networked] private TickTimer life { get; set; }
  private GameLogic gameLogic;

  public void Init (Vector3 forward, GameLogic gameLogic) {
    life = TickTimer.CreateFromSeconds (Runner, 5.0f);
    GetComponent<Rigidbody> ().velocity = forward;
    this.gameLogic = gameLogic;
  }

  public override void FixedUpdateNetwork () {
    if (life.Expired (Runner)){
      Runner.Despawn (Object);
      gameLogic.BallHit(Area.Out);
    }
  }

  private void OnCollisionEnter (Collision other) {
    if (other.gameObject.CompareTag ("Net")) {
      gameLogic.BallHit(Area.Net);
      Runner.Despawn (Object);
    } else if (other.gameObject.CompareTag ("Ground")) {
      if(!gameLogic.BallHit(transform.position))
        Runner.Despawn (Object);
    }
  }

  private void OnTriggerStay(Collider other) {
    if (other.gameObject.CompareTag ("Player")) {
      other.GetComponent<Player>().CatchBall();
      if (Runner != null && Runner.IsServer) //to prevent error when deleting object
        Runner.Despawn(Object);
    }
  }
}