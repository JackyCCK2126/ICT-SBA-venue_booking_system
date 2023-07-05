using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using static EasyConsole;

partial class FileManage
{
    //with system rest

    public static void CopyFileOrFolder(string source_path, string to_directory, bool auto_continue_overwrite = false)
    {
        if (!(Directory.Exists(source_path) || File.Exists(source_path)))
        {
            print("File or folder not found:\n "+source_path);
            return;
        }
        
        if (IsFile(source_path)) copy_file(source_path, to_directory, auto_continue_overwrite);
        else copy_folder(source_path, to_directory, auto_continue_overwrite);
    }

    private static int restCD;
    public static event_ copy_folder(string SourceFolder, string to_directory, bool auto_continue_overwrite = false)
    {
        string toFolder = Path.Combine(to_directory, Path.GetFileName(SourceFolder));
        Directory.CreateDirectory(toFolder);
        //files
        {
            string[] file_names; get_file_names(SourceFolder, out file_names);
            foreach (string f in file_names)
            {
                event_ a = copy_file(Path.Combine(SourceFolder, f), toFolder, auto_continue_overwrite);
                if (a == event_.stop) return event_.stop;
                else if (a == event_.contin_set_all_overwrite) auto_continue_overwrite = true;
                //rest
                restCD++;
                Thread.Sleep(50);
                if (restCD > 5) { Thread.Sleep(100); restCD = 0; }
            }
        }
        //dir
        {
            string[] directory_names; get_directorie_names(SourceFolder, out directory_names);
            foreach(string d in directory_names)
            {
                event_ a = copy_folder(Path.Combine(SourceFolder, d), Path.Combine(toFolder), auto_continue_overwrite);
                if (a == event_.stop) return event_.stop;
                else if (a == event_.contin_set_all_overwrite) auto_continue_overwrite = true;
            }
        }
        if (auto_continue_overwrite) return event_.contin_set_all_overwrite;
        return event_.contin;
    }
    public static event_ copy_file(string sourceFile, string to_directory,bool auto_continue_overwrite = false)
    {//make sure file name with format name

        // Use Path class to manipulate file and directory paths.
        string destPathName = Path.Combine(to_directory,Path.GetFileName(sourceFile));
        // Create a new target folder.
        // If exists, this method does not create a new directory.
        Directory.CreateDirectory(to_directory);

        if (!File.Exists(sourceFile))
        {
            if (!auto_continue_overwrite)
            {
                print("source file not exist:" + sourceFile);
                print("\ncancelled");
                return event_.stop;
            }
        }
        if (File.Exists(destPathName)&&!auto_continue_overwrite)//**
        {
            while(true)
            {
                string txt = input("file exists:" + destPathName + "\n    Overwrite? \n Overwite(yes) || OverwriteAllAfter(all) || ignore(ig)").ToLower();
                if (txt == "yes")
                {
                    print("overwrite\n");break;
                }
                else if(txt == "all")
                {
                    auto_continue_overwrite = true;
                    print("started auto overwrite\n"); break;
                }
                else if (txt == "ig")
                {
                    print("ingore\n"); return event_.contin;
                }
                else
                {
                    print("what?");
                }
            }
        }
        File.Copy(sourceFile, destPathName, true);
        if (auto_continue_overwrite) return event_.contin_set_all_overwrite;
        return event_.contin;
    }
    public enum event_
    {
        contin,
        contin_set_all_overwrite,
        stop,
    }
}
