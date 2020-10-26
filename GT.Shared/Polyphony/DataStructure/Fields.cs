using System;
using System.Collections.Generic;

namespace GT.Shared.Polyphony.DataStructure {
    public class Field {
        public Field_09 Root { get; set; }

        public object GetDataBySymbol(String[] symbol) {
            var field = GetField(Root, symbol);
            switch (field.GetType().Name) {
                case "Field_00":
                    break;
                case "Field_01":
                    return (((Field_01)field).Data);
                case "Field_02":
                    return (((Field_02)field).Data);
                case "Field_03":
                    return (((Field_03)field).Data);
                case "Field_04":
                    return (((Field_04)field).Data);
                case "Field_05":
                    return (((Field_05)field).Data);
                case "Field_06":
                    return (((Field_06)field).Data);
                case "Field_07":
                    return (((Field_07)field).Data);
                case "Field_08":
                    break;
                case "Field_09":
                    break;
                case "Field_0A":
                    return (Field_0A)field;
                case "Field_0B":
                    break;
                case "Field_0C":
                    return (((Field_0C)field).Data);
                case "Field_0D":
                    return (((Field_0D)field).Data);
                case "Field_0E":
                    return (((Field_0E)field).Data);
                case "Field_0F":
                    return (((Field_0F)field).Data);
                default:
                    break;
            }
            return (field);
        }

        /// <summary>
        /// Field_01 = Field_01 or SByte
        /// Field_02 = Field_02 or Int16
        /// Field_03 = Field_03 or Int32
        /// Field_04 = Field_04 or Int64
        /// Field_05 = Field_05 or UInt32
        /// Field_06 = Field_06 or Byte[]
        /// Field_07 = Field_07 or String
        /// Field_08 = 
        /// Field_09 = 
        /// Field_0A = Field_0A
        /// Field_0B = 
        /// Field_0C = Field_0C or Byte
        /// Field_0D = Field_0D or UInt16
        /// Field_0E = Field_0E or UInt32
        /// Field_0F = Field_0F or UInt64
        /// </summary>
        /// <param name="symbol">Symbol Path</param>
        /// <param name="data">Data</param>
        public void SetDataBySymbol(String[] symbol, object data) {
            var field = GetField(Root, symbol);
            switch (field.GetType().Name) {
                case "Field_00":
                    break;
                case "Field_01":
                    if (data.GetType().Name.Equals("Field_01"))
                        (field as Field_01).Data = ((Field_01)data).Data;
                    else if (data.GetType().Name.Equals("SByte"))
                        (((Field_01)field).Data) = (sbyte)data;
                    break;
                case "Field_02":
                    if (data.GetType().Name.Equals("Field_02"))
                        (field as Field_02).Data = ((Field_02)data).Data;
                    else if (data.GetType().Name.Equals("Int16"))
                        (((Field_02)field).Data) = (Int16)data;
                    break;
                case "Field_03":
                    if (data.GetType().Name.Equals("Field_03")) {
                        (field as Field_03).Data = ((Field_03)data).Data;
                        (field as Field_03).Item = ((Field_03)data).Item;
                    }
                    else if (data.GetType().Name.Equals("Int32"))
                        (((Field_03)field).Data) = (Int32)data;
                    break;
                case "Field_04":
                    if (data.GetType().Name.Equals("Field_04"))
                        (field as Field_04).Data = ((Field_04)data).Data;
                    else if (data.GetType().Name.Equals("Int64"))
                        (((Field_04)field).Data) = (Int64)data;
                    break;
                case "Field_05":
                    if (data.GetType().Name.Equals("Field_05"))
                        (field as Field_05).Data = ((Field_05)data).Data;
                    else if (data.GetType().Name.Equals("UInt32"))
                        (((Field_05)field).Data) = (UInt32)data;
                    break;
                case "Field_06":
                    if (data.GetType().Name.Equals("Field_06")) {
                        (((Field_06)field).DataLength) = ((Field_06)data).DataLength;
                        (((Field_06)field).Data) = ((Field_06)data).Data;
                    }
                    else if (data.GetType().Name.Equals("Byte[]")) {
                        (((Field_06)field).DataLength) = ((byte[])data).Length;
                        (((Field_06)field).Data) = (byte[])data;
                    }
                    break;
                case "Field_07":
                    if (data.GetType().Name.Equals("Field_07")) {
                        (((Field_07)field).Index) = ((Field_07)data).Index;
                        (((Field_07)field).Data) = ((Field_07)data).Data;
                        (((Field_07)field).Item) = ((Field_07)data).Item;
                    }
                    else if (data.GetType().Name.Equals("String"))
                        (((Field_07)field).Data) = (string)data;
                    break;
                case "Field_08":
                    if (data.GetType().Name.Equals("Field_08")) {
                        ((Field_08)field).Count = ((Field_08)data).Count;
                        ((Field_08)field).Items = ((Field_08)data).Items;
                    }
                    break;
                case "Field_09":
                    if (data.GetType().Name.Equals("Field_09")) {
                        ((Field_09)field).Count = ((Field_09)data).Count;
                        ((Field_09)field).Items = ((Field_09)data).Items;
                    }
                    break;
                case "Field_0A":
                    if (data.GetType().Name.Equals("Field_0A"))
                        ((Field_0A)field).Item = ((Field_0A)data).Item;
                    break;
                case "Field_0B":
                    break;
                case "Field_0C":
                    if (data.GetType().Name.Equals("Field_0C"))
                        (field as Field_0C).Data = ((Field_0C)data).Data;
                    else if (data.GetType().Name.Equals("Byte"))
                        (((Field_0C)field).Data) = (byte)data;
                    break;
                case "Field_0D":
                    if (data.GetType().Name.Equals("Field_0D"))
                        (field as Field_0D).Data = ((Field_0D)data).Data;
                    else if (data.GetType().Name.Equals("UInt16"))
                        (((Field_0D)field).Data) = (UInt16)data;
                    break;
                case "Field_0E":
                    if (data.GetType().Name.Equals("Field_0E"))
                        (field as Field_0E).Data = ((Field_0E)data).Data;
                    else if (data.GetType().Name.Equals("UInt32"))
                        (((Field_0E)field).Data) = (UInt32)data;
                    break;
                case "Field_0F":
                    if (data.GetType().Name.Equals("Field_0F"))
                        (field as Field_0F).Data = ((Field_0F)data).Data;
                    else if (data.GetType().Name.Equals("UInt64"))
                        (((Field_0F)field).Data) = (UInt64)data;
                    break;
                default:
                    break;
            }
        }

        private object GetField(object field, string[] symbol) {
            int depth = 0;
            Field_09 f09 = (Field_09)field;
            for (int i = 0; i < f09.Count; i++) {
                if (f09.Items[i].GetType().Name.Equals("Field_07"))
                    if (((Field_07)f09.Items[i]).Data.Equals(symbol[depth])) {
                        if (depth >= (symbol.Length - 1))
                            return (((Field_07)f09.Items[i]).Item);
                        else {
                            f09 = (Field_09)((Field_07)f09.Items[i]).Item;
                            depth++;
                            i = -1;
                        }
                    }
            }
            return (null);
        }
    }
    public class Field_00 {

    }
    public class Field_01 {
        public sbyte Data { get; set; }

        public Field_01(sbyte data) {
            this.Data = data;
        }
    }
    public class Field_02 {
        public Int16 Data { get; set; }

        public Field_02(Int16 data) {
            this.Data = data;
        }
    }
    public class Field_03 {
        public Int32 Data { get; set; }
        public object Item { get; set; }

        public Field_03(Int32 data) {
            this.Data = data;
        }

        public Field_03(Int32 data, object item) {
            this.Data = data;
            this.Item = item;
        }
    }
    public class Field_04 {
        public Int64 Data { get; set; }

        public Field_04(Int64 data) {
            this.Data = data;
        }
    }
    public class Field_05 {
        public UInt32 Data { get; set; }

        public Field_05(UInt32 data) {
            this.Data = data;
        }
    }
    public class Field_06 {
        public int DataLength { get; set; }
        public byte[] Data { get; set; }

        public Field_06(int dataLength, byte[] data) {
            this.DataLength = dataLength;
            this.Data = data;
        }
    }
    public class Field_07 {
        public UInt16 Index { get; set; }
        public String Data { get; set; }
        public object Item { get; set; }

        public Field_07(UInt16 index, String data) {
            this.Index = index;
            this.Data = data;
        }

        public Field_07(UInt16 index, String data, object item) {
            this.Index = index;
            this.Data = data;
            this.Item = item;
        }
    }
    public class Field_08 {
        public UInt32 Count { get; set; }
        public List<object> Items { get; set; }

        public Field_08(UInt32 count, List<object> items) {
            this.Count = count;
            this.Items = items;
        }
    }
    public class Field_09 {
        public UInt32 Count { get; set; }
        public List<object> Items { get; set; }

        public Field_09(UInt32 count, List<object> items) {
            this.Count = count;
            this.Items = items;
        }
    }
    public class Field_0A {
        public object Item { get; set; }
        public Field_0A(object item) {
            this.Item = item;
        }
    }
    public class Field_0B {

    }
    public class Field_0C {
        public byte Data { get; set; }

        public Field_0C(byte data) {
            this.Data = data;
        }
    }
    public class Field_0D {
        public UInt16 Data { get; set; }

        public Field_0D(UInt16 data) {
            this.Data = data;
        }
    }
    public class Field_0E {
        public UInt32 Data { get; set; }

        public Field_0E(UInt32 data) {
            this.Data = data;
        }
    }
    public class Field_0F {
        public UInt64 Data { get; set; }

        public Field_0F(UInt64 data) {
            this.Data = data;
        }
    }
}
