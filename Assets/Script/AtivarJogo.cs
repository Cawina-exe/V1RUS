using UnityEngine;

public class AtivarJogo : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;
    [SerializeField] private int fase;
    [SerializeField] private bool toggle;
    [SerializeField] private FasePainelController fasePainelController;

    void OnMouseDown()
    {
        if (targetObject != null)
        {
            if (toggle)
            {
                targetObject.SetActive(!targetObject.activeSelf);
            }
            else
            {
                targetObject.SetActive(true);
            }

            SaveData data = SaveSystem.Load();

            while (data.PlanetaAtivado.Count <= fase)
            {
                data.PlanetaAtivado.Add(false);
            }

            for (int i = 0; i < data.PlanetaAtivado.Count; i++)
            {
                data.PlanetaAtivado[i] = false;
            }

            data.PlanetaAtivado[fase] = true;

            SaveSystem.Save(data);

            fasePainelController.AtivarPainelFase();
        }
    }
}