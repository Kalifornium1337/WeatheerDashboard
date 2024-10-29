using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace WeatherDashboard
{
    public partial class MainWindow : Window
    {
        private List<ForecastItem> hourlyForecast;
        private int currentHourIndex;

        public MainWindow()
        {
            InitializeComponent();
            CityInput.TextChanged += CityInput_TextChanged; // Attach event for TextBox changes
            UpdatePlaceholder(); // Initialize placeholder visibility
            hourlyForecast = new List<ForecastItem>();
            currentHourIndex = 0;

            // Allow the window to be draggable from the custom title bar
            MainGrid.MouseLeftButtonDown += (sender, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                    this.DragMove();
            };
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CityInput_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdatePlaceholder();
        }

        private void UpdatePlaceholder()
        {
            CityInputPlaceholder.Visibility = string.IsNullOrEmpty(CityInput.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void FetchWeatherButton_Click(object sender, RoutedEventArgs e)
        {
            string city = CityInput.Text;
            string apiKey = ; // Your OpenWeatherMap API key
            string currentWeatherApiUrl = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";
            string forecastApiUrl = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&appid={apiKey}&units=metric";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage weatherResponse = await client.GetAsync(currentWeatherApiUrl);
                if (weatherResponse.IsSuccessStatusCode)
                {
                    string result = await weatherResponse.Content.ReadAsStringAsync();
                    var weatherData = JsonConvert.DeserializeObject<WeatherResponse>(result);

                    UpdateCurrentWeatherDisplay(weatherData);
                }

                HttpResponseMessage forecastResponse = await client.GetAsync(forecastApiUrl);
                if (forecastResponse.IsSuccessStatusCode)
                {
                    string forecastResult = await forecastResponse.Content.ReadAsStringAsync();
                    var forecastData = JsonConvert.DeserializeObject<ForecastResponse>(forecastResult);

                    hourlyForecast = forecastData.list;
                    currentHourIndex = 0;

                    if (hourlyForecast.Count > 0)
                    {
                        UpdateHourlyForecastDisplay();
                    }
                }
            }
        }

        private void UpdateCurrentWeatherDisplay(WeatherResponse weatherData)
        {
            TemperatureLabel.Text = $"Temperature: {weatherData.main?.temp}°C";
            FeelsLikeLabel.Text = $"Feels Like: {weatherData.main?.feels_like}°C";
            HumidityLabel.Text = $"Humidity: {weatherData.main?.humidity}%";
            WeatherMainLabel.Text = $"Weather: {weatherData.weather?[0].main}";
            WeatherDescriptionLabel.Text = $"Description: {weatherData.weather?[0].description}";
            WindSpeedLabel.Text = $"Wind Speed: {weatherData.wind?.speed} m/s";
            CountryLabel.Text = $"Country: {weatherData.sys?.country}";
            SunriseLabel.Text = $"Sunrise: {UnixTimeToDateTime(weatherData.sys?.sunrise)}";
            SunsetLabel.Text = $"Sunset: {UnixTimeToDateTime(weatherData.sys?.sunset)}";

            string iconCode = weatherData.weather?[0].icon;
            SetWeatherIcon(iconCode);
        }

        private void UpdateHourlyForecastDisplay()
        {
            if (hourlyForecast.Count == 0 || currentHourIndex < 0 || currentHourIndex >= hourlyForecast.Count)
                return;

            var currentHourForecast = hourlyForecast[currentHourIndex];
            HourLabel.Text = $"Hour: {currentHourForecast.dt_txt}";

            TemperatureLabel.Text = $"Temperature: {currentHourForecast.main?.temp}°C";
            FeelsLikeLabel.Text = $"Feels Like: {currentHourForecast.main?.feels_like}°C";
            HumidityLabel.Text = $"Humidity: {currentHourForecast.main?.humidity}%";
            WeatherMainLabel.Text = $"Weather: {currentHourForecast.weather?[0].main}";
            WeatherDescriptionLabel.Text = $"Description: {currentHourForecast.weather?[0].description}";
            WindSpeedLabel.Text = $"Wind Speed: {currentHourForecast.wind?.speed} m/s";

            string iconCode = currentHourForecast.weather?[0].icon;
            SetWeatherIcon(iconCode);
        }

        private DateTime UnixTimeToDateTime(long? unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime ?? 0).DateTime;
        }

        private void SetWeatherIcon(string iconCode)
        {
            if (!string.IsNullOrEmpty(iconCode))
            {
                string iconUrl = $"http://openweathermap.org/img/wn/{iconCode}.png";
                WeatherIcon.Source = new BitmapImage(new Uri(iconUrl));
            }
        }

        private void PreviousHourButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentHourIndex > 0)
            {
                currentHourIndex--;
                UpdateHourlyForecastDisplay();
            }
        }

        private void NextHourButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentHourIndex < hourlyForecast.Count - 1)
            {
                currentHourIndex++;
                UpdateHourlyForecastDisplay();
            }
        }
    }

    // Forecast classes
    public class WeatherResponse
    {
        public Main main { get; set; }
        public Sys sys { get; set; }
        public Wind wind { get; set; }
        public Weather[] weather { get; set; }
    }

    public class ForecastResponse
    {
        public List<ForecastItem> list { get; set; }
    }

    public class ForecastItem
    {
        public Main main { get; set; }
        public List<Weather> weather { get; set; }
        public Wind wind { get; set; }
        public string dt_txt { get; set; }
    }

    public class Main
    {
        public double temp { get; set; }
        public double feels_like { get; set; }
        public int humidity { get; set; }
    }

    public class Sys
    {
        public string country { get; set; }
        public long sunrise { get; set; }
        public long sunset { get; set; }
    }

    public class Wind
    {
        public double speed { get; set; }
    }

    public class Weather
    {
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }
}
