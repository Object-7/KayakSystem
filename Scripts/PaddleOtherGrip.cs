
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Object.KayakSystem {
    public class PaddleOtherGrip : UdonSharpBehaviour
    {
        public GameObject objPrimary;
        private PaddleMainGrip paddle;
        private bool isHolding;
        private Transform originalParent;

        void Start()
        {
            originalParent = transform.parent;

            paddle = objPrimary.GetComponent<PaddleMainGrip>();
            OnDrop();
        }

        public override void OnPickup()
        {
            isHolding = true;
            if (Networking.LocalPlayer.IsUserInVR())
            {
                originalParent = transform.parent;
                transform.parent = transform.parent.parent;
            }
        }

        public override void OnDrop()
        {
            isHolding = false;

            if (Networking.LocalPlayer.IsUserInVR())
            {
                transform.parent = originalParent;
            }
        }
    }
}