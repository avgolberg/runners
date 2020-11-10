using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Бега
{
    public partial class Form1 : Form
    {
        public static AutoResetEvent waitHandler = new AutoResetEvent(false);
        public static int startX, finishX;
        List<Runner> runners = new List<Runner>();
        public static int step;
        public static int sleep;
        Runner ourRunner;
        public Form1()
        {
            InitializeComponent();

            Random random = new Random();
            step = 1;
            sleep = random.Next(40, 60);

            startX = heroPictureBox1.Location.X;
            finishX = finishButton.Location.X - finishButton.Width - heroPictureBox1.Width;

            ourRunner = new Runner(heroPictureBox1, "Your Mouse");
            ourRunner.lifes = 5;
            ourRunner.skillScores = 1;

            stepLabel.Text = ourRunner.step.ToString();
            sleepLabel.Text = ourRunner.sleep.ToString() + " ms";
            scoresLabel.Text = ourRunner.skillScores.ToString();

            KeyPreview = true;
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (ourRunner.lifes != 0)
            {
                runners.Clear();
                runners.Add(ourRunner);
                runners.Add(new Runner(heroPictureBox2, "Second Mouse"));
                runners.Add(new Runner(heroPictureBox3, "Third Mouse"));

                runners[0].Reset();

                Thread thread = new Thread(WaitFinish);
                thread.IsBackground = true;
                thread.Start();

                foreach (var runner in runners)
                {
                    runner.thread.Start();
                }

                ButtonsEnabledFalse(startButton, sleepPlusButton1, stepPlusButton2, lifePlusButton3);
                resetToolStripMenuItem.Enabled = false;
            }
        }

        private void WaitFinish()
        {
            waitHandler.WaitOne();

            foreach (var runner in runners)
            {
                runner.thread.Abort();
            }

            foreach (var runner in runners)
            {
                if (runner.Finish == true)
                {
                    MessageBox.Show(runner.Name, "Winner");
                    if (runner.Name == ourRunner.Name)
                    {
                        ourRunner.skillScores += 3;
                        scoresLabel.Invoke(new Action(() => scoresLabel.Text = ourRunner.skillScores.ToString()));
                    }
                }
            }

            step = ourRunner.step;
            sleep = ourRunner.sleep;

            if (ourRunner.lifes != 0) startButton.Invoke(new Action(() => startButton.Enabled = true));
            if (ourRunner.sleep != 0) sleepPlusButton1.Invoke(new Action(() => sleepPlusButton1.Enabled = true));
            stepPlusButton2.Invoke(new Action(() => stepPlusButton2.Enabled = true));
            lifePlusButton3.Invoke(new Action(() => lifePlusButton3.Enabled = true));
            resetToolStripMenuItem.GetCurrentParent().Invoke(new Action(() => resetToolStripMenuItem.Enabled = true));
        }

        private void ButtonsEnabledFalse(params object[] senders)
        {
            for (int i = 0; i < senders.Length; i++)
            {
                Button button = senders[i] as Button;
                button.Enabled = false;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && startButton.Enabled == false)
            {
                if (ourRunner.lifes != 0)
                {
                    ourRunner.step += 1;
                    stepLabel.Text = ourRunner.step.ToString();

                    step = ourRunner.step;
                    sleep = ourRunner.sleep;

                    ourRunner.lifes -= 1;

                    if (ourRunner.lifes == 4) life5.Visible = false;
                    if (ourRunner.lifes == 3) life4.Visible = false;
                    if (ourRunner.lifes == 2) life3.Visible = false;
                    if (ourRunner.lifes == 1) life2.Visible = false;   
                    if (ourRunner.lifes == 0) 
                    {
                        life1.Visible = false;
                        ourRunner.thread.Abort();
                    }
                }
            }
        }

        private void sleepPlusButton1_Click(object sender, EventArgs e)
        {
            if (ourRunner.sleep != 0 && ourRunner.skillScores != 0)
            {
                ourRunner.sleep -= 1;
                sleepLabel.Text = ourRunner.sleep.ToString() + " ms";

                ourRunner.skillScores -= 1;
                scoresLabel.Text = ourRunner.skillScores.ToString();

                step = ourRunner.step;
                sleep = ourRunner.sleep;

                if (ourRunner.sleep == 0) sleepPlusButton1.Enabled = false;
            }
        }

        private void stepPlusButton2_Click(object sender, EventArgs e)
        {
            if (ourRunner.skillScores != 0)
            {
                ourRunner.step += 1;
                stepLabel.Text = ourRunner.step.ToString();

                ourRunner.skillScores -= 1;
                scoresLabel.Text = ourRunner.skillScores.ToString();

                step = ourRunner.step;
                sleep = ourRunner.sleep;
            }
        }

        private void lifePlusButton3_Click(object sender, EventArgs e)
        {
            if (ourRunner.skillScores != 0 && life5.Visible != true)
            {
                ourRunner.lifes += 1;
                if (ourRunner.lifes == 1) life1.Visible = true;
                if (ourRunner.lifes == 2) life2.Visible = true;
                if (ourRunner.lifes == 3) life3.Visible = true;
                if (ourRunner.lifes == 4) life4.Visible = true;
                if (ourRunner.lifes == 5) life5.Visible = true;

                if (ourRunner.lifes > 0) startButton.Enabled = true;

                ourRunner.skillScores -= 1;
                scoresLabel.Text = ourRunner.skillScores.ToString();
            }
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Random random = new Random();
            step = 1;
            sleep = random.Next(40, 60);

            ourRunner = new Runner(heroPictureBox1, "Your Mouse");
            ourRunner.lifes = 5;

            runners[1].Reset();
            runners[2].Reset();

            life1.Visible = true;
            life2.Visible = true;
            life3.Visible = true;
            life4.Visible = true;
            life5.Visible = true;

            stepLabel.Text = ourRunner.step.ToString();
            sleepLabel.Text = ourRunner.sleep.ToString() + " ms";
            scoresLabel.Text = ourRunner.skillScores.ToString();
        }

    }

    public class Runner
    {
        static Random random = new Random();
        public string Name { get; set; }
        public PictureBox box { get; set; }
        public Thread thread { get; set; }
        public int sleep { get; set; }
        public int step { get; set; }
        public int lifes { get; set; }
        public int skillScores { get; set; }

        public bool Finish;

        public Runner(PictureBox box, string name)
        {
            this.Name = name;
            this.box = box;

            if (Form1.step - 1 != 0) step = random.Next(Form1.step - 1, Form1.step + 1);
            else step = Form1.step;

            sleep = random.Next(Form1.sleep - 5, Form1.sleep + 5);

            box.Location = new Point(Form1.startX, box.Location.Y);

            thread = new Thread(Run);
            thread.IsBackground = true;

            Finish = false;
        }
        public void Reset()
        {
            thread = new Thread(Run);
            thread.IsBackground = true;

            Finish = false;

            box.Location = new Point(Form1.startX, box.Location.Y);
        }
        private void Run()
        {
            for (int i = 0; i < Form1.finishX;)
            {
                box.Invoke(new Action(() => box.Location = new Point(box.Location.X + step, box.Location.Y)));
                i += step;
                Thread.Sleep(sleep);
            }

            Finish = true;
            Form1.waitHandler.Set();
        }
    }
}
