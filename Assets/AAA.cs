using System;
using System.Collections.Generic;
using UnityEngine;

public struct Box
{
    public readonly Vector3 Max;
    public readonly Vector3 Min;
    public readonly Vector3 Center;
    public readonly Vector3 Size;
    public readonly bool HasVolume;
    // 対角隅2点を指定してBoxを作る
    public Box(Vector3 cornerA, Vector3 cornerB)
    {
        this.Max = Vector3.Max(cornerA, cornerB);
        this.Min = Vector3.Min(cornerA, cornerB);
        this.Center = (this.Max + this.Min) * 0.5f;
        this.Size = this.Max - this.Min;
        this.HasVolume = (this.Size.x * this.Size.y * this.Size.z) > 0.0f;
    }
    // Cubeを伸縮させたオブジェクトをもとにBoxを作る
    public Box(Transform cube)
    {
        if (cube == null)
        {
            this = new Box();
            return;
        }
        var position = cube.position;
        var halfExtents = cube.localScale * 0.5f;
        this = new Box(position + halfExtents, position - halfExtents);
    }
    // BoxをもとにCubeを伸縮させたオブジェクトを作る
    public Transform ToCube(Transform parent = null, Material sharedMaterial = null)
    {
        if (!this.HasVolume)
        {
            return null;
        }
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var cubeTransform = cube.transform;
        cubeTransform.position = this.Center;
        cubeTransform.localScale = this.Size;
        cubeTransform.SetParent(parent);
        if (sharedMaterial != null)
        {
            cubeTransform.GetComponent<Renderer>().sharedMaterial = sharedMaterial;
        }
        return cubeTransform;
    }
    // Boxを特定の軸で切断し2つのBoxに分ける
    // 軸番号はXが0、Yが1、Zが2
    public (Box upper, Box lower) Cut(float at, int axis)
    {
        if (!this.HasVolume)
        {
            return (new Box(), new Box());
        }
        var sourceMax = this.Max[axis];
        var sourceMin = this.Min[axis];
        if (at <= sourceMin)
        {
            return (this, new Box());
        }
        if (at >= sourceMax)
        {
            return (new Box(), this);
        }
        var lowerMin = this.Min;
        var lowerMax = this.Max;
        lowerMax[axis] = at;
        var upperMin = this.Min;
        var upperMax = this.Max;
        upperMin[axis] = at;
        return (new Box(upperMin, upperMax), new Box(lowerMin, lowerMax));
    }
    // Boxを別のBoxでくり抜き、できあがった断片をnewBoxesに詰める
    public void Carve(Box carver, List<Box> newBoxes)
    {
        if (newBoxes == null)
        {
            throw new InvalidOperationException($"{nameof(newBoxes)} must not be null.");
        }
        if (!this.HasVolume)
        {
            throw new InvalidOperationException($"carvee must have volume.");
        }
        newBoxes.Clear();
        var residue = this;
        // X、Y、Z軸についてそれぞれ...
        for (var i = 0; i < 3; i++)
        {
            // 最小側の面で切断
            var (upper, lower) = residue.Cut(carver.Min[i], i);
            if (lower.HasVolume)
            {
                newBoxes.Add(lower);
            }
            if (!upper.HasVolume)
            {
                break;
            }
            residue = upper;
            // 最大側の面で切断
            (upper, lower) = residue.Cut(carver.Max[i], i);
            if (upper.HasVolume)
            {
                newBoxes.Add(upper);
            }
            if (!lower.HasVolume)
            {
                break;
            }
            residue = lower;
        }
    }
}