using System;
using System.Collections.Generic;
using System.Text;

namespace GT.Shared.Polyphony.DataStructure
{
    public class Symbols
    {
        /// <summary>
        /// 
        /// </summary>
        public string[] Data { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Symbols() { }

        /// <summary>
        /// Parse all the symbols into String[]
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public bool Parse(EndianBinReader reader) {
            var data = new List<string>();
            var numKeys = Util.ExtractValueAndAdvance(reader);

            for (var i = 0; i <= (numKeys - 1); i++) {
                var sb = new StringBuilder();
                int length = reader.ReadByte();
                for (var j = 0; j < length; ++j)
                    sb.Append((char)reader.ReadByte());
                data.Add(sb.ToString());
            }
            Data = data.ToArray();

            return true;
        }
    }
}
