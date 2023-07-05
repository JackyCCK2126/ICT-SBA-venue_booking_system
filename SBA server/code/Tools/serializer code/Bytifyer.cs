//This is a serializer contains original from-to-byte serializer and json serializer.
//Main functions: SmartSerialize<T>(T obj); SmartDeserialize<T>(byte[] dataBytes);
using WatermelonDataTool.Serializer;
using static WatermelonDataTool.DataLinker;
using System.Runtime.Serialization;

namespace WatermelonDataTool
{
    static class VarBytifyer
    {
        //string[]
        public static byte[] StringArrToBytes(string[] StringArr, byte array_sep = 36)
        {
            List<byte> vs = new List<byte>();
            foreach (string s in StringArr)
            {
                vs.AddRange(Add_protect_bytes(StringToBytes(s), 234, new byte[] { array_sep }));
                vs.Add(array_sep);
            }
            if (vs.Count != 0) vs.RemoveAt(vs.Count - 1);
            return vs.ToArray();

        }
        public static string[] BytesToStringArr(byte[] data_u, byte array_sep = 36)
        {
            byte[][] bb;
            split_and_deprotect_byte(new ArraySegment<byte>(data_u), out bb, array_sep, 234);
            string[] result = new string[bb.Length];

            for (int i = 0; i < bb.Length; i++)
            {
                result[i] = BytesToString(bb[i]);
            }
            return result;
        }

        //int[]
        public static byte[] IntArrToBytes(int[] IntArr, bool withsign = true, byte array_sep = 36)
        {
            List<byte> vs = new List<byte>();
            foreach (int i in IntArr)
            {
                vs.AddRange(Add_protect_bytes(IntToBytes(i, withsign), 234, new byte[] { array_sep }));
                vs.Add(array_sep);
            }
            if(vs.Count!=0) vs.RemoveAt(vs.Count - 1);
            return vs.ToArray();

        }
        public static int[] BytesToIntArr(byte[] data_u, bool withsign = true, byte array_sep = 36)
        {
            byte[][] bb;
            split_and_deprotect_byte(new ArraySegment<byte>(data_u), out bb, array_sep, 234);
            int[] result = new int[bb.Length];

            for (int i = 0; i < bb.Length; i++)
            {
                result[i] = BytesToInt(bb[i]);
            }
            return result;
        }

        //float[]
        public static byte[] FloatArrToBytes(float[] FloatArr, bool withsign = true, int? to_decimalPlace = 5, byte array_sep = 36)
        {
            List<byte> vs = new List<byte>();
            foreach (float f in FloatArr)
            {
                vs.AddRange(Add_protect_bytes(FloatToBytes(f, withsign, to_decimalPlace), 234, new byte[] { array_sep }));
                vs.Add(array_sep);
            }
            if (vs.Count != 0) vs.RemoveAt(vs.Count - 1);
            return vs.ToArray();

        }
        public static float[] BytesToFloatArr(byte[] data_u, bool withsign = true, byte array_sep = 36)
        {
            byte[][] bb;
            split_and_deprotect_byte(new ArraySegment<byte>(data_u), out bb, array_sep, 234);
            float[] result = new float[bb.Length];

            for (int i = 0; i < bb.Length; i++)
            {
                result[i] = BytesToFloat(bb[i],withsign);
            }
            return result;
        }

        //byte float
        /// Formats
        /// protected: (int)int part | (int)dec part | (int)dec part length
        /// protected: (int)int part
        public static float BytesToFloat(byte[] data, bool withsign = true)
        {
            float result = 0;
            byte[][] splited_data;
            split_and_deprotect_byte(new ArraySegment<byte>(data), out splited_data, 37, 254);
            result = BytesToInt(splited_data[0], withsign);

            if (splited_data.Length == 1) return result;
            float decimal_part = BytesToInt(splited_data[1], false);
            int dec_part_length = splited_data[2][0];//max dec length of float < 255
            if (decimal_part > 0)
            {
                decimal_part /= (int)Math.Pow(10, dec_part_length);
                if (Math.Sign(result) == -1) decimal_part *= -1;

                //print(decimal_part, "d final");print(result,"result");print(decimal_part+result,"sum");/////////////////////////////////////////////

                result += decimal_part;
            }
            return result;
        }
        public static byte[] FloatToBytes(float f, bool withsign = true, int? to_decimalPlace = 3)
        {//max decimal place of float var is 7. more than 7 will be rounded.
            int int_part = (int)f;

            string _ = f.ToString();
            int dec_length = _.Length - _.IndexOf(".") - 1;

            if (to_decimalPlace == null || to_decimalPlace > dec_length) to_decimalPlace = dec_length;
            int decimal_part = (int)Math.Round((f - int_part) * Math.Floor(Math.Pow(10, (int)to_decimalPlace)));

            if (decimal_part == 0) return Add_protect_bytes(IntToBytes(int_part, withsign), 254, new byte[] { 37 });

            List<byte> result = new List<byte>();
            result.AddRange(Add_protect_bytes(IntToBytes(int_part, withsign), 254, new byte[] { 37 })); result.Add(37);
            result.AddRange(Add_protect_bytes(IntToBytes(decimal_part, false), 254, new byte[] { 37 })); result.Add(37);
            result.AddRange(Add_protect_bytes(new byte[] { (byte)to_decimalPlace }, 254, new byte[] { 37 }));//max dec length of float < 255

            return result.ToArray();
        }

        //byte Int conv
        public static byte[] IntToBytes(int number, bool withsign = true, bool p128WhenSigned = true)
        {
            int[] a = DecToBin_IntArray(number, withsign, 0, p128WhenSigned);
            int sign = a[0]; if (withsign) a[0] = 0;

            //int length_before_last = cl.Length / 8;
            int firstbyte_length = a.Length % 8; if (firstbyte_length == 0) firstbyte_length = 8;
            int dnums = (int)Math.Ceiling((double)a.Length / 8), dnum_index = dnums - 1;
            byte[] b = new byte[dnums];
            //print(""+remainder_length+" "+num_byte_length);//
            for (int k = dnum_index; k >= 0; k--)
            {
                int[] bin_Array = new int[8];
                int c_bin_length = 8;
                bool first_byte = (k == dnum_index && firstbyte_length > 0);
                if (first_byte)
                {
                    c_bin_length = firstbyte_length;
                    if (!withsign) bin_Array = new int[firstbyte_length];
                }
                for (int j = c_bin_length - 1; j >= 0; j--)
                {
                    //print(dnum_index+" "+k+" "+bin_Array.Length+" "+j);//

                    int a_index = (firstbyte_length - 1 + (dnum_index - k) * 8 - j);
                    //print(k + " k||j " + j+" : "+ a_index);//

                    bin_Array[bin_Array.Length - 1 - j] = a[a_index];
                }
                if (first_byte && withsign)
                {
                    bin_Array[0] = sign;
                }
                //foreach (int i in bin_Array) Console.Write(i);print();//write bin array/

                b[b.Length - 1 - k] = (byte)BinToDec_Int(bin_Array, false);
            }
            //foreach (byte l in b) Console.Write(" "+l); print();//write byte array/
            return b;
        }
        public static int BytesToInt(byte[] data, bool withsign = true, bool p128WhenSigned = true)
        {//--mode
         //print();//1/
         //result var

            int[] bin_arr = new int[(data.Length) * 8];
            //foreach (int O in bin_arr) print("L " + O); print();//1/
            //print(data.Length + " " + bin_arr.Length);//
            for (int i = 0; i < data.Length; i++)
            {
                DecToBin_IntArray(data[i], false, 8).CopyTo(bin_arr, (i) * 8);
                //foreach (int O in bin_arr) print("L " + O); print();//
            }
            //foreach (int i in bin_arr) print(i + "");//1/
            return BinToDec_Int(bin_arr, withsign, p128WhenSigned);
        }

        //Int base conv
        public static int[] DecToBin_IntArray(int number, bool withsign = true, int minlength = 0, bool p128WhenSigned = true)
        {
            char[] cl = ChangeBase_CharArray(number, 10, 2, (withsign && p128WhenSigned));
            int numoffset = 0; if (withsign) numoffset++; if (cl.Length + numoffset < minlength) numoffset += minlength - numoffset - cl.Length;
            int[] i = new int[cl.Length + numoffset];

            for (int k = 0; k < cl.Length; k++)
            {
                i[k + numoffset] = (int)char.GetNumericValue(cl[k]);
            }

            if (number > 0 && withsign) i[0] = 1;

            return i;
        }
        public static int BinToDec_Int(int[] Bin, bool withsign = true, bool p128WhenSigned = true)
        {
            int result = 0, numlength = Bin.Length; if (withsign) numlength--;
            for (int i = 1; i <= numlength; i++)
            {
                result += Bin[Bin.Length - i] * (int)Math.Pow(2, i - 1);
            }
            if (withsign) { if (Bin[0] == 0) result *= -1; }

            if (withsign && p128WhenSigned)
                // dont use (result >= 0),as p128 mode, (+1 and -0)'s results are both 0
                if (Bin[0] == 1) result++;

            //print(result);//
            return result;
        }
        public static char[] ChangeBase_CharArray(int number, int frombase, int tobase, bool p128 = false)
        {
            if (p128 && number > 0) number--;
            return Convert.ToString(Convert.ToInt32(Math.Abs(number).ToString(), frombase), tobase).ToCharArray();
        }

        //String Byte conv
        public static string BytesToString(byte[] b)
        {
            return System.Text.Encoding.UTF8.GetString(b);
        }
        public static byte[] StringToBytes(string s)
        {
            //if(s==null)return null;byte[]b=new byte[s.Length];for(int i=0;i<s.Length;i++){b[i]=(byte)s[i];}return b;//暫時減省版本
            return System.Text.Encoding.UTF8.GetBytes(s);
        }

        //DataContractSerializer

        /// <summary> no null </summary>
        public static byte[] ObjSerialize_DataContract<T>(T obj)
        {
            if (obj == null)
                return null;

            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            MemoryStream stream = new MemoryStream();
            serializer.WriteObject(stream, obj);
            return stream.ToArray();
        }
        /// <summary> no null </summary>
        public static T ObjDeserialize_DataContract<T>(byte[] data)
        {
            if (data == null)
                return default(T);

            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            MemoryStream stream = new MemoryStream(data);
            return (T)serializer.ReadObject(stream);
        }


        //Auto Type Serializer
        public static T SmartDeserialize<T>(byte[] dataBytes)
        {
            switch (typeof(T))
            {
                case Type intType when intType == typeof(int):
                    return (T)(object)BytesToInt(dataBytes);
                case Type floatType when floatType == typeof(float):
                    return (T)(object)BytesToFloat(dataBytes);
                case Type stringType when stringType == typeof(string):
                    return (T)(object)BytesToString(dataBytes);
                case Type infoGroupType when infoGroupType == typeof(Serializer.Watermelon):
                    return (T)(object)new Serializer.Watermelon(dataBytes);
                case Type intArrayType when intArrayType == typeof(int[]):
                    return (T)(object)BytesToIntArr(dataBytes);
                case Type byteArrayType when byteArrayType == typeof(byte[]):
                    return (T)(object)dataBytes;
                case Type stringArrayType when stringArrayType == typeof(string[]):
                    return (T)(object)BytesToStringArr(dataBytes);
                case Type floatArrayType when floatArrayType == typeof(float[]):
                    return (T)(object)BytesToFloatArr(dataBytes);
            }
            return (T)ObjDeserialize_DataContract<T>(dataBytes);
        }

        public static byte[] SmartSerialize<T>(T obj)
        {
            switch (typeof(T))
            {
                case Type intType when intType == typeof(int):
                    Console.WriteLine("aaa");
                    return IntToBytes((int)(object)obj);
                case Type floatType when floatType == typeof(float):
                    return FloatToBytes((float)(object)obj);
                case Type stringType when stringType == typeof(string):
                    return StringToBytes((string)(object)obj);
                case Type infoGroupType when infoGroupType == typeof(Serializer.Watermelon):
                    return ((Watermelon)(object)obj).ToBytes();
                case Type intArrayType when intArrayType == typeof(int[]):
                    return IntArrToBytes((int[])(object)obj);
                case Type byteArrayType when byteArrayType == typeof(byte[]):
                    return (byte[])(object)obj;
                case Type stringArrayType when stringArrayType == typeof(string[]):
                    return StringArrToBytes((string[])(object)obj);
                case Type floatArrayType when floatArrayType == typeof(float[]):
                    return FloatArrToBytes((float[])(object)obj);
            }
            return ObjSerialize_DataContract<T>(obj);
        }


    }
}