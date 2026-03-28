#if UNITY_EDITOR
using System;
using System.Text;

namespace NgData.NameData
{
    public class NameDataParseEnumerator
    {
        /// <summary>
        /// The character that signifies the start of a variable value.
        /// </summary>
        public const char CharVarValue = ':';
        
        /// <summary>
        /// The character that signfies the start of a variable block.
        /// </summary>
        public const char CharStartVars = '[';

        /// <summary>
        /// The character  that signifies the start of a new variable in a variable block.
        /// </summary>
        public const char CharVarSeperator = ',';
        
        /// <summary>
        /// The character that signifies the end of a variable block.
        /// </summary>
        public const char CharEndVars = ']';
        
        /// <summary>
        /// The character that signifies a block where parsing will be skipped. 
        /// </summary>
        public const char CharString = '"';
        
        /// <summary>
        /// The character that signifies an escape while inside of a parse skip block.
        /// </summary>
        public const char CharEscape = '\\';

        /// <summary>
        /// The character that signifies the start of parsable data.
        /// </summary>
        public const char CharStart = '$';

        /// <summary>
        /// The symbol that signifies the seperation of symbols.
        /// </summary>
        public const char CharSeperator = '_';
        
        private NameDataParseEnumerator() { }

        /// <summary>
        /// Creates a new engine instance.
        /// </summary>
        public NameDataParseEnumerator(string name, int startIndex)
        {
            Name = name;
            Index = 0;
        }
        
        /// <summary>
        /// The name that the parser is enumerating through.
        /// </summary>
        public string Name { private set; get; }

        /// <summary>
        /// The buffer being written to.
        /// </summary>
        public StringBuilder Buffer { private set; get; } = new StringBuilder();
        
        /// <summary>
        /// Whether the parser has found an object.
        /// </summary>
        public bool HasObject { private set; get; }
        
        /// <summary>
        /// The current index of the parser.
        /// </summary>
        public int Index;
        
        /// <summary>
        /// The last parsed character.
        /// </summary>
        public char Character { private set; get; }
        
        /// <summary>
        /// Whether the next character read will be escaped and placed into the buffer without further processing.
        /// </summary>
        public bool Escape { private set; get; }
        
        /// <summary>
        /// Whether the parser is currently inside of a string where any characters will be placed into the buffer without
        /// further processing.
        /// </summary>
        public bool InString { private set; get; }

        /// <summary>
        /// Whether the parser has finished.
        /// </summary>
        public bool HasFinished => Index >= Name.Length;

        public Action<NameDataParseEnumerator> OnParseStartCharacter;
        public Action<NameDataParseEnumerator> OnParseSeperatorCharacter;
        public Action<NameDataParseEnumerator> OnParseStartVariableBlock;
        public Action<NameDataParseEnumerator> OnParseEndVariableBlock;
        public Action<NameDataParseEnumerator> OnParseVariableSeperator;
        public Action<NameDataParseEnumerator> OnParseVariableValueStart;
        
        /// <summary>
        /// Moves to the next character if possible.
        /// </summary>
        /// <returns>Whether the parser has finished.</returns>
        public bool MoveNext()
        {
            if (string.IsNullOrEmpty(Name)) return false;
            if (HasFinished) return false;

            Character = Name[Index++];

            // Passthrough
            if (Escape)
            {
                Escape = false;
                Buffer.Append(Character);
                return !HasFinished;
            }

            if (InString && Character != CharString)
            {
                Buffer.Append(Character);
                return !HasFinished;
            }

            // Characters
            switch (Character)
            {
                case CharEscape:
                    Escape = true;
                    return !HasFinished;
                case CharString:
                    InString = !InString;
                    return !HasFinished;
                case CharStart:
                    HasObject = true;
                    OnParseStartCharacter?.Invoke(this);
                    return !HasFinished;
                case CharSeperator:
                    if (HasObject) OnParseSeperatorCharacter?.Invoke(this);
                    return !HasFinished;
                case CharStartVars:
                    if (HasObject) OnParseStartVariableBlock?.Invoke(this);
                    return !HasFinished;
                case CharEndVars:
                    if (HasObject) OnParseEndVariableBlock?.Invoke(this);
                    return !HasFinished;
                case CharVarSeperator:
                    if (HasObject) OnParseVariableSeperator?.Invoke(this);
                    return !HasFinished;
                case CharVarValue:
                    if (HasObject) OnParseVariableValueStart?.Invoke(this);
                    return !HasFinished;
                default:
                    if (Character != ' ') Buffer.Append(Character);
                    return !HasFinished;
            }
        }

        public Exception Exception(string message)
        {
            string upToBuffer = Name.Substring(0, Index);
            return new Exception($"{message} @{Index - 1}\n{upToBuffer} <--[HERE]");
        }
    }
}
#endif