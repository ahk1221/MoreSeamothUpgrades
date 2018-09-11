using UnityEngine;
using MoreSeamothUpgrades.Patches;

namespace MoreSeamothUpgrades.MonoBehaviours
{
    public class SeamothClaw : MonoBehaviour
    {
        private float cooldownTime;
        private float timeUsed = float.NegativeInfinity;

        private SeaMoth seamoth;

        private bool TryUse(out float cooldownDuration)
        {
            if (Time.time - this.timeUsed >= this.cooldownTime)
            {
                seamoth = base.GetComponentInParent<SeaMoth>();
                Pickupable pickupable = null;
                PickPrefab x = null;
                if (GetActiveTarget(seamoth))
                {
                    pickupable = GetActiveTarget(seamoth).GetComponent<Pickupable>();
                    x = GetActiveTarget(seamoth).GetComponent<PickPrefab>();
                }
                if (pickupable != null && pickupable.isPickupable)
                {
                    if (componentInParent.storageContainer.container.HasRoomFor(pickupable))
                    {
                        this.animator.SetTrigger("use_tool");
                        this.cooldownTime = (cooldownDuration = this.cooldownPickup);
                        this.shownNoRoomNotification = false;
                        return true;
                    }
                    if (!this.shownNoRoomNotification)
                    {
                        ErrorMessage.AddMessage(Language.main.Get("ContainerCantFit"));
                        this.shownNoRoomNotification = true;
                    }
                }
                else
                {
                    if (x != null)
                    {
                        this.animator.SetTrigger("use_tool");
                        this.cooldownTime = (cooldownDuration = this.cooldownPickup);
                        return true;
                    }
                    this.animator.SetTrigger("bash");
                    this.cooldownTime = (cooldownDuration = this.cooldownPunch);
                    this.fxControl.Play(0);
                    return true;
                }
            }
            cooldownDuration = 0f;
            return false;
        }

        public void OnPickup()
        {
            seamoth = base.GetComponentInParent<SeaMoth>();
            if (GetActiveTarget(seamoth))
            {
                Pickupable pickupable = GetActiveTarget(seamoth).GetComponent<Pickupable>();
                PickPrefab component = GetActiveTarget(seamoth).GetComponent<PickPrefab>();
                if (pickupable != null && pickupable.isPickupable && componentInParent.storageContainer.container.HasRoomFor(pickupable))
                {
                    pickupable = pickupable.Initialize();
                    InventoryItem item = new InventoryItem(pickupable);
                    componentInParent.storageContainer.container.UnsafeAdd(item);
                    global::Utils.PlayFMODAsset(this.pickupSound, this.front, 5f);
                }
                else if (component != null && component.AddToContainer(componentInParent.storageContainer.container))
                {
                    component.SetPickedUp();
                }
            }
        }

        public void OnHit()
        {
            seamoth = base.GetComponentInParent<SeaMoth>();
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
                        global::Utils.PlayFMODAsset(this.hitFishSound, this.front, 50f);
                    }
                    else
                    {
                        global::Utils.PlayFMODAsset(this.hitTerrainSound, this.front, 50f);
                    }
                    VFXSurface component2 = gameObject.GetComponent<VFXSurface>();
                    Vector3 euler = MainCameraControl.main.transform.eulerAngles + new Vector3(300f, 90f, 0f);
                    VFXSurfaceTypeManager.main.Play(component2, this.vfxEventType, position, Quaternion.Euler(euler), componentInParent.gameObject.transform);
                    gameObject.SendMessage("BashHit", this, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        GameObject GetActiveTarget(Vehicle vehicle)
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

            return activeTarget;
        }
    }
}