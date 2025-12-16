using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace Exsamen_17_12_2025
{
        public partial class Form1 : Form
        {
            private string path;
            private List<string> wordsToReplace = new List<string>();
            private List<string> replacementWords = new List<string>();

            public Form1()
            {
                InitializeComponent();
            }

            private void button2_Click(object sender, EventArgs e)
            {
                if (string.IsNullOrWhiteSpace(textBox1.Text))
                {
                    MessageBox.Show("Введите слова для замены!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(textBox3.Text))
                {
                    MessageBox.Show("Введите слова для замены!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                
                wordsToReplace = textBox1.Text.Split('\n')
                                  .Where(line => !string.IsNullOrWhiteSpace(line))
                                  .Select(line => line.Trim())
                                  .ToList();

                replacementWords = textBox3.Text.Split('\n')
                                  .Where(line => !string.IsNullOrWhiteSpace(line))
                                  .Select(line => line.Trim())
                                  .ToList();

                
                if (wordsToReplace.Count != replacementWords.Count)
                {
                    MessageBox.Show($"Количество слов не совпадает!\n" +
                                  $"Слов для замены: {wordsToReplace.Count}\n" +
                                  $"Слов для вставки: {replacementWords.Count}",
                                  "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (wordsToReplace.Count == 0)
                {
                    MessageBox.Show("Не найдено слов для замены!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                ProcessFile();
            }

            private void ProcessFile()
            {
                if (string.IsNullOrEmpty(path))
                {
                    MessageBox.Show("Сначала выберите файл!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    if (!File.Exists(path))
                    {
                        MessageBox.Show("Файл не найден!", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    
                    string content = File.ReadAllText(path);
                    string originalContent = content; 

                    
                    Dictionary<string, int> replacementStats = new Dictionary<string, int>();
                    Dictionary<string, string> actualReplacements = new Dictionary<string, string>();

                    
                    for (int i = 0; i < wordsToReplace.Count; i++)
                    {
                        actualReplacements[wordsToReplace[i]] = replacementWords[i];
                    }

                    
                    foreach (var replacement in actualReplacements)
                    {
                        string word = replacement.Key;
                        string newWord = replacement.Value;

                        if (string.IsNullOrWhiteSpace(word)) continue;

                        
                        string pattern;
                        if (radioButton1.Checked) 
                        {
                            pattern = $@"\b{Regex.Escape(word)}\b";
                        }
                        else if (radioButton2.Checked) 
                        {
                            pattern = $@"\b{Regex.Escape(word)}\b";
                            
                            int count = Regex.Matches(content, pattern).Count;
                            if (count > 0)
                            {
                                content = Regex.Replace(content, pattern, newWord,
                                    RegexOptions.None, TimeSpan.FromSeconds(1));
                                replacementStats[word] = count;
                            }
                            continue;
                        }
                        else if (radioButton3.Checked) 
                        {
                            pattern = Regex.Escape(word);
                        }
                        else if (radioButton4.Checked) 
                        {
                            pattern = $@"\b{Regex.Escape(word)}\b";
                            newWord = "";
                        }
                        else
                        {
                            MessageBox.Show("Выберите режим замены!", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                       
                        int matchesCount = Regex.Matches(originalContent, pattern,
                            radioButton2.Checked ? RegexOptions.None : RegexOptions.IgnoreCase).Count;

                        if (matchesCount > 0)
                        {
                            content = Regex.Replace(content, pattern, newWord,
                                radioButton2.Checked ? RegexOptions.None : RegexOptions.IgnoreCase,
                                TimeSpan.FromSeconds(1));
                            replacementStats[word] = matchesCount;
                        }
                    }

                    
                    textBox2.Text = content;

                    
                    string newPath = GetNewFilePath(path);
                    File.WriteAllText(newPath, content);

                   
                    ShowStatistics(replacementStats, newPath);

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обработке файла:\n{ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            private string GetNewFilePath(string originalPath)
            {
                string directory = Path.GetDirectoryName(originalPath);
                string fileName = Path.GetFileNameWithoutExtension(originalPath);
                string extension = Path.GetExtension(originalPath);

                
                string newFileName = $"{fileName}_modified{extension}";

                return Path.Combine(directory, newFileName);
            }

            private void ShowStatistics(Dictionary<string, int> stats, string newPath)
            {
                if (stats.Count == 0)
                {
                    MessageBox.Show("Совпадений не найдено!", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine($"Обработка завершена!");
                sb.AppendLine($"Режим: {(radioButton4.Checked ? "Удаление" : "Замена")}");
                sb.AppendLine($"Новый файл: {Path.GetFileName(newPath)}");
                sb.AppendLine($"\nСтатистика замен:");

                int total = 0;
                foreach (var stat in stats)
                {
                    int index = wordsToReplace.IndexOf(stat.Key);
                    string replacement = index >= 0 && index < replacementWords.Count
                        ? replacementWords[index]
                        : "(удалено)";
                    sb.AppendLine($"  '{stat.Key}' → '{replacement}': {stat.Value} замен");
                    total += stat.Value;
                }

                sb.AppendLine($"\nВсего замен: {total}");

                label4.Text = $"Заменено слов: {total}";
                MessageBox.Show(sb.ToString(), "Результат",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            private void button1_Click(object sender, EventArgs e)
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                    openFileDialog.Title = "Выберите файл";
                    openFileDialog.Multiselect = false;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        path = openFileDialog.FileName;
                        label3.Text = $"Файл: {Path.GetFileName(path)}";

                        
                        try
                        {
                            string preview = File.ReadAllText(path);
                            textBox2.Text = preview.Length > 1000 ?
                                preview.Substring(0, 1000) + "..." : preview;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка чтения файла:\n{ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }

            private void button3_Click(object sender, EventArgs e)
            {
               
                textBox1.Clear();
                textBox2.Clear();
                textBox3.Clear();
                wordsToReplace.Clear();
                replacementWords.Clear();
                label4.Text = "Заменено слов: 0";
            }

            private void button4_Click(object sender, EventArgs e)
            {
               
                if (string.IsNullOrWhiteSpace(textBox2.Text))
                {
                    MessageBox.Show("Нет данных для сохранения!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                    saveFileDialog.Title = "Сохранить результат";
                    saveFileDialog.FileName = "результат.txt";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            File.WriteAllText(saveFileDialog.FileName, textBox2.Text);
                            MessageBox.Show("Файл успешно сохранен!", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка сохранения:\n{ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }

            private void button5_Click(object sender, EventArgs e)
            {
                
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                    openFileDialog.Title = "Загрузить пары слов (старое=новое)";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            string[] lines = File.ReadAllLines(openFileDialog.FileName)
                                               .Where(line => !string.IsNullOrWhiteSpace(line) &&
                                                              !line.StartsWith("#") &&
                                                              line.Contains('='))
                                               .ToArray();

                            List<string> oldWords = new List<string>();
                            List<string> newWords = new List<string>();

                            foreach (string line in lines)
                            {
                                string[] parts = line.Split('=');
                                if (parts.Length >= 2)
                                {
                                    oldWords.Add(parts[0].Trim());
                                    newWords.Add(parts[1].Trim());
                                }
                            }

                            textBox1.Text = string.Join("\n", oldWords);
                            textBox3.Text = string.Join("\n", newWords);

                            MessageBox.Show($"Загружено {oldWords.Count} пар слов", "Информация",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка загрузки:\n{ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }

            private void Form1_Load(object sender, EventArgs e)
            {
                
                radioButton1.Checked = true; 
            }

            private void button6_Click(object sender, EventArgs e)
            {
                
                textBox1.Text = "плохой\nстарый\nмедленный\nошибка";
                textBox3.Text = "хороший\nновый\nбыстрый\nисправлено";
            }

    }
} 


