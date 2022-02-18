using System;
using System.Collections.Generic;

namespace StatesDiagram
{
    public class ParsedState
    {
        public string Name = "";
        public ShapeTag Tag = new ShapeTag();
        public List<StateLink> LinksTo = new List<StateLink>();

        // if state has return path to any previous state
        public bool ToPreviousState = false;
        public ShapeTag ToPreviousStateTag = new ShapeTag();
    }

    public class StateLink
    {
        public string ToState = "";
        public ShapeTag Tag = new ShapeTag();
        public string FileName = "";
        public string JsonPath = "";
        public bool ToPreviousState = false;
    }

    public class ShapeTag
    {
        public string FileName = "";
        public string JsonPath = "";
        public string Description = "";
        public string Color = "";

        public override string ToString()
        {
            return $"File: {FileName}{Environment.NewLine}JSON path: {JsonPath}{Environment.NewLine}Description: {Description}";
        }
    }
}