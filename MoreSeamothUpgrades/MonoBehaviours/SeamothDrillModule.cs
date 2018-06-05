using UnityEngine;

namespace MoreSeamothUpgrades.MonoBehaviours
{
    public class SeamothDrillModule : MonoBehaviour
    {
        private float timeNextDrill;

        public FMOD_CustomLoopingEmitter loopHit;
        public FMOD_CustomLoopingEmitter loop;

        public bool isDrilling;
        public float timeToStartDrilling;

        private bool firstFrame = false;

        void StopEffects()
        {
            loop.Stop();
            loopHit.Stop();
        }

        void Update()
        {
            if(!firstFrame)
            {
                var exosuit = Resources.Load<GameObject>("WorldEntities/Tools/ExosuitDrillArmModule").GetComponent<ExosuitDrillArm>();
                loopHit = exosuit.loopHit;
                loop = exosuit.loop;

                firstFrame = true;
            }

            var seamoth = Player.main.GetVehicle();
            if (seamoth == null || !seamoth.GetType().Equals(typeof(SeaMoth)) || Player.main.GetPDA().isOpen) return;

            if(GameInput.GetButtonDown(GameInput.Button.LeftHand) && !isDrilling)
            {
                isDrilling = true;
                timeToStartDrilling = Time.time + 0.5f;
                loop.Play();
            }
            
            if(GameInput.GetButtonUp(GameInput.Button.LeftHand))
            {
                isDrilling = false;
                StopEffects();
            }

            if(Time.time > timeNextDrill && isDrilling && Time.time > timeToStartDrilling)
            {
                Drill((SeaMoth)seamoth);
                timeNextDrill = Time.time + 0.12f;
            }
        }

        void Drill(SeaMoth moth)
        {
            var pos = Vector3.zero;
            var hitObj = default(GameObject);

            UWE.Utils.TraceFPSTargetPosition(moth.gameObject, 5f, ref hitObj, ref pos, true);

            if (hitObj)
            {
                var drillable = hitObj.FindAncestor<BetterDrillable>();
                //loopHit.Play();

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
                StopEffects();
        }
    }
}
