using System.Collections.Generic;
using UnityEngine;

public interface IKnowledgeBase
{
    /// <summary>
    /// TELLs the KB all new facts from the environment.
    /// Updates internal state and learned maps.
    /// </summary>
    /// <param name="myPos">Current grid position of this vaccine.</param>
    /// <param name="virusPos">Position of the Virus (Pacman), if visible. Null otherwise.</param>
    /// <param name="otherVaccines">List of (ID, Position) for other visible vaccines.</param>
    /// <param name="percepts">Dictionary mapping coordinates to what is seen ("WALL", "EMPTY", etc.).</param>
    void Tell(
        Vector2Int myPos,
        Vector2Int? virusPos,
        List<(string id, Vector2Int pos)> otherVaccines,
        Dictionary<Vector2Int, string> percepts
    );

    /// <summary>
    /// ASKS the KB for a single, final, safe action string (e.g., "UP", "RIGHT").
    /// </summary>
    string Ask();
}