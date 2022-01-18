using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace K181133_Q3
{
    public partial class Service1 : ServiceBase
    {
        Timer t;
        public Service1()
        {
            InitializeComponent();
            t = new Timer();
            t.Interval = 1 * 60 * 1000;                            //Initial Timer of 1 Minute
            t.Elapsed += new System.Timers.ElapsedEventHandler(FolderMoniter);
        }
        public void onDebug()
        {
            OnStart(null);
        }
        public bool FileEquals(string path1, string path2)
        {
            byte[] file1 = File.ReadAllBytes(path1);
            byte[] file2 = File.ReadAllBytes(path2);
            if (file1.Length == file2.Length)
            {
                for(int i = 0; i < file1.Length; i++)
                {
                    if (file1[i] != file2[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
        public void FolderMoniter(object sender, ElapsedEventArgs e)
        {
            try
            {
                string FoldertoMoniter = ConfigurationManager.AppSettings["Moniter"];
                string FoldertoCopy = ConfigurationManager.AppSettings["Copy"];
                var moniterFiles = Directory.GetFiles(FoldertoMoniter, "*.txt", SearchOption.AllDirectories);
                var copyFiles = Directory.GetFiles(FoldertoCopy, "*.txt", SearchOption.AllDirectories);
                var inc = 0;
                if (moniterFiles != null)
                {
                    foreach (var mfile in moniterFiles)
                    {
                        //When Copy Folder is null
                        if(!File.Exists(Path.Combine(FoldertoCopy, Path.GetFileName(mfile))))
                        {
                            File.Copy(Path.Combine(FoldertoMoniter, Path.GetFileName(mfile)), Path.Combine(FoldertoCopy, Path.GetFileName(mfile)), true);
                        }
                    
                        foreach (var cfile in copyFiles)
                        {
                            if(Path.GetFileName(mfile) == Path.GetFileName(cfile))
                            {
                                if(FileEquals(mfile, cfile)==true)                      //No Change
                                {
                                    //increase by 2
                                    inc = 1;
                                    break;
                                }
                                else                                                    //Change
                                {
                                    inc = 0;
                                    File.Copy(Path.Combine(FoldertoMoniter,mfile), Path.Combine(FoldertoCopy,cfile) , true);
                                }
                            }
                        }
                    }
                    
                    if (inc == 1)                                                //Equals (Time increase by 2)
                    {
                        if(t.Interval < 60 * 60 * 1000)
                        {
                            t.Interval += 2 * 60 * 1000;                         //Time increase by 2 after each change
                        }
                        else
                        {
                            t.Interval = 60 * 60 * 1000;                          //Max Time 60 minutes 
                        }
                    }
                }
                //When Extra File Added on CopyFolder 
                if(copyFiles.Length > 0)
                {
                    foreach (var cfile in copyFiles)
                    {
                        if(!File.Exists(Path.Combine(FoldertoMoniter, Path.GetFileName(cfile))))
                        {
                            File.Delete(cfile);
                        }
                    }
                }
               
            }
            catch(Exception)
            {
                throw new Exception();
            }
        }
        protected override void OnStart(string[] args)
        {
            t.Enabled = true;
        }

        protected override void OnStop()
        {
            t.Enabled = false;
        }
    }
}
