using UnityEngine;
using MoreSeamothUpgrades.Patches;

namespace MoreSeamothUpgrades.MonoBehaviours
{
    public class SeamothClaw : MonoBehaviour
    {
        private SeaMoth seamoth;

        void Start()
        {
            // Get Seamoth component on the current GameObject.
            seamoth = GetComponent<SeaMoth>();
        }
    }
}