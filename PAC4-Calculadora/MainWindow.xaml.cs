using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input; // Necesario para capturar el atajo de teclado al salir

namespace PAC4_Calculadora
{
    public partial class MainWindow : Window
    {
        private double _lastNumber, _result;
        private string _currentOperator;
        private bool _isDoomRunning = false;

        public MainWindow()
        {
            InitializeComponent();
            // Registramos un evento para poder salir del juego con Ctrl+Q
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;
        }

        // Lógica para los números
        private void BtnNumber_Click(object sender, RoutedEventArgs e)
        {
            if (_isDoomRunning) return;

            Button button = (Button)sender;
            if (CalcDisplay.Text == "0")
                CalcDisplay.Text = button.Content.ToString();
            else
                CalcDisplay.Text += button.Content.ToString();

            // EL TRIGGER: Si escribes 666, se abre Doom en pantalla completa.
            if (CalcDisplay.Text == "666")
            {
                RunDoomFullScreen();
            }
        }

        // Lógica para los operadores matemáticos (+, -, *, /)
        private void BtnOperator_Click(object sender, RoutedEventArgs e)
        {
            if (_isDoomRunning) return;
            Button button = (Button)sender;
            try
            {
                _lastNumber = double.Parse(CalcDisplay.Text);
                _currentOperator = button.Content.ToString();
                CalcDisplay.Text = "0";
            }
            catch { CalcDisplay.Text = "Error"; }
        }

        // Calcular el resultado
        private void BtnEquals_Click(object sender, RoutedEventArgs e)
        {
            if (_isDoomRunning) return;
            try
            {
                double newNumber = double.Parse(CalcDisplay.Text);
                switch (_currentOperator)
                {
                    case "+": _result = _lastNumber + newNumber; break;
                    case "-": _result = _lastNumber - newNumber; break;
                    case "*": _result = _lastNumber * newNumber; break;
                    case "/":
                        if (newNumber != 0) _result = _lastNumber / newNumber;
                        else { CalcDisplay.Text = "Error"; return; }
                        break;
                }
                CalcDisplay.Text = _result.ToString();
            }
            catch { CalcDisplay.Text = "Error"; }
        }

        // Botón "C" (Clear) - Sirve para reiniciar
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            if (_isDoomRunning)
            {
                ExitDoom();
            }
            else
            {
                CalcDisplay.Text = "0";
                _lastNumber = 0;
                _currentOperator = null;
            }
        }

        // El método mágico que ejecuta Doom de forma local y en pantalla completa
        private async void RunDoomFullScreen()
        {
            if (_isDoomRunning) return;
            _isDoomRunning = true;

            // 1. Ocultamos la calculadora y mostramos el contenedor
            CalculatorUI.Visibility = Visibility.Collapsed;
            DoomContainer.Visibility = Visibility.Visible;

            // 2. Pantalla completa nativa (sin bordes y maximizada)
            this.WindowStyle = WindowStyle.None;
            this.WindowState = WindowState.Maximized;

            // 3. Inicializamos el motor
            if (DoomScreen.CoreWebView2 == null)
            {
                await DoomScreen.EnsureCoreWebView2Async(null);
            }

            // 4. Buscamos la ruta física donde están nuestros archivos locales
            string localFolderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DoomFiles");

            // 5. Creamos un "servidor virtual" falso llamado "doom.offline"
            DoomScreen.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "doom.offline",
                localFolderPath,
                Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow
            );

            // 6. Cargamos el juego usando nuestro dominio falso
            DoomScreen.Source = new Uri("https://doom.offline/index.html");

            // 7. Damos el foco al juego para poder jugar inmediatamente
            DoomScreen.Focus();
        }

        // Método para salir del juego y restaurar la calculadora
        private void ExitDoom()
        {
            _isDoomRunning = false;

            // 1. RESTAURAR LA VENTANA (Muy importante para no quedarse atrapado)
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            this.WindowState = WindowState.Normal;

            // 2. Ocultamos el motor de Doom y lo detenemos
            DoomContainer.Visibility = Visibility.Collapsed;
            //DoomScreen.Source = null;

            // 3. Restauramos la interfaz de la calculadora
            CalculatorUI.Visibility = Visibility.Visible;
            CalcDisplay.Text = "0";
            _lastNumber = 0;
            _currentOperator = null;
        }

        // Atajo de teclado de emergencia (Ctrl + Q) para salir del juego
        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_isDoomRunning && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && e.Key == Key.Q)
            {
                ExitDoom();
                e.Handled = true;
            }
        }
    }
}