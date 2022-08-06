
using UdonSharp;

namespace Object.KayakSystem {
    public class RepawnKayak : UdonSharpBehaviour
    {
        public PaddleMainGrip paddle;
        public KayakController kayak;
        
        public override void Interact()
        {
            paddle._Respawn();
            kayak._Respawn();
        }
    }
}