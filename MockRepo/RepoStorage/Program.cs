using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Federation
{
    ///////////////////////////////////////////////////////////////////
    // RepoMock class
    // - begins to simulate basic Repo operations

    public class RepoMock
    {
        public string storagePath { get; set; } = "../../RepoStorage";
        public string receivePath { get; set; } = "../../BuilderStorage";
        public List<string> files { get; set; } = new List<string>();

        /*----< initialize RepoMock Storage>---------------------------*/

        public RepoMock()
        {
            if (!Directory.Exists(storagePath))
                Directory.CreateDirectory(storagePath);
            if (!Directory.Exists(receivePath))
                Directory.CreateDirectory(receivePath);
        }
        /*----< private helper function for RepoMock.getFiles >--------*/

        private void getFilesHelper(string path, string pattern)
        {
            string[] tempFiles = Directory.GetFiles(path, pattern);
            for (int i = 0; i < tempFiles.Length; ++i)
            {
                tempFiles[i] = Path.GetFullPath(tempFiles[i]);
            }
            files.AddRange(tempFiles);

            string[] dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
            {
                getFilesHelper(dir, pattern);
            }
        }
        /*----< find all the files in RepoMock.storagePath >-----------*/
        /*
        *  Finds all the files, matching pattern, in the entire 
        *  directory tree rooted at repo.storagePath.
        */
        public void getFiles(string pattern)
        {
            files.Clear();
            getFilesHelper(storagePath, pattern);
        }
        /*---< copy file to RepoMock.receivePath >---------------------*/
        /*
        *  Will overwrite file if it exists. 
        */
        public bool sendFile(string fileSpec)
        {
            try
            {
                string fileName = Path.GetFileName(fileSpec);
                string destSpec = Path.Combine(receivePath, fileName);
                File.Copy(fileSpec, destSpec, true);
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("\n--{0}--", ex.Message);
                return false;
            }
        }
    }

#if (TEST_REPOMOCK  )

  
  class TestRepoMock
  {
    static void Main(string[] args)
    {
      Console.Write("\n  Demonstration of Mock Repo");
      Console.Write("\n ============================");

      RepoMock repo = new RepoMock();
      repo.getFiles("*.*");
            //Console.Write("file length", repo.getFiles().count());
      foreach(string file in repo.files)
        Console.Write("\n  \"{0}\"", file);
            //for ( int i = 0; i <= repo.getFiles.count(); i = i + 1)
            //{
                string fileSpec = repo.files[0];
                string fileName = Path.GetFileName(fileSpec);
                Console.Write("\n  sending \"{0}\" to \"{1}\"", fileName, repo.receivePath);
                repo.sendFile(repo.files[0]);
            //}
      Console.Write("\n\n");
    }
  }
#endif
}

