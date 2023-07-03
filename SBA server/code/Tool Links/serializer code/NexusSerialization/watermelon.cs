using System.Collections;
using System.Runtime.InteropServices;

namespace WatermelonDataTool.Serializer
{
	//Parent
	public class Watermelon
	{
		public List<Melon> Fields = new List<Melon>();
		public int count { get { return Fields.Count; } }
		public Melon this[int index]
		{
			get { return Fields[index]; }
			set { Fields[index] = value; }
		}
		public IEnumerator GetEnumerator()
		{
			for (int i = 0; i < Fields.Count; i++)
			{
				yield return Fields[i];
			}
		}

		public Watermelon([Optional]params Melon[] infos)
		{
			if(infos!=null) Fields.AddRange(infos);
		}
		public Watermelon(byte[] b)
		{
			ReloadFromBytes(b);
		}
		
		//convertion
		public void ReloadFromBytes(byte[] b)//not tested
		{
			Fields = new List<Melon>();
			if (b == null) return;
			else Fields.AddRange(Encoding.ToFields(b));
		}
		public byte[] ToBytes()
		{
			return Encoding.ToBytes(Fields.ToArray());
		}
        public string ToStringIndentation = 'ˑ' + "\t";
        public override string ToString()
		{

			string str = "{";
			int i = 0;
			foreach(Melon field in Fields)
			{
				string current_info = field.ToString();
				/*if (field.type == data_type.Nexus) */current_info = current_info.Replace("\n", "\n" + ToStringIndentation);
				str += "\n" + ToStringIndentation + "[" + i + "]" + Str(current_info);
				i++;
			}

			str += "\n}";

			return str;
		}
		/// <summary> similar to Str() in python </summary>
		public static string Str(object? obj)
		{
			if (obj == null)
				return "null";
			if (obj.GetType().IsArray)
			{
				string s = "";
				foreach (var v in (Array)obj)
				{
					s += Str(v) + ", ";
				}
				if (s.Length > 0) s = s[..^2];
				return "["+s+"]";
			}
			return obj.ToString();
		}
		//access
		public enum AccessAction
		{
			set,
			remove,
			get,
			exist
		}
		public Melon AccessField(string PathAndName, AccessAction action,object? setObj = null)
		{
			//adjust path
			if (PathAndName.Length >= 1 && PathAndName[0] == '/') PathAndName = PathAndName.Remove(0, 1);//make formatt like this "xxx/xxx/xxx"
			string[] paths = PathAndName.Split('/');//it should be like xxx,xxx,xxx
			string nextSubPath = paths[0];
			if (paths[0] == "") return null;//make sure no fields have empty name

			//Find index Info of which name is nextSubPath
			int index = -1;
			for (int i = 0; i < Fields.Count; i++)
			{
				if (Fields[i].FieldName == nextSubPath) { index = i; break; }
			}
			if (paths.Length <= 1)//when no subpaths: current hierarchical level should have target field.
			{   //if found, do edit:
				if (index >= 0)
				{
					switch (action)
					{
						case AccessAction.get:
							return Fields[index];
						case AccessAction.set:
							///why did I write this??? : ///tip: Infos[index].SetObj(Obj);  doesn't work. Infos[index] acts like a returned value.
							Fields[index].SetObj(setObj);//it actually works
							return null;
						case AccessAction.remove:
							Fields.RemoveAt(index);
							return null;
						case AccessAction.exist:
							return new Melon("", true);
						default:
							return null;
					}
				}
				else
				{
					if(action == AccessAction.set)
						Fields.Add(new Melon(PathAndName, setObj));
					return null;
				}
			}
			else//when there is subpaths : try open or create and call for edit
			{
				//if field of subpath not found
				if (index == -1)
				{
					if (action == AccessAction.set)
					{   //create new and locate(set index):
						Fields.Add(new Melon(nextSubPath, data_type.Nexus));
						index = Fields.Count - 1;
					}
					else//not want to set: stop procces
						return null;
				}
				//if not a subpath, stop proccess:
				else if (Fields[index].type != data_type.Nexus) return null;

				//open and edit
				return ((Watermelon)Fields[index].obj).AccessField(string.Join("/", paths, 1, paths.Length - 1), action, setObj);
			}
		}

		//edit
		/// <summary> Do not use empty field name or directory name. </summary>
		public void setobj(string PathAndName, object Obj)//not tested
		{
			AccessField(PathAndName, AccessAction.set, Obj);
		}
        /// <summary> Do not use empty field name or directory name. </summary>
        public void LoadField_toDirectory(string DirectoryPath, Melon new_field, bool IncludeWrongFormatt = false)
		{
			if (!(IncludeWrongFormatt || new_field.type != data_type.Wrong_Format)) return;
			if (DirectoryPath[DirectoryPath.Length - 1] != '/') DirectoryPath += '/';

			string path_name = DirectoryPath + new_field.FieldName;
			setobj(path_name, new_field.obj);
		}
        /// <summary> Do not use empty field name or directory name. </summary>
        public void removeField(string PathAndName)
		{
			AccessField(PathAndName, AccessAction.remove);
		}
		/*//
		public void removeField(string PathAndName)
		{
			//adjust path
			if (PathAndName[0] == '/') PathAndName.Remove(0, 1);//make formatt like this "xxx/xxx/xxx"
			string[] paths = PathAndName.Split('/');//it should be like xxx,xxx,xxx
			string nextSubPath = paths[0];

			//Find index Info of which name is nextSubPath
			int index = -1;
			for (int i = 0; i < Infos.Count; i++)
			{
				if (Infos[i].name == nextSubPath) { index = i; break; }
			}
			if (paths.Length <= 1)//when no subpaths: set normally
			{   //if found, edit:
				if (index >= 0)
				{
					Infos.RemoveAt(index);
				}   
			}
			else//when there is subpaths : open and call removeInfo
			{
				//if not found, stop proccess
				if (index == -1) return;

				//call remove in current hierarchical level
				((Nexus)Infos[index].obj).removeField(string.Join("/", paths, 1, paths.Length - 1));
			}
		}
		//*/

		//multiple edit
		public void removeRange(List<string> PathAndName_s)//not tested
		{
			foreach (string p in PathAndName_s)
				AccessField(p, AccessAction.remove);
		}

		//read
		public bool exist(string PathAndName)
		{
			Melon _ = AccessField(PathAndName, AccessAction.exist);
			return (_ != null && (bool)_.obj == true);//"(bool)_.obj == true" just for human see.
		}
		public Melon getField(string PathAndName)
		{
			return AccessField(PathAndName, AccessAction.get);
		}

        /// <returns> Object at the path. Note that the object can be null. If no field is found at the path, it will also return null. </returns>
        public T? getObj<T>(string PathAndName)
		{
			return (T?)getField(PathAndName)?.obj;
		}
        /// <returns> Object at the path. Note that the object can be null. If no field is found at the path, it will also return null. </returns>
        public T? getObj<T>(int index)
		{
			return (T?)Fields[index].obj;
		}
		/// <summary>
		/// get an obj from a path. If no field is found at the path, set TouchObj at the path and return the object.
		/// </summary>
		/// <param name="PathAndName"> target path </param>
		/// <param name="TouchObj"> object to be set when there is no object found in the path </param>
		public T? TouchGet<T>(string PathAndName, object TouchObj)
		{
			if (!exist(PathAndName))
				setobj(PathAndName, TouchObj);
			return getObj<T>(PathAndName);
		}
		public string getFieldName(int index)
		{
			return Fields[index].FieldName;
		}
	}
}
