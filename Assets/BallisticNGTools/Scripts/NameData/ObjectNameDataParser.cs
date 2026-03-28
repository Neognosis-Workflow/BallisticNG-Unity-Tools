#if UNITY_EDITOR
using System.Collections.Generic;

namespace NgData.NameData
{
    /// <summary>
    /// Parses a string to determine data for automatic configuration in the Unity editor.
    /// </summary>
    public class ObjectNameDataParser
    {
        private ObjectNameDataParser() { }
        
        /// <summary>
        /// Creates a new ObjectNameDataParser instance and parses a string into it.
        /// </summary>
        public static ObjectNameData[] Parse(string name)
        {
            List<ObjectNameData> parsed = new List<ObjectNameData>();
            ObjectNameData data = null;
            ObjectNameData.VariableData varData = null;

            EParseMode mode = EParseMode.Name;
            
            NameDataParseEnumerator parser = new NameDataParseEnumerator(name, 0);
            parser.OnParseStartCharacter += p => // create new object
            {
                FinishObjectProcess();
                
                p.Buffer.Clear();
                varData = null;

                data = new ObjectNameData();
                
                mode = EParseMode.Name;
            };

            parser.OnParseSeperatorCharacter += p =>
            {
                if (mode == EParseMode.VariableData || mode == EParseMode.VariableName)
                { 
                    p.Buffer.Append(p.Character);
                    return;
                }

                if (mode == EParseMode.Name) // append object name
                {
                    if (data != null) data.ObjectType = p.Buffer.ToString();
                    p.Buffer.Clear();
                    
                    mode = EParseMode.Flags;
                } else if (mode == EParseMode.Flags) // append flag
                {
                    data?.Flags.Add(p.Buffer.ToString());
                    p.Buffer.Clear();
                }
            };

            parser.OnParseStartVariableBlock += p =>
            {
                switch (mode)
                {
                    case EParseMode.Name when data != null:
                        data.ObjectType = p.Buffer.ToString();
                        break;
                    case EParseMode.VariableName:
                        throw p.Exception("Variable block start inside of variable block");
                    case EParseMode.VariableData:
                        throw p.Exception("Variable block start inside of variable block");
                    case EParseMode.Flags:
                        data?.Flags.Add(p.Buffer.ToString());
                        break;
                }

                p.Buffer.Clear();
                mode = EParseMode.VariableName;
            };

            parser.OnParseEndVariableBlock += p =>
            {
                if (mode == EParseMode.Name) throw p.Exception("Variable block end before action name");
                if (mode == EParseMode.Flags) throw p.Exception("Variable block end outside of variable block");
                if (mode == EParseMode.VariableName) throw p.Exception("Variable block end before a variables value was defined");

                if (varData != null)
                {
                    varData.Value = p.Buffer.ToString();
                    data?.Variables.Add(varData);
                }
                
                p.Buffer.Clear();
                mode = EParseMode.EndVariable;
            };

            parser.OnParseVariableSeperator += p =>
            {
                if (mode != EParseMode.VariableData) throw p.Exception("Variable seperator before variable data delcared");

                if (varData != null) varData.Value = p.Buffer.ToString();
                p.Buffer.Clear();

                data?.Variables.Add(varData);
                varData = null;

                mode = EParseMode.VariableName;
            };

            parser.OnParseVariableValueStart += p =>
            {
                if (mode != EParseMode.VariableName) throw p.Exception("Variable value start without declaring name");

                varData = new ObjectNameData.VariableData { Name = p.Buffer.ToString() };

                p.Buffer.Clear();
                mode = EParseMode.VariableData;
            };

            while (parser.MoveNext()) { }
            
            FinishObjectProcess();

            void FinishObjectProcess()
            {
                if (data == null) return;
                
                switch (mode)
                {
                    case EParseMode.Name:
                        data.ObjectType = parser.Buffer.ToString();
                        break;
                    case EParseMode.Flags:
                        data.Flags.Add(parser.Buffer.ToString());
                        break;
                }

                parsed.Add(data);
            }
            
            return parsed.ToArray();
        }
        
        private enum EParseMode
        {
            Name,
            Flags,
            VariableName,
            VariableData,
            EndVariable
        }
    }
}
#endif