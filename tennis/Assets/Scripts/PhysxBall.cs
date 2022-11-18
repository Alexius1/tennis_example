using UnityEngine;
using Fusion;

public class PhysxBall : NetworkBehaviour
{
    [Networked] private TickTimer life { get; set; }

    public void Init(Vector3 forward)
    {
        life = TickTimer.CreateFromSeconds(Runner, 5.0f);
        GetComponent<Rigidbody>().velocity = forward;
    }

    public override void FixedUpdateNetwork()
    {
        if (life.Expired(Runner))
            Runner.Despawn(Object);
    }

    private void OnCollisionEnter(Collision other) {
        if(other.gameObject.CompareTag("Net")){
            print("Net");
        }else if (other.gameObject.CompareTag("Ground")){
            print(other.GetContact(0).point);
        }
    }
}