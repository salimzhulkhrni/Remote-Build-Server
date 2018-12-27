/*//////////////////// Client-GUI   //////////////////////////
Author: Salim Zhulkhrni Shajahan, sshajaha@syr.edu         //
Date: 31-Oct-2017
Date Modified: 06-Dec-2017                                 //
Version: 1.1                                               //
                                                           //
Demonstrating features of the Client-GUI                   //
                                                           //
Package Functions:                                         //
1)Displays a list files for the client to create test case //
and test request                                           //
2)Provides functionality of adding new files to repo storage//
3)Creates child process and displays the test request for  //
the allocated child process                                //
4) Copies the files from other folders to repo storage   //
5) Provides functionality of killing the processes 
//
Public Interface 
----------------
1.choose_files_click(object sender, RoutedEventArgs e) //select files to create test request on click
2.void create_test_req_Click(object sender, RoutedEventArgs e) //create test request on click
3.build_Click(object sender, RoutedEventArgs e) //starts building process
4.kill_Click(object sender, RoutedEventArgs e) // send msgs to mother builder to kill child process
5.upload_Click(object sender, RoutedEventArgs e) //upload files to create new test request

Maintenance History: 
* -------------------- 
* ver 1.0 : 06 Dec 2017 
* - first release 
//
                                                            //
///////////////////////////////////////////////////////////
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.IO;
using Utilities;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Serialization;
using Repository_Mock;
using System.Threading;
using Mother_Builder_Sp;
using System.Diagnostics;
using Test_Harness;

namespace GUI
{
   
    public partial class MainWindow : Window
    {
        List<String> test_case_names = new List<String>();
        List<String> test_case_files = new List<String>();
        List<String> xml_string = new List<String>();
        int c_test_case = 5;
        int c_test_request = 5;
        int test_case_count = 0;
        int xml_file_count = 6;
        
        

        public MainWindow()
        {
            InitializeComponent();
            String[] existing_xml_files = Directory.GetFiles(@"../../../Repo_Files/", "*.xml");
            foreach (String file in existing_xml_files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {

                }
            }
            
            String[] existing_files = Directory.GetFiles(@"../../../Repo_Files/", "*.txt");
            foreach (String file in existing_files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {

                }
            }
            Load_Repo_Files();
        }       

        private void choose_files_click(object sender, RoutedEventArgs e)
        {
            String temp_file="" ;
            if (lb1.SelectedItems.Count != 0)
            {
                foreach (var str in lb1.SelectedItems)
                {
                    temp_file = temp_file + str.ToString()+" ";
                }
                
                test_case_files.Add(temp_file);
                c_test_case = c_test_case + 1;
                lb2.Items.Add("Test Case :" + c_test_case.ToString());
                test_case_names.Add("Test_Case_" + c_test_case.ToString());
            }
            else
            {
                MessageBox.Show("Please select files for creating test request");
            }
           
        }

        private void create_test_req_Click(object sender, RoutedEventArgs e)
        {
            
            if (lb2.Items.Count != 0)
            {
                int c;
                string[] s;
                TestRequest tr = new TestRequest();
                // create test request for bunch of test cases
                if (lb2.Items.Count != 0)
                {
                    for (int item_count = 1; item_count <= lb2.Items.Count; item_count++)
                    {
                        TestElement te1 = new TestElement();
                        te1.testName = test_case_names[test_case_count];
                        s = test_case_files[test_case_count].Split(new char[0]);
                        c = s.Length;
                        for (int j = 0; j < (c - 1); j++)
                        {
                            
                            if (s[j].ToString().StartsWith("td"))
                            {
                                
                                te1.addDriver(s[j]);
                            }
                            else
                            {
                                
                                te1.addCode(s[j]);
                            }
                        }
                        tr.author = "Salim Zhulkhrni";
                        tr.tests.Add(te1);
                        string xml = tr.ToXml();                        
                        string repo_path = @"../../../Repo_Files/XMLTestRequest_" + xml_file_count + ".xml";
                        File.WriteAllText(repo_path, xml);
                        xml_string.Add(xml);
                        test_case_count = test_case_count + 1;
                    }
                    lb2.Items.Clear(); //clears the list box items before next test req is added
                    xml_file_count = xml_file_count + 1;
                    c_test_request = c_test_request + 1;
                    lb3.Items.Add("Test Request: " + c_test_request.ToString());
                    string xml_path = System.IO.Path.GetFullPath(@"../../../Repo_Files/");
                    MessageBox.Show(String.Format("XML Files Stored in :{0}", xml_path));
                }
            }
            else
            {
                MessageBox.Show("Please create test cases");
            }

        }

        private void build_Click(object sender, RoutedEventArgs e)
        {
            int c_proc;            
            if ((lb3.Items.Count != 0 || lb5.Items.Count!=0) && (!string.IsNullOrEmpty(b_txtbox.Text)))
              {
                c_proc = Int32.Parse(b_txtbox.Text);
                if (c_proc != 0)
                {
                    b5.IsEnabled = false;
                    
                    Mother_Builder mb = new Mother_Builder();
                    mb.start_builder(c_proc);
                    MockRepo rm = new MockRepo();
                    Thread repo_thread_start = new Thread(rm.repo_main_thrd);
                    repo_thread_start.Start();                 
                    
                    
                    
                }
                else
                {
                    MessageBox.Show("Please enter at least one child builder");
                }
              }
              else
              {
                if (lb3.Items.Count == 0 && lb5.Items.Count==0)
                    MessageBox.Show("Please create test request to start building or select from existing test requests");
                else if (string.IsNullOrEmpty(b_txtbox.Text))
                    MessageBox.Show("Please enter the number of child builders");
              }
              
        }

        private void kill_Click(object sender, RoutedEventArgs e)
        {
            
            if (!string.IsNullOrEmpty(b_txtbox.Text))
            {
                int num = Int32.Parse(b_txtbox.Text);
                if (num > 0)
                {
                    foreach (Process proc in Process.GetProcessesByName("Build_Server"))
                    {
                        proc.Kill();
                    }
                    MessageBox.Show("Killed All Child Process");
                    b6.IsEnabled = false;
                }
                else
                {
                    MessageBox.Show("Enter at least 1 child builder to kill");
                }
            }
            else
            {
                MessageBox.Show("Enter the number of child builders to be created");
            }
        }       

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog_box = new Microsoft.Win32.OpenFileDialog();
            dialog_box.Multiselect = true;
            dialog_box.DefaultExt = ".cs";
            dialog_box.Filter = "CS Files (*.cs) | *.cs";
            var result = dialog_box.ShowDialog();
            string str = System.IO.Path.GetFullPath(@"../../../Repo_Files/");
            
            String f_name = "";
            if (result == true)
            {
                string[] file_names = dialog_box.FileNames;
                
                foreach (var file in file_names)
                {
                   f_name = System.IO.Path.GetFileName(file);
                   
                    System.IO.File.Copy(file, str+f_name,true);
                    f_name = "";
                   
                }
                
            }
            
            MessageBox.Show(String.Format("files stored in Repo location :{0}", str));

        }

        private void upload_Click(object sender, RoutedEventArgs e)
        {
            Load_Repo_Files();
            
        }

        private void Load_Repo_Files()
        {
            //add exisiting .cs files from repo
            lb1.Items.Clear();
            DirectoryInfo dir = new DirectoryInfo(@"../../../Repo_Files");
            FileInfo[] list_of_files = dir.GetFiles("*.cs");
            foreach (FileInfo file in list_of_files)
            {
                lb1.Items.Add(file);
            }
            //add exisiting .xml(test request) files from repo
            lb4.Items.Clear();
            DirectoryInfo xml_dir = new DirectoryInfo(@"../../../Repo_Files/Existing_Test_Requests");
            FileInfo[] xml_list_of_files = xml_dir.GetFiles("*.xml");
            foreach (FileInfo file in xml_list_of_files)
            {
                lb4.Items.Add(file);
            }
        }

        private void existing_test_request_click(object sender, RoutedEventArgs e)
        {
            string source_path = @"../../../Repo_Files/Existing_Test_Requests";
            string dest_path = @"../../../Repo_Files";
            string source_file_name = "";
            string source_file = "";
            string destination_file = "";
            if (lb4.SelectedItems.Count != 0)
            {

                foreach (var str in lb4.SelectedItems)
                {
                    lb5.Items.Add(str);
                    // logic to take files from separate folder to repo folder
                    destination_file = "";
                    source_file_name = "" + str;
                    source_file = System.IO.Path.Combine(source_path, source_file_name);
                    destination_file = System.IO.Path.Combine(dest_path, source_file_name);
                    System.IO.File.Copy(source_file, destination_file, true);
                    //
                }
            }
            else
            {
                MessageBox.Show("Please select at least one of the existing test request");
            }
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
        

