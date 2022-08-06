
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ToggleObject : UdonSharpBehaviour
{
    public GameObject toggleObject;
  public override void Interact()
  {
    toggleObject.SetActive(!toggleObject.activeSelf);
  }
}
