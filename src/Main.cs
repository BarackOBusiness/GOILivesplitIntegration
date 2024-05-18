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

    private Rigidbody2D player;

    private void Awake()
    {
        // Plugin startup logic
        Logger.LogInfo($"Livesplit integration has loaded!");
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void OnDestroy() {
        time = -3f;
    }

    private void LateUpdate() {
        if (!player)
            return;

        xPos = player.position.x;
        yPos = player.position.y;
        time = Time.timeSinceLevelLoad;
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode) {
        switch (scene.name) {
            case "Mian":
                player = GameObject.Find("Player").GetComponent<Rigidbody2D>();
                time = 0f;
                break;
            case "Loader":
                time = -1f;
                break;
            case "Reward Loader":
                time = -2f;
                break;
            case "Reward Loader Offline":
                time = -2f;
                break;
        }
    }
}
