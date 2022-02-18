using System;
using System.Collections.Generic;

namespace StatesDiagram
{
    public class DiagramExport
    {
        private int idCounter = 0;
        private Dictionary<string, int> NodeIds = new Dictionary<string, int>(); // Name, Id
        public List<DiagramNode> nodes = new List<DiagramNode>();
        public List<DiagramEdge> edges = new List<DiagramEdge>();

        public void AddNode(DiagramNode newNode)
        {
            if (!NodeIds.ContainsKey(newNode.stringId))
            {
                newNode.id = idCounter;
                NodeIds.Add(newNode.stringId, idCounter);
                nodes.Add(newNode);
                idCounter++;
            }
        }

        public void AddNodes(IEnumerable<DiagramNode> newNodes)
        {
            foreach (var node in newNodes)
            {
                AddNode(node);
            }
        }

        public void AddEdge(DiagramEdge newEdge)
        {
            var r = NodeIds.TryGetValue(newEdge.source, out var fromId);
            r &= NodeIds.TryGetValue(newEdge.target, out var toId);
            if (r)
            {
                newEdge.from = fromId;
                newEdge.to = toId;
                edges.Add(newEdge);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Id's \"{newEdge.source}\" or \"{newEdge.target}\" were not found");
            }
        }
    }

    public class DiagramNode
    {
        public int id = 0;
        public string stringId = "";
        public string label = "";
    }

    public class DiagramEdge
    {
        public int from = 0;
        public int to = 0;
        public string source = "";
        public string target = "";
        public string label;
    }
}