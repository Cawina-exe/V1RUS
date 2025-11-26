using System.Collections.Generic;
using UnityEngine;
using Pacman.Agents.KBs;


namespace Pacman.Agents
{
    /// <summary>
    /// This is the generic Vaccine agent class.
    /// Its only job is to hold a Knowledge Base "Brain" and pass messages to it.
    /// Translates 'ghosts/ghost.py'.
    /// </summary>
    public class Vaccine
    {
        // The "Brain" of this agent
        private readonly IKnowledgeBase _kb;

        /// <summary>
        /// Initializes the Vaccine by plugging in the specific Logic Brain (KB).
        /// </summary>
        /// <param name="kb">An object implementing IKnowledgeBase (e.g. KnowledgeBaseA).</param>
        public Vaccine(IKnowledgeBase kb)
        {
            _kb = kb;
        }

        /// <summary>
        /// The main loop, following the TELL/ASK model.
        /// Called by the Unity Controller script every turn (0.5s).
        /// </summary>
        public string GetNextMove(
            Vector2Int myPos,
            Vector2Int? virusPos,
            List<(string id, Vector2Int pos)> otherVaccines,
            Dictionary<Vector2Int, string> percepts
        )
        {
            // 1. TELL KB all the new facts.
            _kb.Tell(myPos, virusPos, otherVaccines, percepts);

            // 2. ASK the KB for its final, calculated move.
            return _kb.Ask();
        }
    }
}