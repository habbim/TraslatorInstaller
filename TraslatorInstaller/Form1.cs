using ICSharpCode.SharpZipLib.Tar;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Media;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using VDF;
namespace TraslatorInstaller
{
    
    public partial class Form1 : Form
    {
        public bool audio = true;
        public int totalDess = 0;
        public static string[] DataVer;
        private float mBlend;
        private int mDir = 1;
        public int count = 0;
        public Bitmap[] pictures;

        SoundPlayer snd;
        string[,] versions = new string[,] {
            { "es_esV1_4_1", "Adaptación de las frases: ToniBC"+ Environment.NewLine + "Testeo del Proyecto y Corrector: willsk94 y alvaromegias_46" + Environment.NewLine + "Traductores: ToniBC, Daklas, Vontadeh y Xpromisekpt", "111","b" },
            { "finalfile", "Traducción del juego: hasrock17 y YoSoyJebus" + Environment.NewLine + "Traducción de escenarios: Klumb3r y RichardG" + Environment.NewLine + "Correcciones: winamp1 y lacky" + Environment.NewLine + "Pruebas: AMSHELL", "114","a" }
        }; 
        public static bool installDirect;
        Thread workerThread;
        public static string Dir;
        internal string Get(string uri)
        {
            try
            {
                using (WebResponse wr = WebRequest.Create(uri).GetResponse())
                using (StreamReader sr = new StreamReader(wr.GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }
            }
            catch
            {
                return "1;0;0;0";
            }

        }
        public static bool GetSteamLib()
        {
            try
            {
                bool status = false;
                if (File.Exists(@"D:\Games\Life Is Strange Episode 5\Binaries\Win32\LifeIsStrange.exe"))
                {
                    Dir = @"D:\Games\Life Is Strange Episode 5";
                    return true;
                }
                string SteamPath = (string)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam", "SteamPath", null);
                SteamPath = SteamPath.Replace("/", @"\");
                if (!string.IsNullOrEmpty(SteamPath))
                {
                    var confselect = new SteamConfigFile(SteamPath + @"\config\config.vdf");
                    foreach (string folder in confselect.BaseInstallFolders)
                    {
                        if (File.Exists(folder + @"\steamapps\common\Life Is Strange\Binaries\Win32\LifeIsStrange.exe"))
                        {
                            Dir = folder + @"\steamapps\common\Life Is Strange";
                            return true;
                        }

                    }
                }
                if (!status)
                {
                    if (File.Exists(SteamPath + @"\steamapps\common\Life Is Strange\Binaries\Win32\LifeIsStrange.exe"))
                    {
                        status = true;
                        Dir = SteamPath + @"\steamapps\common\Life Is Strange";
                    }
                }
                return status;
            }
            catch
            {
                return false;
            }
        }
        public Form1()
        {
            InitializeComponent();
            this.Shown += new System.EventHandler(this.Form1_Shown);
        }
        private void BlendTick(object sender, EventArgs e)
        {
            mBlend += mDir * 0.02F;
            if (mBlend > 1)
            {
                mBlend = 0.0F;
                if ((count + 1) < pictures.Length)
                {
                    blendPanel1.Image1 = pictures[count];
                    blendPanel1.Image2 = pictures[++count];
                }
                else
                {
                    blendPanel1.Image1 = pictures[count];
                    blendPanel1.Image2 = pictures[0];
                    count = 0;
                }
            }
            blendPanel1.Blend = mBlend;
        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            snd = new SoundPlayer(Properties.Resources.Amber_Run___I_Found);
            snd.Play();
            timer1.Interval = 160; //time of transition
            pictures = new Bitmap[2];
            timer1.Tick += BlendTick;
            pictures[0] = new Bitmap(Properties.Resources._26B);
            pictures[1] = new Bitmap(Properties.Resources._49F);
            blendPanel1.Image1 = pictures[count];
            blendPanel1.Image2 = pictures[++count];
            timer1.Enabled = true;
            label2.Text = "Rev: " + Assembly.GetExecutingAssembly().GetName().Version + versions[Program.VerInstall, 3];
            DataVer = Get("https://subtitulos.eu/lifeisstrange/ver.php?ver=" + Assembly.GetExecutingAssembly().GetName().Version + "&file=" + versions[Program.VerInstall, 0]).Split(';');
            progressBar1.Maximum = Convert.ToInt32(versions[Program.VerInstall, 2]) + Convert.ToInt32(DataVer[2]);
            if (installDirect)
            {
                textBox1.Text = Dir;
                textBox1.Hide();
                button1.Hide();
                button3.Hide();
                button2.Hide();
                pictureBox2.Hide();
                label1.Show();
                progressBar1.Show();
                workerThread = new Thread(Des);
                workerThread.Start();

            }
            else
            {
                Invoke(new Action(() => {
                    try
                    {
                        /*pictureBox2.Hide();
                        SelectVersion verform = new SelectVersion();
                        verform.ShowDialog();
                        pictureBox2.Show();*/
                        if (DataVer[0] == "0")
                        {
                            DialogResult dialogResult = MessageBox.Show("La versión que estas utilizando esta desactualizada." + Environment.NewLine + "Pulsa aceptar para descargar la ultima versión", "Actualización disponible", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                            if (dialogResult == DialogResult.OK)
                            {
                                Program.dirDownload = Environment.GetEnvironmentVariable("temp") + @"\lis-" + Assembly.GetExecutingAssembly().GetName().Version;
                                Directory.CreateDirectory(Program.dirDownload);
                                DownloadUpdate downform = new DownloadUpdate();
                                downform.ShowDialog();
                            }
                        }
                        MessageBox.Show("Este instalador contiene la traducción de los episodios del 1 al 5" + Environment.NewLine + Environment.NewLine + versions[Program.VerInstall, 1] + Environment.NewLine + "Imagenes: Cloudnixus, usuario de deviantart" + Environment.NewLine + "Instalador: Habbim", "Creditos", MessageBoxButtons.OK);
                        if (GetSteamLib())
                            textBox1.Text = Dir;
                        else
                        {
                            MessageBox.Show("No hemos podido encontrar Life is Strange en las rutas por defecto del juego, selecciona manualmente la ruta donde tienes instalado el juego", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            DialogResult result = this.folderBrowserDialog1.ShowDialog();
                            if (result == DialogResult.OK)
                            {
                                textBox1.Text = this.folderBrowserDialog1.SelectedPath;
                            }
                            else
                                textBox1.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\LifeIsStrange-ES";
                            Dir = textBox1.Text;
                        }

                    }
                    catch (Exception i)
                    {
                        Get("https://subtitulos.eu/lifeisstrange/report.php?data=" + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Environment.NewLine + Environment.NewLine + "Error detallado: " + Environment.NewLine + label2.Text + " Build Date: " + TraslatorInstaller.Properties.Resources.BuildDate + Environment.NewLine + i.ToString())));
                        MessageBox.Show("Ops!! ha sucedido un error envia el siguente codigo a bugs@subtitulos.eu " + Environment.NewLine + Environment.NewLine + "Error detallado: " + Environment.NewLine + label2.Text + " Build Date: " + TraslatorInstaller.Properties.Resources.BuildDate + Environment.NewLine + Environment.NewLine + i.ToString(), "Error Critico", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(0);
                    }
                }));

            }

        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            DialogResult result = this.folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox1.Text = this.folderBrowserDialog1.SelectedPath;
                Dir = textBox1.Text;
            }
        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (audio)
            {
                snd.Stop();
                this.pictureBox1.Image = global::TraslatorInstaller.Properties.Resources.play;
                audio = false;
            }
            else
            {
                snd.Play();
                this.pictureBox1.Image = global::TraslatorInstaller.Properties.Resources.stop;
                audio = true;
            }
        }


        private void Des()
        {
            try
            {
                Stream filebin = new MemoryStream((byte [])binaries.ResourceManager.GetObject(versions[Program.VerInstall,0]));
                TarArchive ta = TarArchive.CreateInputTarArchive(new GZipStream(filebin, CompressionMode.Decompress));
                ta.ProgressMessageEvent += MyNotifier;
                ta.ExtractContents(textBox1.Text);
                ta.Close();
                if (DataVer[1] != "0")
                {
                    Stream filebinDown = new MemoryStream(new WebClient().DownloadData("https://subtitulos.eu/lifeisstrange/parch/" + label2.Text + "/" + DataVer[3]));
                    ta = TarArchive.CreateInputTarArchive(new GZipStream(filebinDown, CompressionMode.Decompress));
                    ta.ProgressMessageEvent += MyNotifier;
                    ta.ExtractContents(textBox1.Text);
                    ta.Close();
                }
                if (File.Exists(textBox1.Text + @"\Binaries\Win32\steam_api.ini"))
                {
                    label1.Text = "Parche NO-STEAM aplicado";
                    File.WriteAllText(textBox1.Text + @"\Binaries\Win32\steam_api.ini", File.ReadAllText(textBox1.Text + @"\Binaries\Win32\steam_api.ini").Replace("english", "spanish"));
                }
                else if (File.Exists(textBox1.Text + @"\Binaries\Win32\CODEX.ini"))
                {
                    label1.Text = "Parche NO-STEAM aplicado";
                    File.WriteAllText(textBox1.Text + @"\Binaries\Win32\CODEX.ini", File.ReadAllText(textBox1.Text + @"\Binaries\Win32\CODEX.ini").Replace("english", "spanish"));
                }
                else if (File.Exists(textBox1.Text + @"\Binaries\Win32\3DMGAME.ini"))
                {
                    label1.Text = "Parche NO-STEAM aplicado";
                    File.WriteAllText(textBox1.Text + @"\Binaries\Win32\3DMGAME.ini", File.ReadAllText(textBox1.Text + @"\Binaries\Win32\3DMGAME.ini").Replace("english", "spanish"));
                }
                Invoke(new Action(() => { 
                    MessageBox.Show("La instalación se ha completado con exito" + Environment.NewLine + Environment.NewLine + versions[Program.VerInstall, 1] + Environment.NewLine + "Imagenes: Cloudnixus, usuario de deviantart" + Environment.NewLine + "Instalador: Habbim", "Instalación Completada", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    Environment.Exit(0);
                }));


            }
            catch (OutOfMemoryException e)
            {
                Get("https://subtitulos.eu/lifeisstrange/report.php?data=" + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Environment.NewLine + Environment.NewLine + "Error detallado: " + Environment.NewLine +  label2.Text + " Build Date: " + TraslatorInstaller.Properties.Resources.BuildDate + Environment.NewLine + e.ToString())));
                Invoke(new Action(() =>
                {
                    MessageBox.Show("Ops!! ha sucedido un error: Para poder realizar la instalación necesitas 512MB de memoria libres, si tienes mas de esa memoria libre en el sistema y si el error persite envia el siguiente codigo al correo: bugs@subtitulos.eu" + Environment.NewLine + Environment.NewLine + "Error detallado: " + Environment.NewLine +  label2.Text + " Build Date: " + TraslatorInstaller.Properties.Resources.BuildDate + Environment.NewLine + e.ToString(), "Error Critico", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }));
            }
            catch (PathTooLongException e)
            {
                Get("https://subtitulos.eu/lifeisstrange/report.php?data=" + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Environment.NewLine + Environment.NewLine + "Error detallado: " + Environment.NewLine + label2.Text + " Build Date: " + TraslatorInstaller.Properties.Resources.BuildDate + Environment.NewLine + e.ToString())));
                Invoke(new Action(() =>
                {
                    MessageBox.Show("Ops!! ha sucedido un error: La ruta donde intentas instalar el parche es demasiado larga", "Error Critico", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }));
            }
            catch (IOException e)
            {
                Get("https://subtitulos.eu/lifeisstrange/report.php?data=" + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Environment.NewLine + Environment.NewLine + "Error detallado: " + Environment.NewLine + label2.Text + " Build Date: " + TraslatorInstaller.Properties.Resources.BuildDate + Environment.NewLine + e.ToString())));
                Invoke(new Action(() =>
                {
                    MessageBox.Show("Ops!! ha sucedido un error: No tienes espacio suficiente en tu disco duro para efectuar la instalación", "Error Critico", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }));
            }
            catch (NullReferenceException e)
            {
                Get("https://subtitulos.eu/lifeisstrange/report.php?data=" + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Environment.NewLine + Environment.NewLine + "Error detallado: " + Environment.NewLine +  label2.Text + " Build Date: " + TraslatorInstaller.Properties.Resources.BuildDate + Environment.NewLine + e.ToString())));
                Invoke(new Action(() =>
                {
                    MessageBox.Show("Ops!! ha sucedido un error envia el siguente codigo a bugs@subtitulos.eu " + Environment.NewLine + Environment.NewLine + "Error detallado: " + Environment.NewLine +  label2.Text + " Build Date: " + TraslatorInstaller.Properties.Resources.BuildDate + Environment.NewLine + e.ToString(), "Error Critico", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0); 
                }));

            }
            catch (IndexOutOfRangeException e)
            {
                Get("https://subtitulos.eu/lifeisstrange/report.php?data=" + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Environment.NewLine + Environment.NewLine + "Error detallado: " + Environment.NewLine +  label2.Text + " Build Date: " + TraslatorInstaller.Properties.Resources.BuildDate + Environment.NewLine + e.ToString())));
                Invoke(new Action(() =>
                {
                    MessageBox.Show("Ops!! ha sucedido un error envia el siguente codigo a bugs@subtitulos.eu " + Environment.NewLine + Environment.NewLine + "Error detallado: " + Environment.NewLine +  label2.Text + " Build Date: " + TraslatorInstaller.Properties.Resources.BuildDate + Environment.NewLine + e.ToString(), "Error Critico", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }));
            }
            catch (UnauthorizedAccessException)
            {
                Invoke(new Action(() =>
                {
                    ProcessStartInfo proc = new ProcessStartInfo();
                    proc.UseShellExecute = true;
                    proc.WorkingDirectory = Environment.CurrentDirectory;
                    proc.FileName = Application.ExecutablePath;
                    proc.Verb = "runas";
                    proc.Arguments = textBox1.Text+" "+ Program.VerInstall;
                    try
                    {
                        Process.Start(proc);
                        Application.Exit();
                    }
                    catch
                    {
                        MessageBox.Show("Ops!! No podemos efectuar la instalación en el directorio especificado si no das permisos de administrador.", "Error Critico", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Application.Exit();
                        return;
                    }

                }));
            }
            catch (Exception e)
            {
                Get("https://subtitulos.eu/lifeisstrange/report.php?data=" + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Environment.NewLine + Environment.NewLine + "Error detallado: " + Environment.NewLine +  label2.Text + " Build Date: " + TraslatorInstaller.Properties.Resources.BuildDate + Environment.NewLine + e.ToString())));
                Invoke(new Action(() =>
                {
                    MessageBox.Show("Ops!! ha sucedido un error envia el siguente codigo a bugs@subtitulos.eu " + Environment.NewLine + Environment.NewLine + "Error detallado: " + Environment.NewLine +  label2.Text + " Build Date: " + TraslatorInstaller.Properties.Resources.BuildDate + Environment.NewLine + e.ToString(), "Error Critico", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }));
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            switch (textBox1.Text)
            {
                case "GERONIMOOOO":
                case "Recuerdame":
                case "RememberMe":
                    {
                        MessageBox.Show("Easter Egg eliminado, a ver si encuentras donde esta el nuevo");
                    }
                    break;
                default:
                    {
                        try
                        {
                            if(textBox1.Text == Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\LifeIsStrange-ES" && !Directory.Exists(textBox1.Text))
                                Directory.CreateDirectory(textBox1.Text);
                            string validate_apsolute = textBox1.Text.Split(':')[1];
                            if (!Directory.Exists(textBox1.Text) || String.IsNullOrEmpty(validate_apsolute))
                            {
                                MessageBox.Show("La ruta introducida no exite o es invalida." + Environment.NewLine + "Ten en cuenta que no se permiten rutas relativas solamente apsolutas.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                bool start = false;
                                if (!File.Exists(textBox1.Text + @"\Binaries\Win32\LifeIsStrange.exe") && textBox1.Text != Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\LifeIsStrange-ES")
                                {
                                    DialogResult dialogResult = MessageBox.Show("La ruta especificada no contiene el juego." + Environment.NewLine + "Si tu versión es No-Steam(Pirata) es posible que la traducción no se aplique." + Environment.NewLine + "¿Estás seguro que quieres utilizarla?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                                    if (dialogResult == DialogResult.Yes)
                                        start = true;
                                }
                                else
                                    start = true;
                                if(start)
                                {
                                    textBox1.Hide();
                                    button1.Hide();
                                    button3.Hide();
                                    button2.Hide();
                                    pictureBox2.Hide();
                                    label1.Show();
                                    progressBar1.Show();
                                    workerThread = new Thread(Des);
                                    workerThread.Start();
                                }


                            }
                        }
                        catch (IndexOutOfRangeException)
                        {
                            MessageBox.Show("La ruta introducida no exite o es invalida." + Environment.NewLine + "Ten en cuenta que no se permiten rutas relativas solamente apsolutas.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        catch (Exception i)
                        {
                            MessageBox.Show("Ops!! ha sucedido un error envia el siguente codigo a bugs@subtitulos.eu " + Environment.NewLine + Environment.NewLine + "Error detallado: " + Environment.NewLine +  label2.Text + " Build Date: " + TraslatorInstaller.Properties.Resources.BuildDate + Environment.NewLine + i.ToString(), "Error Critico", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Environment.Exit(0);
                        }
                    }
                    break;

            }

        }

        public void MyNotifier(TarArchive ta, TarEntry te, string msg)
        {
            if (te.Size > 0)
            {
                Invoke(new Action(() =>
                {
                    label1.Text = "Descomprimiendo: " + te.Name;
                    totalDess = totalDess + 1;
                    progressBar1.Value = totalDess;
                }));
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("¿Estás seguro de que quieres salir?", "Salir", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
        private void CheckKeys(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                button3.PerformClick();
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\LifeIsStrange-ES";
            DialogResult dialogResult = MessageBox.Show("La traducción se va a guardar en tu escritorio dentro de la carpeta LifeIsStrange-ES" + Environment.NewLine + "Si tu versión es No-Steam(Pirata) es posible que la traducción no se aplique." + Environment.NewLine +"¿Estás seguro?", "Descomprimir en escritorio", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                button3.PerformClick();
            }
        }

    }
}
