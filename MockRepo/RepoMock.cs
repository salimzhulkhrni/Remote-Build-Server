/*////////////////////// Mock Repository/////////////////////
Source: Prof. Jim Fawcett                                  //
Author: Salim Zhulkhrni Shajahan                           //
Date: 06-Oct-2017                                          //
Date Modified: 06-Dec-2017                                 //
Version: 1.1                                               //
                                                           //
Changes                                                    //
Added functionality of Recieves message from client        //
and transfers the files in mother builder's repository     //
                                                           //
Demonstrating features of the Mock Repository              //
                                                           //
Functions:                                                 //
1)Stores the test request received from client             //
2)Stores the files recieved from client                    //
 3)Test Stubs created to test each function                //
 4) Recieves message from client and transfers the files in// 
 mother builder's repository 
 5)Store build logs & test logs when received as a message/
 
 Public Interface
 ----------------
 //1.void listen_repo_thread() // repo thread to listen
 2.void getFilesHelper(string path, string pattern) // get files from a location
 3.void getFiles(string pattern, string id) //get files based on a pattern

 Maintenance History: 
* -------------------- 
* ver 1.0 : 06 Dec 2017 
* - first release 
///////////////////////////////////////////////////////////
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ServiceModel;
using System.Threading;
using MessagePassingComm;

namespace Repository_Mock
{
    

    public class RepoMock
    {

        
        public string storagePath { get; set; } = "../../../Repo_Files";
        public string receivePath { get; set; } = "../../../Build_Server/Build_Server_Local_Storage";
        public string client_path { get; set; } = "../../../Client/Files";
        
        public List<string> files { get; set; } = new List<string>();

        /*----< initialize RepoMock Storage>---------------------------*/

        public RepoMock()
        {
            if (!Directory.Exists(storagePath))
                Directory.CreateDirectory(storagePath);
            if (!Directory.Exists(receivePath))
                Directory.CreateDirectory(receivePath);
            if (!Directory.Exists(client_path))
                Directory.CreateDirectory(client_path);
            
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
        public void getFiles(string pattern, string id)
        {
            files.Clear();
            if (id == "To_Repo")
            {
                getFilesHelper(client_path, pattern);
            }
            else
            {
                getFilesHelper(storagePath, pattern);
            }
        }
        /*---< copy file to RepoMock.receivePath >---------------------*/
        /*
        *  Will overwrite file if it exists. 
        */
        public bool sendFile(string fileSpec, string id)
        {

            try
            {
                string fileName = Path.GetFileName(fileSpec);
                if (id == "To_Repo")
                {
                    string destSpec = Path.Combine(storagePath, fileName);
                    File.Copy(fileSpec, destSpec, true);
                }
                else
                {
                    string destSpec = Path.Combine(receivePath, fileName);
                    File.Copy(fileSpec, destSpec, true);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("\n--{0}--", ex.Message);
                return false;
            }

        }
        public void send_files_to_mock_repo()
        {
            Console.WriteLine("**************************************************************");
            Console.WriteLine("-----------------Sending files to repository------------------");
            Console.WriteLine("**************************************************************");
            getFiles("*.*", "To_Repo");
            foreach (string file in files)
            {
                string fileSpec = file;
                string fileName = Path.GetFileName(fileSpec);
                
                Console.Write("\n  sending \"{0}\" to \"{1}\"", fileName, storagePath);
                sendFile(file, "To_Repo");
            }
        }
    }

    public class MockRepo
    {

        Comm comm = new Comm("http://localhost", 8092);
        RepoMock rm = new RepoMock();

        public void repo_main_thrd()
        {
            
            Thread repo_thread = new Thread(listen_repo_thread);
            Console.WriteLine();
            Console.WriteLine("**************************************************************");
            Console.WriteLine("*********** Starting Repo Thread******************************");
            Console.WriteLine("**************************************************************");
            repo_thread.Start();
            Console.WriteLine();
            Console.WriteLine("**************************************************************");
            Console.WriteLine("*********** Inside Repo Thread******************************");
            Console.WriteLine("**************************************************************");
            repo_thread.Join();

        }
        //< repo thread starts listening >///////
        void listen_repo_thread()
        {
            Console.WriteLine();
            Console.WriteLine("**************************************************************");
            Console.WriteLine("*********** Repo Thread Running ******************************");
            Console.WriteLine("**************************************************************");
            while (true)
            {
                CommMessage rec_msg_mthr = null;
                rec_msg_mthr = comm.getMessage();
                Console.WriteLine();
                Console.WriteLine("**************************************************************");
                Console.WriteLine("***Inside Repo Thread: Message Received ****");
                Console.WriteLine("**************************************************************");
                rec_msg_mthr.show();
                
                if (rec_msg_mthr.port_number != 0 && rec_msg_mthr.port_number == 8010)
                {
                    Console.WriteLine();
                    Console.WriteLine("**************************************************************");
                    Console.WriteLine("***Inside Repo Thread: Message Received From Test Harness ****");
                    Console.WriteLine("**************************************************************");
                    
                    String[] test_logs = rec_msg_mthr.msg_body.Split('|');
                    test_logs = test_logs.Where(arr => !String.IsNullOrEmpty(arr)).ToArray();
                    foreach (String file in test_logs)
                    {
                        //File.WriteAllText(@"../../../Repo_Files/Test_log_For_" + file.Split(':')[0] + ".txt", file.Split(':')[0] + ":" + file.Split(':')[1]);

                    }
                    String path = Path.GetFullPath(rm.storagePath);
                    Console.WriteLine("*********************************************************************");
                    Console.WriteLine("----------Test Logs stored in :\"{0}\"---------------", path);
                    Console.WriteLine("**********************************************************************");



                }
                
                if (rec_msg_mthr.port_number!=0 && rec_msg_mthr.port_number==8091 )
                  {

                    switch (rec_msg_mthr.command)
                    {
                        case "Get_Files_From_Repo":
                            {
                                if (rec_msg_mthr.msg_body.Equals("8091"))
                                {
                                    CommMessage snd_msg_to_mthr = new CommMessage(CommMessage.MessageType.request);
                                    snd_msg_to_mthr.command = "To_MB";
                                    snd_msg_to_mthr.from = "http://localhost:8092/IMessagePassingComm";
                                    snd_msg_to_mthr.to = "http://localhost:8091/IPluggableComm";
                                    snd_msg_to_mthr.msg_body = "8092";
                                    snd_msg_to_mthr.port_number = 8092;
                                    comm.postMessage(snd_msg_to_mthr);
                                    List<string> file_names = new List<string>();
                                    string[] files = Directory.GetFiles(ClientEnvironment.fileStorage);
                                    foreach (string file in files)
                                    {
                                        file_names.Add(Path.GetFileName(file));
                                    }
                                    String path = Path.GetFullPath(rm.receivePath);
                                    Console.WriteLine();
                                    Console.WriteLine("****************************************************************************");
                                    Console.WriteLine("*******Transferring Files From Repository To Mother Builder Storage at: {0}",path);
                                    Console.WriteLine("****************************************************************************");
                                    foreach (string file in file_names)
                                    {
                                        string fileSpec = file;
                                        string fileName = Path.GetFileName(fileSpec);
                                        Console.WriteLine();
                                        Console.Write("\n sending \"{0}\" to \"{1}\"", fileName, Path.GetFullPath(rm.receivePath));
                                        Console.WriteLine();
                                        TestUtilities.putLine(string.Format("transferring file \"{0}\"", file));
                                        bool result = comm.postFile(file);
                                        TestUtilities.checkResult(result, "check");
                                    }
                                }
                                break;
                            }
                        
                        default:
                            {
                                Console.WriteLine("Default:Invalid operation on the commmand");
                                break;
                            }
                    }
                }
                


            }
            

        }
    }
   ///  Test_Stub 
   
   #if (TEST_REPOMOCK  )


      class TestRepoMock
      {
        static void Main(string[] args)
        {
          Console.Write("\n  Demonstration of Function in Mock Repo");
          Console.Write("\n ============================");

          RepoMock repo = new RepoMock();
            repo.getFiles("*.*","To_Repo");          // testing individual function- sendFile, getFiles
            foreach (string file in repo.files)
                Console.Write("\n  \"{0}\"", file);
                string fileSpec = repo.files[0];
                string fileName = Path.GetFileName(fileSpec);
                Console.Write("\n  sending \"{0}\" to \"{1}\"", fileName, repo.storagePath);
                Console.Write("\n\n");
                repo.sendFile(repo.files[0], "To_Repo");          

            repo.send_files_to_mock_repo();  // testing the function - send_files_to_mock_repo
                
        }
      }
    #endif
    
}    



