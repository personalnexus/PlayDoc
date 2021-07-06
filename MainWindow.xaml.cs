using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace PlayDocumentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Labels = new List<string> { "Choreographie", "Dramaturgie", "Fotos", "Maske", "Mitarbeit", "Musik", "Sprechgestaltung", "Spielleitung", "Technik", "Video" };
            DataContext = this;
            Reset(null, null);
        }

        public List<string> Labels { get; set; }

        public void GenerateXml(object sender, RoutedEventArgs e)
        {
            var play = new play();
            play.description = (Subtitle.Text.Trim() + "\r\n"  + 
                                Teaser.Text.Trim() + "\r\n" + 
                                Description.Text.Trim())
                               .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                               .ToArray();

            play.category = AgeGroup.Text;

            play.duration = GetDurationInMinutes();

            AppendJob(play.jobs, "Von und mit", Players.Text);
            AppendJob(play.jobs, OtherLabel1.Text, OtherText1.Text);
            AppendJob(play.jobs, OtherLabel2.Text, OtherText2.Text);
            AppendJob(play.jobs, OtherLabel3.Text, OtherText3.Text);
            AppendJob(play.jobs, OtherLabel4.Text, OtherText4.Text);
            AppendJob(play.jobs, OtherLabel5.Text, OtherText5.Text);
            AppendJob(play.jobs, OtherLabel6.Text, OtherText6.Text);
            AppendJob(play.jobs, OtherLabel7.Text, OtherText7.Text);
            AppendJob(play.jobs, OtherLabel8.Text, OtherText8.Text);
            AppendJob(play.jobs, OtherLabel9.Text, OtherText9.Text);

            DateTime firstPerformance = DateTime.Today;
            if (!string.IsNullOrWhiteSpace(Shows.Text))
            {
                play.performances = Shows.Text
                                    .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(x => ParsePerformance(x, ref firstPerformance))
                                    .ToArray();
            }

            string xmlFileName = @"C:\Tmp\Plays\" + firstPerformance.Year.ToString() + " - " + (string.IsNullOrWhiteSpace(PlayTitle.Text) ? "XXXX" : PlayTitle.Text.Trim()) + ".xml";
            using (XmlWriter writer = XmlWriter.Create(xmlFileName, new XmlWriterSettings { NewLineOnAttributes = true, Indent = true }))
            {
                var serializer = new XmlSerializer(typeof(play));
                serializer.Serialize(writer, play);
            }
            Process.Start(xmlFileName);
        }

        private void AppendJob(playJob[] jobs, string title, string names)
        {
            if (!string.IsNullOrWhiteSpace(names) && !string.IsNullOrWhiteSpace(title))
            {
                Array.Resize(ref jobs, (jobs?.Length ?? 0) + 1);
                var job = new playJob();
                job.title = title;
                job.person = names.Split(',').Select(x => new playJobPerson { name = x.Trim() }).ToArray();
                jobs[jobs.Length - 1] = job;
            }
        }

        private playPerformance ParsePerformance(string input, ref DateTime firstPerformance)
        {
            DateTime date = default(DateTime);
            bool dateSpecified = false;
            string description = null;
            if (input.Length >= 10)
            {
                if (DateTime.TryParseExact(input.Substring(0, 10), "dd.MM.yyyy", CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out date))
                {
                    dateSpecified = true;
                }
                if (input.Length > 10)
                {
                    description = input.Substring(10).Trim();
                }
            }
            var result = new playPerformance
            {
                date = date,
                dateSpecified = dateSpecified,
                description = description,
            };
            if (firstPerformance.Year > date.Year)
            {
                firstPerformance = date;
            }
            return result;
        }

        public void GenerateHtml(object sender, EventArgs e)
        {
            var output = new StringBuilder();
            string subtitle = Subtitle.Text.Trim();
            string ageGroup = AgeGroup.Text.Trim();
            if (!string.IsNullOrWhiteSpace(ageGroup) && !string.IsNullOrWhiteSpace(subtitle))
            {
                ageGroup = ", " + ageGroup;
            }
            output.AppendLine(subtitle + ageGroup);
            output.AppendLine();
            output.AppendLine(Teaser.Text.Trim().Replace("\r\n\r\n", "\r\n").Replace("\r\n", "\r\n\r\n"));
            output.AppendLine();
            output.AppendLine("<!--more-->");
            output.AppendLine();
            output.AppendLine(Description.Text.Trim().Replace("\r\n\r\n", "\r\n").Replace("\r\n", "\r\n\r\n"));
            output.AppendLine();
            output.AppendLine("<dl class=\"dl-horizontal\">");

            output.AppendKeyValue("Von und mit", Players);
            output.AppendKeyValue(OtherLabel1, OtherText1);
            output.AppendKeyValue(OtherLabel2, OtherText2);
            output.AppendKeyValue(OtherLabel3, OtherText3);
            output.AppendKeyValue(OtherLabel4, OtherText4);
            output.AppendKeyValue(OtherLabel5, OtherText5);
            output.AppendKeyValue(OtherLabel6, OtherText6);
            output.AppendKeyValue(OtherLabel7, OtherText7);
            output.AppendKeyValue(OtherLabel8, OtherText8);
            output.AppendKeyValue(OtherLabel9, OtherText9);
            string duration = GetDurationInMinutes();
            if (!string.IsNullOrWhiteSpace(duration))
            {
                output.AppendKeyValue("Dauer", duration + " Minuten");
            }
            output.AppendKeyValue("Aufführungen", GenerateShows());

            output.AppendLine();
            output.AppendLine("</dl>");

            Output.Text = output.ToString();
            Output.SelectAll();
            Output.Copy();
        }

        private string GetDurationInMinutes()
        {
            string duration = "";
            foreach (char character in Duration.Text)
            {
                int dummy;
                if (int.TryParse(character.ToString(), out dummy))
                {
                    duration += character;
                }
            }
            return duration;
        }

        private string GenerateShows()
        {
            string result = null;

            string[] shows = Shows.Text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (shows.Length != 0)
            {
                if (shows.Length == 1)
                {
                    result = shows[0];
                }
                else
                {
                    bool maskeradeReplaced = false;
                    var showsOutput = new StringBuilder();
                    showsOutput.AppendLine("<ul>");
                    foreach (string show in shows)
                    {
                        string showWithLink;
                        if (show.Contains("Maskerade") && !maskeradeReplaced)
                        {
                            showWithLink = show.Replace("Maskerade", "<a href=\"http://maskerade.de\" target=\"_blank\">Maskerade</a>");
                            maskeradeReplaced = true;
                        }
                        else
                        {
                            showWithLink = show;
                        }
                        showsOutput.AppendLine("<li>" + showWithLink + "</li>");
                    }
                    showsOutput.AppendLine("</ul>");

                    result = showsOutput.ToString();
                }
            }
            return result;
        }

        private void Reset(object sender, RoutedEventArgs e)
        {
            AgeGroup.Text = "Schülerinnen und Schüler der Jahrgangsstufen X-Y";
            Subtitle.Text = "Eigenproduktion";
            Teaser.Clear();
            Description.Clear();
            Players.Clear();
            Duration.Clear();
            Shows.Clear();
            OtherText1.Clear();
            OtherText2.Clear();
            OtherText3.Clear();
            OtherText4.Clear();
            OtherText5.Clear();
            OtherText6.Clear();
            OtherText7.Clear();
            OtherText8.Clear();
            OtherText9.Clear();
            Output.Clear();
        }
    }
}
