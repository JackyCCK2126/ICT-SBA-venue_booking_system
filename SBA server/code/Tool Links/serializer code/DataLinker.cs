using System;
using System.Collections.Generic;
using System.Linq;

namespace WatermelonDataTool{

    class DataLinker
    {
        //get all indexs of target byte of which the previous byte is not equal to protector byte.
        public static void get_byte_indexes(ArraySegment<byte> data, out int[] results, byte target_byte, byte? protector = null)
        {//null protecter will not be considered
            List<int> result_list = new List<int>();
            for (int i = 0; i < data.Count; i++)
            {
                if (data.Array[data.Offset + i] == protector) { i++; continue; }//protect non-operator byte (dont use i+=2;)
                if (data.Array[data.Offset + i] == target_byte) { result_list.Add(i); }
            }

            //if data length is 0, there won't be full stops and getMessageInfo will detect wrong format.
            if (data.Count != 0 && data.Array[data.Offset + data.Count - 1] != target_byte) { result_list.Add(data.Count); }
            results = result_list.ToArray();
        }
        public static void split_and_deprotect_byte(/*Slice*/ArraySegment<byte> data_u, out byte[][] data_v, byte byte_splitter, byte? protecter_byte, int min_length = 0)
        {//null protecter will not be considered
            int[] sep_index;
            get_byte_indexes(data_u, out sep_index, byte_splitter, protecter_byte);

            int last_sep = 0; if (sep_index.Length != 0) last_sep = sep_index[sep_index.Length - 1];
            if (last_sep < data_u.Count - 1) sep_index = sep_index.Concat(new int[] { data_u.Count }).ToArray();

            int v_length = sep_index.Length; if (v_length < min_length) v_length = min_length;
            data_v = new byte[v_length][];

            int i = 0, start_index = 0;

            //.net 4
            byte[] data_u_arr = data_u.ToArray();

            for (; i < sep_index.Length; start_index = sep_index[i] + 1, i++)
            {
                //.net 5 6
                //if(protecter_byte==null) data_v[i] = data_u.Slice(start_index, sep_index[i] - start_index).ToArray();
                //else de_protect_bytes(data_u.Slice(start_index, sep_index[i] - start_index).ToArray(), out data_v[i], (byte)protecter_byte);

                //.net 4
                if (protecter_byte == null) data_v[i] = new ArraySegment<byte>(data_u_arr, start_index, sep_index[i] - start_index).ToArray();
                else de_protect_bytes(new ArraySegment<byte>(data_u_arr, start_index, sep_index[i] - start_index).ToArray(), out data_v[i], (byte)protecter_byte);
            }
        }
        public static void combine_and_protect_byte(byte[][] data_u, out byte[] data_v, byte? byte_splitter, byte? protecter_byte, byte[] bytes_to_protect)
        {
            List<byte> result = new List<byte>();
            if (!(protecter_byte == null || bytes_to_protect == null))//protectable
            {
                foreach (byte[] ba in data_u)
                {
                    result.AddRange(Add_protect_bytes(ba, (byte)protecter_byte, bytes_to_protect));
                    if (byte_splitter != null) result.Add((byte)byte_splitter);
                }
            }
            else
                foreach (byte[] ba in data_u)
                {
                    result.AddRange(ba);
                    if (byte_splitter != null) result.Add((byte)byte_splitter);
                }
            result.RemoveAt(result.Count - 1);//remove excess splitter
            data_v = new byte[result.Count];
            result.CopyTo(data_v);
        }

        public static void de_protect_bytes(byte[] data_u, out byte[] data_v, byte protecter)//can improve
        {
            List<byte> v = new List<byte>();
            for (int i = 0; i < data_u.Length; i++)
            {
                if (data_u[i] == protecter && i + 1 != data_u.Length)//check wheater protector byte and protected byte exist simultaneously
                {
                    i++;
                    v.Add(data_u[i]);
                    continue;
                }
                v.Add(data_u[i]);
            }
            data_v = v.ToArray();
        }
        public static byte[] Add_protect_bytes(byte[] data_u, byte protector, byte[] bytes_to_protect)
        {//protector byte will be automaticly added to protect targets

            bytes_to_protect = bytes_to_protect.Concat(new byte[] { protector }).ToArray();

            List<byte> v = new List<byte>();
            for (int i = 0; i < data_u.Length; i++)
            {
                if (Array.Exists(bytes_to_protect, element => element == data_u[i]))//check wheater protective byte and protected byte exist
                {
                    v.Add(protector);
                }
                v.Add(data_u[i]);
            }
            return v.ToArray();
        }
    }
}