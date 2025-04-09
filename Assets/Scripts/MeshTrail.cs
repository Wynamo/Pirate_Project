using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTrail : MonoBehaviour
{
    public float meshRefreshRate = 0.1f;
    public Transform positionToSpawn;

    [Header("Shader Related")]
    public Material mat;

    public float meshDestroyDelay = 3f;
    private SkinnedMeshRenderer[] skinnedMeshRenderers;

    private Vector3 lastPosition;
    private bool isMoving;

    void Start()
    {
        // Ensure positionToSpawn is assigned
        if (positionToSpawn == null)
        {
            Debug.LogError("positionToSpawn is not assigned! Please assign it in the inspector.");
            return;
        }

        // Ensure SkinnedMeshRenderer exists
        skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        if (skinnedMeshRenderers.Length == 0)
        {
            Debug.LogError("No SkinnedMeshRenderer found on the object. Make sure it has at least one.");
            return;
        }

        lastPosition = transform.position;
        isMoving = false;

        StartCoroutine(ActivateTrail());
    }

    IEnumerator ActivateTrail()
    {
        while (true)
        {
            if (Vector3.Distance(transform.position, lastPosition) > 0.01f)
            {
                if (!isMoving)
                    isMoving = true;

                lastPosition = transform.position;
                SpawnTrailMesh();
            }
            else
            {
                if (isMoving)
                    isMoving = false;
            }

            yield return new WaitForSeconds(meshRefreshRate);
        }
    }

    void SpawnTrailMesh()
    {
        GameObject trailMeshObject = new GameObject("TrailMesh");
        trailMeshObject.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

        MeshRenderer mr = trailMeshObject.AddComponent<MeshRenderer>();
        MeshFilter mf = trailMeshObject.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();
        skinnedMeshRenderers[0].BakeMesh(mesh); // Use the first SkinnedMeshRenderer

        mf.mesh = mesh;

        // Clone material to avoid shared instance issues
        Material matInstance = new Material(mat);
        mr.material = matInstance;

        StartCoroutine(FadeOutAndDestroy(trailMeshObject, meshDestroyDelay));
    }

    IEnumerator FadeOutAndDestroy(GameObject trailMeshObject, float delay)
    {
        Material trailMaterial = trailMeshObject.GetComponent<MeshRenderer>().material;
        Color color = trailMaterial.color;

        float fadeOutDuration = 1f;
        float fadeRate = color.a / fadeOutDuration;

        // Wait before starting the fade-out (optional)
        yield return new WaitForSeconds(delay - fadeOutDuration);

        while (color.a > 0)
        {
            color.a -= fadeRate * Time.deltaTime;
            trailMaterial.color = color;
            yield return null;
        }

        Destroy(trailMeshObject);
    }
}
