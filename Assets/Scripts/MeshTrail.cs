using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTrail : MonoBehaviour
{
    public float meshRefreshRate = 0.1f;
    public Transform positionToSpawn;

    [Header("Shader Related")]
    public Material mat;
    public string shaderVarRef;
    public float shaderVarRate = 0.1f;
    public float shaderVarRefreshRate= 0.05f;

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
            Debug.LogError("No SkinnedMeshRenderer found on the sphere. Make sure the sphere is animated with a SkinnedMeshRenderer.");
            return;
        }

        // Initialize the position and start the trail generation
        lastPosition = transform.position;
        isMoving = false;

        // Start the trail process
        StartCoroutine(ActivateTrail());
    }

    IEnumerator ActivateTrail()
    {
        while (true)
        {
            // Check if the sphere has moved since the last frame
            if (Vector3.Distance(transform.position, lastPosition) > 0.01f)
            {
                if (!isMoving)
                {
                    // If the sphere starts moving, mark it and start spawning trail meshes
                    isMoving = true;
                }

                // Update last position
                lastPosition = transform.position;

                // Spawn a mesh trail at the specified position
                SpawnTrailMesh();
            }
            else
            {
                if (isMoving)
                {
                    // If the sphere stops moving, stop spawning trails
                    isMoving = false;
                }
            }

            // Wait for the next spawn cycle
            yield return new WaitForSeconds(meshRefreshRate);
        }
    }

    void SpawnTrailMesh()
    {
        // Create new GameObject for trail mesh
        GameObject trailMeshObject = new GameObject();
        trailMeshObject.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

        // Add MeshRenderer and MeshFilter to the trail object
        MeshRenderer mr = trailMeshObject.AddComponent<MeshRenderer>();
        MeshFilter mf = trailMeshObject.AddComponent<MeshFilter>();

        // Create a new mesh, bake the current mesh from SkinnedMeshRenderer
        Mesh mesh = new Mesh();
        for (int i = 0; i < skinnedMeshRenderers.Length; i++)
        {
            skinnedMeshRenderers[i].BakeMesh(mesh);
        }

        mf.mesh = mesh;
        mr.material = mat;

        StartCoroutine(AnimateMaterialFloat(mr.material, 0, shaderVarRate,shaderVarRefreshRate));

        // Destroy the trail mesh after a delay
        Destroy(trailMeshObject, meshDestroyDelay);
    }

    IEnumerator AnimateMaterialFloat (Material mat, float goal, float rate, float refreshRate)
    {
        float valueToAnimate = mat.GetFloat(shaderVarRef);
        while (valueToAnimate > goal)
        {
            valueToAnimate -= rate;
            mat.SetFloat(shaderVarRef, valueToAnimate);
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
