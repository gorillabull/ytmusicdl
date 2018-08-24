using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace youtubeVidyabot
{
    class edgeCode
    {
        public void a(string url)
        {
            string result = ""; //for the result of running cmd program
            string output = "";

            
            //the path to the downloader

            string path; //= @"cd C:\Users\Admin\Desktop\youtubeVidyabot\youtubeVidyabot\bin\Debug";
            path = "cd " + System.Environment.CurrentDirectory;
            path += " &&node ytdl -o " + @" ""{ { author.name} } "" " + url + " audioonly";


            // create the ProcessStartInfo using "cmd" as the program to be run,
            // and "/c " as the parameters.
            // Incidentally, /c tells cmd that we want it to execute the command that follows,
            // and then exit.
            System.Diagnostics.ProcessStartInfo procStartInfo =
                new System.Diagnostics.ProcessStartInfo("cmd", "/c " + path);

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
            result = proc.StandardOutput.ReadToEnd();
            output = result;




            //extract the song's name from the downloaded json
            string[] endline = output.Split(new char[] { '\n' });


            System.Collections.Generic.Dictionary<string, string> items = new System.Collections.Generic.Dictionary<string, string>();
            foreach (string item in endline)
            {
                if (item != "")
                {
                    string[] linetoks = item.Split(new char[] { ':' });
                    foreach (var c in System.IO.Path.GetInvalidFileNameChars())//also replace characters which make filenames invalid
                    {
                        linetoks[1] = linetoks[1].Replace(c, '-');

                    }
                    items.Add(linetoks[0], linetoks[1]);
                }
            }

            System.Threading.Thread.Sleep(2000);



            //ADD BAD CHARACTER CHECKING IN HERE

            //find the song in the filesystem to rename it later
            string[] files =
            System.IO.Directory.GetFiles(System.Environment.CurrentDirectory);
            string FILE_NAME = "";
            foreach (string item in files)
            {
                if (item.Contains("author.name"))
                {
                    FILE_NAME = item;
                    break;
                }
            }


            System.IO.File.Move(FILE_NAME, items["title"]);//rename it, items title must always be a proper filename


            path = null;
            //  path = @"cd C:\Users\Admin\Desktop\youtubeVidyabot\youtubeVidyabot\bin\Debug";
            path = "cd " + System.Environment.CurrentDirectory;
            path += " && ffmpeg.exe -i " + " " + '"' + items["title"] + '"' + " " + '"' + items["title"] + '"' + ".mp3";



            // create the ProcessStartInfo using "cmd" as the program to be run,
            // and "/c " as the parameters.
            // Incidentally, /c tells cmd that we want it to execute the command that follows,
            // and then exit.
            System.Diagnostics.ProcessStartInfo procStartInfo2 =
                new System.Diagnostics.ProcessStartInfo("cmd", "/c " + path);

            // The following commands are needed to redirect the standard output.
            // This means that it will be redirected to the Process.StandardOutput StreamReader.
            procStartInfo2.RedirectStandardOutput = true;
            procStartInfo2.UseShellExecute = false;
            // Do not create the black window.
            procStartInfo2.CreateNoWindow = true;
            // Now we create a process, assign its ProcessStartInfo and start it
            System.Diagnostics.Process proc2 = new System.Diagnostics.Process();
            proc2.StartInfo = procStartInfo2;
            proc2.Start();

            // Get the output into a string
            result = proc2.StandardOutput.ReadToEnd();
            output = result;



            System.IO.File.Move(items["title"] + ".mp3", "mp3s\\" + items["title"] + ".mp3");
            System.IO.File.Delete(items["title"] + ".mp3"); //cleanup

        }
    }
}
