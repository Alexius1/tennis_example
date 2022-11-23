using Fusion;
using UnityEngine;

public enum ButtonInputs {
    MOUSEL = 0,
    MOUSER = 1,
    R = 2,
    Q = 2
}

public struct PolarCoords : INetworkStruct {
  public float angle;
  public float magnitude;
}

public struct NetworkInputData : INetworkInput {
  public NetworkButtons buttons;
  public PolarCoords movementInput;
  public PolarCoords mousePosition;


  public override string ToString(){
    return System.Convert.ToString(buttons.Bits, 2) + " movement: " + movementInput.angle + " " + movementInput.magnitude
     + " camera: " + mousePosition.angle + " " + mousePosition.magnitude;
  }
}