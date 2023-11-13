using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GenerateSlpat : MonoBehaviour
{
    NeighboringSplat neighboringSplat = new NeighboringSplat();

#if UNITY_EDITOR
    public void GenerateSplatMaps()
    {
        Terrain terrain = this.GetComponent<Terrain>();
        if (terrain == null)
            return;

        Texture2D[] alphamaps = terrain.terrainData.alphamapTextures;
        if (alphamaps == null)
            return;

        foreach ( Texture2D alphamap in alphamaps )
        {
            byte[] bytes = alphamap.EncodeToTGA();
            System.IO.File.WriteAllBytes("Assets/" + alphamap.name + ".tga", bytes);
        }

        UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceUpdate);
    }


    public void GenerateNeighboringSplatMaps()
    {
        Terrain terrain = this.GetComponent<Terrain>();
        if (terrain == null)
            return;

        (Texture2D alphaMap, Texture2D idMap) = neighboringSplat.ProcessSplatMap(terrain.terrainData);

        byte[] alphaBytes = alphaMap.EncodeToTGA();
        System.IO.File.WriteAllBytes("Assets/AlphaMap.tga", alphaBytes);

        byte[] IdBytes = idMap.EncodeToTGA();
        System.IO.File.WriteAllBytes("Assets/IdMap.tga", IdBytes);

        UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceUpdate);
    }

    public void GenerateTestMaps()
    {
        const int pixelNum = 8;

        Color[] testBuffer = new Color[pixelNum];
        testBuffer[0] = Color.red;
        testBuffer[1] = Color.blue;
        testBuffer[2] = Color.blue;
        testBuffer[3] = Color.cyan;
        testBuffer[4] = Color.cyan;
        testBuffer[5] = Color.green;
        testBuffer[6] = Color.green;
        testBuffer[7] = Color.green;
        Texture2D testMap = new Texture2D(pixelNum, 1, TextureFormat.RGB24, false);
        testMap.filterMode = FilterMode.Bilinear;
        testMap.anisoLevel = 0;
        testMap.SetPixels(testBuffer);
        testMap.Apply(false);
        byte[] testBytes = testMap.EncodeToTGA();
        System.IO.File.WriteAllBytes("Assets/MyTestMap.tga", testBytes);

        Color[] testBuffer2 = new Color[pixelNum/2];
        testBuffer2[0] = Color.red;
        testBuffer2[1] = Color.blue;
        testBuffer2[2] = Color.cyan;
        testBuffer2[3] = Color.green;
        Texture2D testMap2 = new Texture2D(pixelNum/2, 1, TextureFormat.RGB24, false);
        testMap2.filterMode = FilterMode.Bilinear;
        testMap2.anisoLevel = 0;
        testMap2.SetPixels(testBuffer2);
        testMap2.Apply(false);
        byte[] testBytes2 = testMap2.EncodeToTGA();
        System.IO.File.WriteAllBytes("Assets/MyTestMap2.tga", testBytes2);

        UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceUpdate);
    }
#endif
}
