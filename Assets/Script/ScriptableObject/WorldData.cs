using UnityEngine;

[CreateAssetMenu(fileName = "WorldData", menuName = "Scriptable Objects/WorldData")]
public class WorldData : ScriptableObject
{
    [SerializeField] AudioSource AudioSource;
    public float habitantes;
    public float defesa;
    public float cura;

}
