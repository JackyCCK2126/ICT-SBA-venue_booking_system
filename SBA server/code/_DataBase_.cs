using System.IO;
using WatermelonDataTool.Serializer;

class DataBase : FileManage
{
	public static string root_directory = "/Users/jacky/Desktop/VS project/SBA server/DataBase/";

    /// <summary>
	/// if file not found, return an empty serial class.
	/// </summary>
    public static Watermelon GetFromPath(string @path)
	{
		if (!Path.Exists(Path.Combine(root_directory,path))) return new();
		byte[] data = read(Path.Combine(root_directory,path));
		Decrypt(data,12345678);
		return new Watermelon(data);
    }

    /// <summary>
	/// automaticaly creates directories
	/// </summary>
    public static void SaveToPath(string @path ,Watermelon informations)
	{
		byte[] data = informations.ToBytes();
		Encrypt(data,12345678);
        write(Path.Combine(root_directory, path), data);
	}
	public static void Encrypt(byte[] data, int password)
	{
		//operation depends on date
	}
	public static void Decrypt(byte[] data, int password)
	{
        //operation depends on date
    }
}