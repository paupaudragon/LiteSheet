// Skeleton implementation written by Joe Zachary for CS 3500, September 2013.
// Version 1.1 (Fixed error in comment for RemoveDependency.)
// Version 1.2 - Daniel Kopta 
//               (Clarified meaning of dependent and dependee.)
//               (Clarified names in solution/project structure.)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities
{

    /// <summary>
    /// (s1,t1) is an ordered pair of strings
    /// t1 depends on s1; s1 must be evaluated before t1
    /// 
    /// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
    /// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
    /// Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
    /// set, and the element is already in the set, the set remains unchanged.
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
    ///        (The set of things that depend on s)    
    ///        
    ///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
    ///        (The set of things that s depends on) 
    //
    // For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    //     dependents("a") = {"b", "c"}
    //     dependents("b") = {"d"}
    //     dependents("c") = {}
    //     dependents("d") = {"d"}
    //     dependees("a") = {}
    //     dependees("b") = {"a"}
    //     dependees("c") = {"a"}
    //     dependees("d") = {"b", "d"}
    /// </summary>
    public class DependencyGraph
    {
        private Dictionary<string, HashSet<string>> dependents;
        private Dictionary<string, HashSet<string>> dependees;
        private int numOrderPair = 0;

        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph()
        {
             dependents = new Dictionary<string, HashSet<string>>();
             dependees = new Dictionary<string, HashSet<string>>();
        }


        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// </summary>
        public int Size
        {
            get { return numOrderPair; }
        }


        /// <summary>
        /// The size of dependees(s).
        /// This property is an example of an indexer.  If dg is a DependencyGraph, you would
        /// invoke it like this:
        /// dg["a"]
        /// It should return the size of dependees("a")
        /// </summary>
        public int this[string s]
        {
            get {
                if (HasDependees(s))
                    return dependees[s].Count;
                return 0;
            }
        }


        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// </summary>
        public bool HasDependents(string s)
        {
            if (dependents.ContainsKey(s))
                return true;
            return false;
        }


        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// </summary>
        public bool HasDependees(string s)
        {
            if (dependees.ContainsKey(s))
                return true;
            return false;
        }


        /// <summary>
        /// Enumerates dependents(s).
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            if (HasDependents(s))
                return dependents[s];
            return new HashSet<string>();
        }

        /// <summary>
        /// Enumerates dependees(s).
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            if (HasDependees(s))
                return dependees[s];
            return new HashSet<string>();
        }


        /// <summary>
        /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
        /// 
        /// <para>This should be thought of as:</para>   
        /// 
        ///   t depends on s
        ///
        /// </summary>
        /// <param name="s"> s must be evaluated first. T depends on S</param>
        /// <param name="t"> t cannot be evaluated until s is</param>        /// 
        public void AddDependency(string s, string t)
        {
            // add t to dependents' list of s
            //if dependents' list already have the key s
            if (HasDependents(s))
            {
                //add it to the list if t doesn't exist in dependent' list s
                if (!dependents[s].Contains(t))
                {
                    dependents[s].Add(t);
                    numOrderPair++;
                }
            }
            else //doesn't have the key s
            {               
                HashSet<string> dependentSet = new HashSet<string>() {t}; // create new HashSet and add t to it
                dependents.Add(s, dependentSet); // add new key value pair
                numOrderPair++; // update the number of order pairs
            }

            //if dependees' list already have the key t
            if (HasDependees(t))
            {
                //add it to the list if s doesn't exist in dependees' list t
                if (!dependees[t].Contains(s))
                    dependees[t].Add(s);
            }
            else //don't have the key t
            {
                HashSet<string> dependeesSet = new HashSet<string>() {s};// create new HashSet and add s to it
                dependees.Add(t, dependeesSet); // add new key value pair
            }

        }


        /// <summary>
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        public void RemoveDependency(string s, string t)
        {
            if (HasDependents(s) && dependents[s].Contains(t))
            {
                dependents[s].Remove(t);                
                if(dependents[s].Count == 0) // if s doesn't have any dependent, delete the key
                    dependents.Remove(s);
                numOrderPair--;
            }

            if (HasDependees(t) && dependees[t].Contains(s))
            {
                dependees[t].Remove(s);
                if (dependees[t].Count == 0) // if t doesn't have any dependee, delete the key
                    dependees.Remove(t);
            }


        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r).  Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            if (HasDependents(s))
            {
                foreach(string str in dependents[s]) // delete all the existing order pairs
                    RemoveDependency(s, str);
            }

            foreach (string str in newDependents) // add new order pairs
                AddDependency(s, str);
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
            if (HasDependees(s))
            {
                foreach (string str in dependees[s]) // delete all the existing order pairs
                    RemoveDependency(str, s);
            }

            foreach (string str in newDependees) // add new order pairs
                AddDependency(str, s);
        }

    }

}