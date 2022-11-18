using UnityEngine;
using Fusion;

public class Player : NetworkBehaviour
{
    private NetworkCharacterControllerPrototype _cc;
    [SerializeField] private Ball _ballPrefab;
    [SerializeField] private PhysxBall _physxBallPrefab;
    private Vector3 _forward;
    [Networked] private TickTimer delay { get; set; }

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterControllerPrototype>();
        _forward = new Vector3(1, 0, 0);
    }
    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();
            _cc.Move(5 * data.direction * Runner.DeltaTime);
        }
        if (data.direction.sqrMagnitude > 0)
            _forward = data.direction;
        if (delay.ExpiredOrNotRunning(Runner))
        {
            if ((data.buttons & NetworkInputData.MOUSEBUTTON1) != 0)
            {
                delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                Runner.Spawn(_ballPrefab,
                transform.position + _forward, Quaternion.LookRotation(_forward),
                Object.InputAuthority, (runner, o) =>
                {
                    o.GetComponent<Ball>().Init();
                });
            }

            if ((data.buttons & NetworkInputData.MOUSEBUTTON2) != 0)
            {
                delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                Runner.Spawn(_physxBallPrefab,
                transform.position + _forward, Quaternion.LookRotation(_forward),
                Object.InputAuthority, (runner, o) =>
                {
                    o.GetComponent<PhysxBall>().Init(10*_forward);
                });
            }
        }
    }
}