#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using PrisonGame.Combat;
using PrisonGame.PlayerCombat;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PrisonGame.AI.Tests.PlayMode
{
    // The built CombatAttacksSandbox: the dummy is wired to fight back, a punch starts the fight, and he
    // lands a punch on a player who stands still. Input is sent as intents, so no devices are needed.
    [Category("AI")]
    public sealed class InmateSandboxSceneTests
    {
        private const string ScenePath = "Assets/PrisonGame/Scenes/Sandbox/CombatAttacksSandbox.unity";
        private static readonly CombatIntent s_ready = new CombatIntent(true);

        [UnityTest]
        public IEnumerator BuiltSandboxDummyFightsBack()
        {
            bool background = Application.runInBackground;
            try
            {
                Application.runInBackground = true;
                EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
                yield return null;
                GameObject player = GameObject.Find("Player");
                Fighter actor = player.GetComponent<Fighter>();
                GameObject dummyObject = GameObject.Find("Sandbox dummy");
                Fighter dummy = dummyObject.GetComponent<Fighter>();
                var fight = Object.FindFirstObjectByType<FightController>();
                Assert.IsNotNull(fight);
                Assert.AreSame(actor, fight.Challenger);
                Assert.AreSame(dummy, fight.Opponent);
                Assert.IsNotNull(dummyObject.GetComponent<InmateFighterController>());
                Assert.IsNotNull(dummyObject.GetComponent<MeleeSweep>().Aim);
                Assert.IsNotNull(dummyObject.GetComponent<InmatePoseView>().Fist);
                Assert.IsFalse(dummy.Defense.CanTimedBlock);
                Assert.IsTrue(actor.Defense.CanTimedBlock);
                var hud = player.GetComponentInChildren<FightHud>();
                Assert.IsNotNull(hud);
                Assert.IsFalse(hud.HealthBarVisible, "No health bar outside a fight.");

                // Stand where the step-2 scene test stands, facing the dummy, and punch through intents.
                player.GetComponent<LocalCombatInput>().enabled = false;
                var controller = player.GetComponent<CharacterController>();
                controller.enabled = false;
                player.transform.SetPositionAndRotation(new Vector3(0.4f, 0.05f, 0.25f), Quaternion.identity);
                controller.enabled = true;
                yield return null;
                actor.SubmitIntent(new CombatIntent(true, punchPressed: true));
                yield return null;
                // The input adapter sends an enabled intent every frame; without one, combat controls are off
                // and the punch is cancelled.
                float waited = 0f;
                while (!fight.Session.IsActive && waited < 1f)
                {
                    actor.SubmitIntent(s_ready);
                    yield return null;
                    waited += Time.deltaTime;
                }
                Assert.IsTrue(fight.Session.IsActive, "The punch started the fight.");
                Assert.IsTrue(hud.HealthBarVisible);

                waited = 0f;
                while (actor.Health.Current >= 100f && waited < 6f)
                {
                    actor.SubmitIntent(s_ready);
                    yield return null;
                    waited += Time.deltaTime;
                }
                Assert.AreEqual(100f - UnarmedAttack.InmatePunch.Damage, actor.Health.Current, 1e-4f,
                    "He fought back and landed his punch.");
                Assert.Greater(hud.FlashAlphaNow, 0f, "Taking the hit flashes the screen edge.");
                Assert.Less(dummy.GetComponent<Collider>().bounds.max.z, 2.51f, "He stays inside the cell.");
            }
            finally
            {
                Application.runInBackground = background;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        [UnityTearDown]
        public IEnumerator UnloadSandbox()
        {
            Scene populated = SceneManager.GetSceneByName("CombatAttacksSandbox");
            if (!populated.IsValid() || !populated.isLoaded)
            {
                yield break;
            }
            Scene empty = SceneManager.CreateScene("AfterInmateSandboxTest");
            SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync(populated);
        }
    }
}
#endif
