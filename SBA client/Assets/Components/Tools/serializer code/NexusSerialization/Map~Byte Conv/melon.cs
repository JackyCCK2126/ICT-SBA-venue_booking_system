using static WatermelonDataTool.VarBytifyer;
//using static PieTool.EasyConsole;

namespace WatermelonDataTool.Serializer
{
    //info byte format: name? 10 type? 10 data (10) || name? 10 type? 10 empty_data 10 || name? 10 data? || name (10)
    public enum data_type
    {
        Object,//: not assigned
        Int,
        Int_array,
        Float,
        Float_array,
        String,
        String_Array,
        Byte_Array,
        Nexus,
        Null,
        Wrong_Format,
    }
    //Child
    public class Melon
    {
        public string FieldName;
        public data_type type = data_type.Object;
        public object? obj { get; private set; } = null;
        public Melon(string info_name)
        {
            FieldName = info_name;
        }
        public Melon(string info_name, data_type data_type = data_type.Object, byte[] byte_data = null)
        {
            FieldName = info_name;
            type = data_type;
            //dataBytes = byte_data;
            obj = GetObjFromByte(byte_data, data_type);
        }
        public Melon(string info_name, object obj = null)
        {
            FieldName = info_name;
            this.obj = obj; type = GetInfoTypeOfObj();
        }
        //read
        private static object? GetObjFromByte(byte[] dataBytes, data_type t)//usable half complete
        {//If type not match, it will be considered as byte[].
            switch (t)
            {
                case data_type.Int: return BytesToInt(dataBytes);
                case data_type.Float: return BytesToFloat(dataBytes);
                case data_type.String: return BytesToString(dataBytes);
                case data_type.Nexus: return new Watermelon(dataBytes);
                case data_type.Int_array: return BytesToIntArr(dataBytes);
                case data_type.Byte_Array: return dataBytes;
                case data_type.String_Array: return BytesToStringArr(dataBytes);
                case data_type.Float_array: return BytesToFloatArr(dataBytes);
                case data_type.Object: return ObjDeserialize_DataContract<object>(dataBytes);
                case data_type.Null: return null;
            }
            //error formatt
            return null;
        }
        data_type GetInfoTypeOfObj()//usable half complete
        {
            if (obj == null) return data_type.Null;
            var t = obj.GetType();
            if (t == typeof(int)) return data_type.Int;
            else if (t == typeof(int[])) return data_type.Int_array;
            else if (t == typeof(float)) return data_type.Float;
            else if (t == typeof(float[])) return data_type.Float_array;
            else if (t == typeof(string)) return data_type.String;
            else if (t == typeof(string[])) return data_type.String_Array;
            else if (t == typeof(Watermelon)) return data_type.Nexus;
            else if (t == typeof(byte[])) return data_type.Byte_Array;
            return data_type.Object;
        }

        //Convertions
        public override string ToString()
        {
            string s = FieldName;
            if (type != data_type.Object) s += ":" + type.ToString();

            if (type == data_type.Nexus && obj.GetType() == typeof(Watermelon)) s += "\n" + obj.ToString();
            else s += " = " + Watermelon.Str(obj);

            return s;
        }
        public byte[] objToBytes()//usable half complete
        {
            switch (type)
            {
                case data_type.Int: return IntToBytes((int)obj);
                case data_type.Float: return FloatToBytes((float)obj);
                case data_type.String: return StringToBytes((string)obj);
                case data_type.Nexus: return ((Watermelon)obj).ToBytes();
                case data_type.Object: return SmartSerialize<object>(obj);
                case data_type.Int_array: return IntArrToBytes((int[])obj);
                case data_type.Byte_Array: return (byte[])obj;
                case data_type.String_Array: return StringArrToBytes((string[])obj);
                case data_type.Float_array: return FloatArrToBytes((float[])obj);
                case data_type.Null: return new byte[0];
            }
            return null;
        }

        //edit
        public void SetObjWithBytes(byte[] b, data_type t)
        {
            obj = GetObjFromByte(b, t); type = t;
        }
        public void SetObj(object content)
        {
            obj = content;
            type = GetInfoTypeOfObj();
        }
    }
}