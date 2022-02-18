using System;

namespace StatesDiagram
{
    public class ParsedState
    {
        public string Name = "";
        public ShapeTag Tag = new ShapeTag();

        // if state has return path to any previous state
        public bool ToPreviousState = false;
        public ShapeTag ToPreviousStateTag = new ShapeTag();

        //if state was not defined in the file but found in the links
        public bool Orphan = false;
    }

    public class StateLink
    {
        public string FromState = "";
        public string ToState = "";
        public ShapeTag Tag = new ShapeTag();
        public bool ToPreviousState = false;
        public bool ToOrphan = false;
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