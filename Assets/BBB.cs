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
        // くり抜かれる側自体は空オブジェクトになっており、子オブジェクトとしてCubeオブジェクトを持っている
        // Carveを実行すると子オブジェクトをそれぞれくり抜いて、断片を再び自身の子とする
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
