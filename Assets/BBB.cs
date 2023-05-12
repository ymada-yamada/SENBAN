using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BBB : MonoBehaviour
{
    public void Carve(Transform carverTransform)
    {
        if (carverTransform == null)
        {
            return;
        }
        var carver = new Box(carverTransform);
        if (!carver.HasVolume)
        {
            return;
        }
        // ���蔲����鑤���̂͋�I�u�W�F�N�g�ɂȂ��Ă���A�q�I�u�W�F�N�g�Ƃ���Cube�I�u�W�F�N�g�������Ă���
        // Carve�����s����Ǝq�I�u�W�F�N�g�����ꂼ�ꂭ�蔲���āA�f�Ђ��Ăю��g�̎q�Ƃ���
        var newBoxes = new List<Box>();
        var carveeTransforms = this.transform.Cast<Transform>().ToArray();
        foreach (var carveeTransform in carveeTransforms)
        {
            var carvee = new Box(carveeTransform);
            if (!carvee.HasVolume)
            {
                continue;
            }
            carvee.Carve(carver, newBoxes);
            var carveeMaterial = carveeTransform.GetComponent<Renderer>().sharedMaterial;
            foreach (var newBox in newBoxes)
            {
                newBox.ToCube(this.transform, carveeMaterial);
            }
            Destroy(carveeTransform.gameObject);
        }
    }
}
