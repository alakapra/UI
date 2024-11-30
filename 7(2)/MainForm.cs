using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace _7_2_
{
    public partial class MainForm : Form
    {
        // Элементы управления на форме
        private ProgressBar waterLevelBar;
        private Button pumpButton;
        private Button valveButton;
        private Label pressureLabel;
        private Label flowLabel;
        private Label waterLevelLabel;

        // Данные с сервера
        private double pressure = 0.0;
        private double flow = 0.0;
        private int waterLevel = 0;
        private bool pumpActive = false;
        private bool valveOpen = true;

        private HttpClient client;

        public MainForm()
        {
            InitializeComponent();
            InitializeComponents();  // Инициализация интерфейсных компонентов
            client = new HttpClient();  // Создаём новый HttpClient для отправки запросов
            StartPolling();  // Запуск опроса сервера для получения данных
        }

        // Метод для инициализации элементов управления на форме
        private void InitializeComponents()
        {
            waterLevelBar = new ProgressBar { Width = 200, Height = 30, Top = 50, Left = 50, Value = waterLevel };
            this.Controls.Add(waterLevelBar);

            pumpButton = new Button { Top = 100, Left = 50, Text = "Pump: Off" };
            pumpButton.Click += PumpButton_Click;
            this.Controls.Add(pumpButton);

            valveButton = new Button { Top = 150, Left = 50,  Text = "Valve: Open", Width = 150 };
            valveButton.Click += ValveButton_Click;
            this.Controls.Add(valveButton);

            pressureLabel = new Label { Top = 200, Left = 50, Text = $"Pressure: {pressure} bar" };
            this.Controls.Add(pressureLabel);

            flowLabel = new Label { Top = 250, Left = 50, Text = $"Flow: {flow} L/min" };
            this.Controls.Add(flowLabel);

            waterLevelLabel = new Label { Top = 300, Left = 50, Text = $"Water Level: {waterLevel}%" };
            this.Controls.Add(waterLevelLabel);
        }

        // Функция для опроса сервера
        private async void StartPolling()
        {
            while (true)
            {
                await GetSensorData();  // Получение данных от сервера
                await Task.Delay(1000);  // Пауза 1 секунда перед следующим запросом
            }
        }

        // Отправка GET-запроса на сервер и получение данных
        private async Task GetSensorData()
        {
            try
            {
                // Отправляем GET-запрос на сервер
                HttpResponseMessage response = await client.GetAsync("http://localhost:8080/api/sensors");

                if (response.IsSuccessStatusCode)
                {
                    // Получаем данные в формате JSON
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var sensorData = JsonConvert.DeserializeObject<SensorData>(jsonResponse);

                    // Обновляем данные
                    pressure = sensorData.Pressure;
                    flow = sensorData.Flow;
                    waterLevel = sensorData.WaterLevel;
                    pumpActive = sensorData.PumpActive;
                    valveOpen = sensorData.ValveOpen;

                    // Обновляем UI на основе полученных данных
                    UpdateUI();
                }
                else
                {
                    MessageBox.Show("Error: Unable to fetch data from the server.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }

        // Обновление данных на форме
        private void UpdateUI()
        {
            // Безопасное обновление UI
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateUI));
                return;
            }

            // Обновляем элементы на форме
            waterLevelBar.Value = waterLevel;
            waterLevelLabel.Text = $"Water Level: {waterLevel}%";
            pressureLabel.Text = $"Pressure: {pressure} bar";
            flowLabel.Text = $"Flow: {flow} L/min";
            pumpButton.Text = pumpActive ? "Pump: On" : "Pump: Off";
            valveButton.Text = valveOpen ? "Valve: Open" : "Valve: Closed";
        }

        // Обработчик кнопки насоса
        private void PumpButton_Click(object sender, EventArgs e)
        {
            // Логика переключения состояния насоса
            pumpActive = !pumpActive;
            pumpButton.Text = pumpActive ? "Pump: On" : "Pump: Off";
        }

        // Обработчик кнопки клапана
        private void ValveButton_Click(object sender, EventArgs e)
        {
            // Логика переключения состояния клапана
            valveOpen = !valveOpen;
            valveButton.Text = valveOpen ? "Valve: Open" : "Valve: Closed";
        }
    }

    // Класс для данных сенсоров
    public class SensorData
    {
        public double Pressure { get; set; }
        public double Flow { get; set; }
        public int WaterLevel { get; set; }
        public bool PumpActive { get; set; }
        public bool ValveOpen { get; set; }
    }
}
