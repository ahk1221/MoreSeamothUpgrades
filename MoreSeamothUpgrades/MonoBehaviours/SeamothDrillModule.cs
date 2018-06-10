using UnityEngine;

namespace MoreSeamothUpgrades.MonoBehaviours
{
    public class SeamothDrillModule : MonoBehaviour
    {
        public bool Toggle
        {
            get
            {
                return _toggle;
            }
            set
            {
                _toggle = value;
                ErrorMessage.AddDebug("Toggle changed! " + _toggle);
            }
        }

        private bool _toggle;

        private float timeNextDrill;
        public bool isDrilling;

        void StopEffects()
        {
            Main.DrillLoop.Stop();
            Main.DrillLoopHit.Stop();
        }

        void Update()
        {
            ErrorMessage.AddDebug(Toggle.ToString());

            var vehicle = Player.main.GetVehicle();
            if (vehicle == null || !vehicle.GetType().Equals(typeof(SeaMoth))) return;
            if (vehicle.modules.GetCount(Main.SeamothDrillModule) <= 0) return;
            if (!vehicle.GetPilotingMode()) return;
            if (Player.main.GetPDA().isOpen) return;

            var seamoth = (SeaMoth)vehicle;

            UpdateActiveTarget(seamoth);

            if(GameInput.GetButtonDown(GameInput.Button.LeftHand) && !isDrilling)
            {
                isDrilling = true;
                timeNextDrill = Time.time + 0.5f;
                Main.DrillLoop.Play();
            }
            
            if(GameInput.GetButtonUp(GameInput.Button.LeftHand))
            {
                isDrilling = false;
                StopEffects();
            }

            if(Time.time > timeNextDrill && isDrilling)
            {
                Drill(seamoth);
                timeNextDrill = Time.time + 0.12f;
            }
        }

        void UpdateActiveTarget(Vehicle vehicle)
        {
            var activeTarget = default(GameObject);
            Targeting.GetTarget(vehicle.gameObject, 6f, out activeTarget, out float dist, null);

            if (activeTarget)
            {
                var root = UWE.Utils.GetEntityRoot(activeTarget) ?? activeTarget;
                if (root.GetComponentProfiled<Pickupable>() || root.GetComponentProfiled<Drillable>())
                    activeTarget = root;
                else
                    root = null;
            }

            var guiHand = Player.main.GetComponent<GUIHand>();
            if (activeTarget)
                GUIHand.Send(activeTarget, HandTargetEventType.Hover, guiHand);
        }

        void Drill(SeaMoth moth)
        {
            var pos = Vector3.zero;
            var hitObj = default(GameObject);

            UWE.Utils.TraceFPSTargetPosition(moth.gameObject, 5f, ref hitObj, ref pos, true);

            if (hitObj)
            {
                var drillable = hitObj.FindAncestor<BetterDrillable>();
                Main.DrillLoopHit.Play();

                if (drillable)
                    drillable.OnDrill(transform.position, moth, out GameObject hitMesh);
                else
                {
                    LiveMixin liveMixin = hitObj.FindAncestor<LiveMixin>();
                    if (liveMixin)
                    {
                        liveMixin.TakeDamage(4f, pos, DamageType.Drill, null);
                    }

                    hitObj.SendMessage("BashHit", this, SendMessageOptions.DontRequireReceiver);
                }
            }
            else
                StopEffects();
        }
    }
}
