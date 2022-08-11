
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;
using VRC.Udon.Common.Interfaces;

namespace Object7.KayakSystem {
    public class KayakController : UdonSharpBehaviour
    { 
        public KeyCode accelerationKeyLeft = KeyCode.Q;
        public KeyCode accelerationKeyRight = KeyCode.E;
        public KeyCode backAccelerationKeyRight = KeyCode.D;
        public KeyCode backAccelerationKeyLeft = KeyCode.A;
        public KeyCode brakeKey = KeyCode.B;
    
        private Rigidbody rbody;
        public Transform leftThrust;
        public Transform rightThrust;
        public PaddleSpeed paddleLeft;
        public PaddleSpeed paddleRight;

        public SpeedIndicator boatSpeed;

        public GameObject[] floaters;

        public float accelerationTorque = 1.0f;
        [Range(0, 10)] public float accelerationResponse = 1f;
        public float brakeTorque = 1.0f;
        [Range(0, 10)] public float brakeResponse = 1f;
        public float maxSteeringAngle = 40.0f;
        [Range(0, 10)] public float steeringResponse = 1f;

        [UdonSynced(UdonSyncMode.Smooth)] public float AccelerationValueRight;
        [UdonSynced(UdonSyncMode.Smooth)] private float BrakeValue;
        [UdonSynced(UdonSyncMode.Smooth)] private float AccelerationValueLeft;

        public Image speedBarLeftImage;
        public Image speedBarRightImage;

        public bool IsOperating;

        public float waterDrag = 0.02f;
        public float waterAngularDrag = 1f;
        public float speedAmount = 5f;

        public float vrSpeedBoost = 4f;
        public float maxSpeedAmount = 2f;

        public float magnitudeAngularDrag = 1f;
        public float keelAmount = 1f;

        public float keelSpeedMax = 5f;

        private VRCPlayerApi playerApi;

        void Start()
        {
            AccelerationValueRight = 0f;
            BrakeValue = 1.0f;
            AccelerationValueLeft = 0f;
            IsOperating = false;
            rbody = GetComponent<Rigidbody>();
        }

        public void _OnEnteredAsDriver()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            Networking.SetOwner(Networking.LocalPlayer, leftThrust.gameObject);
            Networking.SetOwner(Networking.LocalPlayer, rightThrust.gameObject);
            for (int i = 0; i < floaters.Length; i++)
            {
                Networking.SetOwner(Networking.LocalPlayer, floaters[i]);
            }
            playerApi = Networking.LocalPlayer;
            IsOperating = true;
        }

        public void _OnExited(){
            AccelerationValueRight = 0f;
            AccelerationValueLeft = 0f;
            playerApi = null;
            IsOperating = false;
        }

        private void Update(){
            DriverUpdate();
        }

        private void DriverUpdate(){
            if(!IsOperating) return;

            var deltaTime = Time.deltaTime;

            if(playerApi.IsUserInVR()){ 
                // User VR paddle speed
                var accelerationPaddleRight = Mathf.Clamp((paddleRight.speed-boatSpeed.speed) * vrSpeedBoost / maxSpeedAmount, -1f, 1f) * -1f;
                var accelerationPaddleLeft = Mathf.Clamp((paddleLeft.speed-boatSpeed.speed) * vrSpeedBoost / maxSpeedAmount, -1f, 1f) * -1f;
                AccelerationValueRight = paddleRight.inWater ? LinearLerp(AccelerationValueRight, accelerationPaddleRight, accelerationResponse * deltaTime, -1.0f, 1.0f) : LinearLerp(AccelerationValueRight, 0f, accelerationResponse * deltaTime, -1.0f, 1.0f);
                AccelerationValueLeft = paddleLeft.inWater ? LinearLerp(AccelerationValueLeft, accelerationPaddleLeft, accelerationResponse * deltaTime, -1.0f, 1.0f) : LinearLerp(AccelerationValueLeft, 0f, accelerationResponse * deltaTime, -1.0f, 1.0f);
            }else{
                // User desktop use keyboard speed
                var accelerationInputRight = Input.GetKey(backAccelerationKeyRight) ? -1.0f : (Input.GetKey(accelerationKeyRight) ? 1.0f : 0f);
                var accelerationInputLeft = Input.GetKey(backAccelerationKeyLeft) ? -1.0f : (Input.GetKey(accelerationKeyLeft) ? 1.0f : 0f);
                var brakeInput = Input.GetKey(brakeKey) ? 1.0f : 0f;
                //Debug.Log("acI: "+ accelerationInput+" stI: "+steeringInput+" brI: "+brakeInput);

                AccelerationValueRight = LinearLerp(AccelerationValueRight, accelerationInputRight, accelerationResponse * deltaTime, -1.0f, 1.0f);
                AccelerationValueLeft = LinearLerp(AccelerationValueLeft, accelerationInputLeft, accelerationResponse * deltaTime, -1.0f, 1.0f);
                BrakeValue = (brakeInput < 0.1f) ? 0.0f : LinearLerp(BrakeValue, brakeInput, brakeResponse * deltaTime, 0.0f, 1.0f);
            }

            speedBarRightImage.fillAmount = Mathf.Clamp(Mathf.Abs(AccelerationValueRight), 0, 1f);
            speedBarLeftImage.fillAmount = Mathf.Clamp(Mathf.Abs(AccelerationValueLeft), 0, 1f);
            UpdateThruster(rightThrust, AccelerationValueRight);
            UpdateThruster(leftThrust, AccelerationValueLeft);

            KeelUpdate();
        }

        private float LinearLerp(float currentValue, float targetValue, float speed, float minValue, float maxValue)
        {
            return Mathf.Clamp(Mathf.MoveTowards(currentValue, targetValue, speed), minValue, maxValue);
        }

        private void UpdateThruster(Transform point, float acceleration){
            //if(Mathf.Abs(acceleration) > 0.5f)
            {
                float speedMuliplier = acceleration * speedAmount;
                rbody.AddForceAtPosition(point.forward * speedMuliplier, point.position, ForceMode.Acceleration);
                rbody.AddForce(Mathf.Abs(speedMuliplier) * -rbody.velocity * waterDrag * Time.deltaTime, ForceMode.VelocityChange);
                float magnitudeAmount = rbody.velocity.magnitude * magnitudeAngularDrag;
                Debug.Log(magnitudeAmount);
                rbody.AddTorque(Mathf.Abs(speedMuliplier) * magnitudeAmount * -rbody.angularVelocity * waterAngularDrag * Time.deltaTime, ForceMode.VelocityChange);
                
            }
            
        }

        private void KeelUpdate(){
            Vector3 keelVector = Vector3.Reflect(rbody.velocity * -1, transform.forward);
            
            rbody.AddForce(new Vector3(Mathf.Clamp(keelVector.x, -keelSpeedMax, keelSpeedMax), 0, Mathf.Clamp(keelVector.z, -keelSpeedMax, keelSpeedMax)) * keelAmount * 0.02f, ForceMode.VelocityChange);
            
        }

        public void _Respawn()
        {
            if (IsOperating) return;

            IsOperating = true;

            Networking.SetOwner(Networking.LocalPlayer, gameObject);

            var objectSync = (VRCObjectSync)GetComponent(typeof(VRCObjectSync));
            objectSync.Respawn();

            AccelerationValueRight = 0;
            BrakeValue = 0;
            AccelerationValueLeft = 0;

            IsOperating = false;

            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(OnRespawned));
        }

        public void OnRespawned()
        {
            foreach (var rigidbody in GetComponentsInChildren<Rigidbody>())
            {
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
            }

        }
    }
}