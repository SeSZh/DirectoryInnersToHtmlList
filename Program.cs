using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;



namespace CS1
{
    class Program
    {
        
       // [STAThread]
        static void Main()
        {
            DirInfo dirInfo = new DirInfo();
            var n = (double)dirInfo.rootDirectory.Size/1024;
            HTMLMaker htmlMaker = new HTMLMaker(dirInfo.rootDirectory);
        }
        

    }
    class DirInfo //класс работы с файловой системой
    {
        public class Dir // структура директории 
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public List<Dir> ChildDirs { get; set; }
            public List<Fl> Files { get; set; }
            public long Size { get; set; }
            public long KBSize { get; set; } 
            public Dir()
            {
                Name = "";
                Path = "";
                ChildDirs = new List<Dir>();
                Files = new List<Fl>();
                Size = 0;
                KBSize = 0;
            }
            public Dir(string name, string path, List<Dir> dirs, List<Fl> files, int size)
            {
                Name = name;
                Path = path;
                ChildDirs = new List<Dir>(dirs);
                Files = new List<Fl>(files);
                Size = size;
                KBSize = size / 1024;
            }
            public Dir(string name, string path, List<Dir> dirs, List<Fl> files)
            {
                Name = name;
                Path = path;
                ChildDirs = new List<Dir>(dirs);
                Files = new List<Fl>(files);
                long dirsSize = 0;
                foreach(Dir dir in dirs)
                {
                    dirsSize += dir.Size;
                }
                long filesSize = 0;
                foreach(Fl file in files)
                {
                    filesSize += file.Size;
                }
                Size = dirsSize += filesSize;
                KBSize = Size / 1024;
            }
            public Dir(Dir dir)
            {
                Name = dir.Name;
                Path = dir.Path;
                ChildDirs = new List<Dir>(dir.ChildDirs);
                Files = new List<Fl>(dir.Files);
                Size = dir.Size;
            }
        }
        public class Fl // структура файла
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public long Size { get; set; }
            public long KBSize { get; set; }
            public string Mimetype { get; set; }
            public Fl()
            {
                Name = "";
                Path = "";
                Size = 0;
                Mimetype = "";
            }
            public Fl(Fl file)
            {
                Name = file.Name;
                Path = file.Path;
                Size = file.Size;
                KBSize = file.KBSize;
                Mimetype = file.Mimetype;
            }
            public Fl(string name, string path, long size, string mimetype)
            {
                Name = name;
                Path = path;
                Size = size;
                KBSize = size / 1024;
                Mimetype = mimetype;
            }
        }
        public Dir rootDirectory = null;
        public DirInfo()
        {
            string root = Directory.GetCurrentDirectory();
            List<Dir> childDirectories =  new List<Dir>(GetChildDirectories(root));
            List<Fl> childFiles = new List<Fl>(GetChildFiles(root));
            string rootDirectoryName = root.Substring(root.LastIndexOf("\\")+1);
            string rootDirectoryPath = root.Substring(0, root.LastIndexOf("\\"));
            rootDirectory = new Dir(rootDirectoryName, rootDirectoryPath, childDirectories, childFiles);
            
        }
        private List<string> PathCutter(List<string> dirs, string pathName) // выделяет имя от полного пути
        {
            for (int i = 0; i < dirs.Count; i++)
            {
                dirs[i] = dirs[i].Substring(pathName.Length + 1);
            }
            return dirs;
        }
        private string PathCutter(string dir, string pathName)
        {
            dir = dir.Substring(pathName.Length + 1);
            return dir;
        }
        private List<Dir> GetChildDirectories(string rootPath) // получает полный список дочерних папок
        {
            List<Dir> childDirectories = new List<Dir>();
            var childDirsPaths = Directory.GetDirectories(rootPath);
            foreach(string path in childDirsPaths)
            {
                List<Dir> childDirs = GetChildDirectories(path);
                List<Fl> childFiles = GetChildFiles(path);

                childDirectories.Add(new Dir(PathCutter(path, rootPath), rootPath, childDirs, childFiles));
            }
            return childDirectories;
        }
        private List<Fl> GetChildFiles(string rootPath) // получает полный список файлов в папке
        {
            List<Fl> childFiles = new List<Fl>();
            List<string> childFilesPaths = new List<string>(Directory.GetFiles(rootPath));
            foreach(string file in childFilesPaths)
            {

                var f = new FileInfo(file);
                var s = MimeMapping.MimeUtility.GetMimeMapping(f.Extension);
                childFiles.Add(new Fl(f.Name, f.DirectoryName, f.Length, s));
            }
            return childFiles;
        }
    }

    class HTMLMaker // Класс создания списка на HTML
    {
        private List<DirInfo.Fl> fls = new List<DirInfo.Fl>();
        public HTMLMaker(DirInfo.Dir rootDirectory)
        { 
            FileStream file = new FileStream(rootDirectory.Path + "\\" + rootDirectory.Name + "\\resulthtml.html", FileMode.Create);
            string result = DirectoryMaker(rootDirectory);
            MimeCounter mC = new MimeCounter();
            result = "<!DOCTYPE html>\n<html>\n<head>ТЗ</head>\n\t<body>\n" + result + mC.MimeStatisticHtmlMaker(rootDirectory.Files) + "\t</body>\n</html>";
            

            file.Write(System.Text.Encoding.Default.GetBytes(result), 0, result.Length);
            file.Close();
            
        }
        private string DirectoryMaker(DirInfo.Dir rootDirectory) // создает строку со списком папок
        {
            string directories = "";
            if (rootDirectory.ChildDirs.Count!=0)
            {
                directories = "<ul>";
                foreach(DirInfo.Dir directory in rootDirectory.ChildDirs)
                {
                    string directoryText = "<li><table><tr>";
                    directoryText += ("<td width="+ '\u0022' +"200"+ '\u0022' +"> " + directory.Name + "</td><td width=" + '\u0022' + "100" + '\u0022' + ">" + directory.KBSize + "KB</td></tr></table></li>\n");
                    directoryText += DirectoryMaker(directory);
                    directories += directoryText;
                }
                directories += "</ul>";
            }
            if(rootDirectory.Files.Count != 0)
            {
                directories += FileMaker(rootDirectory);
            }
            if(directories.Length!=0)
            {
                directories += "\n";
            }
            

            return directories;
        }

        private string FileMaker(DirInfo.Dir rootDirectory) // создает строку со списком файлов
        {
            string files = "<ul>";
            foreach (DirInfo.Fl file in rootDirectory.Files)
            {
                string fileText = "<li><table><tr>";
                fileText += ("<td width=" + '\u0022' + "200" + '\u0022' + ">" + file.Name + "</td><td width=" + '\u0022' + "100" + '\u0022' + ">" + file.KBSize + "KB</td><td>" + file.Mimetype + "</td></tr></table></li>\n");
                files += fileText;
                fls.Add(file);
            }
            files += "</ul>\n";
            return files;
        }
    }
    class MimeCounter // класс отвечающий за расчеты Mime типов
    {
        class MimeType
        {
            public string Name;
            public int Counter;
            public long Size;
            public MimeType(string name, long size)
            {
                Name = name;
                Counter = 1;
                Size = size;
            }
        }
        public MimeCounter()
        {
            
        }
        public string MimeStatisticHtmlMaker(List<DirInfo.Fl> files) // главная функция, создающая html таблицу со статистической информацией
        {
            List<MimeType> types = new List<MimeType>();
            //files = MimeTaker(files);
            foreach (DirInfo.Fl file in files)
            {
                int index = types.FindIndex(match => match.Name == file.Mimetype);
                if (index != -1)
                {
                    types[index].Counter++;
                    types[index].Size += file.Size;
                }
                else
                {
                    types.Add(new MimeType(file.Mimetype, file.Size));
                }
            }
            string mimesString = "<table>\n<tr><td>Название</td><td>Количественное соотношение</td><td>Процентное соотношение</td><td>Средний размер, KB</td></tr>\n";
            foreach (MimeType type in types)
            {
                string mimeString = "<tr><td>" + type.Name + "</td><td>" + type.Counter + "/" + files.Count + "</td><td>" + 100 * Math.Round((double)type.Counter / files.Count, 2) + "%</td><td>" + (int)(type.Size / files.Count)/1024 + "</td></tr>\n";
                mimesString += mimeString;
            }
            mimesString += "</table>";
            return mimesString;
        }
        List<DirInfo.Fl> MimeTaker(List<DirInfo.Fl> files)
        {
            foreach(DirInfo.Fl file in files)
            {
                file.Mimetype = file.Mimetype.Substring(0, file.Mimetype.IndexOf('/'));
            }
            return files;
        }
        DirInfo.Fl MimeTaker(DirInfo.Fl file)
        {
            file.Mimetype = file.Mimetype.Substring(0, file.Mimetype.IndexOf('/'));
            return file;
        }
    }
    
}
