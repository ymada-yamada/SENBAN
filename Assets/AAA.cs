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
    // �Ίp��2�_���w�肵��Box�����
    public Box(Vector3 cornerA, Vector3 cornerB)
    {
        this.Max = Vector3.Max(cornerA, cornerB);
        this.Min = Vector3.Min(cornerA, cornerB);
        this.Center = (this.Max + this.Min) * 0.5f;
        this.Size = this.Max - this.Min;
        this.HasVolume = (this.Size.x * this.Size.y * this.Size.z) > 0.0f;
    }
    // Cube��L�k�������I�u�W�F�N�g�����Ƃ�Box�����
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
    // Box�����Ƃ�Cube��L�k�������I�u�W�F�N�g�����
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
    // Box�����̎��Őؒf��2��Box�ɕ�����
    // ���ԍ���X��0�AY��1�AZ��2
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
    // Box��ʂ�Box�ł��蔲���A�ł����������f�Ђ�newBoxes�ɋl�߂�
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
        // X�AY�AZ���ɂ��Ă��ꂼ��...
        for (var i = 0; i < 3; i++)
        {
            // �ŏ����̖ʂŐؒf
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
            // �ő呤�̖ʂŐؒf
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