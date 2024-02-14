using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Projectiles {
    public class RopeLinkController : Controller {

        [SerializeField] private HingeJoint2D joint;
        [SerializeField] private Rigidbody2D rigidBody;

        protected override void Clean() { }

        protected override Task Initialize() {

            joint ??= GetComponent<HingeJoint2D>();
            rigidBody ??= GetComponent<Rigidbody2D>();

            return Task.CompletedTask;
        }

        public void ConnectJointTo(Rigidbody2D bodyToConnectTo) {

            joint.connectedBody = bodyToConnectTo;
            //joint.anchor = Vector3.zero;
            //joint.autoConfigureConnectedAnchor = false;
            //joint.connectedAnchor = Vector3.zero;
        }
    }
}