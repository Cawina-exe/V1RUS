
namespace Pacman.Utils.FOL
{
    // --- Map Topology Predicates ---

    public class LearnedWall : LocationBasedPredicate
    {
        public LearnedWall(Term x, Term y) : base("LearnedWall", x, y) { }
    }

    public class LearnedSafe : LocationBasedPredicate
    {
        public LearnedSafe(Term x, Term y) : base("LearnedSafe", x, y) { }
    }

    public class VirusPos : LocationBasedPredicate // Formerly PacmanPos
    {
        public VirusPos(Term x, Term y) : base("VirusPos", x, y) { }
    }

    // --- Dynamic Belief Predicates ---

    public class VisitedAtTime : Predicate
    {
        // Fact: VisitedAtTime(x, y, t)
        public Term X => Args[0];
        public Term Y => Args[1];
        public Term T => Args[2];

        public VisitedAtTime(Term x, Term y, Term t) : base("VisitedAtTime", x, y, t) { }
    }

    public class EscapeState : Predicate
    {
        // Fact: EscapeState(TargetX, TargetY, StartTime)
        public Term TargetX => Args[0];
        public Term TargetY => Args[1];
        public Term StartTime => Args[2];

        public EscapeState(Term tx, Term ty, Term t) : base("EscapeState", tx, ty, t) { }
    }

    public class LastPos : Predicate
    {
        // Stores last pos to avoid bouncing: LastPos(ID, x, y)
        public LastPos(Term agentId, Term x, Term y) : base("LastPos", agentId, x, y) { }
    }

    public class UnreachableGoal : Predicate
    {
        // Fact: UnreachableGoal(x, y, timestamp)
        public Term X => Args[0];
        public Term Y => Args[1];
        public Term Timestamp => Args[2];

        public UnreachableGoal(Term x, Term y, Term t) : base("UnreachableGoal", x, y, t) { }
    }

    public class VirusVector : Predicate // Formerly PacmanVector
    {
        // Belief: VirusVector(dx, dy)
        public Term DX => Args[0];
        public Term DY => Args[1];

        public VirusVector(Term dx, Term dy) : base("VirusVector", dx, dy) { }
    }

    public class VaccinePos : Predicate // Formerly GhostPos
    {
        // Belief: VaccinePos(AgentID, x, y)
        public Term AgentID => Args[0];
        public Term X => Args[1];
        public Term Y => Args[2];

        public VaccinePos(Term agentId, Term x, Term y) : base("VaccinePos", agentId, x, y) { }
    }
}