using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pacman.Utils.FOL
{
    // --- Base Term Classes ---

    /// <summary>
    /// Base class for FOL terms (constants or variables).
    /// </summary>
    public abstract class Term { }

    public class Constant : Term
    {
        public string Value { get; }
        public Constant(string value) { Value = value; }

        public override string ToString() => Value;

        // Value equality is crucial for logic
        public override bool Equals(object obj) => obj is Constant c && c.Value == Value;
        public override int GetHashCode() => Value.GetHashCode();
    }

    public class Variable : Term
    {
        public string Name { get; }
        public Variable(string name) { Name = name; }

        public override string ToString() => "?" + Name;

        public override bool Equals(object obj) => obj is Variable v && v.Name == Name;
        public override int GetHashCode() => Name.GetHashCode();
    }

    // --- Predicate Classes ---

    /// <summary>
    /// Base class for FOL predicates.
    /// </summary>
    public class Predicate
    {
        public string Name { get; }
        public Term[] Args { get; }

        public Predicate(string name, params Term[] args)
        {
            Name = name;
            Args = args;
        }

        /// <summary>
        /// Check if the predicate contains only constants (no variables).
        /// </summary>
        public bool IsGround()
        {
            return Args.All(arg => arg is Constant);
        }

        public override string ToString() => $"{Name}({string.Join(", ", (IEnumerable<Term>)Args)})";

        // Structural equality is required so HashSet<Predicate> works correctly
        public override bool Equals(object obj)
        {
            if (obj is not Predicate p || p.Name != Name || p.Args.Length != Args.Length)
                return false;

            for (int i = 0; i < Args.Length; i++)
            {
                if (!Args[i].Equals(p.Args[i])) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = Name.GetHashCode();
            foreach (var arg in Args) hash = hash * 31 + arg.GetHashCode();
            return hash;
        }
    }

    /// <summary>
    /// Base class for predicates that refer to a location (x, y).
    /// Provides helper properties X and Y.
    /// </summary>
    public class LocationBasedPredicate : Predicate
    {
        public Term X => Args[0];
        public Term Y => Args[1];

        public LocationBasedPredicate(string name, Term x, Term y) : base(name, x, y)
        {
        }
    }

    // --- Unification Logic ---

    public static class FOLUtils
    {
        /// <summary>
        /// Unify a query predicate with a ground fact.
        /// Returns: Substitution dict mapping variables to constants if unification succeeds, null otherwise.
        /// </summary>
        public static Dictionary<Variable, Constant> Unify(Predicate query, Predicate fact)
        {
            // 1. Must match name and arity
            if (query.Name != fact.Name || query.Args.Length != fact.Args.Length)
                return null;

            var substitution = new Dictionary<Variable, Constant>();

            for (int i = 0; i < query.Args.Length; i++)
            {
                Term qArg = query.Args[i];
                Term fArg = fact.Args[i];

                if (qArg is Constant qConst)
                {
                    // Constant in query must match constant in fact
                    if (fArg is not Constant fConst || qConst.Value != fConst.Value)
                        return null;
                }
                else if (qArg is Variable qVar)
                {
                    // Variable in query binds to constant in fact
                    if (fArg is not Constant fConst)
                        return null; // We usually unify query vs ground fact

                    // Check consistency with existing bindings
                    if (substitution.ContainsKey(qVar))
                    {
                        if (!substitution[qVar].Equals(fConst))
                            return null; // Contradiction: Variable bound to two different values
                    }
                    else
                    {
                        substitution[qVar] = fConst;
                    }
                }
            }

            return substitution;
        }
    }
}