using System.Collections;
using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LivesplitIntegration;

// Remember to synchronize this reference to the autosplitter when editing the state mask
// Reference for the game state bitmask:
// 0001    
//    └─── "Is paused?" bit
// 0010
//   └──── "Should reset?" bit
// 1100
// └┼───── Scene bits:
//  ├───── 00: Main Menu
//  ├───── 01: Default Map (Mian Scene)
//  ├───── 10: Reward
//  └───── 11: Custom Map
// All following these bits is a mapping to custom levels. << 4 all values to get their appropriate bit positions.
// 1: Cavern Map ("Gems and Minerals")

[BepInPlugin("goi.core.livesplitintegration", "Livesplit Integration", "0.2.0")]
public class LivesplitIntegration : BaseUnityPlugin
{
    public static float xPos = 0;
    public static float yPos = 0;
    public static float time = 0;
    public static int state = 0b0000; // Start on main menu, reset and paused bits off.

    private Rigidbody2D player;

    private void Awake()
    {
        // Plugin startup logic
        Logger.LogInfo($"Livesplit integration has loaded!");
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void Update() {
        if (!player)
            return;

        state = Time.timeScale == 0f? state | 0b1 : state & ~0b1;

        xPos = player.position.x;
        yPos = player.position.y;
        time = Time.timeSinceLevelLoad;
    }

    private IEnumerator UnsetResetBit() {
        while (true) {
            yield return new WaitForFixedUpdate();
            break; // Just yield one fixed update
        }
        state = state & ~0b10;
    }

    // On any scene load that demands the timer be reset, load the coroutine to
    // unset the reset bit one tick later.
    private void OnSceneLoad(Scene scene, LoadSceneMode mode) {
        // Motivations for doing this are to determine whether multiple scenes will load when loading a custom map
        Logger.LogInfo($"Scene loaded: {scene.name}");
        switch (scene.name) {
            case "Loader":
                state = (state & 0) | 0b0010; // Set scene to man menu and set reset bit on
                break;
            case "Mian":
                state = (state & 0) | 0b0110; // Set scene to man and set reset bit on
                player = GameObject.Find("Player").GetComponent<Rigidbody2D>();
                time = 0f; xPos = 0f; yPos = 0f;
                break;
            case "Reward Loader":
            case "Reward Loader Offline":
                state = (state & 0) | 0b1000; // Set scene to end screen
                break;
            case "Gems and Minerals":
                state = (state & 0) | 0b11100; // Set scene to custom and map ID to cavern
                break;
            default:
                state = (state & 0) | 0b1110; // Set scene to custom and reset bit on so no timing occurs
                break;
        }

        if ((state & 0b10) == 2) {
            // If we're resetting, yield one tick for the autosplitter then unset the reset bit
            StartCoroutine(UnsetResetBit());
        }
    }
}
