
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Object.KayakSystem {
    public class Floater : UdonSharpBehaviour
    {
        public Transform waterPosition;
        public Rigidbody rbody;
        public float depthBeforeSubmerged = 1f;
        public float displacementAmount = 3f;
        public int floaterCount = 1;

        public float waterDrag = 0.99f;
        public float waterAngularDrag = 0.5f;
        void Start()
        {
        }

        private void FixedUpdate() {
            
            rbody.AddForceAtPosition(Physics.gravity / floaterCount, transform.position, ForceMode.Acceleration);
            if(Networking.IsOwner(gameObject))
            {
                if(transform.position.y < waterPosition.position.y){
                float submergedDelta = waterPosition.position.y - transform.position.y;
                float displacementMultiplier = Mathf.Clamp01(Mathf.Abs(submergedDelta) / depthBeforeSubmerged) * displacementAmount;
                rbody.AddForceAtPosition(new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacementMultiplier, 0f), transform.position, ForceMode.Acceleration);
                rbody.AddForce(displacementMultiplier * -rbody.velocity * waterDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
                rbody.AddTorque(displacementMultiplier * -rbody.angularVelocity * waterAngularDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
                }
            }
            
        }
    }
}
