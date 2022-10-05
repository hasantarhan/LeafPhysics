using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;


public static class Extensions
{
    private static readonly Random Rand = new Random();

    public static float Remap(this float from, float fromMin, float fromMax, float toMin, float toMax)
    {
        var fromAbs = from - fromMin;
        var fromMaxAbs = fromMax - fromMin;

        var normal = fromAbs / fromMaxAbs;

        var toMaxAbs = toMax - toMin;
        var toAbs = toMaxAbs * normal;

        var to = toAbs + toMin;

        return to;
    }

    public static int RoundOff(this int i)
    {
        return ((int)Math.Round(i / 10.0)) * 10;
    }

    public static T[] RemoveAt<T>(this T[] source, int index)
    {
        T[] dest = new T[source.Length - 1];
        if (index > 0)
            Array.Copy(source, 0, dest, 0, index);

        if (index < source.Length - 1)
            Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

        return dest;
    }

    public static long RoundOff(this long i)
    {
        return ((long)Math.Round(i / 10.0)) * 10;
    }

    public static string ToOrdinal(this int value)
    {
        var extension = "th";

        var last_digits = value % 100;

        if (last_digits < 11 || last_digits > 13)
            switch (last_digits % 10)
            {
                case 1:
                    extension = "st";
                    break;
                case 2:
                    extension = "nd";
                    break;
                case 3:
                    extension = "rd";
                    break;
            }

        return extension;
    }

    public static T RandomElement<T>(this T[] items)
    {
        return items[Rand.Next(0, items.Length)];
    }


    public static T RandomElement<T>(this List<T> items)
    {
        return items[Rand.Next(0, items.Count)];
    }

    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        if (gameObject.TryGetComponent(out T t)) return t;

        return gameObject.AddComponent<T>();
    }

    public static bool Approximately(this Quaternion quatA, Quaternion value, float acceptableRange)
    {
        return 1 - Mathf.Abs(Quaternion.Dot(quatA, value)) < acceptableRange;
    }

    public static int Remap(this int from, int fromMin, int fromMax, int toMin, int toMax)
    {
        var fromAbs = from - fromMin;
        var fromMaxAbs = fromMax - fromMin;

        var normal = fromAbs / fromMaxAbs;

        var toMaxAbs = toMax - toMin;
        var toAbs = toMaxAbs * normal;

        var to = toAbs + toMin;

        return to;
    }

    public static float LinearRemap(this float value, float valueRangeMin, float valueRangeMax, float newRangeMin,
        float newRangeMax)
    {
        return (value - valueRangeMin) / (valueRangeMax - valueRangeMin) * (newRangeMax - newRangeMin) +
               newRangeMin;
    }

    public static int WithRandomSign(this int value, float negativeProbability = 0.5f)
    {
        return UnityEngine.Random.value < negativeProbability ? -value : value;
    }
}

public static class VectorExtensionMethods
{
    public static Vector2 xy(this Vector3 v)
    {
        return new Vector2(v.x, v.y);
    }

    public static Vector3 WithX(this Vector3 v, float x)
    {
        return new Vector3(x, v.y, v.z);
    }

    public static Vector3 WithY(this Vector3 v, float y)
    {
        return new Vector3(v.x, y, v.z);
    }

    public static Vector3 WithZ(this Vector3 v, float z)
    {
        return new Vector3(v.x, v.y, z);
    }

    public static Vector2 WithX(this Vector2 v, float x)
    {
        return new Vector2(x, v.y);
    }

    public static Vector2 WithY(this Vector2 v, float y)
    {
        return new Vector2(v.x, y);
    }

    public static Vector3 WithZ(this Vector2 v, float z)
    {
        return new Vector3(v.x, v.y, z);
    }

    public static Vector3 NearestPointOnAxis(this Vector3 axisDirection, Vector3 point, bool isNormalized = false)
    {
        if (!isNormalized) axisDirection.Normalize();
        var d = Vector3.Dot(point, axisDirection);
        return axisDirection * d;
    }

    public static Vector3 GetDirection(Vector3 right, Vector3 left)
    {
        return (right - left).normalized;
    }

    public static Vector2 GetDirection(Vector2 right, Vector2 left)
    {
        return (right - left).normalized;
    }

    public static Vector3 NearestPointOnLine(
        this Vector3 lineDirection, Vector3 point, Vector3 pointOnLine, bool isNormalized = false)
    {
        if (!isNormalized) lineDirection.Normalize();
        var d = Vector3.Dot(point - pointOnLine, lineDirection);
        return pointOnLine + lineDirection * d;
    }

    public static Quaternion Pow(this Quaternion input, float power)
    {
        var inputMagnitude = input.Magnitude();
        var nHat = new Vector3(input.x, input.y, input.z).normalized;
        var vectorBit = new Quaternion(nHat.x, nHat.y, nHat.z, 0)
            .ScalarMultiply(power * Mathf.Acos(input.w / inputMagnitude))
            .Exp();
        return vectorBit.ScalarMultiply(Mathf.Pow(inputMagnitude, power));
    }

    public static Quaternion Exp(this Quaternion input)
    {
        var inputA = input.w;
        var inputV = new Vector3(input.x, input.y, input.z);
        var outputA = Mathf.Exp(inputA) * Mathf.Cos(inputV.magnitude);
        var outputV = Mathf.Exp(inputA) * (inputV.normalized * Mathf.Sin(inputV.magnitude));
        return new Quaternion(outputV.x, outputV.y, outputV.z, outputA);
    }

    public static float Magnitude(this Quaternion input)
    {
        return Mathf.Sqrt(input.x * input.x + input.y * input.y + input.z * input.z + input.w * input.w);
    }

    public static Quaternion ScalarMultiply(this Quaternion input, float scalar)
    {
        return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
    }

    public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
    {
        if (items == null) throw new ArgumentNullException("items");
        if (predicate == null) throw new ArgumentNullException("predicate");

        var retVal = 0;
        foreach (var item in items)
        {
            if (predicate(item)) return retVal;
            retVal++;
        }

        return -1;
    }
}

public static class LayerMaskExtensions
{
    public static LayerMask Create(params string[] layerNames)
    {
        return NamesToMask(layerNames);
    }

    public static LayerMask Create(params int[] layerNumbers)
    {
        return LayerNumbersToMask(layerNumbers);
    }

    public static LayerMask NamesToMask(params string[] layerNames)
    {
        LayerMask ret = 0;
        foreach (var name in layerNames) ret |= 1 << LayerMask.NameToLayer(name);

        return ret;
    }

    public static LayerMask LayerNumbersToMask(params int[] layerNumbers)
    {
        LayerMask ret = 0;
        foreach (var layer in layerNumbers) ret |= 1 << layer;

        return ret;
    }

    public static LayerMask Inverse(this LayerMask original)
    {
        return ~original;
    }

    public static LayerMask AddToMask(this LayerMask original, params string[] layerNames)
    {
        return original | NamesToMask(layerNames);
    }

    public static LayerMask RemoveFromMask(this LayerMask original, params string[] layerNames)
    {
        LayerMask invertedOriginal = ~original;
        return ~(invertedOriginal | NamesToMask(layerNames));
    }

    public static string[] MaskToNames(this LayerMask original)
    {
        var output = new List<string>();

        for (var i = 0; i < 32; ++i)
        {
            var shifted = 1 << i;
            if ((original & shifted) == shifted)
            {
                var layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName)) output.Add(layerName);
            }
        }

        return output.ToArray();
    }

    public static string MaskToString(this LayerMask original)
    {
        return MaskToString(original, ", ");
    }

    public static string MaskToString(this LayerMask original, string delimiter)
    {
        return string.Join(delimiter, MaskToNames(original));
    }

    public static Vector3 Clamp(this Vector3 point, Boundaries boundries)
    {
        var clampedValue = point;
        clampedValue.x = Mathf.Clamp(point.x, boundries.xmin, boundries.xmax);
        clampedValue.y = Mathf.Clamp(point.y, boundries.ymin, boundries.ymax);
        clampedValue.z = Mathf.Clamp(point.z, boundries.zmin, boundries.zmax);
        return clampedValue;
    }
}

public static class RendererExtensions
{
    public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
    {
        var planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }
}

public static class AsyncRun
{
    public static Task StartFunction(Func<Task> function)
    {
        return Task.Factory.StartNew(async () =>
        {
            try
            {
                await function();
            }
            catch (Exception e)
            {
                string errorId = null;

                if (e.Data.Contains("ErrorId"))
                    errorId = e.Data["ErrorId"] as string;

                Debug.Log($"Task Exception: {e.Message}, ErrorId: {errorId}");
            }
        }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
    }

    public static void Delayed(Action action, float delay)
    {
        var calculatedDelay = (int)delay * 1000;
        StartFunction(() => DelayedFunc(action, calculatedDelay));
    }

    private static async Task DelayedFunc(Action action, int delay)
    {
        await Task.Delay(delay);
        action.Invoke();
    }
}

public static class ObjectExtensions
{
    public static void Log(this object obj, string message)
    {
        Debug.Log(message);
    }

    public static void Log(this object obj, string message, Object target)
    {
        Debug.Log(message, target);
    }
}

[Serializable]
public class Boundaries
{
    public float xmin = float.NegativeInfinity;
    public float xmax = float.PositiveInfinity;
    public float ymin = float.NegativeInfinity;
    public float ymax = float.PositiveInfinity;
    public float zmin = float.NegativeInfinity;
    public float zmax = float.PositiveInfinity;
}