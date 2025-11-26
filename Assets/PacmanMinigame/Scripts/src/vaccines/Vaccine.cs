using System.Collections.Generic;
using UnityEngine;

public class Vaccine
{
    // The "Brain" of this agent
    private IKnowledgeBase _kb;

    // Constructor: Plug in the specific brain
    public Vaccine(IKnowledgeBase kb)
    {
        _kb = kb;
    }

    /// <summary>
    /// The main loop, following the TELL/ASK model.
    /// </summary>
    public string GetNextMove(
        Vector2Int myPos,
        Vector2Int? virusPos,
        List<(string id, Vector2Int pos)> otherVaccines,
        Dictionary<Vector2Int, string> percepts
    )
    {
        // 1. TELL the Brain the new facts
        _kb.Tell(myPos, virusPos, otherVaccines, percepts);

        // 2. ASK the Brain for the move
        return _kb.Ask();
    }
}