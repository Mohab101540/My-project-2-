using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Tools > Dodge Game > Set Up Audio
// Builds the "Audio" object in the open scene with one AudioSource child per sound,
// wires them to GameAudio and saves the scene. Run once; delete "Audio" in the Hierarchy to rebuild.
public static class AudioSetup
{
    [MenuItem("Tools/Dodge Game/Set Up Audio")]
    public static void SetUpAudio()
    {
        GameAudio gameAudio = Object.FindFirstObjectByType<GameAudio>(FindObjectsInactive.Include);

        if (gameAudio == null)
        {
            EditorUtility.DisplayDialog("Dodge Game", "Open the main scene first (needs GameAudio on GameManager).", "OK");
            return;
        }

        if (GameObject.Find("Audio") != null)
        {
            EditorUtility.DisplayDialog("Dodge Game", "Audio already exists. Delete \"Audio\" in the Hierarchy to rebuild it.", "OK");
            return;
        }

        GameObject root = new GameObject("Audio");
        Undo.RegisterCreatedObjectUndo(root, "Create Audio");
        Undo.RecordObject(gameAudio, "Create Audio");

        gameAudio.throwSound = AddSound(root, "ThrowSound", "throw", 0.4f);
        gameAudio.hitSound = AddSound(root, "HitSound", "hit", 1f);
        gameAudio.collectSound = AddSound(root, "CollectSound", "collect", 0.8f);
        gameAudio.bossSound = AddSound(root, "BossSound", "boss", 1f);
        gameAudio.winSound = AddSound(root, "WinSound", "win", 1f);
        gameAudio.loseSound = AddSound(root, "LoseSound", "lose", 1f);

        gameAudio.music = AddSound(root, "BackgroundMusic", "music", 0.3f);
        gameAudio.music.playOnAwake = true;
        gameAudio.music.loop = true;

        EditorUtility.SetDirty(gameAudio);
        EditorSceneManager.MarkSceneDirty(gameAudio.gameObject.scene);
        EditorSceneManager.SaveScene(gameAudio.gameObject.scene);

        Debug.Log("Dodge Game: audio objects created and scene saved.");
    }

    static AudioSource AddSound(GameObject root, string name, string file, float volume)
    {
        string path = "Assets/Audio/" + file + ".wav";
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);

        if (clip == null)
            Debug.LogError("Dodge Game: missing audio clip " + path);

        GameObject sound = new GameObject(name);
        sound.transform.SetParent(root.transform);

        AudioSource source = sound.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.playOnAwake = false;

        return source;
    }
}
