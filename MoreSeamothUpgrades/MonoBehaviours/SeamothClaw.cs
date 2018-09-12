using UnityEngine;
using System;
using MoreSeamothUpgrades.Patches;

namespace MoreSeamothUpgrades.MonoBehaviours
{
    public class SeamothClaw : MonoBehaviour
    {
        public bool toggle;

        private float cooldownTime = Time.time;
        public const float cooldownHit = 1f;
        public const float cooldownPickup = 1.533f;

        public Animator animator;
        public FMODAsset hitTerrainSound;
        public FMODAsset hitFishSound;
        public FMODAsset pickupSound;
        public Transform front;
        public VFXEventTypes vfxEventType;
        public VFXController fxControl;
        private bool shownNoRoomNotification;

        private SeaMoth seamoth;

        void Start()
        {
            // Get Seamoth component on the current GameObject.
            seamoth = GetComponent<SeaMoth>();
        }

        void Update()
        {
            // If its not selected, we don't want to run the rest of the function
            if (!toggle) return;

            // Some checks to see if we can pick up or not.
            if (seamoth.modules.GetCount(SeamothModule.SeamothClawModule) <= 0) return;
            if (!seamoth.GetPilotingMode()) return;
            if (Player.main.GetPDA().isOpen) return;

            // Update hovering.
            UpdateActiveTarget(seamoth);

            // If we let up the Left Mouse Button
            if (GameInput.GetButtonUp(GameInput.Button.LeftHand))
            {
                if (Time.time > this.cooldownTime)
                {
                    // Try Use!
                    Console.WriteLine("[MoreSeamothUpgrades] Claw cooldown ok. Trying Use!");
                    TryUse();
                }
            }
        }

        void UpdateActiveTarget(Vehicle vehicle)
        {
            // Get the GameObject we're looking at
            var activeTarget = default(GameObject);
            Targeting.GetTarget(vehicle.gameObject, 6f, out activeTarget, out float dist, null);

            // Check if not null
            if (activeTarget != null)
            {
                // Get the root object, or the hit object if root is null
                var root = UWE.Utils.GetEntityRoot(activeTarget) ?? activeTarget;
                if (root.GetComponentProfiled<Pickupable>())
                    activeTarget = root;
                else
                    root = null;
            }

            // Get the GUIHand component
            var guiHand = Player.main.GetComponent<GUIHand>();
            if (activeTarget)
            {
                // Send the Hover message to the GameObject we're looking at.
                GUIHand.Send(activeTarget, HandTargetEventType.Hover, guiHand);
            }
        }

        public ItemsContainer GetStorageContainer(Pickupable pickupable)
        {
            for (int i = 0; i < 9; i++)
            {
                var storage = seamoth.GetStorageInSlot(i, TechType.VehicleStorageModule);
                if (storage != null && storage.HasRoomFor(pickupable))
                    return storage;
            }

            return null;
        }

        void TryUse()
        {
            Pickupable pickupable = null;
            PickPrefab component = null;

            Console.WriteLine("[MoreSeamothUpgrades] Claw getting hit object.");
            var pos = Vector3.zero;
            var hitObject = default(GameObject);

            UWE.Utils.TraceFPSTargetPosition(seamoth.gameObject, 6f, ref hitObject, ref pos, true);

            if (hitObject)
            {
                Console.WriteLine("[MoreSeamothUpgrades] Claw hit object is not null.");

                pickupable = hitObject.FindAncestor<Pickupable>();
                component = hitObject.FindAncestor<PickPrefab>();
            }
            if (pickupable != null && pickupable.isPickupable)
            {
                Console.WriteLine("[MoreSeamothUpgrades] Claw pickupable is not null and is pickupable.");
                if (GetStorageContainer(pickupable).HasRoomFor(pickupable))
                {
                    //this.animator.SetTrigger("use_tool");
                    this.shownNoRoomNotification = false;
                    OnPickup(pickupable, component);
                }
                else
                {
                    Console.WriteLine("[MoreSeamothUpgrades] Storage container room check failure.");
                }
                if (!this.shownNoRoomNotification)
                {
                    ErrorMessage.AddMessage(Language.main.Get("ContainerCantFit"));
                    this.shownNoRoomNotification = true;
                }
            }
            else
            {
                Console.WriteLine("[MoreSeamothUpgrades] Claw pickupable was null.");
                if (component != null)
                {
                    Console.WriteLine("[MoreSeamothUpgrades] Claw component is not null.");
                    //this.animator.SetTrigger("use_tool");
                    OnPickup(pickupable, component);
                }
                else
                {
                    Console.WriteLine("[MoreSeamothUpgrades] Claw component was also null.");
                }
                //Console.WriteLine("[MoreSeamothUpgrades] Claw setting bash animator trigger.");
                //this.animator.SetTrigger("bash");
                //Console.WriteLine("[MoreSeamothUpgrades] Claw playing fxControl.");
                //this.fxControl.Play(0);
                Console.WriteLine("[MoreSeamothUpgrades] Claw OnHit().");
                OnHit();
            }
        }

        void OnPickup(Pickupable pickupable, PickPrefab component)
        {
            if (pickupable != null && pickupable.isPickupable && GetStorageContainer(pickupable).HasRoomFor(pickupable))
            {
                pickupable = pickupable.Initialize();
                InventoryItem item = new InventoryItem(pickupable);
                GetStorageContainer(pickupable).UnsafeAdd(item);
                global::Utils.PlayFMODAsset(this.pickupSound, this.front, 5f);
            }
            else if (component != null && component.AddToContainer(GetStorageContainer(pickupable)))
            {
                component.SetPickedUp();
            }

            this.cooldownTime = Time.time + cooldownPickup;
        }

        void OnHit()
        {
            if (seamoth.CanPilot() && seamoth.GetPilotingMode())
            {
                Vector3 position = default(Vector3);
                GameObject gameObject = null;
                UWE.Utils.TraceFPSTargetPosition(seamoth.gameObject, 6.5f, ref gameObject, ref position, true);
                if (gameObject == null)
                {
                    InteractionVolumeUser component = Player.main.gameObject.GetComponent<InteractionVolumeUser>();
                    if (component != null && component.GetMostRecent() != null)
                    {
                        gameObject = component.GetMostRecent().gameObject;
                    }
                }
                if (gameObject)
                {
                    LiveMixin liveMixin = gameObject.FindAncestor<LiveMixin>();
                    if (liveMixin)
                    {
                        bool flag = liveMixin.IsAlive();
                        liveMixin.TakeDamage(50f, position, DamageType.Normal, null);
                        //global::Utils.PlayFMODAsset(this.hitFishSound, this.front, 50f);
                    }
                    else
                    {
                        //global::Utils.PlayFMODAsset(this.hitTerrainSound, this.front, 50f);
                    }
                    VFXSurface component2 = gameObject.GetComponent<VFXSurface>();
                    Vector3 euler = MainCameraControl.main.transform.eulerAngles + new Vector3(300f, 90f, 0f);
                    //VFXSurfaceTypeManager.main.Play(component2, this.vfxEventType, position, Quaternion.Euler(euler), seamoth.gameObject.transform);
                    gameObject.SendMessage("BashHit", this, SendMessageOptions.DontRequireReceiver);
                }
            }

            this.cooldownTime = Time.time + cooldownHit;
        }
    }
}