using System.Collections;
using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LivesplitIntegration;

[BepInPlugin("goi.core.livesplitintegration", "Livesplit Integration", "0.1.0")]
public class LivesplitIntegration : BaseUnityPlugin
{
    public static float xPos = 0;
    public static float yPos = 0;
    public static float time = 0;
    public static int state = 2; // Start on main menu

    private Rigidbody2D player;
    private bool isResetting;

    private void Awake()
    {
        // Plugin startup logic
        Logger.LogInfo($"Livesplit integration has loaded!");
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void LateUpdate() {
        if ((state & 0b1000) == 8 && !isResetting) {
            StartCoroutine(UnsetResetBit());
        }

        if (!player)
            return;

        state = Time.timeScale == 0f? state | 0b100 : state & ~0b100;

        xPos = player.position.x;
        yPos = player.position.y;
        time = Time.timeSinceLevelLoad;
    }

    private IEnumerator UnsetResetBit() {
        isResetting = true;
        while (true) {
            yield return new WaitForFixedUpdate();
            break; // Just yield one fixed update...
        }
        state = state & ~0b1000;
        isResetting = false;
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode) {
        switch (scene.name) {
            case "Mian":
                player = GameObject.Find("Player").GetComponent<Rigidbody2D>();
                state = (state & ~0b11) | 0b1000; // Set scene to man and set reset bit on
                time = 0f; xPos = 0f; yPos = 0f;
                break;
            case "Reward Loader":
                state = (state & ~0b11) | 0b01; // Set scene to end screen
                break;
            case "Reward Loader Offline":
                state = (state & ~0b11) | 0b01;
                break;
            case "Loader":
                state = (state & ~0b11) | 0b1010; // Set scene to man menu and set reset bit on
                break;
            default:
                state = state | ~0b1011; // Set scene to unknown and reset bit on so no timing occurs
                break;
        }
    }
}
