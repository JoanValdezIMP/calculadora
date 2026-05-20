using System;
using System.Data; // Necesario para evaluar expresiones matemáticas
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PAC4_Calculadora
{
    /// <summary>
    /// Classe principal que implementa la lògica de la calculadora per a la PAC4.
    /// Incorpora validació d'errors i jerarquia d'operacions, a més d'un Easter Egg ocult.
    /// </summary>
    public partial class MainWindow : Window
    {
        // Variable per emmagatzemar l'operació encadenada completa
        private string _expressio = "";
        private bool _isDoomRunning = false;

        /// <summary>
        /// Constructor de la classe MainWindow. Inicialitza els components gràfics.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.PreviewKeyDown += MainWindow_PreviewKeyDown; // Añade esta línea si no la tienes
        }

        /// <summary>
        /// Mètode que s'executa en fer clic a un botó numèric (0-9) o al punt decimal.
        /// </summary>
        /// <param name="sender">El botó que ha disparat l'esdeveniment.</param>
        /// <param name="e">Arguments de l'esdeveniment.</param>
        private void BtnNumber_Click(object sender, RoutedEventArgs e)
        {
            if (_isDoomRunning) return;

            Button button = (Button)sender;
            string num = button.Content.ToString();

            // Si hi ha un error previ o està a 0, reiniciem la pantalla
            if (CalcDisplay.Text == "0" || CalcDisplay.Text == "Error")
            {
                CalcDisplay.Text = num;
                _expressio = num;
            }
            else
            {
                CalcDisplay.Text += num;
                _expressio += num;
            }

            // TRIGGER DEL DOOM (Intacte)
            if (CalcDisplay.Text == "666")
            {
                RunDoomFullScreen();
            }
        }

        /// <summary>
        /// Mètode que s'executa en fer clic a un operador matemàtic (+, -, ×, ÷).
        /// Gestiona la validació per evitar operadors consecutius.
        /// </summary>
        private void BtnOperator_Click(object sender, RoutedEventArgs e)
        {
            if (_isDoomRunning) return;
            Button button = (Button)sender;
            string op = button.Content.ToString();

            // Adaptem els símbols visuals als operadors que entén DataTable
            string opMatematic = op;
            if (op == "×") opMatematic = "*";
            if (op == "÷") opMatematic = "/";

            // Validació: Comprovar que no s'encadenin dos operadors seguits o faltin operands
            if (_expressio.Length > 0)
            {
                char ultim = _expressio[_expressio.Length - 1];
                if (ultim == '+' || ultim == '-' || ultim == '*' || ultim == '/')
                {
                    MostrarError();
                    return;
                }
            }
            else
            {
                return; // No fem res si es posa un operador sense números
            }

            _expressio += opMatematic;
            CalcDisplay.Text += op; // Mostrem a la pantalla visual el símbol bonic
        }

        /// <summary>
        /// Calcula el resultat de l'expressió acumulada respectant les prioritats operatives.
        /// </summary>
        private void BtnEquals_Click(object sender, RoutedEventArgs e)
        {
            if (_isDoomRunning) return;

            // Validació: Si l'usuari clica '=' just després d'un operador (ex: 5 + =)
            if (_expressio.Length > 0)
            {
                char ultim = _expressio[_expressio.Length - 1];
                if (ultim == '+' || ultim == '-' || ultim == '*' || ultim == '/')
                {
                    MostrarError();
                    return;
                }
            }

            try
            {
                // Mostrem l'expressió completa a la part superior abans de resoldre
                CalcEquation.Text = CalcDisplay.Text + " =";

                // DataTable avalua l'string de text i resol les matemàtiques correctament
                DataTable dt = new DataTable();

                // Reemplacem la coma pel punt per a la sintaxi interna del DataTable si l'usuari fa servir decimals
                string exprPerCalcular = _expressio.Replace(",", ".");
                var resultatFormat = dt.Compute(exprPerCalcular, "");

                // Validem divisions per zero que donen infinit
                if (resultatFormat.ToString() == "∞" || resultatFormat.ToString() == "NaN")
                {
                    MostrarError();
                    return;
                }

                // Convertim de nou al format local i mostrem
                double resultatFinal = Convert.ToDouble(resultatFormat);
                CalcDisplay.Text = resultatFinal.ToString();

                // Guardem el resultat per permetre continuar encadenant operacions
                _expressio = resultatFinal.ToString();
            }
            catch
            {
                MostrarError();
            }
        }

        /// <summary>
        /// Neteja completament la calculadora o surt del motor de joc.
        /// </summary>
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            if (_isDoomRunning)
            {
                ExitDoom();
            }
            else
            {
                CalcDisplay.Text = "0";
                CalcEquation.Text = "";
                _expressio = "";
            }
        }

        /// <summary>
        /// Mètode auxiliar per establir l'estat d'error a la interfície.
        /// </summary>
        private void MostrarError()
        {
            CalcDisplay.Text = "Error";
            CalcEquation.Text = "";
            _expressio = "";
        }

        // ======================================================================
        // A PARTIR D'AQUÍ, EL CODI DE DOOM QUEDA AÏLLAT I NO INTERFEREIX
        // ======================================================================

        /// <summary>
        /// Mètode privat que activa l'entorn de pantalla completa per executar l'Easter Egg.
        /// </summary>
        private async void RunDoomFullScreen()
        {
            if (_isDoomRunning) return;
            _isDoomRunning = true;

            CalculatorUI.Visibility = Visibility.Collapsed;
            DoomContainer.Visibility = Visibility.Visible;

            this.WindowStyle = WindowStyle.None;
            this.WindowState = WindowState.Maximized;

            if (DoomScreen.CoreWebView2 == null)
            {
                await DoomScreen.EnsureCoreWebView2Async(null);
            }

            string localFolderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DoomFiles");

            if (!System.IO.Directory.Exists(localFolderPath))
            {
                MessageBox.Show($"Falten els arxius a: {localFolderPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ExitDoom();
                return;
            }

            DoomScreen.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "doom.offline",
                localFolderPath,
                Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow
            );

            DoomScreen.Source = new Uri("https://doom.offline/index.html");
            DoomScreen.Focus();
        }

        /// <summary>
        /// Tanca l'Easter Egg i restaura la finestra original de la calculadora.
        /// El Doom es pot tancar prement Ctrl+Q o fent clic al botó Clear mentre el Doom està actiu.   
        /// El DoomScreen es manté carregat per si l'usuari vol tornar a activar-lo, evitant errors de recàrrega.
        /// </summary>
        private void ExitDoom()
        {
            _isDoomRunning = false;
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            this.WindowState = WindowState.Normal;
            DoomContainer.Visibility = Visibility.Collapsed;
            //DoomScreen.Source = null; // Aixo dona error si es fa servir, millor deixar la URL carregada per si es torna a activar
            CalculatorUI.Visibility = Visibility.Visible;
            CalcDisplay.Text = "0";
            _expressio = "";
        }

        /// <summary>
        /// Gestor d'esdeveniments de teclat: permet controlar la calculadora i sortir del joc.
        /// </summary>
        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 1. Si Doom està actiu, gestionem la sortida (Ctrl+Q)
            if (_isDoomRunning)
            {
                if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && e.Key == Key.Q)
                {
                    ExitDoom();
                    e.Handled = true;
                }
                return; // Si és Doom, no volem que les tecles facin res a la calculadora
            }

            // 2. Control de la calculadora per teclat
            // Mapeo de tecles numèriques (teclat principal i numèric)
            if (e.Key >= Key.D0 && e.Key <= Key.D9)
                EjecutarBotonPorContenido((e.Key - Key.D0).ToString());
            else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                EjecutarBotonPorContenido((e.Key - Key.NumPad0).ToString());

            // Mapeo d'operadors
            else if (e.Key == Key.Add || e.Key == Key.OemPlus) EjecutarBotonPorContenido("+");
            else if (e.Key == Key.Subtract || e.Key == Key.OemMinus) EjecutarBotonPorContenido("-");
            else if (e.Key == Key.Multiply) EjecutarBotonPorContenido("×");
            else if (e.Key == Key.Divide || e.Key == Key.Oem2) EjecutarBotonPorContenido("÷");
            else if (e.Key == Key.Enter) BtnEquals_Click(null, null);
            else if (e.Key == Key.Escape || e.Key == Key.C) BtnClear_Click(null, null);
        }

        // Mètode auxiliar per buscar el botó i fer clic
        private void EjecutarBotonPorContenido(string contenido)
        {
            foreach (var child in FindVisualChildren<Button>(this))
            {
                if (child.Content != null && child.Content.ToString() == contenido)
                {
                    child.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                }
            }
        }

        // Funció auxiliar necessària per recórrer els botons
        private System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);
                    if (child is T) yield return (T)child;
                    foreach (T childOfChild in FindVisualChildren<T>(child)) yield return childOfChild;
                }
            }
        }
    }
}