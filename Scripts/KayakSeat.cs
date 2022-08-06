
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Object.KayakSystem {
    public class KayakSeat : UdonSharpBehaviour
    {

        public KeyCode getOutKey = KeyCode.Return;
        private VRCPlayerApi localPlayer;

        private VRCStation station;

        private bool InSeat;

        private KayakController kayak;
        private void Start()
        {
            kayak = GetComponentInParent<KayakController>();
            station = (VRCStation)GetComponent(typeof(VRCStation));
            station.disableStationExit = true;
            localPlayer = Networking.LocalPlayer;
        }

        private void Update(){
            if (Input.GetKey(getOutKey))
                {
                    station.ExitStation(localPlayer);
                }
        }

        public override void InputJump(bool value, VRC.Udon.Common.UdonInputEventArgs args)
        {
            if (InSeat && localPlayer.IsUserInVR() && args.boolValue) { station.ExitStation(localPlayer); }
        }

        public override void OnStationEntered(VRCPlayerApi player)
            {
                if (player.isLocal)
                {
                    InSeat = true;
                    kayak._OnEnteredAsDriver();
                }
            }

            public override void OnStationExited(VRCPlayerApi player)
            {
                if (player.isLocal)
                {
                    InSeat = false;
                    kayak._OnExited();
                }
            }
    }
}