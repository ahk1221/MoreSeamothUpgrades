using UnityEngine;
using MoreSeamothUpgrades.Patches;

namespace MoreSeamothUpgrades.MonoBehaviours
{
    public class SeamothDrill : MonoBehaviour
    {
        public bool toggle;

        private float timeNextDrill;
        public bool isDrilling;

        private SeaMoth seamoth;

        void StopEffects()
        {
            Main.DrillLoop.Stop();
            Main.DrillLoopHit.Stop();
        }

        void Start()
        {
            seamoth = GetComponent<SeaMoth>();
        }

        void Update()
        {
            if (!toggle) return;

            if (seamoth.modules.GetCount(Main.SeamothDrillModule) <= 0) return;
            if (!seamoth.GetPilotingMode()) return;
            if (Player.main.GetPDA().isOpen) return;

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
                if (root.GetComponentProfiled<Drillable>())
                    activeTarget = root;
                else
                    root = null;
            }

            var guiHand = Player.main.GetComponent<GUIHand>();
            if (activeTarget)
            {
                GUIHand.Send(activeTarget, HandTargetEventType.Hover, guiHand);
            }
        }

        void Drill(SeaMoth moth)
        {
            var pos = Vector3.zero;
            var hitObj = default(GameObject);

            UWE.Utils.TraceFPSTargetPosition(moth.gameObject, 6f, ref hitObj, ref pos, true);

            if (hitObj)
            {
                var drillable = hitObj.FindAncestor<BetterDrillable>();
                Main.DrillLoopHit.Play();

                if (drillable)
                {
                    drillable.OnDrill(transform.position, moth, out GameObject hitMesh);
                }
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
            {
                StopEffects();
            }
        }
    }
}
