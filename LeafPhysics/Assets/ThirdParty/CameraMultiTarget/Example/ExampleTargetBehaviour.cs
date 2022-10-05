using System.Collections;
using UnityEngine;

public class ExampleTargetBehaviour : MonoBehaviour
{
    private static readonly Vector3 MinPosition = new Vector3(-10f, 1f, -10f);
    private static readonly Vector3 MaxPosition = new Vector3(10f, 4f, 10f);
        
    private Vector3 destinationPosition;

    private void Awake() {
        transform.position = new Vector3(0f, 1.5f, 0f);
        destinationPosition = transform.position;
    }
    
    private IEnumerator Start() {
        for(int i=0; i< 100; i++) {
            yield return new WaitForSeconds(5.0f);
            destinationPosition = GetRandomPosition();
        }
    }

    private void Update() {
        transform.position = Vector3.Lerp(transform.position, destinationPosition, Time.deltaTime);
    }
        
    private static Vector3 GetRandomPosition() {
        var randomPosition = new Vector3(
            Random.Range(MinPosition.x, MaxPosition.x),
            Random.Range(MinPosition.y, MaxPosition.y),
            Random.Range(MinPosition.z, MaxPosition.y));
        return randomPosition;
    }
}