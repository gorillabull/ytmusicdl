using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.Runtime;
using System.Diagnostics;

using System.IO;
namespace youtubeVidyabot
{
    public partial class Form1 : Form
    {
        public string PathToFfmpeg { get; set; }
        public string output = "";

        public void ToFlacFormat(string pathToMp4, string pathToFlac)
        {
            var ffmpeg = new Process
            {
                StartInfo = { UseShellExecute = false, RedirectStandardError = true, FileName = PathToFfmpeg }
            };

            var arguments =
                String.Format(
                    @"-i ""{0}"" -c:a flac ""{1}""",
                    pathToMp4, pathToFlac);

            ffmpeg.StartInfo.Arguments = arguments;

            try
            {
                if (!ffmpeg.Start())
                {
                    Debug.WriteLine("Error starting");
                    return;
                }
                var reader = ffmpeg.StandardError;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Debug.WriteLine(line);
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.ToString());
                return;
            }

            ffmpeg.Close();
        }



        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (!richTextBox1.Text.Contains("you") || (richTextBox1.Text == ""))
            {
                MessageBox.Show("invalid url!");
                return;
            }


            Process ytdl = new Process
            {
                StartInfo = { UseShellExecute = false, RedirectStandardError = true, FileName = @"cmd.exe" }
            };

            string path; //= @"cd C:\Users\Admin\Desktop\youtubeVidyabot\youtubeVidyabot\bin\Debug";
            path = "cd " + System.Environment.CurrentDirectory;

            path += " &&node ytdl -o " + @" ""{ { author.name} } "" " + richTextBox1.Text + " audioonly";
            ExecuteCommandSync(path);
            richTextBox1.Text = output;
            output = output.Replace(' ', '╚'); //these pesky spaces dont work well as command line args 



            string[] endline = output.Split(new char[] { '\n' });

            Dictionary<string, string> items = new Dictionary<string, string>();
            foreach (string item in endline)
            {
                if (item != "")
                {
                    string[] linetoks = item.Split(new char[] { ':' });
                    foreach (var c in Path.GetInvalidFileNameChars())//also replace characters which make filenames invalid 
                    {
                        linetoks[1] = linetoks[1].Replace(c, '-');

                    }
                    items.Add(linetoks[0], linetoks[1]);
                }
            }


            if (System.IO.File.Exists(items["title"]))
            {
                System.IO.File.Delete(items["title"]);
            }

            System.Threading.Thread.Sleep(2000);

            string filename = items["title"];

            //get the filename 
            string[] files =
            System.IO.Directory.GetFiles(Environment.CurrentDirectory); //, "*ProfileHandler.cs", SearchOption.AllDirectories)
            string FILE_NAME = "";
            foreach (string item in files)
            {
                if (item.Contains("author.name"))
                {
                    FILE_NAME = item;
                    break;
                }
            }


            System.IO.File.Move(FILE_NAME, items["title"]);//rename it

            path = null;
            //  path = @"cd C:\Users\Admin\Desktop\youtubeVidyabot\youtubeVidyabot\bin\Debug";
            path = "cd " + System.Environment.CurrentDirectory;
            path += " && ffmpeg.exe -i " + " " + items["title"] + " conv" + items["title"] + ".mp3";

            
            ExecuteCommandSync(path);
            //@"'{ author.name } - { title }'" "
            filename = filename.Replace('╚', ' ');
            filename += ".mp3";


            if (System.IO.File.Exists("mp3s\\" + filename))
            {
                System.IO.File.Delete("mp3s\\" + filename);
            }

            //appends a songname to a list of song names in a file so that the user can later download it 
            File.AppendAllText("C:\\Users\\Admin\\Documents\\Visual Studio 2017\\Projects\\vidyaServer\\vidyaServer\\songlists\\leo_.txt", filename + ";");

            int p = 5;
            p++;


            System.IO.File.Move("conv" + items["title"] + ".mp3", "mp3s\\" + filename);

            foreach (string item in files)
            {
                if (item.Contains("╚"))
                {
                    FILE_NAME = item;
                    break;
                }
            }
            System.IO.File.Delete(FILE_NAME); //cleanup 
            // ExecuteCommandSync(arguments);

            // System.Diagnostics.Process.Start("cmd.exe", arguments);
            //ytdl.StartInfo.Arguments = arguments;
            /*
                         try
            {
                if ((ytdl = System.Diagnostics.Process.Start("cmd.exe", arguments))!= null )
                {
                    Debug.WriteLine("Error starting");
                    return;
                }
                var reader = ytdl.StandardError;
                string line;
                while ((line = reader.ReadLine() )!= null)
                {
                    Debug.WriteLine(line);
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.ToString());
                return;
            }
            ytdl.Close();

     */

            richTextBox1.Text = "Done!";
        }



        /// <span class="code-SummaryComment"><summary></span>
        /// Executes a shell command synchronously.
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment"><param name="command">string command</param></span>
        /// <span class="code-SummaryComment"><returns>string, as output of the command.</returns></span>
        public void ExecuteCommandSync(object command)
        {
            string pal = "abba";

            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();

                // Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();
                output = result;
                // Display the command output.
                Console.WriteLine(result);

            }
            catch (Exception objException)
            {
                // Log the exception
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string param = "explorer.exe " + System.Environment.CurrentDirectory + "\\mp3s";
            ExecuteCommandSync(param);
        }
    }
}
