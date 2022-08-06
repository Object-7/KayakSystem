
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Object.KayakSystem {

    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class PaddleMainGrip : UdonSharpBehaviour
    {
        public PaddleOtherGrip otherHand;
        private Transform targetTransform;
        private Vector3 paddleRespawnPosition;
        private Vector3 otherHandRespawnPosition;

        public Transform paddleParent;

        private VRCPlayerApi playerApi;

        private Vector3 oldTargetPos;
        private bool isPickedUp;

        private Collider ownCollider;
        private Collider targetCollider;

        private Vector3 vectorOne = Vector3.one;
        private Vector3 vectorZero = Vector3.zero;
        private Vector3 vectorUp = Vector3.up;
        private Quaternion upwardsRotation = Quaternion.Euler(-90, 0, 0);
        private Quaternion startingRotation;

        private bool startupCompleted;

        void Start()
        {
            if (Networking.LocalPlayer == null)
            {
                gameObject.SetActive(false);
                return;
            }

            paddleRespawnPosition = transform.localPosition;
            playerApi = Networking.LocalPlayer;

            ownCollider = GetComponent<Collider>();

            targetTransform = otherHand.transform;
            otherHandRespawnPosition = targetTransform.localPosition;

            targetCollider = targetTransform.GetComponent<Collider>();
            if (!targetCollider)
                        
            startingRotation = paddleParent.localRotation;

            startupCompleted = true;
        }

        public void LateUpdate() {
            Vector3 middlePoint =  LerpByDistance(targetTransform.position,transform.position, Vector3.Distance(targetTransform.position,transform.position) * 0.5f);
            
            paddleParent.position = middlePoint;
            paddleParent.LookAt(targetTransform.position);
            
            oldTargetPos = targetTransform.position;
        }

        public Vector3 LerpByDistance(Vector3 A, Vector3 B, float x)
        {
            Vector3 P = x * Vector3.Normalize(B - A) + A;
            return P;
        }

        public override void OnPickup()
            {

                if (!startupCompleted)
                {
                    return;
                }
                isPickedUp = true;

            }
        
        public override void OnDrop()
        {
            if (!startupCompleted)
            {
                return;
            }
            
            isPickedUp = false;

        }

        public void _Respawn()
        {
            transform.localPosition = paddleRespawnPosition;
            paddleParent.localPosition = transform.localPosition;
            transform.localRotation = startingRotation;
            targetTransform.localPosition = otherHandRespawnPosition;
            paddleParent.LookAt(targetTransform.position);
        }

    }
}