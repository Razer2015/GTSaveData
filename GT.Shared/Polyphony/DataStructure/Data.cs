using System;

namespace GT.Shared.Polyphony.DataStructure
{
    public class Data
    {
        /// <summary>
        /// Values that are set from the constructor according to the correct game (GT6/GT5)
        /// </summary>
        public int Length { get; set; }
        public int StartOffset { get; set; }
        public Game SelectedGame { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Field Fields { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Symbols Symbols { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game"></param>
        public Data(Game game) {
            SelectedGame = game;
            switch (game) {
                case Game.GT6:
                    Length = 0x0C;
                    StartOffset = 0x10;
                    break;
                case Game.GT6_BBB:
                    StartOffset = 0x08;
                    break;
                case Game.GT5:
                    Length = 0x08;
                    StartOffset = 0x20;
                    break;
                case Game.GT5_CARPARAMETER:
                    StartOffset = 0x00;
                    break;
                default: // GT6 values
                    Length = 0x0C;
                    StartOffset = 0x10;
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Boolean ParseSymbols(EndianBinReader reader) {
            reader.BaseStream.Position = StartOffset;
            if (reader.ReadByte() != 0x0E)
                return (false);
            reader.BaseStream.Position = (StartOffset + reader.ReadUInt32());
            Symbols = new Symbols();
            return (Symbols.Parse(reader));
        }
    }
}
