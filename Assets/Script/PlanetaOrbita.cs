using UnityEngine;

public class PlanetaOrbita : MonoBehaviour
{
    [SerializeField] private Transform sun;

    public float orbitSpeed = 10f;
    public float rotationSpeed = 50f;

    void Update()
    {
        if (sun != null)
        {
            transform.RotateAround(sun.position, Vector3.up, orbitSpeed * Time.deltaTime);
        }

        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
