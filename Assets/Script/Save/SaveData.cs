using System.Collections.Generic;
using UnityEngine;

[System.Serializable] 
public class SaveData
{
    public List<float> Planeta;
    public List<float> Pontos;
    public List<bool> FaseConcluida;

    
    public float som;

    
    public SaveData()
    {
        Planeta = new List<float> { 1f, 2f, 3f };
        Pontos = new List<float> { 0f, 0f, 0f };
        FaseConcluida = new List<bool> { false, false, false };

        som = 1.0f; 
    }
}