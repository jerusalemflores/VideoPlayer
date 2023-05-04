using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Net.Mime;
using System.Linq.Expressions;

namespace WpfTutorialSamples.Audio_and_Video
{
    public partial class AudioVideoPlayerCompleteSample : Window
    {
        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;
        private List<double[]> SkipTimes = new List<double[]>();
        private string SkipFile = "";
        private Boolean isPlaying = false;
        private bool userIsDraggingVolSlider = false;
        List<string> CutList = new List<string>();
        List<string> RestartList = new List<string>();
        private bool Loop = false;

        public AudioVideoPlayerCompleteSample()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
            mePlayer.MediaEnded += Media_MediaEnded;
        }

        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (Loop)
            {
                mePlayer.Stop();
                mePlayer.Play();
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((mePlayer.Source != null) && (mePlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = mePlayer.Position.TotalSeconds;
                ApplyTimeFilters();
                if (mePlayer.Position.TotalSeconds >= mePlayer.NaturalDuration.TimeSpan.TotalSeconds)
                {
                    mePlayer.Position = TimeSpan.FromSeconds(0);
                }
            }
        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Media files";
            if (dialog.ShowDialog() == true)
            {
                mePlayer.Source = new Uri(dialog.FileName);
                
            }
        }

        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Stop();
            mediaPlayerIsPlaying = false;
        }

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            mePlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblProgressStatus.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            mePlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }

        private void mePlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show(e.ErrorException.Message);
        }

        private void OpenSkipFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                using (var reader = new StreamReader(openFileDialog.FileName))
                {
                    CutList.Clear();
                    RestartList.Clear();
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        char[] delimiters = { ' ', '\t' };
                        string[] words = line.Split(delimiters);
                        timesCombo.Items.Add(words[0] + " - " + words[1]);

                        CutList.Add(words[0]);
                        RestartList.Add(words[1]);
                    }
                }

            }
        } 

        /*private void BuildSkipList()
        {
            if (File.Exists(SkipFile))
            {
                SkipTimes.Clear();
                StreamReader file = new StreamReader(SkipFile);
                string line;

                while ((line = file.ReadLine()) != null)
                {
                    string[] tmp = line.Split(",");
                    double[] add = new double[] { 0, 0 };
                    double.TryParse(tmp[0], out add[0]);
                    double.TryParse(tmp[1], out add[1]);
                    SkipTimes.Add(add);
                }
                file.Close();
            }
            AddTimesToMenu();
        }*/

        /*private void AddNewSkip(object sender, RoutedEventArgs e)
        {

            string newSkip = "";
            //checks for blank inputs
            if (timesCombo.Text != "")
            {
                newSkip = String.Concat(timesCombo.Text, "\n");
            }
            if (newSkip == "")
                return;

            if (File.Exists(SkipFile))
            {
                File.AppendAllText(SkipFile, newSkip);
            }
            timesCombo.Text = "";
            BuildSkipList();
            //AddTimesToMenu();
        }*/

        private void ApplyTimeFilters()
        {

            for (int i = 0; i < CutList.Count; i++)
            {
                double cut = TimeStringToDouble(CutList[i].ToString());
                double restart = TimeStringToDouble(RestartList[i].ToString());
                if (mePlayer.Position.TotalSeconds >= cut &&
                    mePlayer.Position.TotalSeconds < restart)
                {
                    mePlayer.Position = TimeSpan.FromSeconds(restart);
                }
            }
        }

        private double TimeStringToDouble(string s)
        {
            char[] delimiters = { ':' };
            string[] parts = s.Split(delimiters);
            int numParts = parts.Length;
            double timeInSeconds = Double.Parse(parts[numParts - 1]);
            if (numParts > 1)
                timeInSeconds += Double.Parse(parts[numParts - 2]) * 60;
            if (numParts > 2)
                timeInSeconds += Double.Parse(parts[numParts - 3]) * 60 * 60;
            if (numParts > 3)
                timeInSeconds += Double.Parse(parts[numParts - 4]) * 60 * 60 * 24;
            return timeInSeconds;

        }

        /*private void AddTimesToMenu()
        {
            timesCombo.Items.Clear();
            if (File.Exists(SkipFile))
            {
                StreamReader file = new StreamReader(SkipFile);
                string line;
                int i = 0;
                while ((line = file.ReadLine()) != null)
                {
                    timesCombo.Items.Insert(i, line);
                    i++;
                }
                file.Close();
            }
        }*/

        /*private void SkipMenu_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }*/

        /*private void DelSkip(object sender, RoutedEventArgs e)
        {
            if (timesCombo.Text != "Skip Times:")
            {
                string tmp = timesCombo.SelectedItem.ToString();
                string[] skip = tmp.Split(",");
                for (int i = 0; i < SkipTimes.Count; i++)
                {
                    if (SkipTimes[i][0].ToString() == skip[0] && SkipTimes[i][1].ToString() == skip[1])
                    {
                        SkipTimes.RemoveAt(i);
                    }

                }
                timesCombo.Items.Remove(timesCombo.SelectedItem);
                UpdateSkipFile();
                timesCombo.Text = "Skip Times:";
            }

        }

        private void UpdateSkipFile()
        {
            if (File.Exists(SkipFile))
            {
                File.WriteAllText(SkipFile, "");
                for (int i = 0; i < SkipTimes.Count; i++)
                {
                    string time = String.Concat(SkipTimes[i][0].ToString(), ",", SkipTimes[i][1].ToString(), "\n");
                    File.AppendAllText(SkipFile, time);
                }
            }
        }*/
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                mePlayer.Pause();
                isPlaying = false;
                PlayButtonText.Text = "Play";
            }
            else
            {
                mePlayer.Play();
                isPlaying = true;
                mediaPlayerIsPlaying = true;
                PlayButtonText.Text = "Pause";
            }
        }

        private void PlayButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            mePlayer.Stop();
            isPlaying = false;
            mediaPlayerIsPlaying = false;
            PlayButtonText.Text = "Pause";
        }
        private void ToggleSlowMo(object sender, RoutedEventArgs e)
        {
            mePlayer.Position = TimeSpan.FromSeconds(mePlayer.Position.TotalSeconds + 10);
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            mePlayer.Position = TimeSpan.FromSeconds(mePlayer.Position.TotalSeconds - 10);
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            userIsDraggingVolSlider = false;
            mePlayer.Volume = (double)volumeSlider.Value;
        }

        private void Speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
  
            mePlayer.SpeedRatio = (double)speedRatioSlider.Value;
        }

    }
}