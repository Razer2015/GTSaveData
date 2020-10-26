using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GT.Shared.Polyphony.DataStructure
{
    public class PDTree
    {
        /// <summary>
        /// 
        /// </summary>
        public Data PDTreeData { get; set; }
        public byte[] FileData { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="data"></param>
        /// <param name="game"></param>
        public PDTree(byte[] data, Game game) {
            PDTreeData = new Data(game);
            FileData = data;
        }

        /// <summary>
        /// Additional Constructor
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="game"></param>
        public PDTree(String filepath, Game game) {
            PDTreeData = new Data(game);
            FileData = File.ReadAllBytes(filepath);
        }

        public void Read() {
            try {
                MemoryStream ms = new MemoryStream(this.FileData);
                EndianBinReader reader = new EndianBinReader(ms);
                PDTreeData.ParseSymbols(reader);
                Field rootField = new Field();
                reader.BaseStream.Position = (PDTreeData.StartOffset + 0x05);
                var rootNode = Read(reader, true, false);
                if (rootNode.GetType().Name.Equals("Field_0A")) {
                    rootField.Root = (Field_09)((Field_0A)rootNode).Item;
                }
                else {
                    rootField.Root = (Field_09)rootNode;
                }
                PDTreeData.Fields = rootField;
            }
            catch (Exception ex) {
                throw new Exception("Error: Unable to read the save data tree. Is the save corrupted?");
            }
        }

        public void Write() {
            MemoryStream ms = new MemoryStream();
            EndianBinWriter writer = new EndianBinWriter(ms);
            writer.Write((byte)0x0E);
            writer.Write((Int32)0x00);  // Preparation for a PDTree length
            List<string> symbols = new List<string>();
            Write(writer, PDTreeData.Fields.Root, ref symbols);
            PDTreeData.Symbols.Data = symbols.ToArray();
            int len = (int)writer.BaseStream.Length; // Save the length
            Util.PackValueAndAdvance(writer, (uint)PDTreeData.Symbols.Data.Length); // Write the key count
            for (int i = 0; i < PDTreeData.Symbols.Data.Length; i++) // Write all the symbols
                writer.Write(PDTreeData.Symbols.Data[i]);
            writer.BaseStream.Position = 0x01;
            writer.Write(len);
            FileData = ms.ToArray();
#if DEBUG
            File.WriteAllBytes("repack_" + PDTreeData.SelectedGame.ToString() + ".bin", FileData);
#endif
        }

        public string GenerateText(bool debug = false) {
            Field fields = PDTreeData.Fields;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"/*");
            sb.AppendLine($" * File created with xfileFIN's PDTree parser v1.0");
            sb.AppendLine($" * Creation Time: {DateTime.Now}");
            sb.AppendLine($" * Symbol count: {PDTreeData.Symbols.Data.Length}");
            sb.AppendLine($"*/");
            int depth = -1;
            PDTreeToText(ref sb, depth, fields.Root, debug);
            return (sb.ToString());
        }

        public string GenerateSymbolFile(bool debug = false) {
            var symbols = PDTreeData.Symbols;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"/*");
            sb.AppendLine($" * File created with xfileFIN's PDTree parser v1.0");
            sb.AppendLine($" * Creation Time: {DateTime.Now}");
            sb.AppendLine($" * Symbol count: {PDTreeData.Symbols.Data.Length}");
            sb.AppendLine($"*/");
            sb.AppendLine();
            for (uint i = 0; i < symbols.Data.Length; i++) {
                sb.AppendLine($"0x07{BitConverter.ToString(Util.PackedValue(i)).Replace("-", "")} - {symbols.Data[i]}");
            }
            return (sb.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="debug"></param>
        /// <param name="getData"></param>
        /// <param name="key_config"></param>
        /// <returns></returns>
        public object Read(EndianBinReader reader, bool debug, bool getData, bool key_config = false) {
            switch (reader.ReadByte()) {
                case 0x00: //Padding 1Byte
                    return (new Field_00());
                case 0x01: //Byte
                    return (new Field_01(reader.ReadSByte()));
                case 0x02: //Word (2Bytes)
                    return (new Field_02(reader.ReadInt16()));
                case 0x03: //LongWord (4Bytes)
                    if (key_config) {
                        if (getData)
                            return (new Field_03(reader.ReadInt32()));
                        else
                            return (new Field_03(reader.ReadInt32(), Read(reader, debug, true, true)));
                    }
                    else
                        return (new Field_03(reader.ReadInt32()));
                case 0x04: //DoubleLongword (8Bytes)
                    return (new Field_04(reader.ReadInt64()));
                case 0x05: //LongWord (4bytes)
                    return (new Field_05(reader.ReadUInt32()));
                case 0x06: //Variable Length
                    int dataLength = reader.ReadInt32();
                    return (new Field_06(dataLength, reader.ReadBytes(dataLength)));
                case 0x07: //Key
                    UInt16 key = (UInt16)Util.ExtractValueAndAdvance(reader);
                    if (getData)
                        return (new Field_07(key, PDTreeData.Symbols.Data[key]));
                    else
                        return (new Field_07(key, PDTreeData.Symbols.Data[key], Read(reader, debug, true)));
                case 0x08: //Struct (Array)
                    UInt32 numKeys_08 = reader.ReadUInt32();
                    List<object> objects_08 = new List<object>();
                    for (int i = 0; i < numKeys_08; i++) {
                        objects_08.Add(Read(reader, debug, true));
                    }
                    return (new Field_08(numKeys_08, objects_08));
                case 0x09: // Struct
                    UInt32 numKeys_09 = reader.ReadUInt32();
                    List<object> objects_09 = new List<object>();
                    for (int i = 0; i < numKeys_09; i++) {
                        objects_09.Add(Read(reader, false, false, true));
                    }
                    return (new Field_09(numKeys_09, objects_09));
                case 0x0A: //Struct
                    return (new Field_0A(Read(reader, debug, false)));
                case 0x0B:
                    return (new Field_0B());
                case 0x0C: //Byte
                    return (new Field_0C(reader.ReadByte()));
                case 0x0D: //Word (2Bytes)
                    return (new Field_0D(reader.ReadUInt16()));
                case 0x0E: //Longword (4Bytes)
                    return (new Field_0E(reader.ReadUInt32()));
                case 0x0F: //DoubleLongword (8Bytes)
                    return (new Field_0F(reader.ReadUInt64()));
                default:
                    throw new Exception("Error: Unknown field!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="field"></param>
        /// <param name="isData"></param>
        private void Write(EndianBinWriter writer, object field, ref List<string> symbols, bool isData = false) {
            switch (field.GetType().Name) {
                case "Field_00": //Padding 1Byte
                    writer.Write((byte)0x00);
                    break;
                case "Field_01": //Byte
                    writer.Write((sbyte)0x01);
                    writer.Write(((Field_01)field).Data);
                    break;
                case "Field_02": //Word (2Bytes)
                    writer.Write((byte)0x02);
                    writer.Write(((Field_02)field).Data);
                    break;
                case "Field_03": //LongWord (4Bytes)
                    writer.Write((byte)0x03);
                    Field_03 field_03 = (Field_03)field;
                    writer.Write(((Field_03)field).Data);
                    if (field_03.Item != null)
                        Write(writer, field_03.Item, ref symbols);
                    break;
                case "Field_04": //DoubleLongword (8Bytes)
                    writer.Write((byte)0x04);
                    writer.Write(((Field_04)field).Data);
                    break;
                case "Field_05": //LongWord (4bytes)
                    writer.Write((byte)0x05);
                    writer.Write(((Field_05)field).Data);
                    break;
                case "Field_06": //Variable Length
                    writer.Write((byte)0x06);
                    writer.Write(((Field_06)field).DataLength);
                    writer.Write(((Field_06)field).Data);
                    break;
                case "Field_07": //Key
                    writer.Write((byte)0x07);
                    Field_07 field_07 = (Field_07)field;
                    int keyindex = -1;
                    if (symbols.Contains(field_07.Data))
                        keyindex = Array.IndexOf(symbols.ToArray(), field_07.Data);
                    else
                        symbols.Add(field_07.Data);
                    if (keyindex == -1)
                        keyindex = (symbols.Count - 1);
                    Util.PackValueAndAdvance(writer, (uint)keyindex);
                    if (field_07.Item != null)
                        Write(writer, field_07.Item, ref symbols, true);
                    break;
                case "Field_08": //Struct (Array)
                    writer.Write((byte)0x08);
                    Field_08 field_08 = (Field_08)field;
                    writer.Write(field_08.Count);
                    for (int i = 0; i < field_08.Count; i++) {
                        Write(writer, field_08.Items[i], ref symbols);
                    }
                    break;
                case "Field_09": // Struct
                    writer.Write((byte)0x09);
                    Field_09 field_09 = (Field_09)field;
                    writer.Write(field_09.Count);
                    for (int i = 0; i < field_09.Count; i++) {
                        Write(writer, field_09.Items[i], ref symbols);
                    }
                    break;
                case "Field_0A": //Struct
                    writer.Write((byte)0x0A);
                    Write(writer, ((Field_0A)field).Item, ref symbols);
                    break;
                case "Field_0B":
                    break;
                case "Field_0C": //Byte
                    writer.Write((byte)0x0C);
                    writer.Write(((Field_0C)field).Data);
                    break;
                case "Field_0D"://Word (2Bytes)
                    writer.Write((byte)0x0D);
                    writer.Write(((Field_0D)field).Data);
                    break;
                case "Field_0E": //Longword (4Bytes)
                    writer.Write((byte)0x0E);
                    writer.Write(((Field_0E)field).Data);
                    break;
                case "Field_0F": //DoubleLongword (8Bytes)
                    writer.Write((byte)0x0F);
                    writer.Write(((Field_0F)field).Data);
                    break;
                default:
                    throw new Exception("Error: Unexpected error when writing the PDTree.");
            }
        }

        private void PDTreeToText(ref StringBuilder sb, int depth, object field, bool debug, bool isData = false) {
            switch (field.GetType().Name) {
                case "Field_00": //Padding 1Byte
                    sb.AppendLine(string.Concat(Enumerable.Repeat("\t", depth + 1)) + "0x00 | (null)");
                    break;
                case "Field_01": //Byte
                    sb.AppendLine(string.Concat(Enumerable.Repeat("\t", depth + 1)) + (debug ? "0x01 | SByte | " + ((Field_01)field).Data : ((Field_01)field).Data.ToString()));
                    break;
                case "Field_02": //Word (2Bytes)
                    sb.AppendLine(string.Concat(Enumerable.Repeat("\t", depth + 1)) + (debug ? "0x02 | Int16 | " + ((Field_02)field).Data : ((Field_02)field).Data.ToString()));
                    break;
                case "Field_03": //LongWord (4Bytes)
                    Field_03 field_03 = (Field_03)field;
                    if (field_03.Item != null)
                        sb.AppendLine(string.Concat(Enumerable.Repeat("\t", depth + 1)) + $"#Begin {field_03.Data}");

                    if (field_03.Item != null) {
                        sb.AppendLine(string.Concat(Enumerable.Repeat("\t", depth + 1)) + (debug ? "0x03 | 0x03 Field | " + field_03.Data : field_03.Data.ToString()));
                        PDTreeToText(ref sb, depth + 1, field_03.Item, debug);
                        sb.AppendLine(string.Concat(Enumerable.Repeat("\t", depth + 1)) + $"#End {field_03.Data}");
                    }
                    else {
                        sb.AppendLine(string.Concat(Enumerable.Repeat("\t", depth + 1)) + (debug ? "0x03 | Int32 | " + ((Field_03)field).Data : ((Field_03)field).Data.ToString()));
                    }
                    break;
                case "Field_04": //DoubleLongword (8Bytes)
                    sb.AppendLine(string.Concat(Enumerable.Repeat("\t", depth + 1)) + (debug ? "0x04 | Int64 | " + ((Field_04)field).Data : ((Field_04)field).Data.ToString()));
                    break;
                case "Field_05": //LongWord (4bytes)
                    sb.AppendLine(string.Concat(Enumerable.Repeat("\t", depth + 1)) + (debug ? "0x05 | UInt32 | " + ((Field_05)field).Data : ((Field_05)field).Data.ToString()));
                    break;
                case "Field_06": //Variable Length
                    sb.AppendLine(string.Concat(Enumerable.Repeat("\t", depth + 1)) + (debug ? "0x06 | Byte[] | " + ByteArrayToString(((Field_06)field).Data) : ByteArrayToString(((Field_06)field).Data)));
                    break;
                case "Field_07": //Key
                    if (isData) {
                        sb.AppendLine(string.Concat(Enumerable.Repeat("\t", depth + 1)) + (debug ? "0x07 | String Data | " + ((Field_07)field).Data : ((Field_07)field).Data));
                        break;
                    }
                    else {
                        Field_07 field_07 = (Field_07)field;
                        if (field_07.Item != null)
                            sb.AppendLine(string.Concat(Enumerable.Repeat("\t", depth)) + $"#Begin {field_07.Data}");
                        sb.AppendLine(string.Concat(Enumerable.Repeat("\t", depth)) + (debug ? "0x07 | String Key | " + field_07.Data : field_07.Data));
                        if (field_07.Item != null) {
                            PDTreeToText(ref sb, depth++, field_07.Item, debug, true);
                            sb.AppendLine(string.Concat(Enumerable.Repeat("\t", depth - 1)) + $"#End {field_07.Data}");
                        }
                        break;
                    }
                case "Field_08": //Struct
                    Field_08 field_08 = (Field_08)field;
                    if (debug) {
                        sb.AppendLine(string.Concat(Enumerable.Repeat("\t", depth + 1)) + (
                            $"0x08 | Item count: {field_08.Count}"));
                    }
                    depth++;
                    for (int i = 0; i < field_08.Count; i++) {
                        PDTreeToText(ref sb, depth, field_08.Items[i], debug);
                    }
                    break;
                case "Field_09": // Struct
                    Field_09 field_09 = (Field_09)field;
                    if (debug) {
                        sb.AppendLine(string.Concat(Enumerable.Repeat("\t", depth + 1)) + (
                            $"0x09 | Keys: {field_09.Count}"));
                    }
                    depth += 2;
                    for (int i = 0; i < field_09.Count; i++) {
                        PDTreeToText(ref sb, depth, field_09.Items[i], debug);
                    }
                    break;
                case "Field_0A": //Struct
                    Field_0A field_0A = (Field_0A)field;
                    sb.AppendLine(string.Concat(Enumerable.Repeat("\t", depth + 1)) + ("0x0A | 0x0A Field"));
                    PDTreeToText(ref sb, depth + 1, field_0A.Item, debug);
                    break;
                case "Field_0B":
                    sb.AppendLine(string.Concat(Enumerable.Repeat("\t", depth + 1)) + "0x0B | (null)");
                    break;
                case "Field_0C": //Byte
                    sb.AppendLine(string.Concat(Enumerable.Repeat("\t", depth + 1)) + (debug ? "0x0C | Byte | " + ((Field_0C)field).Data : ((Field_0C)field).Data.ToString()));
                    break;
                case "Field_0D"://Word (2Bytes)
                    sb.AppendLine(string.Concat(Enumerable.Repeat("\t", depth + 1)) + (debug ? "0x0D | UInt16 | " + ((Field_0D)field).Data : ((Field_0D)field).Data.ToString()));
                    break;
                case "Field_0E": //Longword (4Bytes)
                    sb.AppendLine(string.Concat(Enumerable.Repeat("\t", depth + 1)) + (debug ? "0x0E | UInt32 | " + ((Field_0E)field).Data : ((Field_0E)field).Data.ToString()));
                    break;
                case "Field_0F": //DoubleLongword (8Bytes)
                    sb.AppendLine(string.Concat(Enumerable.Repeat("\t", depth + 1)) + (debug ? "0x0F | UInt64 | " + ((Field_0F)field).Data : ((Field_0F)field).Data.ToString()));
                    break;
                default:
                    sb.AppendLine(string.Concat(Enumerable.Repeat("\t", depth + 1)) + "Unknown Field!");
                    break;
            }
        }

        private string ByteArrayToString(byte[] ba) {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}
