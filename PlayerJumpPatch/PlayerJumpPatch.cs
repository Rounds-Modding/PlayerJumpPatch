using System.Reflection;
using HarmonyLib;
using BepInEx;
using UnityEngine;

namespace PlayerJumpPatch
{
        [BepInPlugin(ModId, ModName, "0.0.0.0")]
        [BepInProcess("Rounds.exe")]
        public class PlayerJumpPatch : BaseUnityPlugin
        {
            private void Awake()
            {
                new Harmony(ModId).PatchAll();
            }
            private void Start()
            {

            }
            private const string ModId = "pykess.rounds.plugins.playerjumppatch";

            private const string ModName = "PlayerJump Patch";
        }

    // fix the erroneous logic in PlayerJump.Jump by completely replacing it. this is a terrible way of doing this.
    [HarmonyPatch(typeof(PlayerJump), "Jump")]
    class PlayerJumpPatchJump
    {
        private static bool Prefix(PlayerJump __instance, bool forceJump = false, float multiplier = 1f)
        {
            // read private/internal variables
            CharacterData data = (CharacterData)Traverse.Create(__instance).Field("data").GetValue();
            CharacterStatModifiers stats = (CharacterStatModifiers)Traverse.Create(__instance).Field("stats").GetValue();

            if (!forceJump)
            {
                if (data.sinceJump < 0.1f)
                {
                    return false;
                }
                if (data.currentJumps <= 0 && data.sinceWallGrab > 0.1f)
                {
                    return false;
                }
            }
            Vector3 a = Vector3.up;
            Vector3 vector = data.groundPos;
            if (__instance.JumpAction != null)
            {
                __instance.JumpAction();
            }
            bool flag = false;
            if (data.sinceWallGrab < 0.1f && !data.isGrounded)
            {
                a = Vector2.up * 0.8f + data.wallNormal * 0.4f;
                vector = data.wallPos;
                data.currentJumps = data.jumps;
                flag = true;
            }
            else
            {
                if (data.sinceGrounded > 0.05f)
                {
                    vector = __instance.transform.position;
                }
                // this is the error in the original method, so I've just commented it out
                //data.currentJumps = data.jumps;
            }
            // read more private/internal fields
            Vector2 velocity = (Vector2)Traverse.Create(data.playerVel).Field("velocity").GetValue();
            if (velocity.y < 0f)
            {
                // assign new velocity which is an internal field
                Traverse.Create(data.playerVel).Field("velocity").SetValue(new Vector2(velocity.x, 0f));
            }
            data.sinceGrounded = 0f;
            data.sinceJump = 0f;
            data.isGrounded = false;
            data.isWallGrab = false;
            data.currentJumps--;
            // read another private/internal field
            float mass = (float)Traverse.Create(data.playerVel).Field("mass").GetValue();
            // call private/internal method
            typeof(PlayerVelocity).InvokeMember("AddForce", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, data.playerVel, new object[] { a * multiplier * 0.01f * data.stats.jump * mass * (1f - stats.GetSlow()) * __instance.upForce, ForceMode2D.Impulse });
            if (!flag)
            {
                typeof(PlayerVelocity).InvokeMember("AddForce", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, data.playerVel, new object[] { Vector2.right * multiplier * __instance.sideForce * 0.01f * data.stats.jump * mass * (1f - stats.GetSlow()) * velocity.x, ForceMode2D.Impulse });
            }
            for (int i = 0; i < __instance.jumpPart.Length; i++)
            {
                __instance.jumpPart[i].transform.position = new Vector3(vector.x, vector.y, 5f) - a * 0f;
                __instance.jumpPart[i].transform.rotation = Quaternion.LookRotation(velocity);
                __instance.jumpPart[i].Play();
            }
            return false; // do not run the base function or any postfixes.
        }


    }
}
