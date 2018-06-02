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


using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;



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

        public Task periodic_task;

        public Form1()
        {
            ///Create the ui stuff
            InitializeComponent();

            periodic_task = PeriodicTaskFactory.Start(() =>
            {
                string dataset = File.ReadAllText("C:\\Users\\Admin\\Documents\\GitHub\\vidserver\\vidServer\\song_request\\leo_.txt");
                //format is url ; songname | url .. 
                string[] url_namePair = dataset.Split('|');
                List<string> namePair_list = new List<string>();
                namePair_list.AddRange(url_namePair);
                
                List<Tuple<string,string> >song_data = new List<Tuple<string, string>>();
                namePair_list = namePair_list.Distinct().ToList();

                File.WriteAllText("C:\\Users\\Admin\\Documents\\GitHub\\vidserver\\vidServer\\song_request\\leo_.txt", "");

                foreach (string pair in url_namePair)
                {
                    string[] url_name = pair.Split(';');
                    try
                    {
                        Tuple<string, string> t1 = new Tuple<string, string>(url_name[0] , url_name[1]);
                        song_data.Add(t1);
                        if (url_name.Length == 2)
                        {
                            DownloadMusic(url_name[0], url_name[1]);

                        }
                    }
                    catch (Exception)
                    {

                    }


                }
            },
            intervalInMilliseconds: 1000 * 60 *5);//every 60 seconds 


        }



        private void button1_Click(object sender, EventArgs e)
        {

            if (!richTextBox1.Text.Contains("you") || (richTextBox1.Text == ""))
            {
                MessageBox.Show("invalid url!");
                return;
            }

            DownloadMusic(richTextBox1.Text, textBox1.Text);
        }

        private void DownloadMusic(string url, string songname)
        {

            #region download
            //download the song here 

            Process ytdl = new Process
            {
                StartInfo = { UseShellExecute = false, RedirectStandardError = true, FileName = @"cmd.exe" }
            };

            string path; //= @"cd C:\Users\Admin\Desktop\youtubeVidyabot\youtubeVidyabot\bin\Debug";
            path = "cd " + System.Environment.CurrentDirectory;

            path += " &&node ytdl -o " + @" ""{ { author.name} } "" " + url  + " audioonly";
            ExecuteCommandSync(path);

            richTextBox1.Invoke(new Action(() =>
            {
                richTextBox1.Text = output;
            }));


            // this is not thread safe richTextBox1.Text = output;
            // output = output.Replace(' ', '_'); //these pesky spaces dont work well as command line args || this is obviously not nesessary 
            #endregion

            #region get_song_name
            //extract the song's name from the downloaded data 

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

            System.Threading.Thread.Sleep(2000);



            //----------------------------------------------------
            //make items title filename safe
            items["title"] = Path.GetInvalidFileNameChars().Aggregate(items["title"],
           (current, c) => current.Replace(c, '-'));

            //remove anything that is not a letter lol
            List<char> badCh = new List<char>();
            foreach (char item in items["title"])
            {
                if (item < 65 || (item > 90 && item < 97) || item > 122)
                {
                    badCh.Add(item);
                }
            }
            foreach (var item in badCh) //remove everything!
            {
                items["title"] = items["title"].Replace(item, Convert.ToChar(0));
            }
            if (items["title"].Length < 3)
            {
                items["title"] += "looks like u used a bad char sequence rename song next time";
            }

            //--------------------------------------------------------

            //if the user specifies a custom name rename the song, eg if it has weird characters 
            string filename = "";
            if (songname.Length >= 1)
            {
                //it is critical that songs with weird characters are renamed 
                items["title"] = songname; //use this in case of weird ascii characters 

            }
            else
            {
                filename = items["title"];
            }


            //find the song in the filesystem to rename it later 
            string[] files =
            System.IO.Directory.GetFiles(Environment.CurrentDirectory);
            string FILE_NAME = "";
            foreach (string item in files)
            {
                if (item.Contains("author.name"))
                {
                    FILE_NAME = item;
                    break;
                }
            }

            //appends a songname to a list of song names in a file so that the user can later download it 
            //change leo_ with a different person's username if needed 
            File.AppendAllText("C:\\Users\\Admin\\Documents\\GitHub\\vidserver\\vidServer\\songlists\\leo_.txt", items["title"] + ";"); //no need to url encode the songname 
            //because it wont be broken .(its not a url)



            //rename the song to its original name 
            try
            {
                System.IO.File.Move(FILE_NAME, items["title"]);//rename it, items title must always be a proper filename 
            }
            catch (Exception)
            {

                return;
            }

            #endregion


            #region extract_audio
            path = null;
            //  path = @"cd C:\Users\Admin\Desktop\youtubeVidyabot\youtubeVidyabot\bin\Debug";
            path = "cd " + System.Environment.CurrentDirectory;
            path += " && ffmpeg.exe -i " + " " + '"' + items["title"] + '"' + " " + '"' + items["title"] + '"' + ".mp3";


            ExecuteCommandSync(path);

            #endregion





            int songcount = Convert.ToInt32(File.ReadAllText("C:\\Users\\Admin\\Documents\\GitHub\\vidserver\\vidServer\\leo_.txt"));
            songcount++;
            File.WriteAllText("C:\\Users\\Admin\\Documents\\GitHub\\vidserver\\vidServer\\leo_.txt", songcount.ToString()); //update the client's songcount. 




            int p = 5;
            p++;

            //in case of duplicates, it will throw an exception 
            try
            {
                System.IO.File.Move(items["title"] + ".mp3", "mp3s\\" + items["title"] + ".mp3");
            }
            catch (Exception)
            {
                //wrong name!
            }


            foreach (string item in files)
            {
                if (item.Contains("╚"))
                {
                    FILE_NAME = item;
                    break;
                }
            }
            System.IO.File.Delete(items["title"] + ".mp3"); //cleanup 
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

            richTextBox1.Invoke(new Action(()=>{
                richTextBox1.Text = "done!";
            }));
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

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }

    #region factory_periodic_task
    /// <summary>
    /// Factory class to create a periodic Task to simulate a <see cref="System.Threading.Timer"/> using <see cref="Task">Tasks.</see>
    /// </summary>
    public static class PeriodicTaskFactory
    {
        /// <summary>
        /// Starts the periodic task.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="intervalInMilliseconds">The interval in milliseconds.</param>
        /// <param name="delayInMilliseconds">The delay in milliseconds, i.e. how long it waits to kick off the timer.</param>
        /// <param name="duration">The duration.
        /// <example>If the duration is set to 10 seconds, the maximum time this task is allowed to run is 10 seconds.</example></param>
        /// <param name="maxIterations">The max iterations.</param>
        /// <param name="synchronous">if set to <c>true</c> executes each period in a blocking fashion and each periodic execution of the task
        /// is included in the total duration of the Task.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <param name="periodicTaskCreationOptions"><see cref="TaskCreationOptions"/> used to create the task for executing the <see cref="Action"/>.</param>
        /// <returns>A <see cref="Task"/></returns>
        /// <remarks>
        /// Exceptions that occur in the <paramref name="action"/> need to be handled in the action itself. These exceptions will not be 
        /// bubbled up to the periodic task.
        /// </remarks>
        public static Task Start(Action action,
                                 int intervalInMilliseconds = Timeout.Infinite,
                                 int delayInMilliseconds = 0,
                                 int duration = Timeout.Infinite,
                                 int maxIterations = -1,
                                 bool synchronous = false,
                                 CancellationToken cancelToken = new CancellationToken(),
                                 TaskCreationOptions periodicTaskCreationOptions = TaskCreationOptions.None)
        {
            Stopwatch stopWatch = new Stopwatch();
            Action wrapperAction = () =>
            {
                CheckIfCancelled(cancelToken);
                action();
            };

            Action mainAction = () =>
            {
                MainPeriodicTaskAction(intervalInMilliseconds, delayInMilliseconds, duration, maxIterations, cancelToken, stopWatch, synchronous, wrapperAction, periodicTaskCreationOptions);
            };

            return Task.Factory.StartNew(mainAction, cancelToken, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        /// <summary>
        /// Mains the periodic task action.
        /// </summary>
        /// <param name="intervalInMilliseconds">The interval in milliseconds.</param>
        /// <param name="delayInMilliseconds">The delay in milliseconds.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="maxIterations">The max iterations.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <param name="stopWatch">The stop watch.</param>
        /// <param name="synchronous">if set to <c>true</c> executes each period in a blocking fashion and each periodic execution of the task
        /// is included in the total duration of the Task.</param>
        /// <param name="wrapperAction">The wrapper action.</param>
        /// <param name="periodicTaskCreationOptions"><see cref="TaskCreationOptions"/> used to create a sub task for executing the <see cref="Action"/>.</param>
        private static void MainPeriodicTaskAction(int intervalInMilliseconds,
                                                   int delayInMilliseconds,
                                                   int duration,
                                                   int maxIterations,
                                                   CancellationToken cancelToken,
                                                   Stopwatch stopWatch,
                                                   bool synchronous,
                                                   Action wrapperAction,
                                                   TaskCreationOptions periodicTaskCreationOptions)
        {
            TaskCreationOptions subTaskCreationOptions = TaskCreationOptions.AttachedToParent | periodicTaskCreationOptions;

            CheckIfCancelled(cancelToken);

            if (delayInMilliseconds > 0)
            {
                Thread.Sleep(delayInMilliseconds);
            }

            if (maxIterations == 0) { return; }

            int iteration = 0;

            ////////////////////////////////////////////////////////////////////////////
            // using a ManualResetEventSlim as it is more efficient in small intervals.
            // In the case where longer intervals are used, it will automatically use 
            // a standard WaitHandle....
            // see http://msdn.microsoft.com/en-us/library/vstudio/5hbefs30(v=vs.100).aspx
            using (ManualResetEventSlim periodResetEvent = new ManualResetEventSlim(false))
            {
                ////////////////////////////////////////////////////////////
                // Main periodic logic. Basically loop through this block
                // executing the action
                while (true)
                {
                    CheckIfCancelled(cancelToken);

                    Task subTask = Task.Factory.StartNew(wrapperAction, cancelToken, subTaskCreationOptions, TaskScheduler.Current);

                    if (synchronous)
                    {
                        stopWatch.Start();
                        try
                        {
                            subTask.Wait(cancelToken);
                        }
                        catch { /* do not let an errant subtask to kill the periodic task...*/ }
                        stopWatch.Stop();
                    }

                    // use the same Timeout setting as the System.Threading.Timer, infinite timeout will execute only one iteration.
                    if (intervalInMilliseconds == Timeout.Infinite) { break; }

                    iteration++;

                    if (maxIterations > 0 && iteration >= maxIterations) { break; }

                    try
                    {
                        stopWatch.Start();
                        periodResetEvent.Wait(intervalInMilliseconds, cancelToken);
                        stopWatch.Stop();
                    }
                    finally
                    {
                        periodResetEvent.Reset();
                    }

                    CheckIfCancelled(cancelToken);

                    if (duration > 0 && stopWatch.ElapsedMilliseconds >= duration) { break; }
                }
            }
        }

        /// <summary>
        /// Checks if cancelled.
        /// </summary>
        /// <param name="cancelToken">The cancel token.</param>
        private static void CheckIfCancelled(CancellationToken cancellationToken)
        {
            if (cancellationToken == null)
                throw new ArgumentNullException("cancellationToken");

            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}

#endregion