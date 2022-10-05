# Dynamic Multi Target Camera for Unity

Concise Unity library which dynamically keeps a set of objects (e.g. players and important objects) in view, a common problem for a wide range of games. This asset is a direct build from the source code available on [GitHub](https://github.com/lopespm/unity-camera-multi-target).

More information about the library's inner workings and underlying math is available in the related [blog article](https://lopespm.github.io/libraries/games/2018/12/27/camera-multi-target.html)

The library was developed for, and used by [Survival Ball](https://survivalball.com/). The game has an heavy shared screen local co-op component, which requires the camera to dynamically keep many key elements in view.


## Install

Import the `CameraMultiTarget` folder into your project when installing it from the Asset Store.


## Usage

Add the [`CameraMultiTarget`](Assets/CameraMultiTarget/Library/CameraMultiTarget.cs) component to a camera and then you can programatically set which game objects the camera will track via the component's [`SetTargets(GameObject[] targets)`](Assets/CameraMultiTarget/Library/CameraMultiTarget.cs#L23) method.

For example, you can set the targets in your game controller component (if you choose to have one), like the following:

    public class ExampleGameController : MonoBehaviour
    {
        public CameraMultiTarget cameraMultiTarget;
    
        private void Start() {
            var targets = new List<GameObject>();
            targets.Add(CreateTarget());
            targets.Add(CreateTarget());
            targets.Add(CreateTarget());
            cameraMultiTarget.SetTargets(targets.ToArray());
        }

        private GameObject CreateTarget() {
            GameObject target = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            target.transform.position = Random.insideUnitSphere * 10f;
            return target;
        }
    }


## Example Scene

An example scene of the library's usage is included in the `CameraMultiTarget/Example` folder.