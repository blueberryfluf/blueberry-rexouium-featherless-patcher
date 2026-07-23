using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;

/// <summary>
/// blueberry's Rexouium Featherless fox patch — fun presets + QoL radials.
/// Keeps fluff. Requires VRCFury (already used by RoyaltyFT).
/// </summary>
public class RexFeatherlessPatcher : EditorWindow
{
    const string AuthorName = "blueberry";
    const string Version = "0.1";
    const string GithubUrl = "https://github.com/blueberryfluf";
    const string DiscordUrl = "https://discord.gg/2s94q7hebm";
    const string InstagramUrl = "https://www.instagram.com/blueberry_fluf/";

    const string RootFolder = "Assets/Blueberrys_Flufs_Patcher/Featherless";
    const string AnimFolder = RootFolder + "/Animations";
    const string MenuFolder = RootFolder + "/Menus";
    const string BodyPath = "Body";
    const string ChildName = "BlueBerry Fox Patch";

    const string ParamsPath = RootFolder + "/BB_Fox_Params.asset";
    const string FxPath = RootFolder + "/BB_Fox_FX.controller";
    const string RootMenuPath = MenuFolder + "/BB_Fox_Root.asset";

    // Params
    const string PMood = "BBMood";
    const string PStare = "BBStare";
    const string PDerp = "BBDerp";
    const string PFur = "BBFur";
    const string PFace = "BBFace";
    const string PTail = "BBTail";
    const string PTailPose = "BBTailPose";
    const string PPuff = "BBPuff";
    const string PBreath = "BBBreath";
    const string PNose = "BBNose";
    const string PWeight = "BBWeight";
    const string PMuscle = "BBMuscle";
    const string PBreasts = "BBBreasts";
    const string PFeminine = "BBFeminine";
    const string PPaw = "BBPaw";
    const string PBreathSpd = "BBBreathSpd";
    const string PWagSpd = "BBWagSpd";
    const string PNormal = "BBNormal";

    const string StockTailUp = "Assets/Resources/Avatars/Canines/Rexouium/Animations/Tail Up.anim";
    const string StockTailTucked = "Assets/Resources/Avatars/Canines/Rexouium/Animations/Tail Tucked.anim";
    const string TailPhysPath = "Physbones/Tail";
    const string TailCollisionPhysPath = "Physbones/TailCollision";

    // Rest root ~-130. Tail Up ~-117 (toward head). Tucked curls under legs → tip toward face when prone.
    // Lower pins the chain aft / toward the feet so it stays out of the face while laying.
    static readonly (string path, Vector3 euler)[] TailLowerBones =
    {
        ("Armature/Hips/tailroot", new Vector3(-168f, 4f, -2f)),
        ("Armature/Hips/tailroot/tail1", new Vector3(6f, 3f, -2f)),
        ("Armature/Hips/tailroot/tail1/tail2", new Vector3(-18f, 0f, 0f)),
        ("Armature/Hips/tailroot/tail1/tail2/tail3", new Vector3(-14f, 0f, 0f)),
        ("Armature/Hips/tailroot/tail1/tail2/tail3/tail3.001", new Vector3(-12f, 0f, 0f)),
        ("Armature/Hips/tailroot/tail1/tail2/tail3/tail3.001/tail4", new Vector3(-14f, 0f, 0f)),
        ("Armature/Hips/tailroot/tail1/tail2/tail3/tail3.001/tail4/tail4.001", new Vector3(-10f, 0f, 0f)),
        ("Armature/Hips/tailroot/tail1/tail2/tail3/tail3.001/tail4/tail4.001/tail5", new Vector3(-12f, 0f, 0f)),
        ("Armature/Hips/tailroot/tail1/tail2/tail3/tail3.001/tail4/tail4.001/tail5/tail5.001", new Vector3(-6f, 0f, 0f)),
    };

    static readonly string[] FurShapes =
    {
        "HeadFur", "HeadFur2", "CheekFurUp", "CheekFurUp2", "CheekFurDown", "CheekFurDown2",
        "ChinFur", "NeckFur1", "NeckFur2", "NeckFurUpward", "NeckRuffSize",
        "ShoulderFur", "ElbowFur", "ChestFur", "ChestFur2", "BackFur", "BackFurUp",
        "StomachFur", "HipFur", "ButtFur", "LegFur", "LowerLegFur", "TailFur", "TailFurLong"
    };

    string status = "Pick RexouiumFT Featherless, then Patch Avatar.";
    GameObject targetAvatar;
    Vector2 scroll;
    Texture2D iconCommunity;
    const string IconsFolder = "Assets/Blueberrys_Flufs_Patcher/Editor/Icons";

    [MenuItem("Tools/BlueBerry/Rexouium Featherless Fox Patch")]
    public static void ShowWindow()
    {
        var w = GetWindow<RexFeatherlessPatcher>("Featherless Fox Patch v" + Version);
        w.minSize = new Vector2(520, 640);
        w.Show();
    }

    void OnEnable()
    {
        iconCommunity = AssetDatabase.LoadAssetAtPath<Texture2D>(IconsFolder + "/community.png");
    }

    void OnGUI()
    {
        var title = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 16,
            wordWrap = true
        };
        var credit = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 12,
            fontStyle = FontStyle.Bold
        };

        GUILayout.Space(8);
        GUILayout.Label("blueberry's Featherless Fox Patch", title);
        GUI.color = new Color(0.55f, 0.85f, 1f);
        GUILayout.Label("by " + AuthorName + "   ·   v" + Version, credit);
        GUI.color = Color.white;

        EditorGUILayout.HelpBox(
            "UNOFFICIAL community patch — not affiliated with Rexouium / RoyaltyFT authors.\n" +
            "Keeps fluff. Adds fun presets + QoL radials via VRCFury Full Controller.",
            MessageType.None);

        GUILayout.Space(6);
        targetAvatar = (GameObject)EditorGUILayout.ObjectField(
            new GUIContent("Target Avatar", "RexouiumFT - Featherless root"),
            targetAvatar, typeof(GameObject), true);

        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MinHeight(220));
        EditorGUILayout.HelpBox(
            "FUN\n" +
            "• Normal — one button puts everything back to defaults\n" +
            "• Moods: Happy / Smug / Scowl / Blep\n" +
            "• Stare: Prey Panic / Predator\n" +
            "• Puff & Fluff emote (keeps fluff)\n" +
            "• Tail: Rest / Lower (lay) / Up / Tucked + shapes + wag\n" +
            "• Derp: Wa / Niyari / Pero / Wao?!\n\n" +
            "QoL\n" +
            "• Fur: Stock / Soft / Fluffy / Sleek\n" +
            "• Body radials: Weight, Muscle, Breasts, Feminine, PawSize\n" +
            "• Face: Muzzle Short / Long / Thin\n" +
            "• Idle: Breathing on/off + speed, NoseTwitch on/off\n\n" +
            "Menu path after patch: blueberry fox / …",
            MessageType.Info);
        EditorGUILayout.EndScrollView();

        GUILayout.Space(8);
        if (GUILayout.Button("1. Build / Refresh Patch Assets", GUILayout.Height(32)))
        {
            try
            {
                EnsureAllAssets();
                status = "Assets ready under " + RootFolder;
            }
            catch (System.Exception e)
            {
                status = "Build failed: " + e.Message;
                Debug.LogException(e);
            }
        }

        var desc = targetAvatar != null ? targetAvatar.GetComponentInChildren<VRCAvatarDescriptor>(true) : null;
        using (new EditorGUI.DisabledScope(desc == null))
        {
            GUI.backgroundColor = desc != null ? new Color(0.35f, 0.75f, 0.45f) : Color.gray;
            if (GUILayout.Button("2. Patch Avatar", GUILayout.Height(44)))
            {
                try { status = PatchAvatar(targetAvatar); }
                catch (System.Exception e)
                {
                    status = "Patch failed: " + e.Message;
                    Debug.LogException(e);
                }
            }
            GUI.backgroundColor = Color.white;
        }

        GUILayout.Space(6);
        bool ok = status != null && status.IndexOf("applied", System.StringComparison.OrdinalIgnoreCase) >= 0;
        bool fail = status != null && status.IndexOf("fail", System.StringComparison.OrdinalIgnoreCase) >= 0;
        EditorGUILayout.HelpBox(status, fail ? MessageType.Error : ok ? MessageType.Info : MessageType.None);

        GUILayout.FlexibleSpace();
        DrawSocialFooter();
    }

    void DrawSocialFooter()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            if (iconCommunity != null)
                GUILayout.Label(iconCommunity, GUILayout.Width(22), GUILayout.Height(22));
            if (GUILayout.Button("GitHub", GUILayout.Width(70))) Application.OpenURL(GithubUrl);
            if (GUILayout.Button("Discord", GUILayout.Width(70))) Application.OpenURL(DiscordUrl);
            if (GUILayout.Button("Instagram", GUILayout.Width(80))) Application.OpenURL(InstagramUrl);
            GUILayout.FlexibleSpace();
        }
    }

    // -------------------------------------------------------------------------
    // Patch
    // -------------------------------------------------------------------------

    static string PatchAvatar(GameObject root)
    {
        if (root == null) throw new System.Exception("Assign the Featherless avatar root.");
        var desc = root.GetComponentInChildren<VRCAvatarDescriptor>(true);
        if (desc == null) throw new System.Exception("No VRCAvatarDescriptor on that object.");

        EnsureAllAssets();

        var menu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(RootMenuPath);
        var prms = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(ParamsPath);
        var fx = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(FxPath);
        if (menu == null || prms == null || fx == null)
            throw new System.Exception("Patch assets missing — click Build first.");

        // Remove previous install
        var existing = desc.transform.Find(ChildName);
        if (existing != null)
            Undo.DestroyObjectImmediate(existing.gameObject);

        var go = new GameObject(ChildName);
        Undo.RegisterCreatedObjectUndo(go, "Add BlueBerry Fox Patch");
        go.transform.SetParent(desc.transform, false);

        AttachVrcFuryFullController(go, menu, prms, fx);

        // Optional: drive stock TailWag / Breathing speed if those layers exist on FX
        TryHookStockSpeeds(desc);

        EditorUtility.SetDirty(desc.gameObject);
        AssetDatabase.SaveAssets();
        return "Patch v" + Version + " applied on \"" + desc.gameObject.name +
               "\". Upload / enter Play Mode. Open menu: blueberry fox";
    }

    // VRCFury's public API asmdef has autoReferenced=false — call it via reflection.
    static void AttachVrcFuryFullController(
        GameObject go,
        VRCExpressionsMenu menu,
        VRCExpressionParameters prms,
        RuntimeAnimatorController fx)
    {
        var furyComponents = FindType("com.vrcfury.api.FuryComponents", "com.vrcfury.api");
        if (furyComponents == null)
            throw new Exception("VRCFury API not found. Install / enable VRCFury (RoyaltyFT needs it).");

        var create = furyComponents.GetMethod("CreateFullController", BindingFlags.Public | BindingFlags.Static);
        if (create == null)
            throw new Exception("VRCFury CreateFullController missing — update VRCFury?");

        var fc = create.Invoke(null, new object[] { go });
        var fcType = fc.GetType();
        fcType.GetMethod("AddMenu")?.Invoke(fc, new object[] { menu, "blueberry fox" });
        fcType.GetMethod("AddParams")?.Invoke(fc, new object[] { prms });

        var addController = fcType.GetMethod("AddController");
        if (addController == null)
            throw new Exception("VRCFury AddController missing — update VRCFury?");
        addController.Invoke(fc, new object[] { fx, VRCAvatarDescriptor.AnimLayerType.FX });
    }

    static Type FindType(string fullName, string assemblyName)
    {
        var typed = Type.GetType(fullName + ", " + assemblyName);
        if (typed != null) return typed;
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!string.Equals(asm.GetName().Name, assemblyName, StringComparison.Ordinal)) continue;
            typed = asm.GetType(fullName);
            if (typed != null) return typed;
        }
        return null;
    }

    static Type PhysBoneType()
    {
        return FindType(
                   "VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone",
                   "VRC.SDK3.Dynamics.PhysBone")
               ?? FindType(
                   "VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone",
                   "VRC.Dynamics");
    }

    static void TryHookStockSpeeds(VRCAvatarDescriptor desc)
    {
        // Best-effort: if descriptor FX points at Mod FT controller, enable speed params.
        if (desc.baseAnimationLayers == null) return;
        foreach (var layer in desc.baseAnimationLayers)
        {
            if (layer.type != VRCAvatarDescriptor.AnimLayerType.FX) continue;
            var ctrl = layer.animatorController as AnimatorController;
            if (ctrl == null) continue;
            EnsureControllerParam(ctrl, PBreathSpd, AnimatorControllerParameterType.Float, 1f);
            EnsureControllerParam(ctrl, PWagSpd, AnimatorControllerParameterType.Float, 1f);
            foreach (var l in ctrl.layers)
            {
                if (l.name == "Breathing")
                    EnableStateSpeed(l.stateMachine, "Breathing", PBreathSpd);
                if (l.name == "TailWag")
                    EnableStateSpeed(l.stateMachine, "TailWag On", PWagSpd);
            }
            EditorUtility.SetDirty(ctrl);
        }
    }

    static void EnableStateSpeed(AnimatorStateMachine sm, string stateName, string speedParam)
    {
        if (sm == null) return;
        foreach (var cs in sm.states)
        {
            if (cs.state != null && cs.state.name == stateName)
            {
                cs.state.speedParameterActive = true;
                cs.state.speedParameter = speedParam;
                cs.state.speed = 1f;
            }
        }
        foreach (var sub in sm.stateMachines)
            EnableStateSpeed(sub.stateMachine, stateName, speedParam);
    }

    // -------------------------------------------------------------------------
    // Asset build
    // -------------------------------------------------------------------------

    static void EnsureAllAssets()
    {
        EnsureFolder("Assets/Blueberrys_Flufs_Patcher");
        EnsureFolder(RootFolder);
        EnsureFolder(AnimFolder);
        EnsureFolder(MenuFolder);

        BuildClips();
        BuildParams();
        BuildFx();
        BuildMenus();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        var parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        var name = Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }

    static AnimationClip EnsureClip(string name)
    {
        string path = AnimFolder + "/" + name + ".anim";
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip != null) return clip;
        clip = new AnimationClip { name = name };
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    static void SetShape(AnimationClip clip, string shape, float value)
    {
        var binding = new EditorCurveBinding
        {
            path = BodyPath,
            type = typeof(SkinnedMeshRenderer),
            propertyName = "blendShape." + shape
        };
        var curve = AnimationCurve.Constant(0f, 1f / 60f, value);
        AnimationUtility.SetEditorCurve(clip, binding, curve);
    }

    static void SetShapes(AnimationClip clip, Dictionary<string, float> map)
    {
        foreach (var kv in map)
            SetShape(clip, kv.Key, kv.Value);
    }

    static void ClearClipCurves(AnimationClip clip)
    {
        foreach (var b in AnimationUtility.GetCurveBindings(clip))
            AnimationUtility.SetEditorCurve(clip, b, null);
    }

    static void BuildClips()
    {
        // Moods
        WriteHold("Mood_Off", new Dictionary<string, float>());
        WriteHold("Mood_Happy", new Dictionary<string, float> { { "smile", 80f }, { "HappyBlink", 15f } });
        WriteHold("Mood_Smug", new Dictionary<string, float> { { "EyeShapeSmug", 100f }, { "smile", 35f } });
        WriteHold("Mood_Scowl", new Dictionary<string, float> { { "Scowl", 100f }, { "BrowFurrowL", 60f }, { "BrowFurrowR", 60f } });
        WriteHold("Mood_Blep", new Dictionary<string, float> { { "Tongue_Out", 85f }, { "smile", 40f } });

        // Stare
        WriteHold("Stare_Off", new Dictionary<string, float>());
        WriteHold("Stare_Prey", new Dictionary<string, float> { { "EyesWide", 100f }, { "PupilDilate", 100f } });
        WriteHold("Stare_Predator", new Dictionary<string, float> { { "EyesWide", 40f }, { "PupilSlit", 100f }, { "Scowl", 25f } });

        // Puff & fluff (KEEP fluff — boost fur, don't remove)
        var puff = new Dictionary<string, float> { { "PuffedFeathers", 100f } };
        foreach (var s in new[] { "HeadFur", "CheekFurUp", "NeckRuffSize", "ChestFur", "BackFur", "ShoulderFur" })
            puff[s] = 75f;
        WriteHold("Puff_Off", new Dictionary<string, float> { { "PuffedFeathers", 0f } });
        WriteHold("Puff_On", puff);

        // Derp / MMD
        WriteHold("Derp_Off", new Dictionary<string, float>());
        WriteHold("Derp_Wa", new Dictionary<string, float> { { "Wa", 100f } });
        WriteHold("Derp_Niyari", new Dictionary<string, float> { { "Niyari", 100f } });
        WriteHold("Derp_Pero", new Dictionary<string, float> { { "Pero", 100f } });
        WriteHold("Derp_Wao", new Dictionary<string, float> { { "Wao?!", 100f } });

        // Tail shape
        WriteHold("Tail_Off", new Dictionary<string, float>
        {
            { "TailThick", 0f }, { "TailPlume", 0f }, { "TailThin", 0f }
        });
        WriteHold("Tail_Thick", new Dictionary<string, float>
        {
            { "TailThick", 100f }, { "TailPlume", 0f }, { "TailThin", 0f }
        });
        WriteHold("Tail_Plume", new Dictionary<string, float>
        {
            { "TailThick", 0f }, { "TailPlume", 100f }, { "TailThin", 0f }
        });
        WriteHold("Tail_Thin", new Dictionary<string, float>
        {
            { "TailThick", 0f }, { "TailPlume", 0f }, { "TailThin", 100f }
        });

        // Fur density — Normal leaves stock fluff alone (no forced zeros)
        WriteFurNormal();
        WriteFur("Fur_Soft", 35f);
        WriteFur("Fur_Fluffy", 90f);
        WriteFur("Fur_Sleek", 5f);

        // Face muzzle
        WriteHold("Face_Off", new Dictionary<string, float>
        {
            { "MuzzleShort", 0f }, { "MuzzleLong", 0f }, { "MuzzleThin", 0f }
        });
        WriteHold("Face_Short", new Dictionary<string, float>
        {
            { "MuzzleShort", 100f }, { "MuzzleLong", 0f }, { "MuzzleThin", 0f }
        });
        WriteHold("Face_Long", new Dictionary<string, float>
        {
            { "MuzzleShort", 0f }, { "MuzzleLong", 100f }, { "MuzzleThin", 0f }
        });
        WriteHold("Face_Thin", new Dictionary<string, float>
        {
            { "MuzzleShort", 0f }, { "MuzzleLong", 0f }, { "MuzzleThin", 100f }
        });

        // Body ends for blendtrees
        WriteHold("Body_Weight_0", new Dictionary<string, float> { { "Weight", 0f } });
        WriteHold("Body_Weight_100", new Dictionary<string, float> { { "Weight", 100f } });
        WriteHold("Body_Muscle_0", new Dictionary<string, float> { { "Muscle", 0f } });
        WriteHold("Body_Muscle_100", new Dictionary<string, float> { { "Muscle", 100f } });
        WriteHold("Body_Breasts_0", new Dictionary<string, float> { { "Breasts", 0f } });
        WriteHold("Body_Breasts_100", new Dictionary<string, float> { { "Breasts", 100f } });
        WriteHold("Body_Feminine_0", new Dictionary<string, float> { { "FeminineBody", 0f } });
        WriteHold("Body_Feminine_100", new Dictionary<string, float> { { "FeminineBody", 100f } });
        WriteHold("Body_Paw_0", new Dictionary<string, float> { { "PawSize", 0f } });
        WriteHold("Body_Paw_100", new Dictionary<string, float> { { "PawSize", 100f } });

        // Breathing loop (2s)
        var breath = EnsureClip("Idle_Breathing");
        ClearClipCurves(breath);
        var bBind = new EditorCurveBinding
        {
            path = BodyPath,
            type = typeof(SkinnedMeshRenderer),
            propertyName = "blendShape.Breathing"
        };
        var bCurve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(1f, 100f),
            new Keyframe(2f, 0f));
        AnimationUtility.SetEditorCurve(breath, bBind, bCurve);
        var settings = AnimationUtility.GetAnimationClipSettings(breath);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(breath, settings);
        EditorUtility.SetDirty(breath);

        // Nose twitch — reuse pattern from stock if possible; simple L/R wiggle
        var nose = EnsureClip("Idle_NoseTwitch");
        ClearClipCurves(nose);
        SetShapeAnimated(nose, "NoseTwitchL", new[] { (0f, 0f), (0.15f, 80f), (0.3f, 0f), (0.8f, 0f), (1.2f, 0f) });
        SetShapeAnimated(nose, "NoseTwitchR", new[] { (0f, 0f), (0.4f, 0f), (0.55f, 80f), (0.7f, 0f), (1.2f, 0f) });
        var ns = AnimationUtility.GetAnimationClipSettings(nose);
        ns.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(nose, ns);
        EditorUtility.SetDirty(nose);

        WriteHold("Idle_Off", new Dictionary<string, float>());

        // Tail pose: Lower = aft/flat (not tucked-under) + physbones off so prone doesn't flop into face
        WriteTailPoseOff();
        WriteTailPoseLower();
        WriteTailPoseUp();
        WriteTailPoseTucked();
    }

    static void WriteFurNormal()
    {
        // Empty of fur curves → Write Defaults restores prefab fluff (like before Soft/Fluffy/Sleek)
        var clip = EnsureClip("Fur_Normal");
        ClearClipCurves(clip);
        SetShape(clip, "Breathing", 0f);
        EditorUtility.SetDirty(clip);
    }

    static void WriteTailPoseOff()
    {
        var clip = EnsureClip("Tail_Pose_Off");
        ClearClipCurves(clip);
        SetPhysBoneEnabled(clip, TailPhysPath, true);
        SetPhysBoneEnabled(clip, TailCollisionPhysPath, true);
        EditorUtility.SetDirty(clip);
    }

    static void WriteTailPoseLower()
    {
        var clip = EnsureClip("Tail_Pose_Lower");
        ClearClipCurves(clip);
        foreach (var bone in TailLowerBones)
            SetLocalEuler(clip, bone.path, bone.euler);
        // Kill physbone so gravity can't pull the tip into the face while prone
        SetPhysBoneEnabled(clip, TailPhysPath, false);
        SetPhysBoneEnabled(clip, TailCollisionPhysPath, false);
        EditorUtility.SetDirty(clip);
    }

    static void WriteTailPoseUp()
    {
        var clip = EnsureClip("Tail_Pose_Up");
        ClearClipCurves(clip);
        // Prefer stock Tail Up bone curves; fall back to a mild raise if missing
        var stock = AssetDatabase.LoadAssetAtPath<AnimationClip>(StockTailUp);
        if (stock != null)
        {
            foreach (var b in AnimationUtility.GetCurveBindings(stock))
            {
                if (b.type != typeof(Transform)) continue;
                var c = AnimationUtility.GetEditorCurve(stock, b);
                if (c != null) AnimationUtility.SetEditorCurve(clip, b, c);
            }
        }
        else
        {
            SetLocalEuler(clip, "Armature/Hips/tailroot", new Vector3(-117.5f, 10f, -7.7f));
            SetLocalEuler(clip, "Armature/Hips/tailroot/tail1", new Vector3(20.8f, 3.5f, -2.4f));
        }
        SetPhysBoneEnabled(clip, TailPhysPath, false);
        SetPhysBoneEnabled(clip, TailCollisionPhysPath, false);
        EditorUtility.SetDirty(clip);
    }

    static void WriteTailPoseTucked()
    {
        // Stock tucked + physbones ON so the fluff settles / normalizes like before Lower(lay)
        var clip = EnsureClip("Tail_Pose_Tucked");
        ClearClipCurves(clip);
        var stock = AssetDatabase.LoadAssetAtPath<AnimationClip>(StockTailTucked);
        if (stock != null)
        {
            foreach (var b in AnimationUtility.GetCurveBindings(stock))
            {
                if (b.type != typeof(Transform)) continue;
                var c = AnimationUtility.GetEditorCurve(stock, b);
                if (c != null) AnimationUtility.SetEditorCurve(clip, b, c);
            }
        }
        SetPhysBoneEnabled(clip, TailPhysPath, true);
        SetPhysBoneEnabled(clip, TailCollisionPhysPath, true);
        EditorUtility.SetDirty(clip);
    }

    static void SetLocalEuler(AnimationClip clip, string path, Vector3 euler)
    {
        void Axis(string axis, float value)
        {
            var binding = new EditorCurveBinding
            {
                path = path,
                type = typeof(Transform),
                propertyName = "localEulerAnglesRaw." + axis
            };
            AnimationUtility.SetEditorCurve(clip, binding, AnimationCurve.Constant(0f, 0.0167f, value));
        }
        Axis("x", euler.x);
        Axis("y", euler.y);
        Axis("z", euler.z);
    }

    static void SetPhysBoneEnabled(AnimationClip clip, string path, bool enabled)
    {
        var pbType = PhysBoneType();
        if (pbType == null)
        {
            Debug.LogWarning("[Featherless Fox] VRCPhysBone type not found — Lower won't disable physbones.");
            return;
        }

        var binding = new EditorCurveBinding
        {
            path = path,
            type = pbType,
            propertyName = "m_Enabled"
        };
        AnimationUtility.SetEditorCurve(clip, binding, AnimationCurve.Constant(0f, 0.0167f, enabled ? 1f : 0f));
    }

    static void SetShapeAnimated(AnimationClip clip, string shape, (float t, float v)[] keys)
    {
        var binding = new EditorCurveBinding
        {
            path = BodyPath,
            type = typeof(SkinnedMeshRenderer),
            propertyName = "blendShape." + shape
        };
        var curve = new AnimationCurve();
        foreach (var k in keys)
            curve.AddKey(new Keyframe(k.t, k.v));
        AnimationUtility.SetEditorCurve(clip, binding, curve);
    }

    static void WriteHold(string clipName, Dictionary<string, float> shapes)
    {
        var clip = EnsureClip(clipName);
        ClearClipCurves(clip);
        SetShapes(clip, shapes);
        // Always touch a harmless zero if empty so clip isn't totally empty
        if (shapes.Count == 0)
            SetShape(clip, "Breathing", 0f);
        EditorUtility.SetDirty(clip);
    }

    static void WriteFur(string clipName, float amount)
    {
        var map = new Dictionary<string, float>();
        foreach (var s in FurShapes)
            map[s] = amount;
        // Fluffy gets extra ruff / cheek; sleek already low
        if (amount >= 80f)
        {
            map["NeckRuffSize"] = 100f;
            map["CheekFurUp"] = 100f;
            map["TailFurLong"] = 85f;
        }
        WriteHold(clipName, map);
    }

    static void BuildParams()
    {
        var prms = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(ParamsPath);
        if (prms == null)
        {
            prms = ScriptableObject.CreateInstance<VRCExpressionParameters>();
            AssetDatabase.CreateAsset(prms, ParamsPath);
        }

        var list = new List<VRCExpressionParameters.Parameter>
        {
            IntParam(PMood), IntParam(PStare), IntParam(PDerp), IntParam(PFur), IntParam(PFace), IntParam(PTail),
            IntParam(PTailPose),
            BoolParam(PPuff), BoolParam(PBreath, true), BoolParam(PNose),
            BoolParam(PNormal, false, saved: false),
            FloatParam(PWeight), FloatParam(PMuscle), FloatParam(PBreasts), FloatParam(PFeminine), FloatParam(PPaw),
            FloatParam(PBreathSpd, 0.5f), FloatParam(PWagSpd, 0.5f),
        };
        prms.parameters = list.ToArray();
        EditorUtility.SetDirty(prms);
    }

    static VRCExpressionParameters.Parameter IntParam(string name) => new VRCExpressionParameters.Parameter
    {
        name = name,
        valueType = VRCExpressionParameters.ValueType.Int,
        defaultValue = 0,
        saved = true,
        networkSynced = true
    };

    static VRCExpressionParameters.Parameter BoolParam(string name, bool defaultOn = false, bool saved = true) => new VRCExpressionParameters.Parameter
    {
        name = name,
        valueType = VRCExpressionParameters.ValueType.Bool,
        defaultValue = defaultOn ? 1f : 0f,
        saved = saved,
        networkSynced = true
    };

    static VRCExpressionParameters.Parameter FloatParam(string name, float def = 0f) => new VRCExpressionParameters.Parameter
    {
        name = name,
        valueType = VRCExpressionParameters.ValueType.Float,
        defaultValue = def,
        saved = true,
        networkSynced = true
    };

    static void BuildFx()
    {
        var fx = AssetDatabase.LoadAssetAtPath<AnimatorController>(FxPath);
        if (fx == null)
        {
            fx = AnimatorController.CreateAnimatorControllerAtPath(FxPath);
        }

        // Clear old layers except base — rebuild clean
        while (fx.layers.Length > 0)
            fx.RemoveLayer(0);

        // Params on controller
        void Need(string n, AnimatorControllerParameterType t, float def = 0f)
        {
            if (fx.parameters.Any(p => p.name == n)) return;
            fx.AddParameter(new AnimatorControllerParameter { name = n, type = t, defaultFloat = def, defaultBool = def > 0.5f, defaultInt = (int)def });
        }

        Need(PMood, AnimatorControllerParameterType.Int);
        Need(PStare, AnimatorControllerParameterType.Int);
        Need(PDerp, AnimatorControllerParameterType.Int);
        Need(PFur, AnimatorControllerParameterType.Int);
        Need(PFace, AnimatorControllerParameterType.Int);
        Need(PTail, AnimatorControllerParameterType.Int);
        Need(PTailPose, AnimatorControllerParameterType.Int);
        Need(PPuff, AnimatorControllerParameterType.Bool);
        Need(PBreath, AnimatorControllerParameterType.Bool);
        Need(PNose, AnimatorControllerParameterType.Bool);
        Need(PNormal, AnimatorControllerParameterType.Bool);
        Need(PWeight, AnimatorControllerParameterType.Float);
        Need(PMuscle, AnimatorControllerParameterType.Float);
        Need(PBreasts, AnimatorControllerParameterType.Float);
        Need(PFeminine, AnimatorControllerParameterType.Float);
        Need(PPaw, AnimatorControllerParameterType.Float);
        Need(PBreathSpd, AnimatorControllerParameterType.Float, 0.5f);
        Need(PWagSpd, AnimatorControllerParameterType.Float, 0.5f);

        AddDefaultResetLayer(fx);
        AddIntLayer(fx, "BB Mood", PMood, new[]
        {
            (0, "Mood_Off"), (1, "Mood_Happy"), (2, "Mood_Smug"), (3, "Mood_Scowl"), (4, "Mood_Blep")
        });
        AddIntLayer(fx, "BB Stare", PStare, new[]
        {
            (0, "Stare_Off"), (1, "Stare_Prey"), (2, "Stare_Predator")
        });
        AddIntLayer(fx, "BB Derp", PDerp, new[]
        {
            (0, "Derp_Off"), (1, "Derp_Wa"), (2, "Derp_Niyari"), (3, "Derp_Pero"), (4, "Derp_Wao")
        });
        AddIntLayer(fx, "BB Fur", PFur, new[]
        {
            (0, "Fur_Normal"), (1, "Fur_Soft"), (2, "Fur_Fluffy"), (3, "Fur_Sleek")
        });
        AddIntLayer(fx, "BB Face", PFace, new[]
        {
            (0, "Face_Off"), (1, "Face_Short"), (2, "Face_Long"), (3, "Face_Thin")
        });
        AddIntLayer(fx, "BB Tail", PTail, new[]
        {
            (0, "Tail_Off"), (1, "Tail_Thick"), (2, "Tail_Plume"), (3, "Tail_Thin")
        });
        AddTailPoseLayer(fx);
        AddBoolLayer(fx, "BB Puff", PPuff, "Puff_Off", "Puff_On");
        AddBreathLayer(fx);
        AddBoolLayer(fx, "BB Nose", PNose, "Idle_Off", "Idle_NoseTwitch", loopOn: true);
        AddBlendLayer(fx, "BB Weight", PWeight, "Body_Weight_0", "Body_Weight_100");
        AddBlendLayer(fx, "BB Muscle", PMuscle, "Body_Muscle_0", "Body_Muscle_100");
        AddBlendLayer(fx, "BB Breasts", PBreasts, "Body_Breasts_0", "Body_Breasts_100");
        AddBlendLayer(fx, "BB Feminine", PFeminine, "Body_Feminine_0", "Body_Feminine_100");
        AddBlendLayer(fx, "BB Paw", PPaw, "Body_Paw_0", "Body_Paw_100");

        EditorUtility.SetDirty(fx);
    }

    static void AddDefaultResetLayer(AnimatorController fx)
    {
        // One-shot: menu Toggle sets BBNormal → driver restores every BB* param (incl. Normal off).
        fx.AddLayer("BB Default");
        var layers = fx.layers;
        var layer = layers[layers.Length - 1];
        layer.defaultWeight = 1f;
        var sm = layer.stateMachine;

        var idle = sm.AddState("Idle", new Vector3(300, 0, 0));
        idle.motion = Clip("Idle_Off");
        idle.writeDefaultValues = true;

        var reset = sm.AddState("Reset Defaults", new Vector3(300, 80, 0));
        reset.motion = Clip("Idle_Off");
        reset.writeDefaultValues = true;

        var driver = reset.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
        driver.localOnly = false;
        driver.parameters = new List<VRC_AvatarParameterDriver.Parameter>
        {
            SetInt(PMood, 0),
            SetInt(PStare, 0),
            SetInt(PDerp, 0),
            SetInt(PFur, 0),
            SetInt(PFace, 0),
            SetInt(PTail, 0),
            SetInt(PTailPose, 0),
            SetBool(PPuff, false),
            SetBool(PBreath, true),
            SetBool(PNose, false),
            SetFloat(PWeight, 0f),
            SetFloat(PMuscle, 0f),
            SetFloat(PBreasts, 0f),
            SetFloat(PFeminine, 0f),
            SetFloat(PPaw, 0f),
            SetFloat(PBreathSpd, 0.5f),
            SetFloat(PWagSpd, 0.5f),
            SetBool(PNormal, false),
        };

        var toReset = sm.AddAnyStateTransition(reset);
        toReset.hasExitTime = false;
        toReset.duration = 0f;
        toReset.canTransitionToSelf = false;
        toReset.AddCondition(AnimatorConditionMode.If, 0, PNormal);

        var back = reset.AddTransition(idle);
        back.hasExitTime = false;
        back.duration = 0f;
        back.AddCondition(AnimatorConditionMode.IfNot, 0, PNormal);

        sm.defaultState = idle;
        fx.layers = layers;
    }

    static VRC_AvatarParameterDriver.Parameter SetInt(string name, int value) =>
        new VRC_AvatarParameterDriver.Parameter
        {
            type = VRC_AvatarParameterDriver.ChangeType.Set,
            name = name,
            value = value
        };

    static VRC_AvatarParameterDriver.Parameter SetBool(string name, bool value) =>
        new VRC_AvatarParameterDriver.Parameter
        {
            type = VRC_AvatarParameterDriver.ChangeType.Set,
            name = name,
            value = value ? 1f : 0f
        };

    static VRC_AvatarParameterDriver.Parameter SetFloat(string name, float value) =>
        new VRC_AvatarParameterDriver.Parameter
        {
            type = VRC_AvatarParameterDriver.ChangeType.Set,
            name = name,
            value = value
        };

    static void AddTailPoseLayer(AnimatorController fx)
    {
        // 0 Normal = physbones on, stock settle. 1 Lower = lay-locked.
        // 2 Up. 3 Tucked = stock tucked + physbones on (fluff normalizes like before).
        AddIntLayer(fx, "BB Tail Pose", PTailPose, new[]
        {
            (0, "Tail_Pose_Off"),
            (1, "Tail_Pose_Lower"),
            (2, "Tail_Pose_Up"),
            (3, "Tail_Pose_Tucked")
        });
    }

    static AnimationClip Clip(string name) =>
        AssetDatabase.LoadAssetAtPath<AnimationClip>(AnimFolder + "/" + name + ".anim");

    static void AddIntLayer(AnimatorController fx, string layerName, string param,
        (int value, string clip)[] states)
    {
        fx.AddLayer(layerName);
        var layers = fx.layers;
        var layer = layers[layers.Length - 1];
        layer.defaultWeight = 1f;
        var sm = layer.stateMachine;
        sm.anyStatePosition = new Vector3(20, 0, 0);
        sm.entryPosition = new Vector3(20, 60, 0);

        AnimatorState defaultState = null;
        for (int i = 0; i < states.Length; i++)
        {
            var s = states[i];
            var st = sm.AddState(s.clip, new Vector3(300, i * 50f, 0));
            st.motion = Clip(s.clip);
            st.writeDefaultValues = true;
            if (s.value == 0) defaultState = st;

            var any = sm.AddAnyStateTransition(st);
            any.hasExitTime = false;
            any.duration = 0.1f;
            any.AddCondition(AnimatorConditionMode.Equals, s.value, param);
        }
        if (defaultState != null) sm.defaultState = defaultState;
        fx.layers = layers;
    }

    static void AddBoolLayer(AnimatorController fx, string layerName, string param,
        string offClip, string onClip, bool loopOn = false)
    {
        _ = loopOn;
        fx.AddLayer(layerName);
        var layers = fx.layers;
        var layer = layers[layers.Length - 1];
        layer.defaultWeight = 1f;
        var sm = layer.stateMachine;

        var off = sm.AddState(offClip, new Vector3(300, 0, 0));
        off.motion = Clip(offClip);
        off.writeDefaultValues = true;
        var on = sm.AddState(onClip, new Vector3(300, 60, 0));
        on.motion = Clip(onClip);
        on.writeDefaultValues = true;

        var toOn = off.AddTransition(on);
        toOn.hasExitTime = false;
        toOn.duration = 0.1f;
        toOn.AddCondition(AnimatorConditionMode.If, 0, param);

        var toOff = on.AddTransition(off);
        toOff.hasExitTime = false;
        toOff.duration = 0.1f;
        toOff.AddCondition(AnimatorConditionMode.IfNot, 0, param);

        sm.defaultState = off;
        fx.layers = layers;
    }

    static void AddBreathLayer(AnimatorController fx)
    {
        fx.AddLayer("BB Breath");
        var layers = fx.layers;
        var layer = layers[layers.Length - 1];
        layer.defaultWeight = 1f;
        var sm = layer.stateMachine;

        var off = sm.AddState("Breath Off", new Vector3(300, 0, 0));
        off.motion = Clip("Idle_Off");
        off.writeDefaultValues = true;

        // Radial 0..1 blends timeScale 0.35 (slow) → 2.0 (fast); VRC floats can't exceed 1.
        var breathClip = Clip("Idle_Breathing");
        var tree = new BlendTree
        {
            name = "Breath Speed Tree",
            blendParameter = PBreathSpd,
            blendType = BlendTreeType.Simple1D,
            useAutomaticThresholds = false
        };
        AssetDatabase.AddObjectToAsset(tree, fx);
        tree.AddChild(breathClip, 0f);
        tree.AddChild(breathClip, 1f);
        var children = tree.children;
        children[0].timeScale = 0.35f;
        children[1].timeScale = 2.0f;
        tree.children = children;

        var on = sm.AddState("Breath On", new Vector3(300, 70, 0));
        on.motion = tree;
        on.writeDefaultValues = true;

        var toOn = off.AddTransition(on);
        toOn.hasExitTime = false;
        toOn.duration = 0.15f;
        toOn.AddCondition(AnimatorConditionMode.If, 0, PBreath);

        var toOff = on.AddTransition(off);
        toOff.hasExitTime = false;
        toOff.duration = 0.15f;
        toOff.AddCondition(AnimatorConditionMode.IfNot, 0, PBreath);

        sm.defaultState = off;
        fx.layers = layers;
    }

    static void AddBlendLayer(AnimatorController fx, string layerName, string param,
        string clip0, string clip1)
    {
        fx.AddLayer(layerName);
        var layers = fx.layers;
        var layer = layers[layers.Length - 1];
        layer.defaultWeight = 1f;
        var sm = layer.stateMachine;

        var tree = new BlendTree
        {
            name = layerName + " Tree",
            blendParameter = param,
            blendType = BlendTreeType.Simple1D,
            useAutomaticThresholds = false
        };
        // Hide blendtree in controller asset
        AssetDatabase.AddObjectToAsset(tree, fx);
        tree.AddChild(Clip(clip0), 0f);
        tree.AddChild(Clip(clip1), 1f);

        var st = sm.AddState(layerName, new Vector3(300, 0, 0));
        st.motion = tree;
        st.writeDefaultValues = true;
        sm.defaultState = st;
        fx.layers = layers;
    }

    static void EnsureControllerParam(AnimatorController ctrl, string name,
        AnimatorControllerParameterType type, float def)
    {
        if (ctrl.parameters.Any(p => p.name == name)) return;
        ctrl.AddParameter(new AnimatorControllerParameter
        {
            name = name,
            type = type,
            defaultFloat = def
        });
    }

    // -------------------------------------------------------------------------
    // Menus
    // -------------------------------------------------------------------------

    static void BuildMenus()
    {
        var moods = BuildToggleMenu("BB_Fox_Moods", PMood, new[]
        {
            ("Happy", 1), ("Smug", 2), ("Scowl", 3), ("Blep", 4), ("Clear", 0)
        });
        var stare = BuildToggleMenu("BB_Fox_Stare", PStare, new[]
        {
            ("Prey Panic", 1), ("Predator", 2), ("Clear", 0)
        });
        var derp = BuildToggleMenu("BB_Fox_Derp", PDerp, new[]
        {
            ("Wa", 1), ("Niyari", 2), ("Pero", 3), ("Wao?!", 4), ("Clear", 0)
        });
        var fur = BuildToggleMenu("BB_Fox_Fur", PFur, new[]
        {
            ("Stock", 0), ("Soft", 1), ("Fluffy", 2), ("Sleek", 3)
        });
        var face = BuildToggleMenu("BB_Fox_Face", PFace, new[]
        {
            ("Muzzle Short", 1), ("Muzzle Long", 2), ("Muzzle Thin", 3), ("Clear", 0)
        });
        var tail = BuildTailMenu();
        var body = BuildBodyMenu();
        var idle = BuildIdleMenu();
        var more = LoadOrCreateMenu("BB_Fox_More");
        more.controls = new List<VRCExpressionsMenu.Control>
        {
            Sub("Body", body),
            Sub("Idle / Face", BuildIdleFaceHub(idle, face)),
        };
        EditorUtility.SetDirty(more);

        var root = LoadOrCreateMenu("BB_Fox_Root");
        root.controls = new List<VRCExpressionsMenu.Control>
        {
            Toggle("Normal", PNormal),
            Sub("Moods", moods),
            Sub("Stare", stare),
            Toggle("Puff & Fluff", PPuff),
            Sub("Derp", derp),
            Sub("Tail", tail),
            Sub("Fur", fur),
            Sub("More", more),
        };
        EditorUtility.SetDirty(root);
    }

    static VRCExpressionsMenu BuildIdleFaceHub(VRCExpressionsMenu idle, VRCExpressionsMenu face)
    {
        var hub = LoadOrCreateMenu("BB_Fox_IdleFace");
        hub.controls = new List<VRCExpressionsMenu.Control>
        {
            Sub("Idle", idle),
            Sub("Face", face),
        };
        EditorUtility.SetDirty(hub);
        return hub;
    }

    static VRCExpressionsMenu BuildTailMenu()
    {
        var pose = LoadOrCreateMenu("BB_Fox_TailPose");
        pose.controls = new List<VRCExpressionsMenu.Control>
        {
            ToggleVal("Rest", PTailPose, 0),
            ToggleVal("Lower (lay)", PTailPose, 1),
            ToggleVal("Up", PTailPose, 2),
            ToggleVal("Tucked", PTailPose, 3),
        };
        EditorUtility.SetDirty(pose);

        var menu = LoadOrCreateMenu("BB_Fox_Tail");
        menu.controls = new List<VRCExpressionsMenu.Control>
        {
            Sub("Pose", pose),
            ToggleVal("Thick", PTail, 1),
            ToggleVal("Plume", PTail, 2),
            ToggleVal("Thin", PTail, 3),
            ToggleVal("Shape Clear", PTail, 0),
            Radial("Wag Speed", PWagSpd),
        };
        EditorUtility.SetDirty(menu);
        return menu;
    }

    static VRCExpressionsMenu BuildBodyMenu()
    {
        var menu = LoadOrCreateMenu("BB_Fox_Body");
        menu.controls = new List<VRCExpressionsMenu.Control>
        {
            Radial("Weight", PWeight),
            Radial("Muscle", PMuscle),
            Radial("Breasts", PBreasts),
            Radial("Feminine", PFeminine),
            Radial("Paw Size", PPaw),
        };
        EditorUtility.SetDirty(menu);
        return menu;
    }

    static VRCExpressionsMenu BuildIdleMenu()
    {
        var menu = LoadOrCreateMenu("BB_Fox_Idle");
        menu.controls = new List<VRCExpressionsMenu.Control>
        {
            Toggle("Breathing", PBreath),
            Radial("Breath Speed", PBreathSpd),
            Toggle("Nose Twitch", PNose),
        };
        EditorUtility.SetDirty(menu);
        return menu;
    }

    static VRCExpressionsMenu BuildToggleMenu(string assetName, string param,
        (string label, int value)[] items)
    {
        var menu = LoadOrCreateMenu(assetName);
        menu.controls = items.Select(i => ToggleVal(i.label, param, i.value)).ToList();
        EditorUtility.SetDirty(menu);
        return menu;
    }

    static VRCExpressionsMenu LoadOrCreateMenu(string assetName)
    {
        string path = MenuFolder + "/" + assetName + ".asset";
        var menu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(path);
        if (menu != null) return menu;
        menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
        AssetDatabase.CreateAsset(menu, path);
        return menu;
    }

    static VRCExpressionsMenu.Control Sub(string name, VRCExpressionsMenu sub) =>
        new VRCExpressionsMenu.Control
        {
            name = name,
            type = VRCExpressionsMenu.Control.ControlType.SubMenu,
            subMenu = sub
        };

    static VRCExpressionsMenu.Control Button(string name, string param) =>
        new VRCExpressionsMenu.Control
        {
            name = name,
            type = VRCExpressionsMenu.Control.ControlType.Button,
            parameter = new VRCExpressionsMenu.Control.Parameter { name = param },
            value = 1f
        };

    static VRCExpressionsMenu.Control Toggle(string name, string param) =>
        new VRCExpressionsMenu.Control
        {
            name = name,
            type = VRCExpressionsMenu.Control.ControlType.Toggle,
            parameter = new VRCExpressionsMenu.Control.Parameter { name = param },
            value = 1f
        };

    static VRCExpressionsMenu.Control ToggleVal(string name, string param, int value) =>
        new VRCExpressionsMenu.Control
        {
            name = name,
            type = VRCExpressionsMenu.Control.ControlType.Toggle,
            parameter = new VRCExpressionsMenu.Control.Parameter { name = param },
            value = value
        };

    static VRCExpressionsMenu.Control Radial(string name, string param) =>
        new VRCExpressionsMenu.Control
        {
            name = name,
            type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
            subParameters = new[]
            {
                new VRCExpressionsMenu.Control.Parameter { name = param }
            }
        };
}
