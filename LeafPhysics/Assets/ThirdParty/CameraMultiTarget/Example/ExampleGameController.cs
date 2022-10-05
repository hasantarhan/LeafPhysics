using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExampleGameController : MonoBehaviour
{
    public CameraMultiTarget cameraMultiTarget;
    public GameObject targetPrefab;

    private IEnumerator Start() {
        var numberOfTargets = 3;
        var targets = new List<GameObject>(numberOfTargets);
        targets.Add(CreateTarget());
        cameraMultiTarget.SetTargets(targets.ToArray());
        foreach (var _ in Enumerable.Range(0, numberOfTargets - targets.Count)) {
            yield return new WaitForSeconds(5.0f);
            targets.Add(CreateTarget());
            cameraMultiTarget.SetTargets(targets.ToArray());
        }
        yield return null;
    }

    private GameObject CreateTarget() {
        GameObject target = GameObject.Instantiate(targetPrefab);
        target.AddComponent<ExampleTargetBehaviour>();
        return target;
    }

}