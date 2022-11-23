using Fusion;
using UnityEngine;

public class PhysxBall : NetworkBehaviour {
  [Networked] private TickTimer life { get; set; }

  public void Init (Vector3 forward) {
    life = TickTimer.CreateFromSeconds (Runner, 5.0f);
    GetComponent<Rigidbody> ().velocity = forward;
  }

  public override void FixedUpdateNetwork () {
    if (!HasStateAuthoritySafe ())
      return;
    if (life.Expired (Runner)) {
      Runner.Despawn (Object);
      GameLogic.Instance.BallHit (Area.Out);
    }
  }

  private void OnCollisionEnter (Collision other) {
    if (!HasStateAuthoritySafe ())
      return;
    if (other.gameObject.CompareTag ("Net")) {
      GameLogic.Instance.BallHit (Area.Net);
      Destroy ();
    } else if (other.gameObject.CompareTag ("Ground")) {
      if (!GameLogic.Instance.BallHit (transform.position))
        Destroy ();
    }
  }

  private void OnTriggerEnter (Collider other) {
    if (!HasStateAuthoritySafe ())
      return;
    if (other.gameObject.CompareTag ("Player")) {
      GameLogic.Instance.SwitchExpectedSide();
      other.GetComponent<Player> ().CatchBall ();
      Destroy ();
    }
  }

  private bool HasStateAuthoritySafe () {
    return Object != null && Object.HasStateAuthority;
  }

  private void Destroy () {
    if (Runner != null && Runner.IsServer) //to prevent error when deleting object
      Runner.Despawn (Object);
  }
}