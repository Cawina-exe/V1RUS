using System.Collections.Generic;
using UnityEngine;

[System.Serializable] 
public class SaveData
{
    public List<bool> PlanetaAtivado;
    public List<string> Planetas;
    public List<float> Pontos;
    public List<bool> FaseConcluida;

    
    public float som;
    public float sfx;


    public SaveData()
    {
        PlanetaAtivado = new List<bool> { false, false, false };
        Planetas = new List<string> { "Earth", "Aquatic", "Techno" };
        Pontos = new List<float> { 0f, 0f, 0f };

        som = 0.5f;
        sfx = 0.5f;
    }
}