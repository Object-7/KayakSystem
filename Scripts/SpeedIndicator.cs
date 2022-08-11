
using UdonSharp;
using UnityEngine;
using TMPro;

namespace Object7.KayakSystem {

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SpeedIndicator : UdonSharpBehaviour
    {
        public float smoothing = 0.5f;
        private Transform vehicleTransform;
        private Vector3 prevPosition;
        private Quaternion initialRotation;
        public float speed;
        public TextMeshProUGUI speedDisplay;
        private void OnEnable()
        {
            if (vehicleTransform) prevPosition = vehicleTransform.position;
            speed = 0;
        }
        private void Start()
        {
            var vehicleRigidbody = GetComponentInParent<Rigidbody>();
            vehicleTransform = vehicleRigidbody.transform;
            prevPosition = vehicleTransform.position;

            initialRotation = transform.localRotation;
        }
        private void Update()
        {
            var deltaTime = Time.deltaTime;
            var position = vehicleTransform.position;
            var velocity = (position - prevPosition) * (1.0f / deltaTime);
            prevPosition = position;
            speed = Mathf.Lerp(speed, Vector3.Dot(velocity, vehicleTransform.forward), deltaTime / smoothing);
            speedDisplay.text =speed.ToString("0.00");
        }
    }
}