using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Text.RegularExpressions;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using VigenereSimple;

namespace VigenereApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var btnEncrypt = this.FindControl<Button>("BtnEncrypt");
            var btnDecrypt = this.FindControl<Button>("BtnDecrypt");

            if (btnEncrypt != null) btnEncrypt.Click += OnEncryptClick;
            if (btnDecrypt != null) btnDecrypt.Click += OnDecryptClick;
        }

        private void OnEncryptClick(object? sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            string text = this.FindControl<TextBox>("TxtInput")?.Text ?? "";
            string key = this.FindControl<TextBox>("TxtKey")?.Text ?? "";

            var result = VigenereCipher.Process(text, key, true);
            
            var output = this.FindControl<TextBox>("TxtOutput");
            if (output != null) output.Text = result;
        }

        private void OnDecryptClick(object? sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            string text = this.FindControl<TextBox>("TxtInput")?.Text ?? "";
            string key = this.FindControl<TextBox>("TxtKey")?.Text ?? "";

            var result = VigenereCipher.Process(text, key, false);
            
            var output = this.FindControl<TextBox>("TxtOutput");
            if (output != null) output.Text = result;
        }

        private bool ValidateInput()
        {
            string key = this.FindControl<TextBox>("TxtKey")?.Text ?? "";
            string input = this.FindControl<TextBox>("TxtInput")?.Text ?? "";

            if (string.IsNullOrEmpty(key))
            {
                ShowError("Ошибка", "Поле 'Ключ' не может быть пустым.");
                return false;
            }

            if (!Regex.IsMatch(key, @"^[a-zA-Zа-яА-ЯёЁ]+$"))
            {
                ShowError("Ошибка в ключе", "Ключ должен содержать ТОЛЬКО буквы (без цифр и пробелов).");
                return false;
            }

            if (string.IsNullOrEmpty(input))
            {
                ShowError("Ошибка", "Поле 'Текст' не может быть пустым.");
                return false;
            }

            if (Regex.IsMatch(input, @"\d")) 
            {
                ShowError("Ошибка в тексте", "Текст не должен содержать цифр.\nИспользуйте только буквы и знаки препинания.");
                return false;
            }
            
            /*
            if (!Regex.IsMatch(input, @"^[a-zA-Zа-яА-ЯёЁ\s\p{P}]+$"))
            {
                 ShowError("Ошибка в тексте", "Текст содержит недопустимые символы.");
                 return false;
            }
            */

            return true;
        }

        private async void ShowError(string title, string message)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                title,
                message,
                ButtonEnum.Ok
            );

            await box.ShowWindowAsync();
        }
    }
}