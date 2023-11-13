using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeighboringSplat
{
    private void setAlpha(byte[] alphaBuffer, int index, int layer, float alpha)
    {
        alphaBuffer[index + layer] = (byte)(alpha * 255.0f);
    }

    private float getAlpha(byte[] alphaBuffer, int index, int layer)
    {
        return alphaBuffer[index + layer] / 255.0f;
    }

    private void setId(byte[] idBuffer, int index, int layer, float id)
    {
        idBuffer[index + layer] = (byte)(id);
    }

    private int getId(byte[] idBuffer, int index, int layer)
    {
        return idBuffer[index + layer];
    }

    public float[,,] GetBilinearAlphamaps(TerrainData terrain, int width, int height)
    {
        int layerNum = terrain.alphamapLayers;

        float[,,] alphas = new float[width, height, layerNum];
        for (int m = 0; m < terrain.alphamapTextureCount; ++m)
        {
            Texture2D tex = terrain.GetAlphamapTexture(m);

            for (int y = 0; y < tex.height; ++y)
            {
                for (int x = 0; x < tex.width; ++x)
                {
                    int l = m * 4;
                    if (l < layerNum)
                        alphas[x, y, l] = tex.GetPixelBilinear((float)x / (float)tex.width, (float)y / (float)tex.height).r;
                    l = m * 4 + 1;
                    if (l < layerNum)
                        alphas[x, y, l] = tex.GetPixelBilinear((float)x / (float)tex.width, (float)y / (float)tex.height).g;
                    l = m * 4 + 2;
                    if (l < layerNum)
                        alphas[x, y, l] = tex.GetPixelBilinear((float)x / (float)tex.width, (float)y / (float)tex.height).b;
                    l = m * 4 + 3;
                    if (l < layerNum)
                        alphas[x, y, l] = tex.GetPixelBilinear((float)x / (float)tex.width, (float)y / (float)tex.height).a;
                }
            }
        }

        return alphas;
    }

    public void CheckAlpha(float[,,] alphas)
    {
        int width = alphas.GetLength(0);
        int height = alphas.GetLength(1);
        int layerNum = alphas.GetLength(2);

        for (int j = 0; j < height; ++j)
        {
            for (int i = 0; i < width; ++i)
            {
                float testSum = 0;
                for (int n = 0; n < layerNum; ++n)
                {
                    testSum += alphas[i, j, n];
                }

                if (testSum < 0.9999f || testSum > 1.0001f)
                {
                    Debug.DebugBreak();
                    Debug.Log(i + "  " + j + "   " + testSum);
                }
            }
        }
    }

    public (Texture2D, Texture2D) ProcessSplatMap(TerrainData terrain, int maxTexelLayerNum = 3)
    {
        int alphaMapW = terrain.alphamapWidth;
        int alphaMapH = terrain.alphamapHeight;

        int maxX = alphaMapW - 1;
        int maxY = alphaMapH - 1;

        int expW = alphaMapW * 2;
        int expH = alphaMapW * 2;

        int layerNum = terrain.alphamapLayers;

        float[,,] alphas = GetBilinearAlphamaps(terrain, alphaMapW, alphaMapH);
        //float[,,] alphas = terrain.GetAlphamaps(0, 0, alphaMapW, alphaMapH);

        byte[] alphaBuffer = new byte[expW * expH * maxTexelLayerNum];
        byte[] idBuffer = new byte[alphaMapW * alphaMapH * maxTexelLayerNum];

        int sy, sx;
        int saveLayer = 0;
        for (int y = 0; y < alphaMapH; ++y)
        {
            for (int x = 0; x < alphaMapW; ++x)
            {
                sx = x * 2;
                sy = y * 2;

                saveLayer = 0;

                int lb = (sx + sy * expW) * maxTexelLayerNum;
                int lt = (sx + (sy + 1) * expW) * maxTexelLayerNum;
                int rb = ((sx + 1) + sy * expW) * maxTexelLayerNum;
                int rt = ((sx + 1) + (sy + 1) * expW) * maxTexelLayerNum;

                int idIndex = (y * alphaMapW + x) * maxTexelLayerNum;

                for (int l = 0; l < layerNum; ++l)
                {
                    float lbAlpha = alphas[x, y, l];
                    float ltAlpha = alphas[x, Mathf.Min(y + 1, maxY), l];
                    float rbAlpha = alphas[Mathf.Min(x + 1, maxX), y, l];
                    float rtAlpha = alphas[Mathf.Min(x + 1, maxX), Mathf.Min(y + 1, maxY), l];

                    float neighboringAlpha = lbAlpha + ltAlpha + rbAlpha + rtAlpha;
                    if (neighboringAlpha > 0.0f)
                    {
                        setAlpha(alphaBuffer, lb, saveLayer, lbAlpha);
                        setAlpha(alphaBuffer, lt, saveLayer, ltAlpha);
                        setAlpha(alphaBuffer, rb, saveLayer, rbAlpha);
                        setAlpha(alphaBuffer, rt, saveLayer, rtAlpha);

                        setId(idBuffer, idIndex, saveLayer, l);

                        ++saveLayer;
                    }

                    // 你可以选择按权重排序，注意要在编辑器中提供检查工具，保证每个像素的层数不能超过maxTexelLayerNum
                    // id图可以更小，但是也需要检查工具，保证在生产过程中
                    if (saveLayer >= maxTexelLayerNum)
                        break;
                }
            }
        }


        Texture2D alphaMap = new Texture2D(expW, expH, TextureFormat.RGB24, false);
        alphaMap.filterMode = FilterMode.Bilinear;
        alphaMap.anisoLevel = 0;
        alphaMap.SetPixelData(alphaBuffer, 0, 0);
        alphaMap.Apply(false);

        Texture2D idMap = new Texture2D(alphaMapW, alphaMapH, TextureFormat.RGB24, false);
        idMap.filterMode = FilterMode.Point;
        idMap.anisoLevel = 0;
        idMap.SetPixelData(idBuffer, 0, 0);
        idMap.Apply(false);

        return (alphaMap, idMap);
    }
}
