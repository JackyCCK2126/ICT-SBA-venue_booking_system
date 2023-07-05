using System;
using static WatermelonDataTool.DataLinker;
using static WatermelonDataTool.VarBytifyer;
//using static PieTool.EasyConsole;
using System.Linq;
namespace WatermelonDataTool.Serializer
{
    partial class Encoding
    {
    //non-markup combination: 123 123           123 13            123 10            123 36          123 37
        public static byte protect_byte = 123, info_sep = 127, element_sep = 128, array_sep = 200, unit_sep = 170;
        //protect_byte: to indicate that the next byte is not a mark up byte | param_sep: sep elements in an info | unit_sep: sep parts of an element eg. decimal
        //usage of protect_byte: 123 123, 123 127, 123 170, etc.

        public static Melon[] ToFields(byte[] message_data)
        {
            int[] sep_index;//index of chunk_seperator
            //eg. 0 1 2 3 4 5 6 7 >8< 9 10 11 12 13 14 >15<
            Melon[] fields;//info in every chunks

            //find out seperators and set array length
            get_byte_indexes(new ArraySegment<byte>(message_data), out sep_index, info_sep, protect_byte);
            fields = new Melon[sep_index.Length];

            //extract info from chunks
            {
                for (int i = 0, last_start = 0; i < sep_index.Length; last_start = sep_index[i] + 1, i++)
                {

                    //slit params from a chunk
                    byte[][] p;
                    split_and_deprotect_byte(new ArraySegment<byte>(message_data, last_start, sep_index[i] - last_start), out p, element_sep, protect_byte);
                    data_type type = data_type.Object;

                    //print(p.Length,"p length");//de

                    //try to get a type information
                    if (p.Length > 2 && p[1] != null && p[1].Length == 1) type = (data_type)p[1][0];

                    if (p.Length > 1) fields[i] = new Melon(BytesToString(p[0]), type, (p[p.Length - 1]));//have content
                    else if(p.Length==1) fields[i] = new Melon(BytesToString(p[0]), type, new byte[0]);//no content
                    else { fields[i] = new Melon("", data_type.Wrong_Format, new byte[0]); continue; }//error

                }

            }

            return fields;
        }

    }
}
