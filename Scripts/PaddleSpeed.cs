
using UdonSharp;
using UnityEngine;
using TMPro;

namespace Object7.KayakSystem {
    public class PaddleSpeed : UdonSharpBehaviour
    {
        public Transform waterPosition;
        public float smoothing = 0.5f;
        public Transform vehicleTransform;
        public float speed;
        public bool inWater;
        private Vector3 prevPosition;
        public TextMeshProUGUI speedDisplay;

        public MeshRenderer paddleMesh;
        private Color normalColor = Color.white;
        private Color activeColor = Color.red;
        
        private void OnEnable(){
            speed = 0;
            inWater = false;
            paddleMesh.material.color = normalColor;
            prevPosition = transform.position;
        }
        private void Start()
        {
            prevPosition = transform.position;
        }

        private void Update(){
            var deltaTime = Time.deltaTime;
            var position = transform.position;
            var velocity = (position - prevPosition) * (1.0f / deltaTime);
            prevPosition = position;
            speed = Mathf.Lerp(speed, Vector3.Dot(velocity, vehicleTransform.forward), deltaTime / smoothing);
            speedDisplay.text =speed.ToString("0.00");

            if(waterPosition.position.y > position.y){
                inWater = true;
                paddleMesh.material.color = activeColor;
            }else{
                inWater = false;
                paddleMesh.material.color = normalColor;
            }
        }

    }
}