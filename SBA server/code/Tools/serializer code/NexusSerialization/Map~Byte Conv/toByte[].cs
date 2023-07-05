using System.Collections.Generic;
using static WatermelonDataTool.DataLinker;
//using static PieTool.EasyConsole;

namespace WatermelonDataTool.Serializer
{
    partial class Encoding
    {
        static byte[] bytes_to_protect { get { return new byte[] { info_sep, element_sep, array_sep, unit_sep }; } }

        public static byte[] ToBytes(Melon[] fields)
        {
            List<byte> result = new List<byte>();
            foreach (Melon field in fields)
            {
                //add name
                result.AddRange(Add_protect_bytes(VarBytifyer.StringToBytes(field.FieldName), protect_byte, bytes_to_protect));
                //add type
                if (field.type != data_type.Object)
                {
                    result.Add(element_sep);
                    result.AddRange(Add_protect_bytes(new byte[] { (byte)field.type }, protect_byte, bytes_to_protect));
                }

                byte[] content_bytes = field.objToBytes();
                if ((content_bytes == null || content_bytes.Length == 0))//if no data
                {
                    if (field.type != data_type.Object)//if have type
                    { result.Add(element_sep); result.Add(element_sep); }
                }
                else//have data
                {
                    result.Add(element_sep);
                    result.AddRange(Add_protect_bytes(field.objToBytes(), protect_byte, bytes_to_protect));
                }

                result.Add(info_sep);
            }

            return result.ToArray();
        }

    }
}
