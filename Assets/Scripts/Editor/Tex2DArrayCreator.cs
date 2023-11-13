using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;

public class Tex2DArrayCreator : EditorWindow
{

    private string fileName;
    private List<Texture2D> textures = new List<Texture2D>();
    private bool mipmapsEnabled = true;
    private ReorderableList reorderableList;

    private Texture2DArray loadTexture2DArray;
    private List<Texture2D> tempLoadedTextures = new List<Texture2D>();

    [MenuItem("Window/Create Texture2DArray")]
    static void Init()
    {
        // Get/Create EditorWindow
        Tex2DArrayCreator window = (Tex2DArrayCreator)GetWindow(typeof(Tex2DArrayCreator));
        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Load Existing Texture2DArray Asset", EditorStyles.boldLabel);
        // Load
        loadTexture2DArray = (Texture2DArray)EditorGUILayout.ObjectField(loadTexture2DArray, typeof(Texture2DArray), false);
        if (GUILayout.Button("Load") && loadTexture2DArray != null)
        {
            if (textures.Count != 0)
            {
                if (!EditorUtility.DisplayDialog("Load Texture2DArray",
                    "Warning : This will override textures in the list!",
                    "Load!", "Cancel!"))
                {
                    return;
                }
            }
            LoadTexturesFromTex2DArray();
        }

        GUILayout.Space(5);
        EditorGUILayout.LabelField("Texture Array Slices", EditorStyles.boldLabel);
        // Texture List
        reorderableList.DoLayoutList();

        // Settings
        GUILayout.Space(5);
        GUILayout.Label("Save Texture2DArray Asset", EditorStyles.boldLabel);
        fileName = EditorGUILayout.TextField("File Name", fileName);
        mipmapsEnabled = GUILayout.Toggle(mipmapsEnabled, "Mip Maps Enabled?");

        // Save
        if (GUILayout.Button("Save (in Assets)") && textures.Count > 0)
        {
            SaveTexture2DArray();
        }

        if (loadTexture2DArray != null)
        {
            if (GUILayout.Button("Override Existing Texture2DArray Asset") && textures.Count > 0)
            {
                if (!EditorUtility.DisplayDialog("Override Existing Texture2DArray Asset",
                    "Warning : Are you sure you want to override the existing Texture2DArray asset?",
                    "Override!", "Cancel!"))
                {
                    return;
                }
                SaveTexture2DArray(true);
            }
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorStyles.label.wordWrap = true;
        EditorGUILayout.LabelField("Note : The first texture is used to determine the size and format of the array. " +
            "All textures must have same width/height, same mipmap count, and use the same format " +
            "/ compression type to correctly handle copying! (Crunch compression not supported)", EditorStyles.label);
    }

    private void SaveTexture2DArray(bool overrideLoadedAsset = false)
    {
        Texture2D tex0 = textures[0];

        UnityEngine.Experimental.Rendering.TextureCreationFlags flags = mipmapsEnabled ?
            UnityEngine.Experimental.Rendering.TextureCreationFlags.MipChain :
            UnityEngine.Experimental.Rendering.TextureCreationFlags.None;

        int mipmapCount = mipmapsEnabled ? tex0.mipmapCount : 1;

        Texture2DArray tex2dArray = new Texture2DArray(tex0.width, tex0.height, textures.Count, tex0.graphicsFormat, flags, mipmapCount);
        for (int i = 0; i < textures.Count; i++)
        {
            Texture2D tex = textures[i];

            if (!mipmapsEnabled)
            {
                // Copy only Mip0
                Graphics.CopyTexture(tex, 0, 0, tex2dArray, i, 0);
            }
            else
            {
                // Copy all Mips
                Graphics.CopyTexture(tex, 0, tex2dArray, i);
            }
        }

        string assetPath = "Assets/" + Path.GetFileNameWithoutExtension(fileName) + ".asset";
        Object existingAsset;
        if (overrideLoadedAsset)
        {
            existingAsset = loadTexture2DArray;
            if (existingAsset == null)
            {
                Debug.LogError("Attempted to override existing Texture2DArray asset, but it is null?");
            }
        }
        else
        {
            existingAsset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (existingAsset != null)
            {
                if (!EditorUtility.DisplayDialog("Save Texture2DArray",
                    "Warning : Asset with that name already exists, override it?",
                    "Override!", "Cancel!"))
                {
                    return;
                }
            }
        }

        if (existingAsset == null)
        {
            AssetDatabase.CreateAsset(tex2dArray, assetPath);
        }
        else
        {
            EditorUtility.CopySerialized(tex2dArray, existingAsset);
        }
        AssetDatabase.SaveAssets();
    }

    private void LoadTexturesFromTex2DArray()
    {
        CleanupTempTextures();

        int width = loadTexture2DArray.width;
        int height = loadTexture2DArray.height;
        UnityEngine.Experimental.Rendering.GraphicsFormat graphicsFormat = loadTexture2DArray.graphicsFormat;
        int mipMapCount = loadTexture2DArray.mipmapCount;
        UnityEngine.Experimental.Rendering.TextureCreationFlags flags = (loadTexture2DArray.mipmapCount > 1) ?
            UnityEngine.Experimental.Rendering.TextureCreationFlags.MipChain :
            UnityEngine.Experimental.Rendering.TextureCreationFlags.None;

        textures.Clear();
        for (int i = 0; i < loadTexture2DArray.depth; i++)
        {
            Texture2D temp = new Texture2D(width, height, graphicsFormat, mipMapCount, flags);
            Graphics.CopyTexture(loadTexture2DArray, i, temp, 0);
            tempLoadedTextures.Add(temp);
            textures.Add(temp);
        }
    }

    private void CleanupTempTextures()
    {
        for (int i = tempLoadedTextures.Count - 1; i >= 0; i--)
        {
            DestroyImmediate(tempLoadedTextures[i]);
        }
        tempLoadedTextures.Clear();
    }

    private void OnEnable()
    {
        // Create Reorderable List
        reorderableList = new ReorderableList(textures, typeof(Texture2D));
        reorderableList.elementHeight = 52;
        reorderableList.drawHeaderCallback = DrawHeader;
        reorderableList.drawElementCallback = DrawElement;
        reorderableList.onAddCallback = OnAdd;
        reorderableList.onRemoveCallback = OnRemove;
    }

    private void OnDisable()
    {
        CleanupTempTextures();
    }

    // List Callbacks

    private void DrawHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, "Textures");
    }

    private void DrawElement(Rect rect, int index, bool active, bool focus)
    {
        Rect r = new Rect(rect.x, rect.y, 50, 50);
        Texture2D tex = textures[index];
        textures[index] = (Texture2D)EditorGUI.ObjectField(r, tex, typeof(Texture2D), false);

        if (tex != null)
        {
            r = new Rect(rect.x + 52, rect.y, rect.width - 52, 15);
            EditorGUI.LabelField(r, "Width : " + tex.width + " Height : " + tex.height);
            r = new Rect(rect.x + 52, rect.y + 15, rect.width - 52, 15);
            EditorGUI.LabelField(r, "Mipmap Count : " + tex.mipmapCount);
            r = new Rect(rect.x + 52, rect.y + 30, rect.width - 52, 15);
            EditorGUI.LabelField(r, "Format : " + tex.format);
        }
    }

    private void OnAdd(ReorderableList list)
    {
        textures.Add(Texture2D.whiteTexture);
    }

    private void OnRemove(ReorderableList list)
    {
        textures.RemoveAt(list.index);
    }

}