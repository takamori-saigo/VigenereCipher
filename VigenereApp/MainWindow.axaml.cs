using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using VigenereSimple;

namespace VigenereApp
{
    public partial class MainWindow : Window
    {
        private int _cycleStep = 0;
        private int _alphabetSize = 33;

        public MainWindow()
        {
            InitializeComponent();

            var btnEncrypt = this.FindControl<Button>("BtnEncrypt");
            var btnDecrypt = this.FindControl<Button>("BtnDecrypt");
            var btnNext = this.FindControl<Button>("BtnNextCandidate");

            if (btnEncrypt != null) btnEncrypt.Click += OnEncryptClick;
            if (btnDecrypt != null) btnDecrypt.Click += OnDecryptClick;
            if (btnNext != null) btnNext.Click += OnNextCandidateClick;
        }

        private int StepToShift(int step)
        {
            int n = _alphabetSize;
            int cycleLen = 2 * n;
            step = ((step - 1) % cycleLen) + 1;
            return step <= n ? -step : step - n;
        }

        private void OnEncryptClick(object? sender, RoutedEventArgs e)
        {
            if (!ValidateEncrypt(out int shift)) return;

            string text = this.FindControl<TextBox>("TxtInput")?.Text ?? "";
            var result = CaesarCipher.Encrypt(text, shift);

            var output = this.FindControl<TextBox>("TxtOutput");
            if (output != null) output.Text = result;

            ResetNextButton();
        }

        private void OnDecryptClick(object? sender, RoutedEventArgs e)
        {
            if (!ValidateDecrypt()) return;

            string text = this.FindControl<TextBox>("TxtInput")?.Text ?? "";
            string shiftText = this.FindControl<TextBox>("TxtShift")?.Text ?? "";

            var output = this.FindControl<TextBox>("TxtOutput");
            if (output == null) return;

            if (string.IsNullOrWhiteSpace(shiftText))
            {
                autoCrack(text);
            }
            else
            {
                if (int.TryParse(shiftText, out int shift))
                {
                    output.Text = CaesarCipher.Decrypt(text, shift);
                    ResetNextButton();
                }
                else
                {
                    ShowError("Ошибка", "Поле 'Смещение' должно быть целым числом.");
                }
            }
        }

        private void autoCrack(string text)
        {
            _alphabetSize = text.Any(c => char.IsLetter(c) && c >= 128) ? 33 : 26;

            var candidates = CaesarCipher.GetAllDecryptions(text);
            int bestShift = candidates.OrderBy(v => v.Score).First().Shift;

            var output = this.FindControl<TextBox>("TxtOutput");
            if (output != null) output.Text = CaesarCipher.Decrypt(text, bestShift);

            var info = this.FindControl<TextBlock>("LblCandidateInfo");
            if (info != null)
                info.Text = $"МНК определил сдвиг: {bestShift}";

            var btn = this.FindControl<Button>("BtnNextCandidate");
            if (btn != null)
            {
                _cycleStep = 0;
                btn.IsEnabled = true;
                btn.Content = "Далее (сдвиг -1)";
            }
        }

        private void OnNextCandidateClick(object? sender, RoutedEventArgs e)
        {
            string text = this.FindControl<TextBox>("TxtInput")?.Text ?? "";
            if (string.IsNullOrWhiteSpace(text)) return;

            if (_alphabetSize == 0)
                _alphabetSize = text.Any(c => char.IsLetter(c) && c >= 128) ? 33 : 26;

            _cycleStep++;
            int shift = StepToShift(_cycleStep);

            var result = CaesarCipher.Decrypt(text, shift);

            var output = this.FindControl<TextBox>("TxtOutput");
            if (output != null) output.Text = result;

            int cycleLen = 2 * _alphabetSize;
            int displayStep = ((_cycleStep - 1) % cycleLen) + 1;

            var info = this.FindControl<TextBlock>("LblCandidateInfo");
            if (info != null)
                info.Text = $"Перебор: сдвиг {shift} ({displayStep}/{cycleLen})";

            var btn = this.FindControl<Button>("BtnNextCandidate");
            if (btn != null)
            {
                int nextShift = StepToShift(_cycleStep + 1);
                btn.Content = $"Далее (сдвиг {nextShift})";
                btn.IsEnabled = true;
            }
        }

        private void ResetNextButton()
        {
            _cycleStep = 0;

            var btn = this.FindControl<Button>("BtnNextCandidate");
            if (btn != null)
            {
                btn.IsEnabled = true;
                btn.Content = "Далее (сдвиг -1)";
            }

            var info = this.FindControl<TextBlock>("LblCandidateInfo");
            if (info != null) info.Text = "";
        }

        private bool ValidateEncrypt(out int shift)
        {
            shift = 0;
            string shiftText = this.FindControl<TextBox>("TxtShift")?.Text ?? "";
            string input = this.FindControl<TextBox>("TxtInput")?.Text ?? "";

            if (string.IsNullOrWhiteSpace(shiftText) || !int.TryParse(shiftText, out shift))
            {
                ShowError("Ошибка", "Поле 'Смещение' должно быть целым числом.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                ShowError("Ошибка", "Поле 'Текст' не может быть пустым.");
                return false;
            }

            return true;
        }

        private bool ValidateDecrypt()
        {
            string input = this.FindControl<TextBox>("TxtInput")?.Text ?? "";

            if (string.IsNullOrWhiteSpace(input))
            {
                ShowError("Ошибка", "Поле 'Текст' не может быть пустым.");
                return false;
            }

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
