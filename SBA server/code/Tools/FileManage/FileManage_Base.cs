using System;
using System.IO;
using System.Linq;
//base
partial class FileManage{
    
    /*//
    private static void File_Stream_Example()
    {
        //code note
        using (FileStream newFile = new FileStream(@"C:\Users\Jacky\Downloads\b.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            for (var i = 0; i < 255; i++)
            {
                newFile.WriteByte((byte)i);
            }
        }
    }
    //*/

    public static byte[] read(string @file_path)
    {
        return File.ReadAllBytes(@file_path);
    }
    public static void write(string @file_path, byte[] Data)
    {//create and write

        if (!File.Exists(file_path))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file_path));
            //using (File.Create(file_path)) {/*do nothing*/}
        }
        File.WriteAllBytes(@file_path, Data);
    }
    public static void writeAsync(string file_path, byte[] Data)
    {//create and write
        if (!File.Exists(file_path))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file_path));
            //using (File.Create(file_path)) {/*do nothing*/}
        }
        File.WriteAllBytesAsync(@file_path, Data);
    }

    public static bool IsFile(string @path)
	{
		return !File.GetAttributes(path).HasFlag(FileAttributes.Directory);
	}
    public static bool IsDirectory(string @path)
    {
        return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
    }

    public static void get_directorie_names(string @root, out string[] directories)
	{
		directories = new DirectoryInfo(root)
			.GetDirectories()
			.Select(subDirectory => subDirectory.Name).ToArray();
	}
	public static void get_file_names(string @root, out string[] file_names)
	{
		file_names = new DirectoryInfo(root)
			.GetFiles()
			.Select(f => f.Name).ToArray();
	}
}

